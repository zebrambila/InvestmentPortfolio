namespace InvestmentPortfolio.Core.Responses;

public class AssetPagedResponse : PagedResponse<List<AssetPositionResponse>?>
{
    public AssetPagedResponse() { }

    public AssetPagedResponse(
        List<AssetPositionResponse>? data,
        int totalCount,
        int currentPage,
        int pageSize) : base(data, totalCount, currentPage, pageSize) { }
}