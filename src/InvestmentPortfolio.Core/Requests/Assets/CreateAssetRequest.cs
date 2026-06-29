using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Requests.Assets;

public class CreateAssetRequest
{
    [Required(ErrorMessage = "O ticker é obrigatório")]
    [MaxLength(10, ErrorMessage = "O ticker deve ter no máximo 10 caracteres")]
    public string Ticker { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "O tipo é obrigatório")]
    public EAssetType Type { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
    public decimal? Price { get; set; }
}