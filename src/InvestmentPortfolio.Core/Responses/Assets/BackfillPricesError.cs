namespace InvestmentPortfolio.Core.Responses.Assets;

public class BackfillPricesError
{
    public Guid AssetId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
