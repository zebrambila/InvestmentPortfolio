using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Infra.Data;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class TransactionHandlerTests
{
    private static async Task<Guid> SeedAssetAsync(InvestmentContext ctx)
    {
        var asset = new Asset("PETR4", "Petrobras", EAssetType.Ações);
        ctx.Assets.Add(asset);
        await ctx.SaveChangesAsync();
        return asset.Id;
    }

    [TestMethod]
    public async Task Dado_RequestValido_Quando_CreateAsyncChamado_Entao_TotalValueCalculado()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);

        var response = await handler.CreateAsync(new CreateTransactionRequest
        {
            AssetId = assetId,
            Type = EOperationType.Compra,
            Quantity = 100m,
            UnitPrice = 35m,
            DateBuySell = DateTime.UtcNow,
            Custodian = ECustodian.BTG
        });

        Assert.AreEqual(201, response.Code);
        Assert.AreEqual(3500m, response.Data!.TotalValue);
    }

    [TestMethod]
    public async Task Dado_AssetInexistente_Quando_CreateAsyncChamado_Entao_RetornaCode404()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new TransactionHandler(ctx);

        var response = await handler.CreateAsync(new CreateTransactionRequest
        {
            AssetId = Guid.NewGuid(),
            Type = EOperationType.Compra,
            Quantity = 100m,
            UnitPrice = 35m,
            DateBuySell = DateTime.UtcNow,
            Custodian = ECustodian.BTG
        });

        Assert.AreEqual(404, response.Code);
    }

    [TestMethod]
    public async Task Dado_TransactionExistente_Quando_UpdateAsyncChamado_Entao_TotalValueRecalculado()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        var created = await handler.CreateAsync(new CreateTransactionRequest
        {
            AssetId = assetId, Type = EOperationType.Compra, Quantity = 100m, UnitPrice = 35m,
            DateBuySell = DateTime.UtcNow, Custodian = ECustodian.BTG
        });

        var response = await handler.UpdateAsync(new UpdateTransactionRequest
        {
            Id = created.Data!.Id,
            Type = EOperationType.Compra,
            Quantity = 50m,
            UnitPrice = 40m,
            DateBuySell = DateTime.UtcNow,
            Custodian = ECustodian.Itau
        });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(2000m, response.Data!.TotalValue);
    }

    [TestMethod]
    public async Task Dado_TransactionInexistente_Quando_UpdateAsyncChamado_Entao_RetornaCode404()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new TransactionHandler(ctx);

        var response = await handler.UpdateAsync(new UpdateTransactionRequest
        {
            Id = Guid.NewGuid(), Type = EOperationType.Compra, Quantity = 10m, UnitPrice = 10m,
            DateBuySell = DateTime.UtcNow, Custodian = ECustodian.BTG
        });

        Assert.AreEqual(404, response.Code);
    }

    [TestMethod]
    public async Task Dado_TransactionExistente_Quando_DeleteAsyncChamado_Entao_ExclusaoLogicaAplicada()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        var created = await handler.CreateAsync(new CreateTransactionRequest
        {
            AssetId = assetId, Type = EOperationType.Compra, Quantity = 100m, UnitPrice = 35m,
            DateBuySell = DateTime.UtcNow, Custodian = ECustodian.BTG
        });

        var response = await handler.DeleteAsync(new DeleteTransactionRequest { Id = created.Data!.Id });

        Assert.AreEqual(200, response.Code);
        var inDb = await ctx.Transactions.FindAsync(created.Data!.Id);
        Assert.IsTrue(inDb!.IsDeleted);
    }

    [TestMethod]
    public async Task Dado_TransactionExistente_Quando_GetByIdAsyncChamado_Entao_RetornaTransaction()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        var created = await handler.CreateAsync(new CreateTransactionRequest
        {
            AssetId = assetId, Type = EOperationType.Compra, Quantity = 100m, UnitPrice = 35m,
            DateBuySell = DateTime.UtcNow, Custodian = ECustodian.BTG
        });

        var response = await handler.GetByIdAsync(new GetTransactionByIdRequest { Id = created.Data!.Id });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(created.Data!.Id, response.Data!.Id);
    }

    [TestMethod]
    public async Task Dado_TransactionsAtivas_Quando_GetAllAsyncChamado_Entao_RetornaListaPaginada()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        await handler.CreateAsync(new CreateTransactionRequest
        {
            AssetId = assetId, Type = EOperationType.Compra, Quantity = 100m, UnitPrice = 35m,
            DateBuySell = DateTime.UtcNow, Custodian = ECustodian.BTG
        });
        var t2 = await handler.CreateAsync(new CreateTransactionRequest
        {
            AssetId = assetId, Type = EOperationType.Venda, Quantity = 20m, UnitPrice = 40m,
            DateBuySell = DateTime.UtcNow, Custodian = ECustodian.Itau
        });
        await handler.DeleteAsync(new DeleteTransactionRequest { Id = t2.Data!.Id });

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(1, response.TotalCount);
    }
}
