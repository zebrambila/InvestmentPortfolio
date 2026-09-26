using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Assets;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class BackfillPricesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("v1/assets/BackfillPrices", async (BackfillPricesRequest? request, IAssetHandler handler, CancellationToken ct) =>
        {
            var result = await handler.BackfillPricesAsync(request ?? new BackfillPricesRequest(), ct);
            return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(result.Code);
        })
        .WithName("BackfillAssetPrices")
        .WithTags("Assets")
        .WithSummary("Preencher histórico de preços retroativo")
        .WithDescription("Busca e grava, para cada ativo cadastrado, o preço de fechamento dos últimos N meses (padrão 12) via API externa de cotações, complementando o histórico usado na evolução patrimonial.")
        .Produces<Response<BackfillPricesResponse>>(StatusCodes.Status200OK)
        .Produces<Response<BackfillPricesResponse>>(StatusCodes.Status500InternalServerError);
    }
}
