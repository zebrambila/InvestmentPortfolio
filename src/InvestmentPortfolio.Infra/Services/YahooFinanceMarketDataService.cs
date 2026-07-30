using System.Text.Json;
using InvestmentPortfolio.Core.Services;
using Microsoft.Extensions.Configuration;

namespace InvestmentPortfolio.Infra.Services;

public class YahooFinanceMarketDataService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IMarketDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly SemaphoreSlim Throttle = new(1, 1);
    private static DateTimeOffset _lastRequest = DateTimeOffset.MinValue;

    public async Task<decimal?> GetCurrentPriceAsync(string ticker, CancellationToken ct = default)
    {
        var prices = await GetCurrentPricesAsync([ticker], ct);
        return prices.GetValueOrDefault(ticker.ToUpper());
    }

    public async Task<Dictionary<string, decimal?>> GetCurrentPricesAsync(IEnumerable<string> tickers, CancellationToken ct = default)
    {
        var tickerList = tickers.Select(t => t.ToUpper()).ToList();
        var result = tickerList.ToDictionary(t => t, t => (decimal?)null);

        if (tickerList.Count == 0)
            return result;

        var client = httpClientFactory.CreateClient("MarketData");
        var intervalMs = GetIntervalMs();

        foreach (var ticker in tickerList)
        {
            var info = await FetchWithThrottleAsync(client, ticker, intervalMs, ct);
            result[ticker] = info?.Price;
        }

        return result;
    }

    public async Task<AssetInfo?> GetAssetInfoAsync(string ticker, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("MarketData");
        return await FetchWithThrottleAsync(client, ticker.ToUpper(), GetIntervalMs(), ct);
    }

    public async Task<Dictionary<DateTime, decimal?>> GetHistoricalClosePricesAsync(string ticker, IEnumerable<DateTime> dates, CancellationToken ct = default)
    {
        var targetDates = dates.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
        var result = targetDates.ToDictionary(d => d, d => (decimal?)null);
        if (targetDates.Count == 0)
            return result;

        var client = httpClientFactory.CreateClient("MarketData");
        var periodFrom = targetDates[0].AddDays(-10);
        var periodTo = targetDates[^1].AddDays(1);

        var series = await ThrottledFetchAsync(GetIntervalMs(), ct,
            () => FetchHistoricalSeriesAsync(client, ticker.ToUpper(), periodFrom, periodTo, ct));

        foreach (var date in targetDates)
        {
            var priorPoints = series.Where(p => p.Date <= date).ToList();
            result[date] = priorPoints.Count > 0 ? priorPoints[^1].Close : null;
        }

        return result;
    }

    public async Task<List<DividendEvent>> GetDividendEventsAsync(string ticker, DateTime periodFrom, DateTime periodTo, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("MarketData");

        return await ThrottledFetchAsync(GetIntervalMs(), ct,
            () => FetchDividendEventsAsync(client, ticker.ToUpper(), periodFrom, periodTo, ct));
    }

    private int GetIntervalMs() => configuration.GetValue("MarketData:RequestIntervalMs", 300);

    private static Task<AssetInfo?> FetchWithThrottleAsync(HttpClient client, string ticker, int intervalMs, CancellationToken ct)
        => ThrottledFetchAsync(intervalMs, ct, () => FetchAssetInfoAsync(client, ticker, ct));

    private static async Task<T> ThrottledFetchAsync<T>(int intervalMs, CancellationToken ct, Func<Task<T>> fetch)
    {
        await Throttle.WaitAsync(ct);
        try
        {
            var elapsed = (DateTimeOffset.Now - _lastRequest).TotalMilliseconds;
            if (elapsed < intervalMs)
                await Task.Delay(TimeSpan.FromMilliseconds(intervalMs - elapsed), ct);

            _lastRequest = DateTimeOffset.Now;
            return await fetch();
        }
        finally
        {
            Throttle.Release();
        }
    }
    
    private static async Task<AssetInfo?> FetchAssetInfoAsync(HttpClient client, string ticker, CancellationToken ct)
    {
        try
        {
            var response = await client.GetAsync($"v8/finance/chart/{ticker}.SA?interval=1d&range=1d", ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);

            var meta = doc
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta");

            var price = meta.TryGetProperty("regularMarketPrice", out var p) ? p.GetDecimal() : (decimal?)null;
            var name = meta.TryGetProperty("longName", out var n) ? n.GetString() : null;

            return new AssetInfo(name, price);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<List<(DateTime Date, decimal Close)>> FetchHistoricalSeriesAsync(HttpClient client, string ticker, DateTime periodFrom, DateTime periodTo, CancellationToken ct)
    {
        try
        {
            var period1 = new DateTimeOffset(periodFrom, TimeSpan.Zero).ToUnixTimeSeconds();
            var period2 = new DateTimeOffset(periodTo, TimeSpan.Zero).ToUnixTimeSeconds();

            var response = await client.GetAsync($"v8/finance/chart/{ticker}.SA?interval=1d&period1={period1}&period2={period2}", ct);
            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);

            var chartResult = doc.GetProperty("chart").GetProperty("result")[0];
            var timestamps = chartResult.GetProperty("timestamp");
            var closes = chartResult.GetProperty("indicators").GetProperty("quote")[0].GetProperty("close");

            var series = new List<(DateTime Date, decimal Close)>();
            for (var i = 0; i < timestamps.GetArrayLength(); i++)
            {
                if (closes[i].ValueKind != JsonValueKind.Number)
                    continue;

                var date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64()).UtcDateTime.Date;
                series.Add((date, closes[i].GetDecimal()));
            }

            return [.. series.OrderBy(p => p.Date)];
        }
        catch
        {
            return [];
        }
    }

    private static async Task<List<DividendEvent>> FetchDividendEventsAsync(HttpClient client, string ticker, DateTime periodFrom, DateTime periodTo, CancellationToken ct)
    {
        try
        {
            var period1 = new DateTimeOffset(periodFrom, TimeSpan.Zero).ToUnixTimeSeconds();
            var period2 = new DateTimeOffset(periodTo, TimeSpan.Zero).ToUnixTimeSeconds();

            var response = await client.GetAsync($"v8/finance/chart/{ticker}.SA?interval=1d&period1={period1}&period2={period2}&events=div", ct);
            if (!response.IsSuccessStatusCode)
                return [];

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);

            var chartResult = doc.GetProperty("chart").GetProperty("result")[0];
            if (!chartResult.TryGetProperty("events", out var events) || !events.TryGetProperty("dividends", out var dividends))
                return [];

            var result = new List<DividendEvent>();
            foreach (var dividend in dividends.EnumerateObject())
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(dividend.Value.GetProperty("date").GetInt64()).UtcDateTime.Date;
                var amount = dividend.Value.GetProperty("amount").GetDecimal();
                result.Add(new DividendEvent(date, amount));
            }

            return [.. result.OrderBy(e => e.Date)];
        }
        catch
        {
            return [];
        }
    }
}