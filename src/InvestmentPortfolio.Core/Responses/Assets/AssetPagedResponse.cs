using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Core.Responses.Assets;

public class AssetPagedResponse : PagedResponse<List<AssetPositionResponse>?>
{
    public AssetPagedResponse() { }

    public AssetPagedResponse(
        List<AssetPositionResponse>? data,
        int totalCount,
        int currentPage,
        int pageSize) : base(data, totalCount, currentPage, pageSize) { }
}