using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class CreateAssetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("v1/assets/CreateAsset", async (CreateAssetRequest request, IAssetHandler handler) =>
        {
            var result = await handler.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"v1/assets/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        })
        .WithName("CreateAsset")
        .WithTags("Assets")
        .WithSummary("Cadastrar ativo")
        .WithDescription("Cria um novo ativo (Ação ou FII). O Ticker deve ser único.")
        .Produces<Response<Asset>>(StatusCodes.Status201Created)
        .Produces<Response<Asset>>(StatusCodes.Status400BadRequest);
    }
}
