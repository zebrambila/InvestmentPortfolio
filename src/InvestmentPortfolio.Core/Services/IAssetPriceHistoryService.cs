namespace InvestmentPortfolio.Core.Services;

public interface IAssetPriceHistoryService
{
    Task BackfillAsync(CancellationToken ct = default);
    Task SyncCurrentMonthAsync(CancellationToken ct = default);
}
