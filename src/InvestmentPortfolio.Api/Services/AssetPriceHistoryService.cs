using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Api.Services;

public class AssetPriceHistoryService(
    InvestmentContext context,
    IMarketDataService marketDataService,
    ILogger<AssetPriceHistoryService> logger) : IAssetPriceHistoryService
{
    public async Task BackfillAsync(CancellationToken ct = default)
    {
        var assets = await context.Assets.Where(a => !a.IsDeleted).ToListAsync(ct);
        if (assets.Count == 0)
            return;

        var firstTransactionDates = await context.Transactions
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.AssetId)
            .Select(g => new { AssetId = g.Key, FirstDate = g.Min(t => t.DateBuySell) })
            .ToDictionaryAsync(x => x.AssetId, x => x.FirstDate, ct);

        var monthEnds = GetLastTwelveCompletedMonthEnds(DateTime.Now.Date);

        foreach (var asset in assets)
        {
            try
            {
                if (!firstTransactionDates.TryGetValue(asset.Id, out var firstTransactionDate))
                    continue;

                var targetDates = monthEnds.Where(d => d >= firstTransactionDate.Date).ToList();
                if (targetDates.Count == 0)
                    continue;

                var existingDates = await context.AssetPriceHistories
                    .Where(h => h.AssetId == asset.Id && targetDates.Contains(h.Date))
                    .Select(h => h.Date)
                    .ToListAsync(ct);

                var missingDates = targetDates.Except(existingDates).ToList();
                if (missingDates.Count == 0)
                    continue;

                var prices = await marketDataService.GetHistoricalClosePricesAsync(asset.Ticker, missingDates, ct);

                foreach (var date in missingDates)
                {
                    if (prices.TryGetValue(date, out var price) && price.HasValue)
                        context.AssetPriceHistories.Add(new AssetPriceHistory(asset.Id, date, price.Value));
                }

                await context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no backfill de histórico de preços para o ativo {Ticker}.", asset.Ticker);
            }
        }
    }

    public async Task SyncCurrentMonthAsync(CancellationToken ct = default)
    {
        var assets = await context.Assets.Where(a => !a.IsDeleted).ToListAsync(ct);
        if (assets.Count == 0)
            return;

        var monthEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

        foreach (var asset in assets)
        {
            try
            {
                var price = await marketDataService.GetCurrentPriceAsync(asset.Ticker, ct);
                if (!price.HasValue)
                {
                    logger.LogWarning("Preço não encontrado para o ativo {Ticker} ao gravar histórico mensal.", asset.Ticker);
                    continue;
                }

                var existing = await context.AssetPriceHistories
                    .FirstOrDefaultAsync(h => h.AssetId == asset.Id && h.Date == monthEnd, ct);

                if (existing is not null)
                    existing.UpdateClosePrice(price.Value);
                else
                    context.AssetPriceHistories.Add(new AssetPriceHistory(asset.Id, monthEnd, price.Value));

                await context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao gravar histórico de preço mensal para o ativo {Ticker}.", asset.Ticker);
            }
        }
    }

    private static List<DateTime> GetLastTwelveCompletedMonthEnds(DateTime reference)
    {
        var result = new List<DateTime>();
        var cursor = new DateTime(reference.Year, reference.Month, 1).AddMonths(-1);

        for (var i = 0; i < 12; i++)
        {
            result.Add(cursor.AddMonths(1).AddDays(-1));
            cursor = cursor.AddMonths(-1);
        }

        result.Reverse();
        return result;
    }
}
