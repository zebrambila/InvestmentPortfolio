namespace InvestmentPortfolio.Core.Responses;

public class DividendMonthlySummaryResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
}
