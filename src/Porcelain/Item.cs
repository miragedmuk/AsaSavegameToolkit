using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;
using System.Security.Principal;

namespace AsaSavegameToolkit.Porcelain;

/// <summary>
/// Represents an individual item in an ARK save file (weapon, resource, consumable, etc.).
/// </summary>
public class Item
{
    
    /// <summary>
    /// Unique object ID of this item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The blueprint class name (e.g., "PrimalItem_WeaponSword_C").
    /// </summary>
    public required string ClassName { get; set; }

    /// <summary>
    /// The stack quantity of this item.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// True if this item is a blueprint (not a craftable/usable copy).
    /// </summary>
    public bool IsBlueprint { get; set; }

    /// <summary>
    /// Remaining durability as a fraction from 0.0 to 1.0.
    /// </summary>
    public float? Durability { get; set; }

    /// <summary>
    /// Item quality from 0.0 (primitive) to higher values (ascendant).
    /// </summary>
    public float? ItemRating { get; set; }

    /// <summary>
    ///
    /// </summary>
    public ActorTransform Location { get; set; }

    internal static Item Create(GameObjectRecord record, ActorTransform? location = default)
    {
        var properties = record.Properties;

        var className = record.GetClassName();

        if (properties?.Count == 0)
        {
            return new Item

            {
                Id = record.Uuid,
                ClassName = className,
                Quantity = 1,
                IsBlueprint = false,
                Durability = 0,
                ItemRating = 0,
                Location = location ?? default
            };
        }

        uint itemId1 = 0;
        uint itemId2 = 0;
        double creationTime = 0;    
        int itemQuantity = 1;
        bool isBlueprint = false;
        bool isEngram = false;
        bool isInitialItem = false;
        bool isCustomRecipe = false;
        bool isFoodRecipe = false;
        bool allowRemovalFromInventory = false;
        float savedDurability = 0;
        float itemDurability = 0;
        float itemRating = 0;
        byte itemQualityIndex = 0;
        string crafterTribeName = "";
        string crafterCharacterName = "";
        float craftingSkill = 0;
        float craftingSkillBonus = 0;
        string customItemName = string.Empty;
        string customItemDescription = string.Empty;
        uint[] itemStatValues = new uint[8];
        int[] itemColors = new int[6];

        var itemIdProperties = (List<Property>?)properties.Get<StructProperty>("ItemID")?.Value;
        if (itemIdProperties != null)
        {
            itemId1 = itemIdProperties.Get<uint>("ItemID1");
            itemId2 = itemIdProperties.Get<uint>("ItemID2");
        }

        creationTime = properties.Get<double>("CreationTime");
        itemQuantity = properties.Get<int>("ItemQuantity");
        if (itemQuantity == 0)
            itemQuantity = 1;

        isBlueprint = properties.Get<bool>("bIsBlueprint");
        isEngram = properties.Get<bool>("bIsEngram");
        isInitialItem = properties.Get<bool>("bIsInitialItem");
        isCustomRecipe = properties.Get<bool>("bIsCustomRecipe");
        isFoodRecipe = properties.Get<bool>("bIsFoodRecipe");

        allowRemovalFromInventory = properties.Get<bool>("bAllowRemovalFromInventory");
        savedDurability = properties.Get<float>("SavedDurability");
        itemDurability = properties.Get<float>("ItemDurability");

        itemRating = properties.Get<float>("ItemRating");
        itemQualityIndex = properties.Get<byte>("ItemQualityIndex");

        crafterTribeName = properties.Get<string>("CrafterTribeName");
        crafterCharacterName = properties.Get<string>("CrafterCharacterName");
        craftingSkill = properties.Get<float>("CraftingSkill");
        craftingSkillBonus = properties.Get<float>("CraftedSkillBonus");

        customItemName = properties.Get<string>("CustomItemName")??"";
        customItemName = properties.Get<string>("CustomItemDescription")??"";


        //ItemStatValues //uint16
        for(int i = 0; i<itemStatValues.Length; i++)
        {
            itemStatValues[i] = properties.Get<uint>($"ItemStatValues",i);
        }

        //ItemColorID //int
        for(int i = 0; i<itemColors.Length; i++)
        {
            itemColors[i] = properties.Get<int>($"ItemColors",i);
        }

        //EggNumberOfLevelUpPointsApplied //byte
        byte[] eggStats = new byte[12];
        int eggLevel = 0;
        for(int i = 0; i < 12; i++)
        {
            eggStats[i] = properties.Get<byte>($"EggNumberOfLevelUpPointsApplied",i);
            eggLevel+= eggStats[i];
        }
        if(eggLevel > 0)
        {
            //we have a fertile egg
            eggLevel++;

            //EggNumberMutationsApplied //byte
            //EggColorSetIndices //byte
            //EggRandomMutationsFemale //int
            //EggRandomMutationsMale //int
            //EggGenderOverride //int
            //EggDinoAncestors
            //EggDinoAncestorsMale
            //EggDinoGeneTraits
        }



        return new Item

        {
            Id = record.Uuid,
            ClassName = className,
            Quantity = itemQuantity,
            IsBlueprint = isBlueprint,
            Durability = savedDurability,
            ItemRating = itemRating,
            Location = location ?? default
        };
    }

    internal static Item FromCryoSaddle(GameObjectRecord record)
    {
        var className = record.Properties.TryGet<ObjectReference>("ItemArchetype", out var archetype) ? archetype.Value : "Unknown Saddle";

        return new Item
        {
            Id = record.Uuid == default ? Guid.NewGuid() : record.Uuid,
            ClassName = className,
            Quantity = 1,
            IsBlueprint = false,
            Durability = record.Properties.TryGet<float>("ItemDurability", out var dur) ? dur : null,
            ItemRating = record.Properties.TryGet<float>("ItemRating", out var rating) ? rating : null,
            Location = default
        };
    }

    // TODO: expose crafting stats, custom name once property interpretation (2.2) is implemented.

    public override string ToString()
    {
        var bp = IsBlueprint ? " (BP)" : "";
        var qty = Quantity > 1 ? $" x{Quantity}" : "";
        return $"{ClassName}{bp}{qty}";
    }
}
