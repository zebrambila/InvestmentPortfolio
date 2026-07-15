using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class GetAllAssetsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/assets/GetAllAssets", async ([AsParameters] GetAllAssetsRequest request, IAssetHandler handler) =>
        {
            var result = await handler.GetAllAsync(request);
            return Results.Ok(result);
        })
        .WithName("GetAllAssets")
        .WithTags("Assets")
        .WithSummary("Listar ativos")
        .WithDescription("Retorna lista paginada de ativos com posição em carteira calculada via transações. Filtra opcionalmente por Type (Ações/FIIs).")
        .Produces<AssetPagedResponse>(StatusCodes.Status200OK);
    }
}
