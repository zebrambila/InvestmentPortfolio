using InvestmentPortfolio.Api.Services;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Infra.Data;
using InvestmentPortfolio.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace InvestmentPortfolio.Tests.Services;

[TestClass]
public class AssetPriceHistoryServiceTests
{
    private static async Task<Guid> SeedAssetAsync(InvestmentContext ctx, string ticker = "PETR4")
    {
        var asset = new Asset(ticker, "Petrobras", EAssetType.Ações);
        ctx.Assets.Add(asset);
        await ctx.SaveChangesAsync();
        return asset.Id;
    }

    private static Dictionary<DateTime, decimal?> BuildDailyPrices(decimal price, int daysBack = 400)
    {
        var dict = new Dictionary<DateTime, decimal?>();
        var start = DateTime.Now.Date.AddDays(-daysBack);
        for (var d = start; d <= DateTime.Now.Date; d = d.AddDays(1))
            dict[d] = price;
        return dict;
    }

    [TestMethod]
    public async Task Dado_AtivoSemTransacoes_Quando_BackfillAsync_Entao_NaoGeraHistorico()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);

        var market = new MarketDataServiceFake(historicalPrices: new()
        {
            ["PETR4"] = BuildDailyPrices(50m)
        });
        var service = new AssetPriceHistoryService(ctx, market, NullLogger<AssetPriceHistoryService>.Instance);

        await service.BackfillAsync();

        var count = await ctx.AssetPriceHistories.CountAsync(h => h.AssetId == assetId);
        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task Dado_AtivoComTransacaoAntiga_Quando_BackfillAsyncChamadoDuasVezes_Entao_NaoDuplicaRegistros()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Compra, 10m, 100m, DateTime.Now.AddYears(-5), ECustodian.BTG));
        await ctx.SaveChangesAsync();

        var market = new MarketDataServiceFake(historicalPrices: new()
        {
            ["PETR4"] = BuildDailyPrices(50m)
        });
        var service = new AssetPriceHistoryService(ctx, market, NullLogger<AssetPriceHistoryService>.Instance);

        await service.BackfillAsync();
        var firstRunCount = await ctx.AssetPriceHistories.CountAsync(h => h.AssetId == assetId);

        await service.BackfillAsync();
        var secondRunCount = await ctx.AssetPriceHistories.CountAsync(h => h.AssetId == assetId);

        Assert.AreEqual(12, firstRunCount);
        Assert.AreEqual(firstRunCount, secondRunCount);
    }

    [TestMethod]
    public async Task Dado_HistoricoJaExistenteNoMes_Quando_SyncCurrentMonthAsync_Entao_AtualizaPrecoSemDuplicar()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var monthEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
        ctx.AssetPriceHistories.Add(new AssetPriceHistory(assetId, monthEnd, 10m));
        await ctx.SaveChangesAsync();

        var market = new MarketDataServiceFake(prices: new() { ["PETR4"] = 25m });
        var service = new AssetPriceHistoryService(ctx, market, NullLogger<AssetPriceHistoryService>.Instance);

        await service.SyncCurrentMonthAsync();

        var records = await ctx.AssetPriceHistories.Where(h => h.AssetId == assetId).ToListAsync();
        Assert.HasCount(1, records);
        Assert.AreEqual(25m, records[0].ClosePrice);
    }
}
