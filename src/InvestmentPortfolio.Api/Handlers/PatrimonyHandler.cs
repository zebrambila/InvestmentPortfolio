using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Patrimony;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Api.Handlers;

public class PatrimonyHandler(InvestmentContext context) : IPatrimonyHandler
{
    public async Task<Response<PatrimonyEvolutionResponse?>> GetEvolutionByMonthAsync(GetPatrimonyEvolutionRequest request)
    {
        try
        {
            var today = DateTime.Now.Date;
            var dateTo = request.DateTo?.Date ?? today;
            var dateFrom = request.DateFrom?.Date ?? new DateTime(dateTo.Year, dateTo.Month, 1).AddMonths(-11);

            var months = GetMonthRange(dateFrom, dateTo);
            if (months.Count == 0)
                return new Response<PatrimonyEvolutionResponse?>(new PatrimonyEvolutionResponse(), 200);

            var lastMonthEnd = new DateTime(months[^1].Year, months[^1].Month, DateTime.DaysInMonth(months[^1].Year, months[^1].Month));

            var transactions = await context.Transactions
                .Where(t => !t.IsDeleted && t.DateBuySell <= lastMonthEnd)
                .Select(t => new { t.AssetId, t.Side, t.Quantity, t.TotalValue, t.DateBuySell })
                .ToListAsync();

            var priceHistoryByAsset = (await context.AssetPriceHistories
                    .Where(h => h.Date <= lastMonthEnd)
                    .Select(h => new { h.AssetId, h.Date, h.ClosePrice })
                    .ToListAsync())
                .GroupBy(h => h.AssetId)
                .ToDictionary(g => g.Key, g => g.OrderBy(h => h.Date).ToList());

            var currentPriceByAsset = await context.Assets
                .Select(a => new { a.Id, a.CurrentPrice })
                .ToDictionaryAsync(a => a.Id, a => a.CurrentPrice);

            var monthResponses = new List<PatrimonyMonthResponse>();

            foreach (var (year, month) in months)
            {
                var isCurrentMonth = year == today.Year && month == today.Month;
                var monthEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                var byAsset = transactions
                    .Where(t => t.DateBuySell.Date <= monthEnd)
                    .GroupBy(t => t.AssetId)
                    .Select(g => new
                    {
                        AssetId = g.Key,
                        PositionQty = g.Where(t => t.Side == EOperationType.Compra).Sum(t => t.Quantity)
                                    - g.Where(t => t.Side == EOperationType.Venda).Sum(t => t.Quantity),
                        Invested = g.Where(t => t.Side == EOperationType.Compra).Sum(t => t.TotalValue)
                                 - g.Where(t => t.Side == EOperationType.Venda).Sum(t => t.TotalValue),
                    })
                    .ToList();

                var invested = byAsset.Sum(a => a.Invested);

                var marketValue = 0m;
                foreach (var a in byAsset)
                {
                    if (a.PositionQty == 0)
                        continue;

                    if (isCurrentMonth)
                    {
                        if (!currentPriceByAsset.TryGetValue(a.AssetId, out var currentPrice))
                            continue;

                        marketValue += a.PositionQty * currentPrice;
                        continue;
                    }

                    if (!priceHistoryByAsset.TryGetValue(a.AssetId, out var series))
                        continue;

                    var closest = series.LastOrDefault(h => h.Date <= monthEnd);
                    if (closest is null)
                        continue;

                    marketValue += a.PositionQty * closest.ClosePrice;
                }

                monthResponses.Add(new PatrimonyMonthResponse
                {
                    Year = year,
                    Month = month,
                    Invested = invested,
                    MarketValue = marketValue
                });
            }

            return new Response<PatrimonyEvolutionResponse?>(new PatrimonyEvolutionResponse { Months = monthResponses }, 200);
        }
        catch
        {
            return new Response<PatrimonyEvolutionResponse?>(null, 500, "Erro ao calcular evolução patrimonial");
        }
    }

    private static List<(int Year, int Month)> GetMonthRange(DateTime dateFrom, DateTime dateTo)
    {
        var result = new List<(int Year, int Month)>();
        var cursor = new DateTime(dateFrom.Year, dateFrom.Month, 1);
        var end = new DateTime(dateTo.Year, dateTo.Month, 1);

        while (cursor <= end)
        {
            result.Add((cursor.Year, cursor.Month));
            cursor = cursor.AddMonths(1);
        }

        return result;
    }
}
