using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolio.Core.Requests.Assets;

public class UpdateAssetPriceRequest
{
    public Guid Id { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    public decimal? CurrentPrice { get; set; }
}