using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Requests.Patrimony;
using InvestmentPortfolio.Infra.Data;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class PatrimonyHandlerTests
{
    private static async Task<Guid> SeedAssetAsync(InvestmentContext ctx, string ticker = "PETR4")
    {
        var asset = new Asset(ticker, "Petrobras", EAssetType.Ações);
        ctx.Assets.Add(asset);
        await ctx.SaveChangesAsync();
        return asset.Id;
    }

    [TestMethod]
    public async Task Dado_SemTransacoes_Quando_GetEvolutionByMonth_Entao_ZeroFillEmTodosOsMeses()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new PatrimonyHandler(ctx);

        var response = await handler.GetEvolutionByMonthAsync(new GetPatrimonyEvolutionRequest
        {
            DateFrom = new DateTime(2024, 1, 1),
            DateTo = new DateTime(2024, 3, 1)
        });

        Assert.AreEqual(200, response.Code);
        Assert.HasCount(3, response.Data!.Months);
        Assert.IsTrue(response.Data.Months.All(m => m.Invested == 0 && m.MarketValue == 0));
        Assert.AreEqual(1, response.Data.Months[0].Month);
        Assert.AreEqual(3, response.Data.Months[2].Month);
    }

    [TestMethod]
    public async Task Dado_ComprasEVendas_Quando_GetEvolutionByMonth_Entao_CalculaInvestidoLiquidoEValorDeMercado()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);

        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Compra, 100m, 10m, new DateTime(2024, 1, 10), ECustodian.BTG));
        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Venda, 40m, 12m, new DateTime(2024, 2, 10), ECustodian.BTG));
        ctx.AssetPriceHistories.Add(new AssetPriceHistory(assetId, new DateTime(2024, 1, 31), 11m));
        ctx.AssetPriceHistories.Add(new AssetPriceHistory(assetId, new DateTime(2024, 2, 29), 12m));
        await ctx.SaveChangesAsync();

        var handler = new PatrimonyHandler(ctx);
        var response = await handler.GetEvolutionByMonthAsync(new GetPatrimonyEvolutionRequest
        {
            DateFrom = new DateTime(2024, 1, 1),
            DateTo = new DateTime(2024, 2, 1)
        });

        Assert.AreEqual(200, response.Code);
        Assert.HasCount(2, response.Data!.Months);

        var jan = response.Data.Months[0];
        Assert.AreEqual(1000m, jan.Invested);
        Assert.AreEqual(1100m, jan.MarketValue);

        var feb = response.Data.Months[1];
        Assert.AreEqual(520m, feb.Invested);
        Assert.AreEqual(720m, feb.MarketValue);
    }

    [TestMethod]
    public async Task Dado_MesSemPrecoRegistrado_Quando_GetEvolutionByMonth_Entao_UsaPrecoMaisRecenteAnterior()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);

        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Compra, 10m, 100m, new DateTime(2024, 1, 10), ECustodian.BTG));
        ctx.AssetPriceHistories.Add(new AssetPriceHistory(assetId, new DateTime(2024, 1, 31), 105m));
        await ctx.SaveChangesAsync();

        var handler = new PatrimonyHandler(ctx);
        var response = await handler.GetEvolutionByMonthAsync(new GetPatrimonyEvolutionRequest
        {
            DateFrom = new DateTime(2024, 1, 1),
            DateTo = new DateTime(2024, 3, 1)
        });

        Assert.AreEqual(200, response.Code);
        Assert.HasCount(3, response.Data!.Months);
        Assert.IsTrue(response.Data.Months.All(m => m.MarketValue == 1050m));
        Assert.IsTrue(response.Data.Months.All(m => m.Invested == 1000m));
    }

    [TestMethod]
    public async Task Dado_SemHistoricoDePreco_Quando_GetEvolutionByMonth_Entao_MarketValueOmiteAtivoSemPreco()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);

        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Compra, 10m, 100m, new DateTime(2024, 1, 10), ECustodian.BTG));
        await ctx.SaveChangesAsync();

        var handler = new PatrimonyHandler(ctx);
        var response = await handler.GetEvolutionByMonthAsync(new GetPatrimonyEvolutionRequest
        {
            DateFrom = new DateTime(2024, 1, 1),
            DateTo = new DateTime(2024, 1, 1)
        });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(1000m, response.Data!.Months[0].Invested);
        Assert.AreEqual(0m, response.Data.Months[0].MarketValue);
    }
}
