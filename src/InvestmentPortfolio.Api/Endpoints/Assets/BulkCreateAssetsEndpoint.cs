using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Assets;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class BulkCreateAssetsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("v1/assets/BulkCreateAssets", async (BulkCreateAssetsRequest request, IAssetHandler handler) =>
        {
            var result = await handler.BulkCreateAsync(request);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("BulkCreateAssets")
        .WithTags("Assets")
        .WithSummary("Cadastrar ativos em lote")
        .WithDescription("Cria múltiplos ativos a partir de uma lista estruturada (ex.: importação de arquivo CSV feita no navegador). Cada linha é processada de forma independente: falhas individuais não impedem a criação das demais.")
        .Produces<Response<BulkCreateAssetsResponse>>(StatusCodes.Status200OK)
        .Produces<Response<BulkCreateAssetsResponse>>(StatusCodes.Status400BadRequest);
    }
}
