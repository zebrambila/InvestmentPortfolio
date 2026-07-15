using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Infra.Data;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class DividendHandlerTests
{
    private static async Task<Guid> SeedAssetAsync(InvestmentContext ctx)
    {
        var asset = new Asset("PETR4", "Petrobras", EAssetType.Ações);
        ctx.Assets.Add(asset);
        await ctx.SaveChangesAsync();
        return asset.Id;
    }

    [TestMethod]
    public async Task Dado_TotalFinancialInformado_Quando_CreateAsync_Entao_DividendCriadoComValor()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);

        var response = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId,
            ReceiptDate = DateTime.UtcNow,
            TotalFinancial = 40.00m,
            Custodian = ECustodian.BTG
        });

        Assert.AreEqual(201, response.Code);
        Assert.AreEqual(40.00m, response.Data!.TotalFinancial);
        Assert.AreEqual(ECustodian.BTG, response.Data!.Custodian);
    }

    [TestMethod]
    public async Task Dado_DividendExistente_Quando_UpdateComNovoTotalFinancial_Entao_ValorAtualizado()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        var created = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId,
            ReceiptDate = DateTime.UtcNow,
            TotalFinancial = 40.00m,
            Custodian = ECustodian.BTG
        });

        var response = await handler.UpdateAsync(new UpdateDividendRequest
        {
            Id = created.Data!.Id,
            ReceiptDate = DateTime.UtcNow,
            TotalFinancial = 80.00m,
            Custodian = ECustodian.Itau
        });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(80.00m, response.Data!.TotalFinancial);
        Assert.AreEqual(ECustodian.Itau, response.Data!.Custodian);
    }

    [TestMethod]
    public async Task Dado_DividendExistente_Quando_DeleteAsyncChamado_Entao_ExclusaoLogicaAplicada()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        var created = await handler.CreateAsync(new CreateDividendRequest
        {
            AssetId = assetId, ReceiptDate = DateTime.UtcNow, TotalFinancial = 40.00m, Custodian = ECustodian.BTG
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
            AssetId = assetId, ReceiptDate = DateTime.UtcNow, TotalFinancial = 40.00m, Custodian = ECustodian.BTG
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
        await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = DateTime.UtcNow, TotalFinancial = 40.00m, Custodian = ECustodian.BTG });
        var d2 = await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = DateTime.UtcNow.AddMonths(-1), TotalFinancial = 32.00m, Custodian = ECustodian.Itau });
        await handler.DeleteAsync(new DeleteDividendRequest { Id = d2.Data!.Id });

        var response = await handler.GetAllAsync(new GetAllDividendsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(1, response.TotalCount);
    }

    [TestMethod]
    public async Task Dado_DividendsEmDatasDiferentes_Quando_GetAllComPeriodo_Entao_RetornaApenasDoPeriodo()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = new DateTime(2024, 1, 10), TotalFinancial = 40.00m, Custodian = ECustodian.BTG });
        await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = new DateTime(2024, 6, 10), TotalFinancial = 32.00m, Custodian = ECustodian.Itau });

        var response = await handler.GetAllAsync(new GetAllDividendsRequest
        {
            PageNumber = 1,
            PageSize = 25,
            DateFrom = new DateTime(2024, 1, 1),
            DateTo = new DateTime(2024, 2, 1)
        });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(1, response.TotalCount);
        Assert.AreEqual(new DateTime(2024, 1, 10), response.Data![0].ReceiptDate);
    }

    [TestMethod]
    public async Task Dado_DividendsEmMesesDiferentes_Quando_GetSummaryByMonth_Entao_AgrupaERetornaMedia()
    {
        using var ctx = InvestmentContextFake.Create();
        var assetId = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = new DateTime(2024, 1, 5), TotalFinancial = 40.00m, Custodian = ECustodian.BTG });
        await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = new DateTime(2024, 1, 20), TotalFinancial = 10.00m, Custodian = ECustodian.BTG });
        await handler.CreateAsync(new CreateDividendRequest { AssetId = assetId, ReceiptDate = new DateTime(2024, 2, 10), TotalFinancial = 30.00m, Custodian = ECustodian.Itau });

        var response = await handler.GetSummaryByMonthAsync(new GetDividendsSummaryByMonthRequest());

        Assert.AreEqual(200, response.Code);
        Assert.HasCount(2, response.Data!.Months);
        Assert.AreEqual(2024, response.Data.Months[0].Year);
        Assert.AreEqual(1, response.Data.Months[0].Month);
        Assert.AreEqual(50.00m, response.Data.Months[0].Total);
        Assert.AreEqual(2, response.Data.Months[1].Month);
        Assert.AreEqual(30.00m, response.Data.Months[1].Total);
        Assert.AreEqual(40.00m, response.Data.AverageMonthly);
    }

    [TestMethod]
    public async Task Dado_SemDividendos_Quando_GetSummaryByMonth_Entao_MediaZero()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new DividendHandler(ctx);

        var response = await handler.GetSummaryByMonthAsync(new GetDividendsSummaryByMonthRequest());

        Assert.AreEqual(200, response.Code);
        Assert.IsEmpty(response.Data!.Months);
        Assert.AreEqual(0m, response.Data.AverageMonthly);
    }

    [TestMethod]
    public async Task Dado_DividendsDeAtivosDiferentes_Quando_GetSummaryByAsset_Entao_AgrupaEOrdenaPorTotalDecrescente()
    {
        using var ctx = InvestmentContextFake.Create();
        var petr4Id = await SeedAssetAsync(ctx);
        var taee11 = new Asset("TAEE11", "Taesa", EAssetType.Ações);
        ctx.Assets.Add(taee11);
        await ctx.SaveChangesAsync();
        var handler = new DividendHandler(ctx);

        await handler.CreateAsync(new CreateDividendRequest { AssetId = petr4Id, ReceiptDate = new DateTime(2024, 1, 5), TotalFinancial = 40.00m, Custodian = ECustodian.BTG });
        await handler.CreateAsync(new CreateDividendRequest { AssetId = petr4Id, ReceiptDate = new DateTime(2024, 2, 10), TotalFinancial = 30.00m, Custodian = ECustodian.Itau });
        await handler.CreateAsync(new CreateDividendRequest { AssetId = taee11.Id, ReceiptDate = new DateTime(2024, 1, 20), TotalFinancial = 100.00m, Custodian = ECustodian.BTG });

        var response = await handler.GetSummaryByAssetAsync(new GetDividendsSummaryByAssetRequest());

        Assert.AreEqual(200, response.Code);
        Assert.HasCount(2, response.Data!.Assets);
        Assert.AreEqual("TAEE11", response.Data.Assets[0].Ticker);
        Assert.AreEqual(100.00m, response.Data.Assets[0].Total);
        Assert.AreEqual("PETR4", response.Data.Assets[1].Ticker);
        Assert.AreEqual(70.00m, response.Data.Assets[1].Total);
        Assert.AreEqual(170.00m, response.Data.TotalReceived);
    }

    [TestMethod]
    public async Task Dado_TopInformado_Quando_GetSummaryByAsset_Entao_TruncaListaMasMantemTotalReceivedCompleto()
    {
        using var ctx = InvestmentContextFake.Create();
        var petr4Id = await SeedAssetAsync(ctx);
        var taee11 = new Asset("TAEE11", "Taesa", EAssetType.Ações);
        ctx.Assets.Add(taee11);
        await ctx.SaveChangesAsync();
        var handler = new DividendHandler(ctx);

        await handler.CreateAsync(new CreateDividendRequest { AssetId = petr4Id, ReceiptDate = new DateTime(2024, 1, 5), TotalFinancial = 40.00m, Custodian = ECustodian.BTG });
        await handler.CreateAsync(new CreateDividendRequest { AssetId = taee11.Id, ReceiptDate = new DateTime(2024, 1, 20), TotalFinancial = 100.00m, Custodian = ECustodian.BTG });

        var response = await handler.GetSummaryByAssetAsync(new GetDividendsSummaryByAssetRequest { Top = 1 });

        Assert.AreEqual(200, response.Code);
        Assert.HasCount(1, response.Data!.Assets);
        Assert.AreEqual("TAEE11", response.Data.Assets[0].Ticker);
        Assert.AreEqual(140.00m, response.Data.TotalReceived);
    }

    [TestMethod]
    public async Task Dado_AtivoSemDividendoNoPeriodo_Quando_GetSummaryByAsset_Entao_NaoAparece()
    {
        using var ctx = InvestmentContextFake.Create();
        var petr4Id = await SeedAssetAsync(ctx);
        var handler = new DividendHandler(ctx);
        await handler.CreateAsync(new CreateDividendRequest { AssetId = petr4Id, ReceiptDate = new DateTime(2024, 1, 5), TotalFinancial = 40.00m, Custodian = ECustodian.BTG });

        var response = await handler.GetSummaryByAssetAsync(new GetDividendsSummaryByAssetRequest
        {
            DateFrom = new DateTime(2024, 6, 1),
            DateTo = new DateTime(2024, 12, 31)
        });

        Assert.AreEqual(200, response.Code);
        Assert.IsEmpty(response.Data!.Assets);
        Assert.AreEqual(0m, response.Data.TotalReceived);
    }
}
