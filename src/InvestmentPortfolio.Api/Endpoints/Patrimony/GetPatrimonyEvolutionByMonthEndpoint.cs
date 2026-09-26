using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Patrimony;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;
using InvestmentPortfolio.Core.Responses.Patrimony;

namespace InvestmentPortfolio.Api.Endpoints.Patrimony;

public static class GetPatrimonyEvolutionByMonthEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/patrimony/GetEvolutionByMonth", async ([AsParameters] GetPatrimonyEvolutionRequest request, IPatrimonyHandler handler) =>
        {
            var result = await handler.GetEvolutionByMonthAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("GetPatrimonyEvolutionByMonth")
        .WithTags("Patrimony")
        .WithSummary("Evolução patrimonial mensal")
        .WithDescription("Retorna, para cada mês do período, o capital investido e o valor de mercado da carteira, a partir das transações e do histórico de preços de fechamento. Se DateFrom/DateTo forem omitidos, considera os últimos 12 meses.")
        .Produces<Response<PatrimonyEvolutionResponse>>(StatusCodes.Status200OK);
    }
}
