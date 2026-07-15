using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Dividends;

public static class GetDividendsSummaryByMonthEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/dividends/GetSummaryByMonth", async ([AsParameters] GetDividendsSummaryByMonthRequest request, IDividendHandler handler) =>
        {
            var result = await handler.GetSummaryByMonthAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("GetDividendsSummaryByMonth")
        .WithTags("Dividends")
        .WithSummary("Resumo de dividendos por mês")
        .WithDescription("Retorna o somatório de dividendos recebidos agrupados por mês e a média mensal do período. Filtra opcionalmente por AssetId e/ou período (DateFrom/DateTo).")
        .Produces<Response<DividendSummaryByMonthResponse>>(StatusCodes.Status200OK);
    }
}
