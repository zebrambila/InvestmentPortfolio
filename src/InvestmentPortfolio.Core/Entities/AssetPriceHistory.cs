namespace InvestmentPortfolio.Core.Entities;

public class AssetPriceHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid AssetId { get; private set; }
    public Asset? Asset { get; private set; }
    public DateTime Date { get; private set; }
    public decimal ClosePrice { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.Now;

    private AssetPriceHistory() { }

    public AssetPriceHistory(Guid assetId, DateTime date, decimal closePrice)
    {
        AssetId = assetId;
        Date = date.Date;
        ClosePrice = closePrice;
    }

    public void UpdateClosePrice(decimal closePrice)
    {
        ClosePrice = closePrice;
    }
}
