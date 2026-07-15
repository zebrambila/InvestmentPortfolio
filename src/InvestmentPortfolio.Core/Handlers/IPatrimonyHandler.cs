using InvestmentPortfolio.Core.Requests.Patrimony;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Core.Handlers;

public interface IPatrimonyHandler
{
    Task<Response<PatrimonyEvolutionResponse?>> GetEvolutionByMonthAsync(GetPatrimonyEvolutionRequest request);
}
