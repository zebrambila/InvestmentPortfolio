using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class DeleteAssetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("v1/assets/DeleteAsset/{id:guid}", async (Guid id, IAssetHandler handler) =>
        {
            var result = await handler.DeleteAsync(new DeleteAssetRequest { Id = id });
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("DeleteAsset")
        .WithTags("Assets")
        .WithSummary("Remover ativo (soft delete)")
        .WithDescription("Marca o ativo como excluído. O registro permanece no banco com IsDeleted = true.")
        .Produces<Response<Asset>>(StatusCodes.Status200OK)
        .Produces<Response<Asset>>(StatusCodes.Status404NotFound);
    }
}
