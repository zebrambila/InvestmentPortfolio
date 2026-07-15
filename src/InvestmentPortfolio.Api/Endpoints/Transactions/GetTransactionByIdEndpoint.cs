using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Transactions;

public static class GetTransactionByIdEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/transactions/GetTransactionById/{id:guid}", async (Guid id, ITransactionHandler handler) =>
        {
            var result = await handler.GetByIdAsync(new GetTransactionByIdRequest { Id = id });
            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetTransactionById")
        .WithTags("Transactions")
        .WithSummary("Buscar operação por ID")
        .WithDescription("Retorna uma operação ativa (não excluída) pelo seu identificador único.")
        .Produces<Response<Transaction>>(StatusCodes.Status200OK)
        .Produces<Response<Transaction>>(StatusCodes.Status404NotFound);
    }
}
