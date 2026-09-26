using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class GetAssetByIdEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/assets/GetAssetById/{id:guid}", async (Guid id, IAssetHandler handler) =>
        {
            var result = await handler.GetByIdAsync(new GetAssetByIdRequest { Id = id });
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetAssetById")
        .WithTags("Assets")
        .WithSummary("Buscar ativo por ID")
        .WithDescription("Retorna um ativo ativo (não excluído) pelo seu identificador único.")
        .Produces<Response<Asset>>(StatusCodes.Status200OK)
        .Produces<Response<Asset>>(StatusCodes.Status404NotFound);
    }
}
