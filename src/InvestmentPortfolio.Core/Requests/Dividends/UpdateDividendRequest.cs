using System.ComponentModel.DataAnnotations;

namespace InvestmentPortfolio.Core.Requests.Dividends;

public class UpdateDividendRequest
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "A data de recebimento é obrigatória")]
    public DateTime ReceiptDate { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "O valor unitário deve ser maior que zero")]
    public decimal UnitValue { get; set; }
}