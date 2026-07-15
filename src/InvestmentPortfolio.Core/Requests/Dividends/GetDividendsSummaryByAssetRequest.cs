namespace InvestmentPortfolio.Core.Requests.Dividends;

public class GetDividendsSummaryByAssetRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? Top { get; set; }
}