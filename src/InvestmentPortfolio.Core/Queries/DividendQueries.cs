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

    public static Expression<Func<Dividend, bool>> GetByPeriod(DateTime dateFrom, DateTime dateTo)
        => d => !d.IsDeleted && d.ReceiptDate >= dateFrom && d.ReceiptDate <= dateTo;

    public static Expression<Func<Dividend, bool>> GetByPeriodFrom(DateTime dateFrom)
        => d => !d.IsDeleted && d.ReceiptDate >= dateFrom;

    public static Expression<Func<Dividend, bool>> GetByPeriodTo(DateTime dateTo)
        => d => !d.IsDeleted && d.ReceiptDate <= dateTo;
}