using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolio.Core.Entities;

public class Dividend : AuditableEntity
{
    public Guid AssetId { get; private set; }
    public Asset? Asset { get; private set; }

    [Required]
    public DateTime ReceiptDate { get; private set; }

    [Range(0.0001, double.MaxValue)]
    public decimal UnitValue { get; private set; }

    public decimal PositionQuantity { get; private set; }
    public decimal TotalFinancial { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Dividend() { }

    public Dividend(Guid assetId, DateTime receiptDate, decimal unitValue, decimal positionQuantity)
    {
        AssetId = assetId;
        ReceiptDate = receiptDate;
        UnitValue = unitValue;
        PositionQuantity = positionQuantity;
        TotalFinancial = positionQuantity * unitValue;
    }

    public void Update(DateTime receiptDate, decimal unitValue, decimal positionQuantity)
    {
        ReceiptDate = receiptDate;
        UnitValue = unitValue;
        PositionQuantity = positionQuantity;
        TotalFinancial = positionQuantity * unitValue;
        Touch();
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
        Touch();
    }
}