using InvestmentPortfolio.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Tests.Fakes;

public static class InvestmentContextFake
{
    public static InvestmentContext Create()
    {
        var options = new DbContextOptionsBuilder<InvestmentContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InvestmentContext(options);
    }
}
