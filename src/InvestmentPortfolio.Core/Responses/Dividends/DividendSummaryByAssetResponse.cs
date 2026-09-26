namespace InvestmentPortfolio.Core.Responses.Dividends;

public class DividendSummaryByAssetResponse
{
    public List<DividendAssetSummaryResponse> Assets { get; set; } = [];
    public decimal TotalReceived { get; set; }
}