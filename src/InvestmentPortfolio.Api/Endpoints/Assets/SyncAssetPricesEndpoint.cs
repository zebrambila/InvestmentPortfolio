using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class SyncAssetPricesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("v1/assets/sync-prices", async (IAssetHandler handler, CancellationToken ct) =>
        {
            var result = await handler.SyncPricesAsync(ct);
            return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(result.Code);
        })
        .WithName("SyncAssetPrices")
        .WithTags("Assets")
        .WithSummary("Sincronizar preços via API externa")
        .WithDescription("Busca e atualiza os preços de mercado de todos os ativos cadastrados via API externa (Yahoo Finance).")
        .Produces<Response<int>>(StatusCodes.Status200OK);
    }
}
