using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Requests.Transactions;

public class GetAllTransactionsRequest
{
    public int PageNumber { get; set; } = Core.Configuration.DefaultPageNumber;
    public int PageSize { get; set; } = Core.Configuration.DefaultPageSize;
    public Guid? AssetId { get; set; }
    public ECustodian? Custodian { get; set; }
    public EOperationType? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
