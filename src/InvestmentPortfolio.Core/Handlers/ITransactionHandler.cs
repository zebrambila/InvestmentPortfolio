using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;
using InvestmentPortfolio.Core.Responses.Transactions;

namespace InvestmentPortfolio.Core.Handlers;

public interface ITransactionHandler
{
    Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request);
    Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request);
    Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request);
    Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request);
    Task<TransactionPagedResponse> GetAllAsync(GetAllTransactionsRequest request);
}