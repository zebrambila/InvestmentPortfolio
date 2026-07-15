using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class TransactionGetAllTests
{
    private static async Task<Guid> SeedAssetAsync(InvestmentPortfolio.Infra.Data.InvestmentContext ctx)
    {
        var asset = new Asset("PETR4", "Petrobras", EAssetType.Ações);
        ctx.Assets.Add(asset);
        await ctx.SaveChangesAsync();
        return asset.Id;
    }

    private static CreateTransactionRequest Compra(Guid assetId, decimal qty, decimal price, DateTime date) =>
        new() { AssetId = assetId, Type = EOperationType.Compra, Quantity = qty, UnitPrice = price, DateBuySell = date, Custodian = ECustodian.BTG };

    private static CreateTransactionRequest Venda(Guid assetId, decimal qty, decimal price, DateTime date) =>
        new() { AssetId = assetId, Type = EOperationType.Venda, Quantity = qty, UnitPrice = price, DateBuySell = date, Custodian = ECustodian.BTG };

    // --- Paginação e totais base ---

    [TestMethod]
    public async Task Dado_TransacoesAtivas_Quando_GetAll_Entao_ExcluiDeletadas()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        var date = new DateTime(2024, 6, 1);

        await handler.CreateAsync(Compra(assetId, 100m, 35m, date));
        var t2 = await handler.CreateAsync(Venda(assetId, 20m, 40m, date));
        await handler.DeleteAsync(new DeleteTransactionRequest { Id = t2.Data!.Id });

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(1, response.TotalCount);
        Assert.AreEqual(1, response.Summary.TotalCount);
    }

    [TestMethod]
    public async Task Dado_TransacoesAtivas_Quando_GetAll_Entao_SummaryCalculaVolumes()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        var date = new DateTime(2024, 6, 1);

        await handler.CreateAsync(Compra(assetId, 100m, 35m, date));  // TotalValue = 3500
        await handler.CreateAsync(Compra(assetId, 50m, 40m, date));   // TotalValue = 2000
        await handler.CreateAsync(Venda(assetId, 30m, 42m, date));    // TotalValue = 1260

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(3, response.Summary.TotalCount);
        Assert.AreEqual(5500m, response.Summary.VolumeCompra);
        Assert.AreEqual(1260m, response.Summary.VolumeVenda);
    }

    [TestMethod]
    public async Task Dado_SemTransacoes_Quando_GetAll_Entao_SummaryZerado()
    {
        using var ctx = InvestmentContextFake.Create();
        await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(0, response.Summary.TotalCount);
        Assert.AreEqual(0m, response.Summary.VolumeCompra);
        Assert.AreEqual(0m, response.Summary.VolumeVenda);
    }

    // --- Filtro por Tipo ---

    [TestMethod]
    public async Task Dado_FiltroCompra_Quando_GetAll_Entao_RetornaApenasCompras()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        var date = new DateTime(2024, 6, 1);

        await handler.CreateAsync(Compra(assetId, 100m, 35m, date));
        await handler.CreateAsync(Compra(assetId, 50m, 40m, date));
        await handler.CreateAsync(Venda(assetId, 30m, 42m, date));

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest
        {
            PageNumber = 1, PageSize = 25,
            Type = EOperationType.Compra
        });

        Assert.AreEqual(2, response.TotalCount);
        Assert.IsTrue(response.Data!.All(t => t.Side == EOperationType.Compra));
    }

    [TestMethod]
    public async Task Dado_FiltroVenda_Quando_GetAll_Entao_SummaryRefleteApenasVendas()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);
        var date = new DateTime(2024, 6, 1);

        await handler.CreateAsync(Compra(assetId, 100m, 35m, date));
        await handler.CreateAsync(Venda(assetId, 30m, 42m, date));   // TotalValue = 1260

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest
        {
            PageNumber = 1, PageSize = 25,
            Type = EOperationType.Venda
        });

        Assert.AreEqual(1, response.Summary.TotalCount);
        Assert.AreEqual(0m, response.Summary.VolumeCompra);
        Assert.AreEqual(1260m, response.Summary.VolumeVenda);
    }

    // --- Filtro por Período ---

    [TestMethod]
    public async Task Dado_FiltroPeriodo_Quando_GetAll_Entao_RetornaApenasTransacoesDoPeriodo()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);

        var dentro  = new DateTime(2024, 6, 15);
        var fora1   = new DateTime(2024, 5, 31);
        var fora2   = new DateTime(2024, 7, 1);

        await handler.CreateAsync(Compra(assetId, 100m, 35m, dentro));
        await handler.CreateAsync(Compra(assetId, 50m, 40m, fora1));
        await handler.CreateAsync(Venda(assetId, 20m, 42m, fora2));

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest
        {
            PageNumber = 1, PageSize = 25,
            DateFrom = new DateTime(2024, 6, 1),
            DateTo   = new DateTime(2024, 6, 30)
        });

        Assert.AreEqual(1, response.TotalCount);
        Assert.AreEqual(dentro, response.Data![0].DateBuySell);
    }

    [TestMethod]
    public async Task Dado_FiltroPeriodo_Quando_GetAll_Entao_IncluiDatasDosExtremosDoIntervalo()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);

        var inicio = new DateTime(2024, 6, 1);
        var fim    = new DateTime(2024, 6, 30);

        await handler.CreateAsync(Compra(assetId, 10m, 10m, inicio));
        await handler.CreateAsync(Compra(assetId, 10m, 10m, fim));

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest
        {
            PageNumber = 1, PageSize = 25,
            DateFrom = inicio, DateTo = fim
        });

        Assert.AreEqual(2, response.TotalCount);
    }

    // --- Filtros combinados ---

    [TestMethod]
    public async Task Dado_FiltroPeriodoETipo_Quando_GetAll_Entao_AplicaAmbosOsFiltros()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new TransactionHandler(ctx);

        var dentro = new DateTime(2024, 6, 15);
        var fora   = new DateTime(2024, 5, 1);

        await handler.CreateAsync(Compra(assetId, 100m, 35m, dentro));   // passa
        await handler.CreateAsync(Venda(assetId, 30m, 42m, dentro));     // período ok, tipo errado
        await handler.CreateAsync(Compra(assetId, 50m, 40m, fora));      // tipo ok, período errado

        var response = await handler.GetAllAsync(new GetAllTransactionsRequest
        {
            PageNumber = 1, PageSize = 25,
            Type     = EOperationType.Compra,
            DateFrom = new DateTime(2024, 6, 1),
            DateTo   = new DateTime(2024, 6, 30)
        });

        Assert.AreEqual(1, response.TotalCount);
        Assert.AreEqual(3500m, response.Summary.VolumeCompra);
        Assert.AreEqual(0m, response.Summary.VolumeVenda);
    }
}