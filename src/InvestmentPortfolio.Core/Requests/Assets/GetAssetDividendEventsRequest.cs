namespace InvestmentPortfolio.Core.Requests.Assets;

public class GetAssetDividendEventsRequest
{
    public string Ticker { get; set; } = string.Empty;
    public int? Months { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}