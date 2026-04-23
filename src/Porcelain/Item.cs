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
    /// The underlying raw record for direct property access.
    /// </summary>
    public GameObjectRecord? Record { get; set; }

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
        return new Item
        {
            Record = record,
            Id = record.Uuid,
            ClassName = record.GetClassName(),
            Quantity = record.Properties.TryGet<int>("ItemQuantity", out var qty) ? qty : 1,
            IsBlueprint = record.Properties.Get<bool>("bIsBlueprint"),
            Durability = record.Properties.TryGet<float>("SavedDurability", out var dur) ? dur : null,
            ItemRating = record.Properties.TryGet<float>("ItemRating", out var rating) ? rating : null,
            Location = location ?? default
        };
    }

    internal static Item FromCryoSaddle(GameObjectRecord record)
    {
        var className = record.Properties.TryGet<ObjectReference>("ItemArchetype", out var archetype) ? archetype.Value : "Unknown Saddle";

        return new Item
        {
            Record = record,
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
