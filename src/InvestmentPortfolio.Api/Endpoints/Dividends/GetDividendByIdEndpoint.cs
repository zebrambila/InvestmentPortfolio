using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Dividends;

public static class GetDividendByIdEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/dividends/GetDividendById/{id:guid}", async (Guid id, IDividendHandler handler) =>
        {
            var result = await handler.GetByIdAsync(new GetDividendByIdRequest { Id = id });
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetDividendById")
        .WithTags("Dividends")
        .WithSummary("Buscar dividendo por ID")
        .WithDescription("Retorna um dividendo ativo (não excluído) pelo seu identificador único.")
        .Produces<Response<Dividend>>(StatusCodes.Status200OK)
        .Produces<Response<Dividend>>(StatusCodes.Status404NotFound);
    }
}
