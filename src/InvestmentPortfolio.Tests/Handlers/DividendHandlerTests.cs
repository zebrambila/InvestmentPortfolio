using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Infra.Data;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class DividendHandlerTests
{
    private static async Task<Guid> SeedAssetAsync(InvestmentContext ctx)
    {
        var asset = new Asset("PETR4", "Petrobras", EAssetType.Acao);
        ctx.Assets.Add(asset);
        await ctx.SaveChangesAsync();
        return asset.Id;
    }

    private static async Task SeedTransactionAsync(InvestmentContext ctx, Guid assetId, EOperationType type, decimal qty, DateTime date)
    {
        var t = new Transaction(assetId, type, qty, 35m, date, ECustodian.BTG);
        ctx.Transactions.Add(t);
        await ctx.SaveChangesAsync();
    }

    [TestMethod]
    public async Task Dado_100ComprasE20Vendas_Quando_CreateDividend_Entao_PositionQuantity80()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var baseDate = new DateTime(2024, 1, 1);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, baseDate);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 20m, baseDate.AddDays(1));
        var handler = new DividendHandler(ctx);

        var response = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId,
            ReceiptDate = baseDate.AddDays(10),
            UnitValue = 0.50m
        });

        Assert.AreEqual(201, response.Code);
        Assert.AreEqual(80m, response.Data!.PositionQuantity);
    }

    [TestMethod]
    public async Task Dado_PositionQuantity80EUnitValue050_Quando_CreateAsync_Entao_TotalFinancial4000()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var baseDate = new DateTime(2024, 1, 1);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, baseDate);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Venda, 20m, baseDate.AddDays(1));
        var handler = new DividendHandler(ctx);

        var response = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId,
            ReceiptDate = baseDate.AddDays(10),
            UnitValue = 0.50m
        });

        Assert.AreEqual(40.00m, response.Data!.TotalFinancial);
    }

    [TestMethod]
    public async Task Dado_SemTransacoes_Quando_CreateDividendAsync_Entao_PositionQuantityZero()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);

        var response = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId,
            ReceiptDate = DateTime.UtcNow,
            UnitValue = 1m
        });

        Assert.AreEqual(0m, response.Data!.PositionQuantity);
        Assert.AreEqual(0m, response.Data!.TotalFinancial);
    }

    [TestMethod]
    public async Task Dado_TransacoesAposReceiptDate_Quando_CreateDividend_Entao_NaoContabilizadas()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var receiptDate = new DateTime(2024, 1, 10);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 100m, receiptDate.AddDays(-5));
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 50m, receiptDate.AddDays(5));
        var handler = new DividendHandler(ctx);

        var response = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId,
            ReceiptDate = receiptDate,
            UnitValue = 1m
        });

        Assert.AreEqual(100m, response.Data!.PositionQuantity);
    }

    [TestMethod]
    public async Task Dado_DividendExistente_Quando_UpdateComNovoUnitValue_Entao_TotalFinancialRecalculado()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var baseDate = new DateTime(2024, 1, 1);
        await SeedTransactionAsync(ctx, assetId, EOperationType.Compra, 80m, baseDate);
        var handler = new DividendHandler(ctx);
        var created = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId, ReceiptDate = baseDate.AddDays(10), UnitValue = 0.50m
        });

        var response = await handler.UpdateAsync(new UpdateDividendRequest
        {
            Id = created.Data!.Id,
            ReceiptDate = baseDate.AddDays(10),
            UnitValue = 1.00m
        });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(80m, response.Data!.TotalFinancial);
    }

    [TestMethod]
    public async Task Dado_DividendExistente_Quando_DeleteAsyncChamado_Entao_ExclusaoLogicaAplicada()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        var created = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId, ReceiptDate = DateTime.UtcNow, UnitValue = 0.50m
        });

        var response = await handler.DeleteAsync(new DeleteDividendRequest { Id = created.Data!.Id });

        Assert.AreEqual(200, response.Code);
        var inDb = await ctx.Dividends.FindAsync(created.Data!.Id);
        Assert.IsTrue(inDb!.IsDeleted);
    }

    [TestMethod]
    public async Task Dado_DividendExistente_Quando_GetByIdAsyncChamado_Entao_RetornaDividend()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        var created = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId, ReceiptDate = DateTime.UtcNow, UnitValue = 0.50m
        });

        var response = await handler.GetByIdAsync(new GetDividendByIdRequest { Id = created.Data!.Id });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(created.Data!.Id, response.Data!.Id);
    }

    [TestMethod]
    public async Task Dado_DividendsAtivos_Quando_GetAllAsyncChamado_Entao_RetornaListaPaginada()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = DateTime.UtcNow, UnitValue = 0.50m });
        var d2 = await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = DateTime.UtcNow.AddMonths(-1), UnitValue = 0.40m });
        await handler.DeleteAsync(new DeleteDividendRequest { Id = d2.Data!.Id });

        var response = await handler.GetAllAsync(new GetAllDividendsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(1, response.TotalCount);
    }
}
