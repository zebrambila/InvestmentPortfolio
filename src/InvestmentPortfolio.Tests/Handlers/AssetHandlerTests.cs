using InvestmentPortfolio.Api.Handlers;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Tests.Fakes;

namespace InvestmentPortfolio.Tests.Handlers;

[TestClass]
public class AssetHandlerTests
{
    [TestMethod]
    public async Task Dado_RequestValido_Quando_CreateAsyncChamado_Entao_RetornaCode201()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        var request = new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações };

        var response = await handler.CreateAsync(request);

        Assert.AreEqual(201, response.Code);
        Assert.AreEqual("PETR4", response.Data!.Ticker);
    }

    [TestMethod]
    public async Task Dado_TickerDuplicado_Quando_CreateAsyncChamado_Entao_RetornaCode400()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        var request = new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações };
        await handler.CreateAsync(request);

        var response = await handler.CreateAsync(request);

        Assert.AreEqual(400, response.Code);
    }

    [TestMethod]
    public async Task Dado_AssetExistente_Quando_UpdateAsyncChamado_Entao_DadosAlterados()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        var created = await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });

        var response = await handler.UpdateAsync(new UpdateAssetRequest
        {
            Id = created.Data!.Id,
            Ticker = "PETR3",
            Name = "Petrobras PN",
            Type = EAssetType.Ações
        });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual("PETR3", response.Data!.Ticker);
    }

    [TestMethod]
    public async Task Dado_AssetInexistente_Quando_UpdateAsyncChamado_Entao_RetornaCode404()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());

        var response = await handler.UpdateAsync(new UpdateAssetRequest
        {
            Id = Guid.NewGuid(),
            Ticker = "PETR4",
            Name = "Petrobras",
            Type = EAssetType.Ações
        });

        Assert.AreEqual(404, response.Code);
    }

    [TestMethod]
    public async Task Dado_AssetExistente_Quando_UpdatePriceAsyncChamado_Entao_PrecoAtualizado()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        var created = await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });

        var response = await handler.UpdatePriceAsync(new UpdateAssetPriceRequest { Id = created.Data!.Id, CurrentPrice = 38.50m });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(38.50m, response.Data!.CurrentPrice);
    }

    [TestMethod]
    public async Task Dado_AssetExistente_Quando_DeleteAsyncChamado_Entao_ExclusaoLogicaAplicada()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        var created = await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });

        var response = await handler.DeleteAsync(new DeleteAssetRequest { Id = created.Data!.Id });

        Assert.AreEqual(200, response.Code);
        var inDb = await ctx.Assets.FindAsync(created.Data!.Id);
        Assert.IsTrue(inDb!.IsDeleted);
    }

    [TestMethod]
    public async Task Dado_AssetInexistente_Quando_DeleteAsyncChamado_Entao_RetornaCode404()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());

        var response = await handler.DeleteAsync(new DeleteAssetRequest { Id = Guid.NewGuid() });

        Assert.AreEqual(404, response.Code);
    }

    [TestMethod]
    public async Task Dado_AssetExistente_Quando_GetByIdAsyncChamado_Entao_RetornaAsset()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        var created = await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });

        var response = await handler.GetByIdAsync(new GetAssetByIdRequest { Id = created.Data!.Id });

        Assert.AreEqual(200, response.Code);
        Assert.AreEqual(created.Data!.Id, response.Data!.Id);
    }

    [TestMethod]
    public async Task Dado_AssetDeletado_Quando_GetByIdAsyncChamado_Entao_RetornaCode404()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        var created = await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });
        await handler.DeleteAsync(new DeleteAssetRequest { Id = created.Data!.Id });

        var response = await handler.GetByIdAsync(new GetAssetByIdRequest { Id = created.Data!.Id });

        Assert.AreEqual(404, response.Code);
    }

    [TestMethod]
    public async Task Dado_AssetsAtivos_Quando_GetAllAsyncChamado_Entao_RetornaListaPaginada()
    {
        using var ctx = InvestmentContextFake.Create();
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(), new AssetPriceHistoryServiceFake());
        await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });
        await handler.CreateAsync(new CreateAssetRequest { Ticker = "MXRF11", Name = "Maxi Renda", Type = EAssetType.FIIs });
        var created3 = await handler.CreateAsync(new CreateAssetRequest { Ticker = "VALE3", Name = "Vale", Type = EAssetType.Ações });
        await handler.DeleteAsync(new DeleteAssetRequest { Id = created3.Data!.Id });

        var response = await handler.GetAllAsync(new GetAllAssetsRequest { PageNumber = 1, PageSize = 25 });

        Assert.AreEqual(200, response.Code);
        Assert.HasCount(2, response.Data!);
        Assert.AreEqual(2, response.TotalCount);
    }

    [TestMethod]
    public async Task Dado_PosicaoEmDoisCustodiantes_Quando_GetDividendEventsAsyncChamado_Entao_CriaDividendoUnicoNaItauComPosicaoTotal()
    {
        using var ctx = InvestmentContextFake.Create();
        var eventDate = new DateTime(2026, 6, 10);
        var dividendEvents = new Dictionary<string, List<DividendEvent>>
        {
            ["PETR4"] = [new DividendEvent(eventDate, 2.00m)]
        };
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(dividendEvents: dividendEvents), new AssetPriceHistoryServiceFake());
        var created = await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });
        var assetId = created.Data!.Id;

        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Compra, 100m, 30m, eventDate.AddMonths(-1), ECustodian.BTG));
        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Compra, 50m, 30m, eventDate.AddMonths(-1), ECustodian.Itau));
        await ctx.SaveChangesAsync();

        var response = await handler.GetDividendEventsAsync(new GetAssetDividendEventsRequest { Ticker = "PETR4", From = eventDate, To = eventDate });

        Assert.AreEqual(200, response.Code);
        var dividends = ctx.Dividends.Where(d => d.AssetId == assetId).ToList();
        Assert.HasCount(1, dividends);
        Assert.AreEqual(ECustodian.Itau, dividends.Single().Custodian);
        Assert.AreEqual(300.00m, dividends.Single().TotalFinancial);
    }

    [TestMethod]
    public async Task Dado_DividendoJaInserido_Quando_GetDividendEventsAsyncChamadoNovamente_Entao_NaoDuplica()
    {
        using var ctx = InvestmentContextFake.Create();
        var eventDate = new DateTime(2026, 6, 10);
        var dividendEvents = new Dictionary<string, List<DividendEvent>>
        {
            ["PETR4"] = [new DividendEvent(eventDate, 2.00m)]
        };
        var handler = new AssetHandler(ctx, new MarketDataServiceFake(dividendEvents: dividendEvents), new AssetPriceHistoryServiceFake());
        var created = await handler.CreateAsync(new CreateAssetRequest { Ticker = "PETR4", Name = "Petrobras", Type = EAssetType.Ações });
        var assetId = created.Data!.Id;

        ctx.Transactions.Add(new Transaction(assetId, EOperationType.Compra, 100m, 30m, eventDate.AddMonths(-1), ECustodian.BTG));
        await ctx.SaveChangesAsync();

        var request = new GetAssetDividendEventsRequest { Ticker = "PETR4", From = eventDate, To = eventDate };
        await handler.GetDividendEventsAsync(request);
        await handler.GetDividendEventsAsync(request);

        var dividends = ctx.Dividends.Where(d => d.AssetId == assetId).ToList();
        Assert.HasCount(1, dividends);
    }
}
