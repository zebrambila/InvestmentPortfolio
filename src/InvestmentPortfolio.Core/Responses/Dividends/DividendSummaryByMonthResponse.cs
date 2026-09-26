namespace InvestmentPortfolio.Core.Responses.Dividends;

public class DividendSummaryByMonthResponse
{
    public List<DividendMonthlySummaryResponse> Months { get; set; } = [];
    public decimal AverageMonthly { get; set; }
}
