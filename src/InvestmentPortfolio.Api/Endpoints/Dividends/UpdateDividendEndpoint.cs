using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Dividends;

public static class UpdateDividendEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("v1/dividends/UpdateDividend/{id:guid}", async (Guid id, UpdateDividendRequest request, IDividendHandler handler) =>
        {
            request.Id = id;
            var result = await handler.UpdateAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("UpdateDividend")
        .WithTags("Dividends")
        .WithSummary("Atualizar dividendo")
        .WithDescription("Atualiza ReceiptDate e TotalFinancial.")
        .Produces<Response<Dividend>>(StatusCodes.Status200OK)
        .Produces<Response<Dividend>>(StatusCodes.Status404NotFound);
    }
}
