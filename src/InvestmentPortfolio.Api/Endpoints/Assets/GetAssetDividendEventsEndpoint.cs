using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Services;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class GetAssetDividendEventsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/assets/{ticker}/dividends", async (string ticker, int? months, DateTime? from, DateTime? to, IAssetHandler handler, CancellationToken ct) =>
        {
            var result = await handler.GetDividendEventsAsync(new GetAssetDividendEventsRequest { Ticker = ticker, Months = months, From = from, To = to }, ct);
            return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(result.Code);
        })
        .WithName("GetAssetDividendEvents")
        .WithTags("Assets")
        .WithSummary("Buscar eventos de dividendos de um ativo")
        .WithDescription("Consulta na API externa (Yahoo Finance) os eventos de pagamento de dividendos do ativo. Aceita um intervalo exato via 'from'/'to'; se omitidos, usa 'months' (padrão: últimos 12 meses) a partir de hoje.")
        .Produces<Response<List<DividendEvent>>>(StatusCodes.Status200OK)
        .Produces<Response<List<DividendEvent>>>(StatusCodes.Status400BadRequest)
        .Produces<Response<List<DividendEvent>>>(StatusCodes.Status404NotFound);
    }
}