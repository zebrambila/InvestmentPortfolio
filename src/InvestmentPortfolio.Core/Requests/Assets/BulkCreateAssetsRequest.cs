namespace InvestmentPortfolio.Core.Requests.Assets;

public class BulkCreateAssetsRequest
{
    public List<CreateAssetRequest> Assets { get; set; } = [];
}
