namespace InvestmentPortfolio.Core.Requests.Dividends;

public class GetAllDividendsRequest
{
    public int PageNumber { get; set; } = Core.Configuration.DefaultPageNumber;
    public int PageSize { get; set; } = Core.Configuration.DefaultPageSize;
    public Guid? AssetId { get; set; }
}