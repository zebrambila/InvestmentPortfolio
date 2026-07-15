namespace InvestmentPortfolio.Core.Requests.Dividends;

public class GetDividendsSummaryByMonthRequest
{
    public Guid? AssetId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
