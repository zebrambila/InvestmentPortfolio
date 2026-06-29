using InvestmentPortfolio.Core.Handlers;

namespace InvestmentPortfolio.Api.BackgroundJobs;

public class PriceSyncJob(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<PriceSyncJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = configuration.GetValue<int>("MarketData:SyncIntervalMinutes", 60);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        logger.LogInformation("PriceSyncJob iniciado. Intervalo: {Interval} minutos.", intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IAssetHandler>();
                var result = await handler.SyncPricesAsync(stoppingToken);
                logger.LogInformation("Sincronização automática: {Message}", result.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro na sincronização automática de preços.");
            }
        }
    }
}
