using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Core.Handlers;

public interface IDividendHandler
{
    Task<Response<Dividend?>> CreateAsync(CreateDividendRequest request);
    Task<Response<Dividend?>> UpdateAsync(UpdateDividendRequest request);
    Task<Response<Dividend?>> DeleteAsync(DeleteDividendRequest request);
    Task<Response<Dividend?>> GetByIdAsync(GetDividendByIdRequest request);
    Task<PagedResponse<List<Dividend>?>> GetAllAsync(GetAllDividendsRequest request);
    Task<Response<DividendSummaryByMonthResponse?>> GetSummaryByMonthAsync(GetDividendsSummaryByMonthRequest request);
    Task<Response<DividendSummaryByAssetResponse?>> GetSummaryByAssetAsync(GetDividendsSummaryByAssetRequest request);
}