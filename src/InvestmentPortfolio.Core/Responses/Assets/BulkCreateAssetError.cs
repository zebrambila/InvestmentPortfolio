namespace InvestmentPortfolio.Core.Responses.Assets;

public class BulkCreateAssetError
{
    public int Row { get; set; }
    public string? Ticker { get; set; }
    public string Error { get; set; } = string.Empty;
}
