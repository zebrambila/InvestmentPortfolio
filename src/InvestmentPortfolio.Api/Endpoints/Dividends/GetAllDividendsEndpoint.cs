using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Dividends;

public static class GetAllDividendsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/dividends/GetAllDividends", async ([AsParameters] GetAllDividendsRequest request, IDividendHandler handler) =>
        {
            var result = await handler.GetAllAsync(request);
            return Results.Ok(result);
        })
        .WithName("GetAllDividends")
        .WithTags("Dividends")
        .WithSummary("Listar dividendos")
        .WithDescription("Retorna lista paginada de dividendos ativos, incluindo o ativo (com Ticker). Filtra opcionalmente por AssetId e/ou período (DateFrom/DateTo).")
        .Produces<PagedResponse<List<Dividend>>>(StatusCodes.Status200OK);
    }
}
