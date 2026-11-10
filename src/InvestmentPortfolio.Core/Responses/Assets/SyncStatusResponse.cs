namespace InvestmentPortfolio.Core.Responses.Assets;

public class JobExecutionStatusResponse
{
    public DateTime ExecutedAt { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class SyncStatusResponse
{
    /// <summary>Última execução de sincronização de cotações (null se nunca executou).</summary>
    public JobExecutionStatusResponse? SyncPrices { get; set; }

    /// <summary>Última execução de carga do histórico de preços (null se nunca executou).</summary>
    public JobExecutionStatusResponse? BackfillPrices { get; set; }
}
