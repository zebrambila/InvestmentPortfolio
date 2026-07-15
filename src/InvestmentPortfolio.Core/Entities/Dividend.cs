using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Entities;

public class Dividend : AuditableEntity
{
    public Guid AssetId { get; private set; }
    public Asset? Asset { get; private set; }

    [Required]
    public DateTime ReceiptDate { get; private set; }

    [Range(0.0001, double.MaxValue)]
    public decimal TotalFinancial { get; private set; }

    [Required]
    public ECustodian Custodian { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Dividend() { }

    public Dividend(Guid assetId, DateTime receiptDate, decimal totalFinancial, ECustodian custodian)
    {
        AssetId = assetId;
        ReceiptDate = receiptDate;
        TotalFinancial = totalFinancial;
        Custodian = custodian;
    }

    public void Update(DateTime receiptDate, decimal totalFinancial, ECustodian custodian)
    {
        ReceiptDate = receiptDate;
        TotalFinancial = totalFinancial;
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