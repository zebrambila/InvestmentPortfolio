using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;

namespace InvestmentPortfolio.Core.Handlers;

public interface IAssetHandler
{
    Task<Response<Asset?>> CreateAsync(CreateAssetRequest request);
    Task<Response<Asset?>> UpdateAsync(UpdateAssetRequest request);
    Task<Response<Asset?>> UpdatePriceAsync(UpdateAssetPriceRequest request);
    Task<Response<Asset?>> DeleteAsync(DeleteAssetRequest request);
    Task<Response<Asset?>> GetByIdAsync(GetAssetByIdRequest request);
    Task<PagedResponse<List<Asset>?>> GetAllAsync(GetAllAssetsRequest request);
    Task<Response<int>> SyncPricesAsync(CancellationToken ct = default);
}