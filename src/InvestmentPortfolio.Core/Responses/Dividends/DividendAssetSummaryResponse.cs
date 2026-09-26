namespace InvestmentPortfolio.Core.Responses.Dividends;

public class DividendAssetSummaryResponse
{
    public Guid AssetId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Total { get; set; }
}