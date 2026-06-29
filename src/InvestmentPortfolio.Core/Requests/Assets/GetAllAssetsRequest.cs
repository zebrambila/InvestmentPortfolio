namespace InvestmentPortfolio.Core.Requests.Assets;

public class GetAllAssetsRequest
{
    public int PageNumber { get; set; } = Core.Configuration.DefaultPageNumber;
    public int PageSize { get; set; } = Core.Configuration.DefaultPageSize;
}