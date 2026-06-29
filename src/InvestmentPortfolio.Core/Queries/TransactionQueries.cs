using System.Linq.Expressions;
using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Queries;

public static class TransactionQueries
{
    public static Expression<Func<Transaction, bool>> GetActive()
        => t => !t.IsDeleted;

    public static Expression<Func<Transaction, bool>> GetById(Guid id)
        => t => !t.IsDeleted && t.Id == id;

    public static Expression<Func<Transaction, bool>> GetByAsset(Guid assetId)
        => t => !t.IsDeleted && t.AssetId == assetId;

    public static Expression<Func<Transaction, bool>> GetByCustodian(ECustodian custodian)
        => t => !t.IsDeleted && t.Custodian == custodian;
}