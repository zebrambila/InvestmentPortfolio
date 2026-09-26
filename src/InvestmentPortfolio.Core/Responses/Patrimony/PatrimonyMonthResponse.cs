namespace InvestmentPortfolio.Core.Responses.Patrimony;

public class PatrimonyMonthResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Invested { get; set; }
    public decimal MarketValue { get; set; }
}
