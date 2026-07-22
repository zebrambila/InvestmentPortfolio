namespace InvestmentPortfolio.Core.Responses;

public class BackfillPricesResult
{
    public Guid AssetId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public int MonthsFilled { get; set; }
}
