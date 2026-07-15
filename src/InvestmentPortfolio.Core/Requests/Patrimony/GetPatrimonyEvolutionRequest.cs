namespace InvestmentPortfolio.Core.Requests.Patrimony;

public class GetPatrimonyEvolutionRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
