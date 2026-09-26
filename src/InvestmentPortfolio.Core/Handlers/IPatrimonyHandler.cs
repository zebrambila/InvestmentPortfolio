using InvestmentPortfolio.Core.Requests.Patrimony;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Commom;
using InvestmentPortfolio.Core.Responses.Patrimony;

namespace InvestmentPortfolio.Core.Handlers;

public interface IPatrimonyHandler
{
    Task<Response<PatrimonyEvolutionResponse?>> GetEvolutionByMonthAsync(GetPatrimonyEvolutionRequest request);
}
