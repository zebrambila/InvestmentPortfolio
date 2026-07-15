using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;

namespace InvestmentPortfolio.Tests.Entities;

[TestClass]
public class DividendTests
{
    private static Dividend CreateDividend() =>
        new(Guid.NewGuid(), DateTime.UtcNow, 40.00m, ECustodian.BTG);

    [TestMethod]
    public void Dado_DividendCriado_Quando_Inspecionado_Entao_PropriedadesCorretas()
    {
        var assetId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var dividend = new Dividend(assetId, date, 40.00m, ECustodian.BTG);

        Assert.AreEqual(assetId, dividend.AssetId);
        Assert.AreEqual(date, dividend.ReceiptDate);
        Assert.AreEqual(40.00m, dividend.TotalFinancial);
        Assert.AreEqual(ECustodian.BTG, dividend.Custodian);
        Assert.IsFalse(dividend.IsDeleted);
    }

    [TestMethod]
    public void Dado_DividendExistente_Quando_UpdateChamado_Entao_DadosAlterados()
    {
        var dividend = CreateDividend();
        var before = dividend.UpdatedAt;
        var newDate = DateTime.UtcNow.AddDays(1);
        Thread.Sleep(10);

        dividend.Update(newDate, 80.00m, ECustodian.Itau);

        Assert.AreEqual(newDate, dividend.ReceiptDate);
        Assert.AreEqual(80.00m, dividend.TotalFinancial);
        Assert.AreEqual(ECustodian.Itau, dividend.Custodian);
        Assert.IsTrue(dividend.UpdatedAt > before);
    }

    [TestMethod]
    public void Dado_DividendExistente_Quando_DeleteChamado_Entao_ExclusaoLogicaAplicada()
    {
        var dividend = CreateDividend();
        var before = dividend.UpdatedAt;
        Thread.Sleep(10);

        dividend.Delete();

        Assert.IsTrue(dividend.IsDeleted);
        Assert.IsNotNull(dividend.DeletedAt);
        Assert.IsTrue(dividend.UpdatedAt > before);
    }
}
