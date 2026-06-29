using System.Linq.Expressions;
using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Core.Queries;

public static class AssetQueries
{
    public static Expression<Func<Asset, bool>> GetActive()
        => a => !a.IsDeleted;

    public static Expression<Func<Asset, bool>> GetByTicker(string ticker)
        => a => !a.IsDeleted && a.Ticker == ticker;
}