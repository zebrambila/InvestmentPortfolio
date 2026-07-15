using System.ComponentModel.DataAnnotations;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Core.Requests.Transactions;

public class UpdateTransactionRequest
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O tipo de operação é obrigatório (Compra/Venda)")]
    public EOperationType Type { get; set; }

    [Range(0.001, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
    public decimal Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "O preço unitário deve ser maior que zero")]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "A data é obrigatória")]
    public DateTime DateBuySell { get; set; }

    [Required(ErrorMessage = "A custodiante é obrigatória (BTG/Itau)")]
    public ECustodian Custodian { get; set; }
}