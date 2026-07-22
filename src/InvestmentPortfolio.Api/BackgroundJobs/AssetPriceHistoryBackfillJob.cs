using InvestmentPortfolio.Core.Services;

namespace InvestmentPortfolio.Api.BackgroundJobs;

public class AssetPriceHistoryBackfillJob(IServiceScopeFactory scopeFactory, ILogger<AssetPriceHistoryBackfillJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Backfill de histórico de preços iniciado.");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAssetPriceHistoryService>();
            var result = await service.BackfillAsync(ct: stoppingToken);
            logger.LogInformation(
                "Backfill de histórico de preços concluído. {Processed} processado(s), {Failed} falha(s).",
                result.Processed.Count, result.Failed.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro no backfill de histórico de preços.");
        }
    }
}
