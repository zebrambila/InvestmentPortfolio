using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Transactions;

public static class DeleteTransactionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("v1/transactions/{id:guid}", async (Guid id, ITransactionHandler handler) =>
        {
            var result = await handler.DeleteAsync(new DeleteTransactionRequest { Id = id });
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("DeleteTransaction")
        .WithTags("Transactions")
        .WithSummary("Remover operação (soft delete)")
        .WithDescription("Marca a operação como excluída. O registro permanece no banco com IsDeleted = true.")
        .Produces<Response<Transaction>>(StatusCodes.Status200OK)
        .Produces<Response<Transaction>>(StatusCodes.Status404NotFound);
    }
}
