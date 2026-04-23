using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;

namespace AsaSavegameToolkit.Porcelain;

/// <summary>
/// Represents an inventory component attached to a creature, player, or structure.
/// </summary>
/// <remarks>
/// Inventories are separate <see cref="GameObjectRecord"/> objects linked to their
/// owner via ObjectProperty. Each item in the inventory is also a separate record.
/// </remarks>
public class Inventory
{
    /// <summary>
    /// The underlying raw record for direct property access.
    /// </summary>
    public GameObjectRecord? Record { get; set; }

    /// <summary>
    /// Unique object ID of this inventory.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The blueprint class name (e.g., "PrimalInventoryBP_C").
    /// </summary>
    public required string ClassName { get; set; }

    public Dictionary<Guid, Item> Items { get; } = [];

    public override string ToString() => ClassName;

    public static Inventory Create(GameObjectRecord record, IDictionary<Guid, Item> inventoryItems)
    {
        var inventory = new Inventory
        {
            Record = record,
            Id = record.Uuid,
            ClassName = record.ClassName.FullName
        };

        // Parse items from the inventory's "InventoryItems" property (array of structs)
        if (record.Properties.TryGet("InventoryItems", out ArrayProperty? itemsProp))
        {
            foreach (var itemStruct in itemsProp.Value.OfType<StructProperty>())
            {

                //var item = Item.FromStruct(itemStruct.Value);
                //inventory.Items[item.Id] = item;
            }
        }

        return inventory;
    }

}
