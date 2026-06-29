using System.Linq.Expressions;
using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Core.Queries;

public static class DividendQueries
{
    public static Expression<Func<Dividend, bool>> GetActive()
        => d => !d.IsDeleted;

    public static Expression<Func<Dividend, bool>> GetById(Guid id)
        => d => !d.IsDeleted && d.Id == id;

    public static Expression<Func<Dividend, bool>> GetByAsset(Guid assetId)
        => d => !d.IsDeleted && d.AssetId == assetId;
}