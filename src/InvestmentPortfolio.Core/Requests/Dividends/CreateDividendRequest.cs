using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Requests.Dividends;

public class CreateDividendRequest
{
    [Required(ErrorMessage = "O ativo é obrigatório")]
    public Guid AssetId { get; set; }

    [Required(ErrorMessage = "A data de recebimento é obrigatória")]
    public DateTime ReceiptDate { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "O valor total deve ser maior que zero")]
    public decimal TotalFinancial { get; set; }

    [Required(ErrorMessage = "A custodiante é obrigatória (BTG/Itau)")]
    public ECustodian Custodian { get; set; }
}