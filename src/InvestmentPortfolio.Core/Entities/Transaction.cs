using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Entities;

public class Transaction : AuditableEntity
{
    public Guid AssetId { get; private set; }
    public Asset? Asset { get; private set; }

    [Required]
    public EOperationType Type { get; private set; }

    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; private set; }

    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; private set; }

    public decimal TotalValue { get; private set; }

    [Required]
    public DateTime DateBuyorSell { get; private set; }

    [Required]
    public ECustodian Custodian { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Transaction() { }

    public Transaction(Guid assetId, EOperationType type, decimal quantity, decimal unitPrice, DateTime date, ECustodian custodian)
    {
        AssetId = assetId;
        Type = type;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalValue = quantity * unitPrice;
        DateBuyorSell = date;
        Custodian = custodian;
    }

    public void Update(EOperationType type, decimal quantity, decimal unitPrice, DateTime date, ECustodian custodian)
    {
        Type = type;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalValue = quantity * unitPrice;
        DateBuyorSell = date;
        Custodian = custodian;
        Touch();
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
        Touch();
    }
}