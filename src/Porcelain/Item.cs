using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;

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

        var itemIdProperties = (List<Property>?)properties.Get<StructProperty>("ItemID")?.Value;
        uint itemId1 = 0;
        uint itemId2 = 0;

        if (itemIdProperties != null)
        {
            itemId1 = itemIdProperties.Get<uint>("ItemID1");
            itemId2 = itemIdProperties.Get<uint>("ItemID2");
        }

        var creationTime = properties.Get<double>("CreationTime");
        int itemQuantity = properties.Get<int>("ItemQuantity");
        if (itemQuantity == 0)
            itemQuantity = 1;
        var isBlueprint = properties.Get<bool>("bIsBlueprint");
        var isEngram = properties.Get<bool>("bIsEngram");
        var isInitialItem = properties.Get<bool>("bIsInitialItem");
        var isCustomRecipe= properties.Get<bool>("bIsCustomRecipe");
        var isFoodRecipe = properties.Get<bool>("bIsFoodRecipe");


        var allowRemovalFromInventory = properties.Get<bool>("bAllowRemovalFromInventory");
        var savedDurability = properties.Get<float>("SavedDurability");
        var itemDurability = properties.Get<float>("ItemDurability");
       
        var itemRating = properties.Get<float>("ItemRating");
        var itemQualityIndex = properties.Get<byte>("ItemQualityIndex");

        var crafterTribeName = properties.Get<string>("CrafterTribeName");
        var crafterCharacterName = properties.Get<string>("CrafterCharacterName");
        var craftingSkill = properties.Get<float>("CraftingSkill");
        var craftingSkillBonus = properties.Get<float>("CraftedSkillBonus");

        uint[] itemStatValues = new uint[8]; //ItemStatValues //uint16
        int[] itemColors = new int[6]; //ItemColorID 

        //EggNumberOfLevelUpPointsApplied //byte
        //EggNumberMutationsApplied //byte
        //EggColorSetIndices //byte
        //EggRandomMutationsFemale //int
        //EggRandomMutationsMale //int
        //EggGenderOverride //int
        //EggDinoAncestors
        //EggDinoAncestorsMale
        //EggDinoGeneTraits

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
