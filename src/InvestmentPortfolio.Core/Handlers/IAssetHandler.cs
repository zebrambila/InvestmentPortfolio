using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Requests.Assets;
using InvestmentPortfolio.Core.Responses;
using InvestmentPortfolio.Core.Responses.Assets;
using InvestmentPortfolio.Core.Responses.Commom;
using InvestmentPortfolio.Core.Services;

namespace InvestmentPortfolio.Core.Handlers;

public interface IAssetHandler
{
    Task<Response<Asset?>> CreateAsync(CreateAssetRequest request);
    Task<Response<BulkCreateAssetsResponse?>> BulkCreateAsync(BulkCreateAssetsRequest request);
    Task<Response<Asset?>> UpdateAsync(UpdateAssetRequest request);
    Task<Response<Asset?>> UpdatePriceAsync(UpdateAssetPriceRequest request);
    Task<Response<Asset?>> DeleteAsync(DeleteAssetRequest request);
    Task<Response<Asset?>> GetByIdAsync(GetAssetByIdRequest request);
    Task<AssetPagedResponse> GetAllAsync(GetAllAssetsRequest request);
    Task<Response<int>> SyncPricesAsync(CancellationToken ct = default);
    Task<Response<BackfillPricesResponse?>> BackfillPricesAsync(BackfillPricesRequest request, CancellationToken ct = default);
    Task<Response<SyncStatusResponse?>> GetSyncStatusAsync(CancellationToken ct = default);
    Task<Response<List<DividendEvent>?>> GetDividendEventsAsync(GetAssetDividendEventsRequest request, CancellationToken ct = default);
}