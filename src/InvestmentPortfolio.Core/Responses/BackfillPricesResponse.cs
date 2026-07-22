namespace InvestmentPortfolio.Core.Responses;

public class BackfillPricesResponse
{
    public List<BackfillPricesResult> Processed { get; set; } = [];
    public List<BackfillPricesError> Failed { get; set; } = [];
}
