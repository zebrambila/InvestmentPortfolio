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

    private int GetIntervalMs() => configuration.GetValue("MarketData:RequestIntervalMs", 300);

    private static async Task<AssetInfo?> FetchWithThrottleAsync(HttpClient client, string ticker, int intervalMs, CancellationToken ct)
    {
        await Throttle.WaitAsync(ct);
        try
        {
            var elapsed = (DateTimeOffset.Now - _lastRequest).TotalMilliseconds;
            if (elapsed < intervalMs)
                await Task.Delay(TimeSpan.FromMilliseconds(intervalMs - elapsed), ct);

            _lastRequest = DateTimeOffset.Now;
            return await FetchAssetInfoAsync(client, ticker, ct);
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
}