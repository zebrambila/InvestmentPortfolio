using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Requests.Dividends;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Api.Endpoints.Dividends;

public static class CreateDividendEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("v1/dividends", async (CreateDividendRequest request, IDividendHandler handler) =>
        {
            var result = await handler.CreateAsync(request);
            return result.IsSuccess
                ? Results.Created($"v1/dividends/{result.Data?.Id}", result)
                : Results.BadRequest(result);
        })
        .WithName("CreateDividend")
        .WithTags("Dividends")
        .WithSummary("Registrar dividendo")
        .WithDescription("Registra um dividendo. PositionQuantity e TotalFinancial são calculados automaticamente com base nas operações até a ReceiptDate.")
        .Produces<Response<Dividend>>(StatusCodes.Status201Created)
        .Produces<Response<Dividend>>(StatusCodes.Status400BadRequest);
    }
}
