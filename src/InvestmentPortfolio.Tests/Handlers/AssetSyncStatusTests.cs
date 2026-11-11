using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class AssetSyncStatusTests
{
    private static AssetHandler CreateHandler(Infra.Data.InvestmentContext ctx) =>
        new(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());

    [TestMethod]
    public async Task Dado_NenhumaExecucao_Quando_GetSyncStatusAsyncChamado_Entao_RetornaStatusNulos()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = CreateHandler(ctx);

        var result = await handler.GetSyncStatusAsync();

        Assert.AreEqual(200, result.Code);
        Assert.IsNull(result.Data!.SyncPrices);
        Assert.IsNull(result.Data.BackfillPrices);
    }

    [TestMethod]
    public async Task Dado_SyncExecutado_Quando_GetSyncStatusAsyncChamado_Entao_RetornaDataDaUltimaExecucao()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = CreateHandler(ctx);
        var before = DateTime.Now;

        await handler.SyncPricesAsync();
        await handler.SyncPricesAsync();
        var result = await handler.GetSyncStatusAsync();

        Assert.IsNotNull(result.Data!.SyncPrices);
        Assert.IsTrue(result.Data.SyncPrices.Success);
        Assert.IsTrue(result.Data.SyncPrices.ExecutedAt >= before);
        Assert.IsNull(result.Data.BackfillPrices);
        Assert.AreEqual(1, ctx.JobExecutions.Count());
    }

    [TestMethod]
    public async Task Dado_BackfillExecutado_Quando_GetSyncStatusAsyncChamado_Entao_RetornaDataDoBackfill()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = CreateHandler(ctx);

        await handler.BackfillPricesAsync(new BackfillPricesRequest());
        var result = await handler.GetSyncStatusAsync();

        Assert.IsNotNull(result.Data!.BackfillPrices);
        Assert.IsTrue(result.Data.BackfillPrices.Success);
        Assert.IsNull(result.Data.SyncPrices);
    }
}
