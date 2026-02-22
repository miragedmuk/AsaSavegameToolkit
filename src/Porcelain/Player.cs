using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;

namespace AsaSavegameToolkit.Porcelain;

/// <summary>
/// Represents a player character in an ARK save file.
/// Wraps a <see cref="GameObjectRecord"/> and exposes typed accessors for common player properties.
/// </summary>
/// <remarks>
/// Full player data (engrams, stats, ascensions) lives in .arkprofile files.
/// This class covers the in-world pawn record from the main .ark save.
/// </remarks>
public class Player
{
    /// <summary>
    /// Unique object ID of this player pawn.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The blueprint class name (e.g., "PlayerPawnTest_Male_C").
    /// </summary>
    public required string ClassName { get; set; }

    /// <summary>
    /// The player's in-game name.
    /// </summary>
    public string? PlayerName { get; set; }

    /// <summary>
    /// The tribe ID this player belongs to. 0 if not in a tribe.
    /// </summary>
    public int TribeId { get; set; }

    /// <summary>
    /// The platform-specific player data ID (Steam ID, etc.).
    /// </summary>
    public long? PlayerDataId { get; set; }

    /// <summary>
    /// Gets the current location represented as a three-dimensional vector, or null if the location is not set.
    /// </summary>
    public FVector? Location { get; private set; }

    /// <summary>
    /// Gets the rotation represented by a quaternion, if available.
    /// </summary>
    public FQuat? Rotation { get; private set; }

    /// <summary>
    /// The underlying raw record for direct property access.
    /// </summary>
    public GameObjectRecord? Record { get; set; }


    public override string ToString() => PlayerName ?? ClassName;

    /// <summary>
    /// Creates a new Player instance from a record.
    /// </summary>
    public static Player Create(GameObjectRecord record, ActorTransform? transform)
    {
        var player = new Player
        {
            Id = record.Uuid,
            ClassName = record.GetClassName(),
            PlayerName = record.Properties.Get<string>("PlayerName"),
            TribeId = record.Properties.Get<int>("TribeID"),
            PlayerDataId = record.Properties.TryGet<long>("PlayerDataID", out var value) ? value : null,
            Location = transform?.Location,
            Rotation = transform?.Rotation,
            Record = record
        };

        return player;
    }
}
