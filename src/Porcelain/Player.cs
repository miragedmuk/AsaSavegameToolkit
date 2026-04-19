using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;
using System.Diagnostics;

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

    public string? ChracterName { get; set;  }

    public long Level { get; set; } = 1;

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
    public static Player Create(GameObjectRecord profileRecord, GameObjectRecord gameRecord, ActorTransform? transform)
    {


        var myData = profileRecord.Properties.Get<StructProperty>("MyData").Value as List<Property>;
        var persistentConfigProperties = myData.Get<StructProperty>("MyPersistentCharacterStats").Value as List<Property>;
        var persistentStatusProperties = myData.Get<StructProperty>("MyPlayerCharacterConfig").Value as List<Property>;
        var linkedPlayerDataId = myData.Get<ulong>("PlayerDataID");
        string characterName = persistentStatusProperties.Get<string>("PlayerCharacterName");
        int playerLevel = 0;

        if (gameRecord == null)
        {

            for(int i = 0; i < 12; i++)
            {
                playerLevel += persistentStatusProperties.Get<byte>($"CharacterStatusComponent_NumberOfLevelUpPointsApplied",i);
            }
               
            var player = new Player
            {
                Id = profileRecord.Uuid,
                ClassName = profileRecord.ClassName.Name,
                PlayerName = myData.Get<string>("PlayerName"),
                ChracterName = characterName,
                TribeId = (int)linkedPlayerDataId,
                Level = playerLevel,
                PlayerDataId = (long)linkedPlayerDataId,
                Location = transform?.Location,
                Rotation = transform?.Rotation,
                Record = profileRecord
            };

            return player;
        }


        return new Player
        {
            Id = gameRecord.Uuid,
            ClassName = gameRecord.GetClassName(),
            PlayerName = gameRecord.Properties.Get<string>("PlatformProfileName"),
            ChracterName = gameRecord.Properties.Get<string>("PlayerName"),
            TribeId = gameRecord.Properties.Get<int>("TargetingTeam"),
            Level = gameRecord.GetFullLevel(),
            PlayerDataId = (long)linkedPlayerDataId,
            Location = transform?.Location,
            Rotation = transform?.Rotation,
            Record = profileRecord
        };

    }
}
