using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Transactions;

public static class CreateTransactionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("v1/transactions", async (CreateTransactionRequest request, ITransactionHandler handler) =>
        {
            var result = await handler.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"v1/transactions/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        })
        .WithName("CreateTransaction")
        .WithTags("Transactions")
        .WithSummary("Registrar operação")
        .WithDescription("Registra uma operação de compra ou venda. TotalValue é calculado automaticamente (Quantidade × Preço Unitário).")
        .Produces<Response<Transaction>>(StatusCodes.Status201Created)
        .Produces<Response<Transaction>>(StatusCodes.Status400BadRequest)
        .Produces<Response<Transaction>>(StatusCodes.Status404NotFound);
    }
}
