using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Entities;

public class Asset : AuditableEntity
{
    [Required]
    [MaxLength(10)]
    public string Ticker { get; private set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; private set; } = string.Empty;

    [Required]
    public EAssetType Type { get; private set; }

    [Range(0, double.MaxValue)]
    public decimal CurrentPrice { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Asset() { }

    public Asset(string ticker, string name, EAssetType type)
    {
        Ticker = ticker;
        Name = name;
        Type = type;
    }

    public void Update(string ticker, string name, EAssetType type)
    {
        Ticker = ticker;
        Name = name;
        Type = type;
        Touch();
    }

    public void UpdateCurrentPrice(decimal price)
    {
        CurrentPrice = price;
        Touch();
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
        Touch();
    }
}