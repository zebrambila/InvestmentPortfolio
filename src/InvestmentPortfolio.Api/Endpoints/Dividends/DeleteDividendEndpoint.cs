using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Dividends;

public static class DeleteDividendEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("v1/dividends/DeleteDividend/{id:guid}", async (Guid id, IDividendHandler handler) =>
        {
            var result = await handler.DeleteAsync(new DeleteDividendRequest { Id = id });
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("DeleteDividend")
        .WithTags("Dividends")
        .WithSummary("Remover dividendo (soft delete)")
        .WithDescription("Marca o dividendo como excluído. O registro permanece no banco com IsDeleted = true.")
        .Produces<Response<Dividend>>(StatusCodes.Status200OK)
        .Produces<Response<Dividend>>(StatusCodes.Status404NotFound);
    }
}
