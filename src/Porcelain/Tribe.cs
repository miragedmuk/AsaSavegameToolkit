using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;

namespace AsaSavegameToolkit.Porcelain;

/// <summary>
/// Represents a tribe in an ARK save file.
/// Tribe records appear in both the main .ark save and .arktribe files.
/// </summary>
/// <remarks>
/// Full tribe data (members, admin list, logs) lives in .arktribe files.
/// This class covers the tribe record from the main .ark save.
/// </remarks>
public class Tribe
{
    public Tribe(GameObjectRecord record)
    {
        Record = record;
    }

    /// <summary>
    /// The underlying raw record for direct property access.
    /// </summary>
    public GameObjectRecord Record { get; }

    /// <summary>
    /// Unique object ID of this tribe record.
    /// </summary>
    public Guid Id => Record.Uuid;

    /// <summary>
    /// The tribe's display name.
    /// </summary>
    public string? TribeName => Record.Properties.Get<string>("TribeName");

    /// <summary>
    /// The numeric tribe ID.
    /// </summary>
    public int TribeId => Record.Properties.Get<int>("TribeID");

    /// <summary>
    /// Owner player data ID (tribe founder/owner).
    /// </summary>
    public long? OwnerPlayerDataId => Record.Properties.TryGet<long>("OwnerPlayerDataID", out var value) ? value : null;

    public override string ToString() => TribeName ?? Record.GetClassName();
}
