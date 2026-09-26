using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Transactions;

public static class UpdateTransactionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPut("v1/transactions/UpdateTransaction/{id:guid}", async (Guid id, UpdateTransactionRequest request, ITransactionHandler handler) =>
        {
            request.Id = id;
            var result = await handler.UpdateAsync(request);
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("UpdateTransaction")
        .WithTags("Transactions")
        .WithSummary("Atualizar operação")
        .WithDescription("Atualiza os dados de uma operação existente. TotalValue é recalculado automaticamente.")
        .Produces<Response<Transaction>>(StatusCodes.Status200OK)
        .Produces<Response<Transaction>>(StatusCodes.Status404NotFound);
    }
}
