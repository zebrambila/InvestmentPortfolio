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
            await service.BackfillAsync(stoppingToken);
            logger.LogInformation("Backfill de histórico de preços concluído.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro no backfill de histórico de preços.");
        }
    }
}
