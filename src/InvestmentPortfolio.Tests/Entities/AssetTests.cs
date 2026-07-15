using InvestmentPortfolio.Core.Entities;
using InvestmentPortfolio.Core.Enums;
using InvestmentPortfolio.Core.Queries;

namespace InvestmentPortfolio.Tests.Entities;

[TestClass]
public class AssetTests
{
    private static Asset CreateAsset() => new("PETR4", "Petrobras", EAssetType.Ações);

    [TestMethod]
    public void Dado_AssetCriado_Quando_Inspecionado_Entao_PropriedadesCorretas()
    {
        var asset = CreateAsset();

        Assert.AreEqual("PETR4", asset.Ticker);
        Assert.AreEqual("Petrobras", asset.Name);
        Assert.AreEqual(EAssetType.Ações, asset.Type);
        Assert.IsFalse(asset.IsDeleted);
    }

    [TestMethod]
    public void Dado_AssetExistente_Quando_UpdateChamado_Entao_PropriedadesAlteradas()
    {
        var asset = CreateAsset();
        var before = asset.UpdatedAt;
        Thread.Sleep(10);

        asset.Update("VALE3", "Vale", EAssetType.Ações);

        Assert.AreEqual("VALE3", asset.Ticker);
        Assert.AreEqual("Vale", asset.Name);
        Assert.IsTrue(asset.UpdatedAt > before);
    }

    [TestMethod]
    public void Dado_AssetExistente_Quando_UpdateCurrentPriceChamado_Entao_PrecoAtualizado()
    {
        var asset = CreateAsset();
        var before = asset.UpdatedAt;
        Thread.Sleep(10);

        asset.UpdateCurrentPrice(35.50m);

        Assert.AreEqual(35.50m, asset.CurrentPrice);
        Assert.IsTrue(asset.UpdatedAt > before);
    }

    [TestMethod]
    public void Dado_AssetExistente_Quando_DeleteChamado_Entao_ExclusaoLogicaAplicada()
    {
        var asset = CreateAsset();
        var before = asset.UpdatedAt;
        Thread.Sleep(10);

        asset.Delete();

        Assert.IsTrue(asset.IsDeleted);
        Assert.IsNotNull(asset.DeletedAt);
        Assert.IsTrue(asset.UpdatedAt > before);
    }

    [TestMethod]
    public void Dado_TickerMinusculo_Quando_AssetCriado_Entao_TickerEmMaiusculas()
    {
        var asset = new Asset("petr4", "Petrobras", EAssetType.Ações);

        Assert.AreEqual("PETR4", asset.Ticker);
    }

    [TestMethod]
    public void Dado_TickerMisto_Quando_AssetCriado_Entao_TickerEmMaiusculas()
    {
        var asset = new Asset("Petr4", "Petrobras", EAssetType.Ações);

        Assert.AreEqual("PETR4", asset.Ticker);
    }

    [TestMethod]
    public void Dado_TickerMinusculo_Quando_UpdateChamado_Entao_TickerEmMaiusculas()
    {
        var asset = CreateAsset();

        asset.Update("vale3", "Vale", EAssetType.Ações);

        Assert.AreEqual("VALE3", asset.Ticker);
    }

    [TestMethod]
    public void Dado_TickerMisto_Quando_UpdateChamado_Entao_TickerEmMaiusculas()
    {
        var asset = CreateAsset();

        asset.Update("Vale3", "Vale", EAssetType.Ações);

        Assert.AreEqual("VALE3", asset.Ticker);
    }

    [TestMethod]
    public void Dado_AssetDeletado_Quando_IncluidoNaQuery_Entao_NaoRetornado()
    {
        var active = CreateAsset();
        var deleted = CreateAsset();
        deleted.Delete();

        var assets = new[] { active, deleted };
        var filter = AssetQueries.GetActive().Compile();
        var result = assets.Where(filter).ToList();

        Assert.HasCount(1, result);
        Assert.AreEqual(active.Id, result[0].Id);
    }
}