namespace InvestmentPortfolio.Core.Responses.Dividends;

public class DividendMonthlySummaryResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
}
