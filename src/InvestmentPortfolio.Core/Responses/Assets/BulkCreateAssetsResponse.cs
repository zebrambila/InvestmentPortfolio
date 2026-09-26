namespace InvestmentPortfolio.Core.Responses.Assets;

public class BulkCreateAssetsResponse
{
    public List<BulkCreateAssetResult> Created { get; set; } = [];
    public List<BulkCreateAssetError> Failed { get; set; } = [];
}
