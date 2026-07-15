using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Responses;

public class AssetPositionResponse
{
    public Guid AssetId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public EAssetType Type { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PositionQuantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal TotalInvested { get; set; }
    public decimal CurrentTotalValue { get; set; }
}
