namespace InvestmentPortfolio.Core.Services;

public record AssetInfo(string? Name, decimal? Price);

public interface IMarketDataService
{
    Task<decimal?> GetCurrentPriceAsync(string ticker, CancellationToken ct = default);
    Task<Dictionary<string, decimal?>> GetCurrentPricesAsync(IEnumerable<string> tickers, CancellationToken ct = default);
    Task<AssetInfo?> GetAssetInfoAsync(string ticker, CancellationToken ct = default);
    Task<Dictionary<DateTime, decimal?>> GetHistoricalClosePricesAsync(string ticker, IEnumerable<DateTime> dates, CancellationToken ct = default);
}
