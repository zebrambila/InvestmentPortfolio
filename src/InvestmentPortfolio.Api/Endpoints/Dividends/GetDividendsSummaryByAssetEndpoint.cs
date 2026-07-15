using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Dividends;

public static class GetDividendsSummaryByAssetEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/dividends/GetSummaryByAsset", async ([AsParameters] GetDividendsSummaryByAssetRequest request, IDividendHandler handler) =>
        {
            var result = await handler.GetSummaryByAssetAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("GetDividendsSummaryByAsset")
        .WithTags("Dividends")
        .WithSummary("Resumo de dividendos por ativo")
        .WithDescription("Retorna o somatório de dividendos recebidos agrupados por ativo, ordenados por total decrescente. Filtra opcionalmente por período (DateFrom/DateTo) e limita a quantidade de ativos retornados (Top).")
        .Produces<Response<DividendSummaryByAssetResponse>>(StatusCodes.Status200OK);
    }
}