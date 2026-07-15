using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class AssetGetAllTests
{
    private static AssetHandler AssetHandler(InvestmentPortfolio.Infra.Data.InvestmentContext ctx, decimal currentPrice = 0m)
    {
        var prices = new Dictionary<string, decimal?> { ["PETR4"] = currentPrice, ["MXRF11"] = currentPrice };
        return new AssetHandler(ctx, new MarketDataServiceFake(prices));
    }

    private static async Task<Guid> SeedAssetAsync(
        InvestmentPortfolio.Infra.Data.InvestmentContext ctx,
        string ticker, EAssetType type, decimal currentPrice = 0m)
    {
        var asset = new Asset(ticker, ticker, type);
        asset.UpdateCurrentPrice(currentPrice);
        ctx.Assets.Add(asset);
        await ctx.SaveChangesAsync();
        return asset.Id;
    }

    private static async Task SeedTransactionAsync(
        InvestmentPortfolio.Infra.Data.InvestmentContext ctx,
        Guid assetId, EOperationType side, decimal qty, decimal unitPrice)
    {
        var t = new Transaction(assetId, side, qty, unitPrice, DateTime.UtcNow, ECustodian.BTG);
        ctx.Transactions.Add(t);
        await ctx.SaveChangesAsync();
    }

    // --- Filtros ---

    [TestMethod]
    public async Task Dado_AssetsAtivos_Quando_GetAll_Entao_ExcluiDeletados()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        var id = await SeedAssetAsync(ctx, "MXRF11", EAssetType.FIIs);
        var asset = await ctx.Assets.FindAsync(id);
        asset!.Delete();
        await ctx.SaveChangesAsync();

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(1, response.TotalCount);
        Assert.AreEqual("PETR4", response.Data![0].Ticker);
    }

    [TestMethod]
    public async Task Dado_FiltroAcoes_Quando_GetAll_Entao_RetornaApenasAcoes()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedAssetAsync(ctx, "VALE3", EAssetType.Ações);
        await SeedAssetAsync(ctx, "MXRF11", EAssetType.FIIs);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest
        {
            PageNumber = 1, PageSize = 25,
            Type = EAssetType.Ações
        });

        Assert.AreEqual(2, response.TotalCount);
        Assert.IsTrue(response.Data!.All(a => a.Type == EAssetType.Ações));
    }

    [TestMethod]
    public async Task Dado_FiltroFIIs_Quando_GetAll_Entao_RetornaApenasFIIs()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedAssetAsync(ctx, "MXRF11", EAssetType.FIIs);
        await SeedAssetAsync(ctx, "HGLG11", EAssetType.FIIs);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest
        {
            PageNumber = 1, PageSize = 25,
            Type = EAssetType.FIIs
        });

        Assert.AreEqual(2, response.TotalCount);
        Assert.IsTrue(response.Data!.All(a => a.Type == EAssetType.FIIs));
    }

    // --- PositionQuantity ---

    [TestMethod]
    public async Task Dado_ComprasEVendas_Quando_GetAll_Entao_PositionQuantityEhSaldoLiquido()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var assetId = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, 35m);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 50m, 38m);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 30m, 40m);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(120m, response.Data![0].PositionQuantity);
    }

    [TestMethod]
    public async Task Dado_SemTransacoes_Quando_GetAll_Entao_PositionQuantityZero()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(0m, response.Data![0].PositionQuantity);
    }

    // --- AveragePrice ---

    [TestMethod]
    public async Task Dado_DuasCompras_Quando_GetAll_Entao_AveragePriceEhCustoTotalDivididoPorQtyCompra()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var assetId = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, 30m); // 3000
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, 40m); // 4000
        // TotalInvested = 7000, QtyCompra = 200 → AveragePrice = 35

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(35m, response.Data![0].AveragePrice);
    }

    [TestMethod]
    public async Task Dado_VendaNaoAfetaPrecoMedio_Quando_GetAll_Entao_AveragePriceBasadoSoNasCompras()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var assetId = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, 35m); // TotalInvested = 3500
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 20m, 50m);
        // AveragePrice = 3500 / 100 = 35 (venda não altera)

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(35m, response.Data![0].AveragePrice);
    }

    [TestMethod]
    public async Task Dado_SemTransacoes_Quando_GetAll_Entao_AveragePriceZero()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(0m, response.Data![0].AveragePrice);
    }

    // --- TotalInvested ---

    [TestMethod]
    public async Task Dado_Compras_Quando_GetAll_Entao_TotalInvestedEhSomaDasCompras()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var assetId = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, 35m); // 3500
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 50m, 40m);  // 2000
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 20m, 45m);   // não soma

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(5500m, response.Data![0].TotalInvested);
    }

    // --- CurrentTotalValue ---

    [TestMethod]
    public async Task Dado_PositionQuantityECurrentPrice_Quando_GetAll_Entao_CurrentTotalValueEhProduto()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx, currentPrice: 40m);
        var assetId = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações, currentPrice: 40m);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, 35m);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 20m, 42m);
        // PositionQuantity = 80, CurrentPrice = 40 → CurrentTotalValue = 3200

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(3200m, response.Data![0].CurrentTotalValue);
    }

    [TestMethod]
    public async Task Dado_SemTransacoes_Quando_GetAll_Entao_CurrentTotalValueZero()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx, currentPrice: 40m);
        await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações, currentPrice: 40m);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(0m, response.Data![0].CurrentTotalValue);
    }

    // --- Filtro de posição negativa ---

    [TestMethod]
    public async Task Dado_VendasMaioresQueCompras_Quando_GetAll_Entao_AtivoExcluido()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var assetId = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 50m, 35m);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 80m, 40m);
        // PositionQuantity = 50 - 80 = -30

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(0, response.TotalCount);
        Assert.IsEmpty(response.Data!);
    }

    [TestMethod]
    public async Task Dado_PosicaoZerada_Quando_GetAll_Entao_AtivoApareceNoResultado()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var assetId = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, 35m);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 100m, 40m);
        // PositionQuantity = 0

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(1, response.TotalCount);
        Assert.AreEqual(0m, response.Data![0].PositionQuantity);
    }

    [TestMethod]
    public async Task Dado_MixDeAtivosComPosicaoPositivaZeradaENegativa_Quando_GetAll_Entao_ExcluiApenasNegativa()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var idPositivo  = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        var idZerado    = await SeedAssetAsync(ctx, "VALE3", EAssetType.Ações);
        var idNegativo  = await SeedAssetAsync(ctx, "MXRF11", EAssetType.FIIs);

        await SeedTransactionAsync(ctx, idPositivo, EOperationType.Compra, 100m, 35m);

        await SeedTransactionAsync(ctx, idZerado, EOperationType.Compra, 50m, 35m);
        await SeedTransactionAsync(ctx, idZerado, EOperationType.Venda, 50m, 40m);

        await SeedTransactionAsync(ctx, idNegativo, EOperationType.Compra, 30m, 10m);
        await SeedTransactionAsync(ctx, idNegativo, EOperationType.Venda, 60m, 12m);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(2, response.TotalCount);
        Assert.IsTrue(response.Data!.Any(a => a.Ticker == "PETR4"));
        Assert.IsTrue(response.Data!.Any(a => a.Ticker == "VALE3"));
        Assert.IsFalse(response.Data!.Any(a => a.Ticker == "MXRF11"));
    }

    // --- Isolamento entre ativos ---

    [TestMethod]
    public async Task Dado_DoisAtivos_Quando_GetAll_Entao_AgregacoesIsoladasPorAtivo()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = AssetHandler(ctx);
        var id1 = await SeedAssetAsync(ctx, "PETR4", EAssetType.Ações);
        var id2 = await SeedAssetAsync(ctx, "MXRF11", EAssetType.FIIs);
        await SeedTransactionAsync(ctx, id1, EOperationType.Compra, 100m, 35m);
        await SeedTransactionAsync(ctx, id2, EOperationType.Compra, 200m, 10m);

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        var petr4 = response.Data!.Single(a => a.Ticker == "PETR4");
        var mxrf11 = response.Data!.Single(a => a.Ticker == "MXRF11");
        Assert.AreEqual(100m, petr4.PositionQuantity);
        Assert.AreEqual(200m, mxrf11.PositionQuantity);
        Assert.AreEqual(3500m, petr4.TotalInvested);
        Assert.AreEqual(2000m, mxrf11.TotalInvested);
    }
}