namespace InvestmentPortfolio.Core.Responses.Transactions;

public class TransactionSummaryResponse
{
    public int TotalCount { get; set; }
    public decimal VolumeCompra { get; set; }
    public decimal VolumeVenda { get; set; }
}
