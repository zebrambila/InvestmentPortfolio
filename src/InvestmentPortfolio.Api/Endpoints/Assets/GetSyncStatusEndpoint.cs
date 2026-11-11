using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Responses.Assets;
using InvestmentPortfolio.Core.Responses.Commom;

namespace InvestmentPortfolio.Api.Endpoints.Assets;

public static class GetSyncStatusEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("v1/assets/sync-status", async (IAssetHandler handler, CancellationToken ct) =>
        {
            var result = await handler.GetSyncStatusAsync(ct);
            return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(result.Code);
        })
        .WithName("GetSyncStatus")
        .WithTags("Assets")
        .WithSummary("Status das últimas sincronizações")
        .WithDescription("Retorna data/hora, resultado e mensagem da última execução da sincronização de cotações e da carga de histórico de preços.")
        .Produces<Response<SyncStatusResponse>>(StatusCodes.Status200OK);
    }
}
