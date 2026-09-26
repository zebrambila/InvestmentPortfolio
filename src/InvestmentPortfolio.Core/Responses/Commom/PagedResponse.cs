namespace InvestmentPortfolio.Core.Responses.Commom;

public class PagedResponse<TData> : Response<TData>
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public PagedResponse() { }

    public PagedResponse(TData? data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        Code = 200;
    }
}