using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Core.Services;

public interface IAssetPriceHistoryService
{
    Task<BackfillPricesResponse> BackfillAsync(int? months = null, CancellationToken ct = default);
    Task SyncCurrentMonthAsync(CancellationToken ct = default);
}
