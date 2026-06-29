namespace InvestmentPortfolio.Tests.Entities;

internal class ConcreteEntity : InvestmentPortfolio.Core.Entities.AuditableEntity
{
    public void PublicTouch() => Touch();
}

[TestClass]
public class AuditableEntityTests
{
    private ConcreteEntity CreateEntity() => new();

    [TestMethod]
    public void Dado_EntidadeCriada_Quando_Inspecionada_Entao_IdNaoEhGuidVazio()
    {
        var entity = CreateEntity();
        Assert.AreNotEqual(Guid.Empty, entity.Id);
    }

    [TestMethod]
    public void Dado_EntidadeCriada_Quando_Inspecionada_Entao_CreatedAtPreenchido()
    {
        var before = DateTime.Now.AddSeconds(-1);
        var entity = CreateEntity();
        var after = DateTime.Now.AddSeconds(1);

        Assert.IsTrue(entity.CreatedAt >= before && entity.CreatedAt <= after);
    }

    [TestMethod]
    public void Dado_EntidadeCriada_Quando_TouchChamado_Entao_UpdatedAtAlterado()
    {
        var entity = CreateEntity();
        var before = entity.UpdatedAt;

        Thread.Sleep(10);
        entity.PublicTouch();

        Assert.IsTrue(entity.UpdatedAt > before);
    }
}

