namespace InvestmentPortfolio.Core.Responses.Assets;

public class BulkCreateAssetResult
{
    public int Row { get; set; }
    public Guid AssetId { get; set; }
    public string Ticker { get; set; } = string.Empty;
}
