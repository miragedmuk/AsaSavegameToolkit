using AsaSavegameToolkit.Plumbing.Readers;
using AsaSavegameToolkit.Plumbing.Utilities;
using AsaSavegameToolkit.Porcelain;
using AsaSavegameToolkit.Tests.Helpers;

namespace AsaSavegameToolkit.Tests.Porcelain;

[TestClass]
public class CreatureTests : SaveTests
{
    // Yutyrannus from game object record tests - confirmed present in both versions
    private const string YutyrannusId = "fe4b48ad-4d96-de0d-98e8-9d9b75e8413a";
    private const string RexId = "f2ad10f5-4317-3b46-36b2-afb639b20329";

    [TestMethod]
    public void IsCryoRelease_DataMatched()
    {

        var cryoSaveFile = Path.Combine(TestSettings.AssetsDirectory, "version_14/LostColony_WP_Cryo.ark");
        using var reader1 = new AsaSaveReader(cryoSaveFile, TestContext.GetLogger());
        var records1 = reader1.ReadGameRecords(TestContext.CancellationToken);


        var releaseSaveFile = Path.Combine(TestSettings.AssetsDirectory, "version_14/LostColony_WP_Free.ark");
        using var reader2 = new AsaSaveReader(releaseSaveFile, TestContext.GetLogger());
        var records2 = reader2.ReadGameRecords(TestContext.CancellationToken);

        var cryoCreatureRecord = records1[Guid.Parse(RexId)];
        Assert.IsNotNull(cryoCreatureRecord, "Cryo creature cannot be found.");
        var freeCreatureRecord = records2[Guid.Parse(RexId)];
        Assert.IsNotNull(freeCreatureRecord, "Free creature cannot be found.");
        
        var cryoCreature = Creature.Create(cryoCreatureRecord, transform: null, true);
        var freeCreature = Creature.Create(freeCreatureRecord, transform: null, false);

        Assert.IsTrue(cryoCreature.IsInCryo, "Cryo creature should have IsInCryo=true");
        Assert.IsFalse(freeCreature.IsInCryo, "Free creature should have IsInCryo=false");
        Assert.AreEqual(cryoCreature.DinoId, freeCreature.DinoId, "DinoId should match between cryo and free versions of the same creature");
        Assert.AreEqual(cryoCreature.ClassName, freeCreature.ClassName, "ClassName should match between cryo and free versions of the same creature");       
        Assert.AreEqual(cryoCreature.BaseLevel, freeCreature.BaseLevel, "BaseLevel should match between cryo and free versions of the same creature");
        Assert.AreEqual(cryoCreature.ColorRegions, freeCreature.ColorRegions, "Colors should match between cryo and free versions of the same creature");
    }

    [TestMethod]
    public void IsCreature_Version13_ReturnsTrue()
    {
        var saveFile = Path.Combine(TestSettings.AssetsDirectory, "version_13/TheIsland_WP.ark");
        using var reader = new AsaSaveReader(saveFile, TestContext.GetLogger());
        var record = reader.ReadGameRecord(Guid.Parse(YutyrannusId), TestContext.CancellationToken);

        Assert.IsTrue(record.IsCreature());
    }

    [TestMethod]
    public void IsCreature_Version14_ReturnsTrue()
    {
        var saveFile = Path.Combine(TestSettings.AssetsDirectory, "version_14/TheIsland_WP.ark");
        using var reader = new AsaSaveReader(saveFile, TestContext.GetLogger());
        var record = reader.ReadGameRecord(Guid.Parse(YutyrannusId), TestContext.CancellationToken);

        Assert.IsTrue(record.IsCreature());
    }

    [TestMethod]
    public void CanWrapCreature_Version13_ClassNameIsCorrect()
    {
        var saveFile = Path.Combine(TestSettings.AssetsDirectory, "version_13/TheIsland_WP.ark");
        using var reader = new AsaSaveReader(saveFile, TestContext.GetLogger());
        var record = reader.ReadGameRecord(Guid.Parse(YutyrannusId), TestContext.CancellationToken);

        var creature = Creature.Create(record, transform: null);

        Assert.Contains("Yutyrannus", creature.ClassName);
    }

    [TestMethod]
    public void CanWrapCreature_Version14_ClassNameIsCorrect()
    {
        var saveFile = Path.Combine(TestSettings.AssetsDirectory, "version_14/TheIsland_WP.ark");
        using var reader = new AsaSaveReader(saveFile, TestContext.GetLogger());
        var record = reader.ReadGameRecord(Guid.Parse(YutyrannusId), TestContext.CancellationToken);

        var creature = Creature.Create(record, transform: null);

        Assert.Contains("Yutyrannus", creature.ClassName);
    }

    [TestMethod]
    public void CreatureProperties_Version13_DoNotThrow()
    {
        var saveFile = Path.Combine(TestSettings.AssetsDirectory, "version_13/TheIsland_WP.ark");
        using var reader = new AsaSaveReader(saveFile, TestContext.GetLogger());
        var record = reader.ReadGameRecord(Guid.Parse(YutyrannusId), TestContext.CancellationToken);

        var creature = Creature.Create(record, transform: null);

        // Validate all accessors return without throwing
        _ = creature.Id;
        _ = creature.ClassName;
        _ = creature.IsTamed;
        _ = creature.IsFemale;
        _ = creature.TamedName;
        _ = creature.TribeName;
        _ = creature.TamerString;
        _ = creature.BaseLevel;
        _ = creature.TotalLevel;
        _ = creature.MutationsMale;
        _ = creature.MutationsFemale;
        _ = creature.TotalMutations;
        _ = creature.ImprintQuality;
        _ = creature.ImprinterName;
        _ = creature.BabyAge;
        _ = creature.IsJuvenile;
        _ = creature.TamedAtTime;
        _ = creature.DinoId;
        _ = creature.ColorRegions;
        _ = creature.ToString();
    }

    [TestMethod]
    public void CreatureProperties_Version14_DoNotThrow()
    {
        var saveFile = Path.Combine(TestSettings.AssetsDirectory, "version_14/TheIsland_WP.ark");
        using var reader = new AsaSaveReader(saveFile, TestContext.GetLogger());
        var record = reader.ReadGameRecord(Guid.Parse(YutyrannusId), TestContext.CancellationToken);

        var creature = Creature.Create(record, transform: null);

        _ = creature.Id;
        _ = creature.ClassName;
        _ = creature.IsTamed;
        _ = creature.IsFemale;
        _ = creature.TamedName;
        _ = creature.TribeName;
        _ = creature.TamerString;
        _ = creature.BaseLevel;
        _ = creature.TotalLevel;
        _ = creature.MutationsMale;
        _ = creature.MutationsFemale;
        _ = creature.TotalMutations;
        _ = creature.ImprintQuality;
        _ = creature.ImprinterName;
        _ = creature.BabyAge;
        _ = creature.IsJuvenile;
        _ = creature.TamedAtTime;
        _ = creature.DinoId;
        _ = creature.ColorRegions;
        _ = creature.ToString();
    }

    [TestMethod]
    public void AsaSaveGame_IncludesCryoCreatures()
    {
        var saveFile = Path.Combine(TestSettings.AssetsDirectory, "version_13/TheIsland_WP.ark");
        var saveGame = AsaSaveGame.ReadFrom(saveFile, TestContext.GetLogger(), cancellationToken: TestContext.CancellationToken);

        var cryoCreatures = saveGame.CryopoddedCreatures;
        TestContext.WriteLine($"Found {cryoCreatures.Count} cryo creatures in AsaSaveGame.CryopoddedCreatures");

        Assert.IsNotEmpty(cryoCreatures, "Expected extracted cryo creatures to be surfaced in AsaSaveGame.CryopoddedCreatures");

        foreach (var c in cryoCreatures.Values)
        {
            TestContext.WriteLine($"  {c.ClassName} '{c.TamedName}' lv={c.TotalLevel} tribe={c.TribeName} isInCryo={c.IsInCryo}");
            Assert.IsTrue(c.IsInCryo, $"Cryo creature {c.Id} should have IsInCryo=true");
        }
    }
}
