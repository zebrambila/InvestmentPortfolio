using InvestmentPortfolio.Core.Entities;
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
    public async Task<Response<Asset?>> CreateAsync(CreateAssetRequest request)
    {
        try
        {
            var exists = await context.Assets.AnyAsync(AssetQueries.GetByTicker(request.Ticker));
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

    public async Task<PagedResponse<List<Asset>?>> GetAllAsync(GetAllAssetsRequest request)
    {
        try
        {
            var query = context.Assets.Where(AssetQueries.GetActive());
            var total = await query.CountAsync();
            var assets = await query
                .OrderBy(a => a.Ticker)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<List<Asset>?>(assets, total, request.PageNumber, request.PageSize);
        }
        catch
        {
            return new PagedResponse<List<Asset>?>(null, 0, request.PageNumber, request.PageSize)
            {
                Code = 500,
                Message = "Erro ao buscar ativos"
            };
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
