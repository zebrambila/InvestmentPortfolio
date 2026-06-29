using InvestmentPortfolio.Core.Entities;

namespace InvestmentPortfolio.Tests.Entities;

[TestClass]
public class DividendTests
{
    private static Dividend CreateDividend() =>
        new(Guid.NewGuid(), DateTime.UtcNow, 0.50m, 100m);

    [TestMethod]
    public void Dado_DividendCriado_Quando_Inspecionado_Entao_PropriedadesCorretas()
    {
        var assetId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var dividend = new Dividend(assetId, date, 0.50m, 100m);

        Assert.AreEqual(assetId, dividend.AssetId);
        Assert.AreEqual(date, dividend.ReceiptDate);
        Assert.AreEqual(0.50m, dividend.UnitValue);
        Assert.IsFalse(dividend.IsDeleted);
    }

    [TestMethod]
    public void Dado_DividendExistente_Quando_UpdateChamado_Entao_DadosAlterados()
    {
        var dividend = CreateDividend();
        var before = dividend.UpdatedAt;
        var newDate = DateTime.UtcNow.AddDays(1);
        Thread.Sleep(10);

        dividend.Update(newDate, 1.00m, 100m);

        Assert.AreEqual(newDate, dividend.ReceiptDate);
        Assert.AreEqual(1.00m, dividend.UnitValue);
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