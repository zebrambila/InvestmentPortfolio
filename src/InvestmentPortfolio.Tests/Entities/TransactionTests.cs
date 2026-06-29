using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Tests.Entities;

[TestClass]
public class TransactionTests
{
    private static Transaction CreateTransaction() =>
        new(Guid.NewGuid(), EOperationType.Compra, 100m, 35.00m, DateTime.UtcNow, ECustodian.BTG);

    [TestMethod]
    public void Dado_TransactionCriada_Quando_Inspecionada_Entao_TotalValueCalculado()
    {
        var transaction = CreateTransaction();

        Assert.AreEqual(3500.00m, transaction.TotalValue);
    }

    [TestMethod]
    public void Dado_TransactionExistente_Quando_UpdateChamado_Entao_TotalValueRecalculado()
    {
        var transaction = CreateTransaction();
        var before = transaction.UpdatedAt;
        Thread.Sleep(10);

        transaction.Update(EOperationType.Compra, 50m, 40.00m, DateTime.UtcNow, ECustodian.Itau);

        Assert.AreEqual(2000.00m, transaction.TotalValue);
        Assert.IsTrue(transaction.UpdatedAt > before);
    }

    [TestMethod]
    public void Dado_TransactionExistente_Quando_DeleteChamado_Entao_ExclusaoLogicaAplicada()
    {
        var transaction = CreateTransaction();
        var before = transaction.UpdatedAt;
        Thread.Sleep(10);

        transaction.Delete();

        Assert.IsTrue(transaction.IsDeleted);
        Assert.IsNotNull(transaction.DeletedAt);
        Assert.IsTrue(transaction.UpdatedAt > before);
    }
}