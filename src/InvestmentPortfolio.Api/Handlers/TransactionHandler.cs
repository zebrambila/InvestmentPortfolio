using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Queries;
using InvestmentPortfolio.Core.Requests.Transactions;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;
using InvestmentPortfolio.Core.Responses.Transactions;
using InvestmentPortfolio.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Api.Handlers;

public class TransactionHandler(InvestmentContext context) : ITransactionHandler
{
    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        try
        {
            var assetExists = await context.Assets
                .AnyAsync(a => !a.IsDeleted && a.Id == request.AssetId);

            if (!assetExists)
                return new Response<Transaction?>(null, 404, "Ativo não encontrado");

            var transaction = new Transaction(
                request.AssetId, request.Type, request.Quantity,
                request.UnitPrice, request.DateBuySell, request.Custodian);

            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 201, "Operação criada com sucesso");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Erro ao criar operação");
        }
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        try
        {
            var transaction = await context.Transactions
                .FirstOrDefaultAsync(TransactionQueries.GetById(request.Id));

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Operação não encontrada");

            transaction.Update(request.Type, request.Quantity, request.UnitPrice, request.DateBuySell, request.Custodian);
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 200, "Operação atualizada com sucesso");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Erro ao atualizar operação");
        }
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        try
        {
            var transaction = await context.Transactions
                .FirstOrDefaultAsync(TransactionQueries.GetById(request.Id));

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Operação não encontrada");

            transaction.Delete();
            await context.SaveChangesAsync();

            return new Response<Transaction?>(transaction, 200, "Operação removida com sucesso");
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Erro ao remover operação");
        }
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
    {
        try
        {
            var transaction = await context.Transactions
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(TransactionQueries.GetById(request.Id));

            if (transaction is null)
                return new Response<Transaction?>(null, 404, "Operação não encontrada");

            return new Response<Transaction?>(transaction, 200);
        }
        catch
        {
            return new Response<Transaction?>(null, 500, "Erro ao buscar operação");
        }
    }

    public async Task<TransactionPagedResponse> GetAllAsync(GetAllTransactionsRequest request)
    {
        try
        {
            var query = context.Transactions.Include(t => t.Asset).Where(TransactionQueries.GetActive());

            if (request.AssetId.HasValue)
                query = query.Where(t => t.AssetId == request.AssetId.Value);

            if (request.Custodian.HasValue)
                query = query.Where(TransactionQueries.GetByCustodian(request.Custodian.Value));

            if (request.Type.HasValue)
                query = query.Where(TransactionQueries.GetByType(request.Type.Value));

            if (request.DateFrom.HasValue && request.DateTo.HasValue)
                query = query.Where(TransactionQueries.GetByPeriod(request.DateFrom.Value, request.DateTo.Value));

            var total = await query.CountAsync();

            var summary = new TransactionSummaryResponse
            {
                TotalCount = total,
                VolumeCompra = await query.Where(t => t.Side == EOperationType.Compra).SumAsync(t => (decimal?)t.TotalValue) ?? 0,
                VolumeVenda  = await query.Where(t => t.Side == EOperationType.Venda).SumAsync(t => (decimal?)t.TotalValue) ?? 0,
            };

            var transactions = await query
                .OrderByDescending(t => t.DateBuySell)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new TransactionPagedResponse(transactions, total, request.PageNumber, request.PageSize, summary);
        }
        catch
        {
            return new TransactionPagedResponse
            {
                Code = 500,
                Message = "Erro ao buscar operações"
            };
        }
    }
}
