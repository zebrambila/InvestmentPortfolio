using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolio.Core.Requests.Dividends;

public class CreateDividendRequest
{
    [Required(ErrorMessage = "O ativo é obrigatório")]
    public Guid AssetId { get; set; }

    [Required(ErrorMessage = "A data de recebimento é obrigatória")]
    public DateTime ReceiptDate { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "O valor unitário deve ser maior que zero")]
    public decimal UnitValue { get; set; }
}