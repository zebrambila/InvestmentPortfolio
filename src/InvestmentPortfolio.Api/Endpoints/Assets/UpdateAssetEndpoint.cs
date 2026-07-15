using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class UpdateAssetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("v1/assets/UpdateAsset/{id:guid}", async (Guid id, UpdateAssetRequest request, IAssetHandler handler) =>
        {
            request.Id = id;
            var result = await handler.UpdateAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("UpdateAsset")
        .WithTags("Assets")
        .WithSummary("Atualizar ativo")
        .WithDescription("Atualiza Ticker, Nome e Tipo de um ativo existente.")
        .Produces<Response<Asset>>(StatusCodes.Status200OK)
        .Produces<Response<Asset>>(StatusCodes.Status404NotFound);
    }
}
