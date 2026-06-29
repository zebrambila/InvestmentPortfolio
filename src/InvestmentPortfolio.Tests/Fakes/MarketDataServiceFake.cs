using InvestmentPortfolio.Core.Services;

namespace InvestmentPortfolio.Tests.Fakes;

public class MarketDataServiceFake : IMarketDataService
{
    private readonly Dictionary<string, decimal?> _prices;
    private readonly Dictionary<string, string?> _names;

    public MarketDataServiceFake(
        Dictionary<string, decimal?>? prices = null,
        Dictionary<string, string?>? names = null)
    {
        _prices = prices ?? [];
        _names = names ?? [];
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
}
