using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Transactions;

public static class GetAllTransactionsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/transactions/GetAllTransactions", async ([AsParameters] GetAllTransactionsRequest request, ITransactionHandler handler) =>
        {
            var result = await handler.GetAllAsync(request);
            return Results.Ok(result);
        })
        .WithName("GetAllTransactions")
        .WithTags("Transactions")
        .WithSummary("Listar operações")
        .WithDescription("Retorna lista paginada de operações ativas com resumo de volume financeiro. Filtra por AssetId, Custodian, Tipo (Compra/Venda) e/ou período (DateFrom/DateTo).")
        .Produces<TransactionPagedResponse>(StatusCodes.Status200OK);
    }
}
