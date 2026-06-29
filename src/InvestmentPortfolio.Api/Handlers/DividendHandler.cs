using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Queries;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Api.Handlers;

public class DividendHandler(InvestmentContext context) : IDividendHandler
{
    private async Task<decimal> CalculatePositionAsync(Guid assetId, DateTime receiptDate)
    {
        return await context.Transactions
            .Where(t => t.AssetId == assetId && !t.IsDeleted && t.DateBuyorSell <= receiptDate)
            .SumAsync(t => t.Type == EOperationType.Compra ? t.Quantity : -t.Quantity);
    }

    public async Task<Response<Dividend?>> CreateAsync(CreateDividendRequest request)
    {
        try
        {
            var positionQuantity = await CalculatePositionAsync(request.AssetId, request.ReceiptDate);
            var dividend = new Dividend(request.AssetId, request.ReceiptDate, request.UnitValue, positionQuantity);

            context.Dividends.Add(dividend);
            await context.SaveChangesAsync();

            return new Response<Dividend?>(dividend, 201, "Dividendo criado com sucesso");
        }
        catch
        {
            return new Response<Dividend?>(null, 500, "Erro ao criar dividendo");
        }
    }

    public async Task<Response<Dividend?>> UpdateAsync(UpdateDividendRequest request)
    {
        try
        {
            var dividend = await context.Dividends
                .FirstOrDefaultAsync(DividendQueries.GetById(request.Id));

            if (dividend is null)
                return new Response<Dividend?>(null, 404, "Dividendo não encontrado");

            var positionQuantity = await CalculatePositionAsync(dividend.AssetId, request.ReceiptDate);
            dividend.Update(request.ReceiptDate, request.UnitValue, positionQuantity);

            await context.SaveChangesAsync();

            return new Response<Dividend?>(dividend, 200, "Dividendo atualizado com sucesso");
        }
        catch
        {
            return new Response<Dividend?>(null, 500, "Erro ao atualizar dividendo");
        }
    }

    public async Task<Response<Dividend?>> DeleteAsync(DeleteDividendRequest request)
    {
        try
        {
            var dividend = await context.Dividends
                .FirstOrDefaultAsync(DividendQueries.GetById(request.Id));

            if (dividend is null)
                return new Response<Dividend?>(null, 404, "Dividendo não encontrado");

            dividend.Delete();
            await context.SaveChangesAsync();

            return new Response<Dividend?>(dividend, 200, "Dividendo removido com sucesso");
        }
        catch
        {
            return new Response<Dividend?>(null, 500, "Erro ao remover dividendo");
        }
    }

    public async Task<Response<Dividend?>> GetByIdAsync(GetDividendByIdRequest request)
    {
        try
        {
            var dividend = await context.Dividends
                .FirstOrDefaultAsync(DividendQueries.GetById(request.Id));

            if (dividend is null)
                return new Response<Dividend?>(null, 404, "Dividendo não encontrado");

            return new Response<Dividend?>(dividend, 200);
        }
        catch
        {
            return new Response<Dividend?>(null, 500, "Erro ao buscar dividendo");
        }
    }

    public async Task<PagedResponse<List<Dividend>?>> GetAllAsync(GetAllDividendsRequest request)
    {
        try
        {
            var query = request.AssetId.HasValue
                ? context.Dividends.Where(DividendQueries.GetByAsset(request.AssetId.Value))
                : context.Dividends.Where(DividendQueries.GetActive());

            var total = await query.CountAsync();
            var dividends = await query
                .OrderByDescending(d => d.ReceiptDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<List<Dividend>?>(dividends, total, request.PageNumber, request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Dividend>?>(null, 0, request.PageNumber, request.PageSize)
            {
                Code = 500,
                Message = "Erro ao buscar dividendos"
            };
        }
    }
}
