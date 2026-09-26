using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Assets;
using InvestmentPortfolio.Core.Services;

namespace InvestmentPortfolio.Tests.Fakes;

public class AssetPriceHistoryServiceFake : IAssetPriceHistoryService
{
    public Task<BackfillPricesResponse> BackfillAsync(int? months = null, CancellationToken ct = default)
        => Task.FromResult(new BackfillPricesResponse());

    public Task SyncCurrentMonthAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
