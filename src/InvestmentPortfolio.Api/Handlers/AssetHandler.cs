using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Handlers;
using InvestmentPortfolio.Core.Queries;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Services;
using InvestmentPortfolio.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestmentPortfolio.Api.Handlers;

public class AssetHandler(InvestmentContext context, IMarketDataService marketDataService) : IAssetHandler
{
    private const int MaxBulkAssets = 500;

    public async Task<Response<Asset?>> CreateAsync(CreateAssetRequest request)
    {
        try
        {
            var exists = await context.Assets.AnyAsync(AssetQueries.GetByTicker(request.Ticker.ToUpper()));
            if (exists)
                return new Response<Asset?>(null, 400, "Ticker já cadastrado");

            var needsInfo = string.IsNullOrWhiteSpace(request.Name) || request.Price is null or 0;
            var info = needsInfo ? await marketDataService.GetAssetInfoAsync(request.Ticker) : null;

            var name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : info?.Name ?? request.Ticker;
            var price = (request.Price is null or 0) ? info?.Price ?? 0 : request.Price.Value;

            var asset = new Asset(request.Ticker, name, request.Type);
            asset.UpdateCurrentPrice(price);
            context.Assets.Add(asset);
            await context.SaveChangesAsync();

            return new Response<Asset?>(asset, 201, "Ativo criado com sucesso");
        }
        catch
        {
            return new Response<Asset?>(null, 500, "Erro ao criar ativo");
        }
    }

    public async Task<Response<BulkCreateAssetsResponse?>> BulkCreateAsync(BulkCreateAssetsRequest request)
    {
        if (request.Assets.Count == 0)
            return new Response<BulkCreateAssetsResponse?>(null, 400, "A lista de ativos não pode ser vazia");

        if (request.Assets.Count > MaxBulkAssets)
            return new Response<BulkCreateAssetsResponse?>(null, 400, $"O lote não pode conter mais de {MaxBulkAssets} ativos");

        var result = new BulkCreateAssetsResponse();

        for (var row = 0; row < request.Assets.Count; row++)
        {
            var item = request.Assets[row];

            var validationError = ValidateBulkItem(item);
            if (validationError is not null)
            {
                result.Failed.Add(new BulkCreateAssetError { Row = row, Ticker = item.Ticker, Error = validationError });
                continue;
            }

            var created = await CreateAsync(item);
            if (created.IsSuccess && created.Data is not null)
                result.Created.Add(new BulkCreateAssetResult { Row = row, AssetId = created.Data.Id, Ticker = created.Data.Ticker });
            else
                result.Failed.Add(new BulkCreateAssetError { Row = row, Ticker = item.Ticker, Error = created.Message ?? "Erro ao criar ativo" });
        }

        return new Response<BulkCreateAssetsResponse?>(result, 200, $"{result.Created.Count} criado(s), {result.Failed.Count} falha(s)");
    }

    private static string? ValidateBulkItem(CreateAssetRequest item)
    {
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(item, new ValidationContext(item), validationResults, validateAllProperties: true))
            return string.Join(" ", validationResults.Select(r => r.ErrorMessage));

        if (!Enum.IsDefined(item.Type))
            return "O tipo do ativo é inválido";

        return null;
    }

    public async Task<Response<Asset?>> UpdateAsync(UpdateAssetRequest request)
    {
        try
        {
            var asset = await context.Assets
                .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == request.Id);

            if (asset is null)
                return new Response<Asset?>(null, 404, "Ativo não encontrado");

            var needsInfo = string.IsNullOrWhiteSpace(request.Name) || request.Price is null or 0;
            var info = needsInfo ? await marketDataService.GetAssetInfoAsync(request.Ticker) : null;

            var name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : info?.Name ?? asset.Name;
            asset.Update(request.Ticker, name, request.Type);

            var price = request.Price is null or 0 ? info?.Price : request.Price;
            if (price.HasValue)
                asset.UpdateCurrentPrice(price.Value);

            await context.SaveChangesAsync();

            return new Response<Asset?>(asset, 200, "Ativo atualizado com sucesso");
        }
        catch
        {
            return new Response<Asset?>(null, 500, "Erro ao atualizar ativo");
        }
    }

    public async Task<Response<Asset?>> UpdatePriceAsync(UpdateAssetPriceRequest request)
    {
        try
        {
            var asset = await context.Assets
                .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == request.Id);

            if (asset is null)
                return new Response<Asset?>(null, 404, "Ativo não encontrado");

            var price = (request.CurrentPrice is null or 0)
                ? await marketDataService.GetCurrentPriceAsync(asset.Ticker) ?? 0
                : request.CurrentPrice.Value;

            asset.UpdateCurrentPrice(price);
            await context.SaveChangesAsync();

            return new Response<Asset?>(asset, 200, "Preço atualizado com sucesso");
        }
        catch
        {
            return new Response<Asset?>(null, 500, "Erro ao atualizar preço");
        }
    }

    public async Task<Response<Asset?>> DeleteAsync(DeleteAssetRequest request)
    {
        try
        {
            var asset = await context.Assets
                .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == request.Id);

            if (asset is null)
                return new Response<Asset?>(null, 404, "Ativo não encontrado");

            asset.Delete();
            await context.SaveChangesAsync();

            return new Response<Asset?>(asset, 200, "Ativo removido com sucesso");
        }
        catch
        {
            return new Response<Asset?>(null, 500, "Erro ao remover ativo");
        }
    }

    public async Task<Response<Asset?>> GetByIdAsync(GetAssetByIdRequest request)
    {
        try
        {
            var asset = await context.Assets
                .FirstOrDefaultAsync(a => !a.IsDeleted && a.Id == request.Id);

            if (asset is null)
                return new Response<Asset?>(null, 404, "Ativo não encontrado");

            return new Response<Asset?>(asset, 200);
        }
        catch
        {
            return new Response<Asset?>(null, 500, "Erro ao buscar ativo");
        }
    }

    public async Task<AssetPagedResponse> GetAllAsync(GetAllAssetsRequest request)
    {
        try
        {
            var query = context.Assets.Where(AssetQueries.GetActive());

            if (request.Type.HasValue)
                query = query.Where(a => a.Type == request.Type.Value);

            var assets = await query.OrderBy(a => a.Ticker).ToListAsync();

            var assetIds = assets.Select(a => a.Id).ToList();

            var aggregations = await context.Transactions
                .Where(t => !t.IsDeleted && assetIds.Contains(t.AssetId))
                .GroupBy(t => t.AssetId)
                .Select(g => new
                {
                    AssetId       = g.Key,
                    QtyCompra     = g.Where(t => t.Side == EOperationType.Compra).Sum(t => (decimal?)t.Quantity) ?? 0,
                    QtyVenda      = g.Where(t => t.Side == EOperationType.Venda).Sum(t => (decimal?)t.Quantity) ?? 0,
                    TotalInvested = g.Where(t => t.Side == EOperationType.Compra).Sum(t => (decimal?)t.TotalValue) ?? 0,
                })
                .ToDictionaryAsync(x => x.AssetId);

            var positions = assets
                .Select(a =>
                {
                    aggregations.TryGetValue(a.Id, out var agg);
                    var positionQty   = agg != null ? agg.QtyCompra - agg.QtyVenda : 0m;
                    var totalInvested = agg?.TotalInvested ?? 0m;
                    var averagePrice  = agg?.QtyCompra > 0 ? totalInvested / agg!.QtyCompra : 0m;

                    return new AssetPositionResponse
                    {
                        AssetId           = a.Id,
                        Ticker            = a.Ticker,
                        Name              = a.Name,
                        Type              = a.Type,
                        CurrentPrice      = a.CurrentPrice,
                        PositionQuantity  = positionQty,
                        AveragePrice      = averagePrice,
                        TotalInvested     = totalInvested,
                        CurrentTotalValue = positionQty * a.CurrentPrice,
                    };
                })
                .Where(p => p.PositionQuantity >= 0)
                .ToList();

            var total = positions.Count;
            var paged = positions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new AssetPagedResponse(paged, total, request.PageNumber, request.PageSize);
        }
        catch
        {
            return new AssetPagedResponse { Code = 500, Message = "Erro ao buscar ativos" };
        }
    }

    public async Task<Response<int>> SyncPricesAsync(CancellationToken ct = default)
    {
        try
        {
            var assets = await context.Assets
                .Where(AssetQueries.GetActive())
                .ToListAsync(ct);

            if (assets.Count == 0)
                return new Response<int>(0, 200, "Nenhum ativo cadastrado");

            var tickers = assets.Select(a => a.Ticker);
            var prices = await marketDataService.GetCurrentPricesAsync(tickers, ct);

            var updated = 0;
            foreach (var asset in assets)
            {
                if (prices.TryGetValue(asset.Ticker.ToUpper(), out var price) && price.HasValue)
                {
                    asset.UpdateCurrentPrice(price.Value);
                    updated++;
                }
            }

            if (updated > 0)
                await context.SaveChangesAsync(ct);

            return new Response<int>(updated, 200, $"{updated} preço(s) atualizado(s)");
        }
        catch
        {
            return new Response<int>(0, 500, "Erro ao sincronizar preços");
        }
    }
}
