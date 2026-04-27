
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
    public long? DinoId { get; set; }

    /// <summary>
    /// True if this creature is tamed.
    /// </summary>
    public bool IsTamed { get; set; }

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

    public bool IsCryo { get; set; } = false;
    public bool IsNeutered { get; set; } = false;
    public float Scale { get; set; } = 1;




    // Tamed info
    public Inventory? Inventory { get; internal set; }

    public long FatherId { get; set; } = 0;
    public string FatherName { get; set; } = string.Empty;
    public long MotherId { get; set; } = 0;
    public string MotherName { get; set; } = string.Empty;
    public bool IsClaimed { get; set; } = true;
    
    /// <summary>
    /// The player-given tame name (e.g., "Fluffy"). Null if not tamed or unnamed.
    /// </summary>
    public string? TamedName { get; set; }

    public long TribeId { get; set; } = 0;
    /// <summary>
    /// The name of the tribe that owns this creature. Null if untamed.
    /// </summary>
    /// 

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

    public byte[] TamedStats { get; set; } = new byte[12];

    public byte[] TamedMutations { get; set; } = new byte[12];

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
    public static Creature Create(GameObjectRecord actor, ActorTransform? transform)
    {
        if (!actor.IsCreature())
            throw new AsaSavegameToolkit.Plumbing.AsaDataException($"Gameobject {actor.Uuid} is cannot be parsed as a Creature.");
      
        var properties = actor.Properties;

        //read base properties common for wild and tamed
        var dinoId = long.Parse(string.Concat(properties.Get<uint>("DinoID1", 0), properties.Get<uint>("DinoID2", 0)));       
        bool isInCryo = properties.Get<bool>("IsStored");
        var colorRegions = new byte[12];
        for(int i = 0; i < colorRegions.Length; i++)
        {
            colorRegions[i] = properties.Get<byte>($"ColorSetIndices", i);
        }
        List<string> geneTraits = new List<string>();
        var geneTraitsArray = properties.Get<ArrayProperty>("GeneTraits");
        if(geneTraitsArray!=null && geneTraitsArray.Value.Count > 0)
        {
            foreach(var geneTraitValue in geneTraitsArray.Value)
            {
                geneTraits.Add(geneTraitValue.ToString());
            }
        }
        bool isFemale = properties.Get<bool>("bIsFemale");
        bool isJuvenile = properties.Get<bool>("bBabyInitialized");
        bool isTameable = properties.Get<bool>("bForceDisablingTaming") == false;
        float babyAge = properties.Get<float>("BabyAge");
        int targetingTeam = properties.Get<int>("TargetingTeam");
        var originalCreationTime = properties.Get<double>("OriginalCreationTime");
        var wildScale = properties.Get<float>("WildRandomScale");

        //return wild
        if (!TeamInfo.IsTamed(targetingTeam))
        {
            return new CreatureWild
            {
                Id = actor.Uuid,
                ClassName = actor.GetClassName(),
                IsCryo = isInCryo,
                IsFemale = isFemale,
                BabyAge = babyAge,
                IsJuvenile = isJuvenile,
                DinoId = dinoId,
                ColorRegions = colorRegions,
                Scale = wildScale,
                TribeId = targetingTeam,
                Traits = geneTraits.ToArray(),
                Location = transform?.Location,
                Rotation = transform?.Rotation
            };
        }

        var tamedName = properties.Get<string>("TamedName");
        var tamedTimestamp = properties.Get<string>("TamedTimeStamp");
        var imprintNetId = properties.Get<string>("ImprinterPlayerUniqueNetId");
        var imprinterName = properties.Get<string>("ImprinterName");
        var uploadedFromServer = properties.Get<string>("UploadedFromServerName");
        var tamedOnServer = properties.Get<string>("TamedOnServerName");
        var imprintClassName = properties.Get<ObjectProperty>("BabyCuddleFood")?.Value?.Path;
        var lastTameConsumedFoodTime = properties.Get<double>("LastTameConsumedFoodTime");        
        var tamedAtTime = properties.Get<double>("TamedAtTime");
        var tamedAggressionLevel = properties.Get<int>("TamedAggressionLevel");
        var isMating = properties.Get<bool>("bEnableTamedMating");
        var isWandering = properties.Get<bool>("bEnableTamedWandering");
        var tribeName = properties.Get<string>("TribeName");
        var isNeutered = properties.Get<bool>("bNeutered");
        var isClone = properties.Get<bool>("bIsClone");

        var femaleAncestorsProperty = properties.Get<ArrayProperty>("DinoAncestors")?.Value;
        if(femaleAncestorsProperty != null)
        {
            for(int i = 0; i < femaleAncestorsProperty.Count; i++)
            {
                List<Property> propertyList = femaleAncestorsProperty[i] as List<Property>;
                var maleName = propertyList.Get<string>("MaleName");
                var maleId1 = propertyList.Get<uint>("MaleDinoID1");
                var maleId2 = propertyList.Get<uint>("MaleDinoID2");
                var femaleName = propertyList.Get<string>("FemaleName");
                var femaleId1 = propertyList.Get<uint>("FemaleDinoID1");
                var femaleId2 = propertyList.Get<uint>("FemaleDinoID2");

            }

        }
        var maleAncestorsProperty = properties.Get<ArrayProperty>("DinoAncestorsMale")?.Value;
        if (maleAncestorsProperty != null)
        {
            for (int i = 0; i < maleAncestorsProperty.Count; i++)
            {
                List<Property> propertyList = maleAncestorsProperty[i] as List<Property>;
                var maleName = propertyList.Get<string>("MaleName");
                var maleId1 = propertyList.Get<uint>("MaleDinoID1");
                var maleId2 = propertyList.Get<uint>("MaleDinoID2");
                var femaleName = propertyList.Get<string>("FemaleName");
                var femaleId1 = propertyList.Get<uint>("FemaleDinoID1");
                var femaleId2 = propertyList.Get<uint>("FemaleDinoID2");

            }
        }

        //tamed
        return new CreatureTamed
        {
            Id = actor.Uuid,
            ClassName = actor.GetClassName(),
            IsTamed=true,
            IsCryo = isInCryo,
            IsFemale = isFemale,
            BabyAge = babyAge,
            IsJuvenile = isJuvenile,
            DinoId = dinoId,
            ColorRegions = colorRegions,
            Scale = wildScale,
            TribeId = targetingTeam,
            Traits = geneTraits.ToArray(),
            ImprinterName = imprinterName,
            IsMating = isMating,
            IsNeutered = isNeutered,
            TamedName = tamedName,
            Location = transform?.Location,
            Rotation = transform?.Rotation
        };

    }

    internal void IngestInventory(Inventory inventory)
    {
        Inventory = inventory;
    }

    internal void IngestStatusRecord(GameObjectRecord statusComponent)
    {
        var properties = statusComponent.Properties;
     
        //wild
        var baseLevel = properties.Get<int>("BaseCharacterLevel");
        var extraLevels = properties.Get<int>("ExtraCharacterLevel");

        var wildLevels = 1;
        byte[] wildStats = new byte[12];
        for(int i = 0; i < wildStats.Length; i++)
        {
            wildStats[i] = properties.Get<byte>("NumberOfLevelUpPointsApplied", i);
            wildLevels += wildStats[i];
        }


        float[] currentStatusValues = new float[12];
        for(int i = 0; i < currentStatusValues.Length; i++)
        {
            currentStatusValues[i] = properties.Get<float>("CurrentStatusValues", i);
        }


        //tamed
        var randomMutationsMale = properties.Get<int>("RandomMutationsMale");
        var randomMutationsFemale = properties.Get<int>("RandomMutationsFemale");
        var imprintQuality = properties.Get<float>("DinoImprintQuality");
        var experiencePoints = properties.Get<float>("ExperiencePoints");

        var tameLevels = 0;
        byte[] tameStats = new byte[12];
        for (int i = 0; i < tameStats.Length; i++)
        {
            tameStats[i] = properties.Get<byte>("NumberOfLevelUpPointsAppliedTamed", i);
            tameLevels += tameStats[i];
        }

        var tameMutations = 0;
        byte[] mutationStats = new byte[12];
        for (int i = 0; i < mutationStats.Length; i++)
        {
            mutationStats[i] = properties.Get<byte>("NumberOfMutationsAppliedTamed", i);
            tameMutations += mutationStats[i];
        }

        TamedMutations = mutationStats;
        BaseLevel = baseLevel ;
        TotalLevel = wildLevels + tameLevels;
        MutationsFemale = randomMutationsFemale;
        MutationsMale = randomMutationsMale;
        TotalMutations = tameMutations + randomMutationsMale + randomMutationsFemale;
    }

}
