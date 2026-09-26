using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class UpdateAssetPriceEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPatch("v1/assets/{id:guid}/price", async (Guid id, UpdateAssetPriceRequest request, IAssetHandler handler) =>
        {
            request.Id = id;
            var result = await handler.UpdatePriceAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("UpdateAssetPrice")
        .WithTags("Assets")
        .WithSummary("Atualizar preço atual")
        .WithDescription("Atualiza o preço de mercado atual do ativo (inserção manual).")
        .Produces<Response<Asset>>(StatusCodes.Status200OK)
        .Produces<Response<Asset>>(StatusCodes.Status404NotFound);
    }
}
