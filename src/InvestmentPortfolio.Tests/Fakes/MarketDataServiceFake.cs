using InvestmentPortfolio.Core.Services;

namespace InvestmentPortfolio.Tests.Fakes;

public class MarketDataServiceFake : IMarketDataService
{
    private readonly Dictionary<string, decimal?> _prices;
    private readonly Dictionary<string, string?> _names;
    private readonly Dictionary<string, Dictionary<DateTime, decimal?>> _historicalPrices;

    public MarketDataServiceFake(
        Dictionary<string, decimal?>? prices = null,
        Dictionary<string, string?>? names = null,
        Dictionary<string, Dictionary<DateTime, decimal?>>? historicalPrices = null)
    {
        _prices = prices ?? [];
        _names = names ?? [];
        _historicalPrices = historicalPrices ?? [];
    }

    public Task<decimal?> GetCurrentPriceAsync(string ticker, CancellationToken ct = default)
        => Task.FromResult(_prices.GetValueOrDefault(ticker.ToUpper()));

    public Task<Dictionary<string, decimal?>> GetCurrentPricesAsync(IEnumerable<string> tickers, CancellationToken ct = default)
    {
        var result = tickers.ToDictionary(
            t => t.ToUpper(),
            t => _prices.GetValueOrDefault(t.ToUpper()));
        return Task.FromResult(result);
    }

    public Task<AssetInfo?> GetAssetInfoAsync(string ticker, CancellationToken ct = default)
    {
        var key = ticker.ToUpper();
        var info = new AssetInfo(_names.GetValueOrDefault(key), _prices.GetValueOrDefault(key));
        return Task.FromResult<AssetInfo?>(info);
    }

    public Task<Dictionary<DateTime, decimal?>> GetHistoricalClosePricesAsync(string ticker, IEnumerable<DateTime> dates, CancellationToken ct = default)
    {
        var map = _historicalPrices.GetValueOrDefault(ticker.ToUpper()) ?? [];
        var result = dates.Select(d => d.Date).Distinct().ToDictionary(d => d, d => map.GetValueOrDefault(d));
        return Task.FromResult(result);
    }
}
