
using System.Collections;
using System.Transactions;
using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;

namespace AsaSavegameToolkit.Porcelain;

/// <summary>
/// Represents a creature (dinosaur or other animal) in an ARK save file.
/// Wraps a <see cref="GameObjectRecord"/> and exposes typed accessors for common creature properties.
/// </summary>
public class Creature
{
    /// <summary>
    /// Unique object ID of this creature.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The blueprint class name (e.g., "Yutyrannus_Character_BP_C").
    /// </summary>
    public required string ClassName { get; set; }

    /// <summary>
    /// Unique dino identifier. 
    /// forms the globally-unique dino ID used in-game.
    /// </summary>
    public uint? DinoId { get; set; }

    /// <summary>
    /// True if this creature is tamed.
    /// </summary>
    public bool IsTamed { get; set; }

    /// <summary>
    /// True if this creature is stored in a cryopod.
    /// </summary>
    public bool IsInCryo { get; set; }

    /// <summary>
    /// True if this creature is female.
    /// </summary>
    public bool IsFemale { get; set; }

    /// <summary>
    /// The base character level (wild levels allocated). Null if not set.
    /// Note: tamed bonus levels are separate - see <see cref="ExtraLevel"/>.
    /// </summary>
    public int? BaseLevel { get; set; }

    /// <summary>
    /// Total displayed level (BaseLevel + ExtraLevel).
    /// </summary>
    public int? TotalLevel { get; set; }

    /// <summary>
    /// Number of mutations from the male lineage.
    /// </summary>
    public int MutationsMale { get; set; }

    /// <summary>
    /// Number of mutations from the female lineage.
    /// </summary>
    public int MutationsFemale { get; set; }

    /// <summary>
    /// Total mutation count (male + female lineage).
    /// </summary>
    public int TotalMutations { get; set; }

    /// <summary>
    /// Baby/juvenile age from 0.0 (newborn) to 1.0 (fully grown).
    /// Null if the creature is not a juvenile.
    /// </summary>
    public float? BabyAge { get; set; }

    /// <summary>
    /// True if this creature is still in the juvenile growth phase.
    /// </summary>
    public bool IsJuvenile { get; set; }

    public string[]? Traits { get; set; } = [];

    public string[]? Rigs { get; set; } = [];


    /// <summary>
    /// Color region indices (up to 6 regions, indexed 0-5).
    /// Each value is a color ID (0 = default/no color).
    /// Corresponds to ArrayProperty(ByteProperty) "ColorSetIndices".
    /// </summary>
    public byte[] ColorRegions { get; set; } = new byte[6] { 0, 0, 0, 0, 0, 0 };

    public byte[] WildStats { get; set; } = new byte[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public float?[] StatsCurrent { get; set; } = [];
    public string[]? ProductionResources { get; set; }

    public FVector? Location { get; private set; }

    public FQuat? Rotation { get; private set; }

    public Inventory? Inventory { get; internal set; }




    // Tamed info

    public long FatherId { get; set; } = 0;
    public string FatherName { get; set; } = string.Empty;
    public long MotherId { get; set; } = 0;
    public string MotherName { get; set; } = string.Empty;
    public bool IsClaimed { get; set; } = true;
    public bool IsCryo { get; set; } = false;




    /// <summary>
    /// The player-given tame name (e.g., "Fluffy"). Null if not tamed or unnamed.
    /// </summary>
    public string? TamedName { get; set; }

    public long TribeId { get; set; } = 0;
    /// <summary>
    /// The name of the tribe that owns this creature. Null if untamed.
    /// </summary>
    public string? TribeName { get; set; }

    /// <summary>
    /// The player who last tamed this creature (player identifier string).
    /// </summary>
    public string? TamerString { get; set; }


    /// <summary>
    /// Imprint quality from 0.0 (no imprint) to 1.0 (100% imprint).
    /// </summary>
    public float ImprintQuality { get; set; }

    /// <summary>
    /// The player name who imprinted this creature.
    /// </summary>
    public string? ImprinterName { get; set; }

    public bool IsWandering { get; set; } = false;
    public bool IsMating { get; set; } = false;
    public float ExperiencePoints { get; set; } = 0;

    /// <summary>
    /// Server time (seconds since epoch) when this creature was tamed. Null if not tamed.
    /// </summary>
    public double? TamedAtTime { get; set; }
    public double? LastAllyInRangeTime { get; set; }

    public byte?[] TamedStats { get; set; } = [];

    public byte?[] TamedMutations { get; set; } = [];

    public int RandomMutationsMale { get; set; } = 0;
    public int RandomMutationsFemale { get; set; } = 0;

    public string TamedServer { get; set; } = string.Empty;
    public string UploadedServer { get; set; } = string.Empty;  

    public double? UploadedTime { get; set; }

    public override string ToString()
    {
        var name = TamedName ?? ClassName;
        var tribe = TribeName != null ? $" [{TribeName}]" : "";
        var level = TotalLevel.HasValue ? $" Lv{TotalLevel}" : "";
        return $"{name}{level}{tribe}";
    }

    /// <summary>
    /// Creates a new Creature instance from a record and transform.
    /// </summary>
    public static Creature Create(GameObjectRecord actor, ActorTransform? transform, bool isInCryo = false)
    {
        var properties = actor.Properties;

        var dinoId = long.Parse(string.Concat(properties.Get<uint>("DinoID1", 0), properties.Get<uint>("DinoID2", 0)));

        var mutationsMale = properties.Get<int>("RandomMutationsMale");
        var mutationsFemale = properties.Get<int>("RandomMutationsFemale");
        var baseLevel = actor.GetBaseLevel();
        var extraCharacterLevel = properties.Get<int>("ExtraCharacterLevel");
        var colorRegions = new byte?[12];
        if (properties.TryGet<IList>("ColorSetIndices", out var arrayElements))
        {
            foreach(var element in arrayElements.OfType<ByteProperty>())
            {
                colorRegions[element.Tag.ArrayIndex] = element.Value;
            }
        }


        var creature = new Creature
        {
            Id = actor.Uuid,
            ClassName = actor.GetClassName(),
            IsInCryo = isInCryo,
            TamedName = properties.Get<string>("TamedName"),
            TribeName = properties.Get<string>("TribeName"),
            TamerString = properties.Get<string>("TamerString"),
            IsTamed = actor.IsTamed(),
            IsFemale = properties.Get<bool>("bIsFemale"),
            BaseLevel = baseLevel,
            TotalLevel = actor.GetFullLevel(),
            MutationsMale = mutationsMale,
            MutationsFemale = mutationsFemale,
            TotalMutations = mutationsMale + mutationsFemale,
            ImprintQuality = properties.Get<float>("ImprintQuality"),
            ImprinterName = properties.Get<string>("ImprinterName"),
            BabyAge = properties.TryGet<float>("BabyAge", out var babyAge) ? babyAge : null,
            IsJuvenile = properties.Get<bool>("bBabyInitialized"),
            TamedAtTime = properties.TryGet<double>("TamedAtTime", out var tamedAtTime) ? tamedAtTime : null,
            DinoId = properties.TryGet<uint>("DinoID1", out var dinoId1) ? dinoId1 : null,
            ColorRegions = [],
            Location = transform?.Location,
            Rotation = transform?.Rotation
        };

        var statusComponent = actor.GetCharacterStatusComponent();
        if (statusComponent != null)
        {
            creature.IngestStatusRecord(statusComponent);
        }

        return creature;
    }

    private void IngestInventoryRecord(GameObjectRecord inventoryComponent)
    {
        throw new NotImplementedException();
    }

    private static T?[] GetStatValues<T>(GameObjectRecord statusComponent, string propertyName, int count) where T : struct
    {
        var levels = new T?[count];
        for (var i = 0; i < count; i++)
        {
            if (statusComponent.Properties.TryGet<T>(propertyName, i, out var level))
            {
                levels[i] = level;
            }
        }
        return levels;
    }

    internal void IngestStatusRecord(GameObjectRecord statusComponent)
    {


    }

    internal void IngestInventoryRecord(GameObjectRecord inventoryComponent, IDictionary<Guid, Item> inventoryItems)
    {
        Inventory = Inventory.Create(inventoryComponent, inventoryItems);
    }
}
