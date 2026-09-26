using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Queries;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;
using InvestmentPortfolio.Core.Responses.Dividends;
using InvestmentPortfolio.Core.Responses.Patrimony;
using InvestmentPortfolio.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Api.Handlers;

public class DividendHandler(InvestmentContext context) : IDividendHandler
{
    public async Task<Response<Dividend?>> CreateAsync(CreateDividendRequest request)
    {
        try
        {
            var dividend = new Dividend(request.AssetId, request.ReceiptDate, request.TotalFinancial, request.Custodian);

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

            dividend.Update(request.ReceiptDate, request.TotalFinancial, request.Custodian);

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
                ? context.Dividends.Include(d => d.Asset).Where(DividendQueries.GetByAsset(request.AssetId.Value))
                : context.Dividends.Include(d => d.Asset).Where(DividendQueries.GetActive());

            if (request.DateFrom.HasValue && request.DateTo.HasValue)
                query = query.Where(DividendQueries.GetByPeriod(request.DateFrom.Value, request.DateTo.Value));

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

    public async Task<Response<DividendSummaryByMonthResponse?>> GetSummaryByMonthAsync(GetDividendsSummaryByMonthRequest request)
    {
        try
        {
            var query = request.AssetId.HasValue
                ? context.Dividends.Where(DividendQueries.GetByAsset(request.AssetId.Value))
                : context.Dividends.Where(DividendQueries.GetActive());

            if (request.DateFrom.HasValue && request.DateTo.HasValue)
                query = query.Where(DividendQueries.GetByPeriod(request.DateFrom.Value, request.DateTo.Value));

            var months = await query
                .GroupBy(d => new { d.ReceiptDate.Year, d.ReceiptDate.Month })
                .Select(g => new DividendMonthlySummaryResponse
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(d => d.TotalFinancial)
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToListAsync();

            var summary = new DividendSummaryByMonthResponse
            {
                Months = months,
                AverageMonthly = months.Count > 0 ? months.Average(m => m.Total) : 0
            };

            return new Response<DividendSummaryByMonthResponse?>(summary, 200);
        }
        catch
        {
            return new Response<DividendSummaryByMonthResponse?>(null, 500, "Erro ao calcular resumo de dividendos por mês");
        }
    }

    public async Task<Response<DividendSummaryByAssetResponse?>> GetSummaryByAssetAsync(GetDividendsSummaryByAssetRequest request)
    {
        try
        {
            var query = context.Dividends.Include(d => d.Asset).Where(DividendQueries.GetActive());

            if (request.DateFrom.HasValue)
                query = query.Where(DividendQueries.GetByPeriodFrom(request.DateFrom.Value));

            var dateTo = request.DateTo ?? DateTime.Now;
            query = query.Where(DividendQueries.GetByPeriodTo(dateTo));

            var assets = await query
                .GroupBy(d => new { d.AssetId, d.Asset!.Ticker, d.Asset.Name })
                .Select(g => new DividendAssetSummaryResponse
                {
                    AssetId = g.Key.AssetId,
                    Ticker = g.Key.Ticker,
                    Name = g.Key.Name,
                    Total = g.Sum(d => d.TotalFinancial)
                })
                .OrderByDescending(a => a.Total)
                .ToListAsync();

            var totalReceived = assets.Sum(a => a.Total);

            if (request.Top.HasValue)
                assets = assets.Take(request.Top.Value).ToList();

            var summary = new DividendSummaryByAssetResponse
            {
                Assets = assets,
                TotalReceived = totalReceived
            };

            return new Response<DividendSummaryByAssetResponse?>(summary, 200);
        }
        catch
        {
            return new Response<DividendSummaryByAssetResponse?>(null, 500, "Erro ao calcular resumo de dividendos por ativo");
        }
    }
}
