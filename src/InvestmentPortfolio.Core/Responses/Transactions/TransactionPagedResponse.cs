using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Core.Responses.Transactions;

public class TransactionPagedResponse : PagedResponse<List<Transaction>?>
{
    public TransactionSummaryResponse Summary { get; set; } = new();

    public TransactionPagedResponse() { }

    public TransactionPagedResponse(
        List<Transaction>? data,
        int totalCount,
        int currentPage,
        int pageSize,
        TransactionSummaryResponse summary) : base(data, totalCount, currentPage, pageSize)
    {
        Summary = summary;
    }
}
