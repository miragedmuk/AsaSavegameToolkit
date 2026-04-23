using AsaSavegameToolkit.Plumbing;
using AsaSavegameToolkit.Plumbing.Properties;
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
    /// <summary>
    /// Unique object ID of this tribe record.
    /// </summary>
    public Guid Id { get; set; }
      
    /// <summary>
    /// The tribe's display name.
    /// </summary>
    public string? TribeName { get; set; }

    /// <summary>
    /// The numeric tribe ID.
    /// </summary>
    public int TribeId { get; set; }

    /// <summary>
    /// Owner player data ID (tribe founder/owner).
    /// </summary>
    public long? OwnerPlayerDataId{ get; set; }

    public string[] MemberNames { get; set; } = [];
    public string[] MemberIds { get; set; } = [];
    public string[] LogLines { get; set; } = [];
    
    public override string ToString() => TribeName;


    /// <summary>
    /// Creates a new Tribe instance from a record.
    /// </summary>
    internal static Tribe? Create(GameObjectRecord tribeRecord)
    {
        var properties = tribeRecord.Properties.Get<StructProperty>("TribeData").Value as List<Property>;
        var tribeId = properties.Get<int>("TribeID");
        var tribeName = properties.Get<string>("TribeName");
        var ownerPlayerId = properties.Get<uint>("OwnerPlayerDataId");

        var memberNamesProperty = properties.Get<ArrayProperty>("MembersPlayerName");
        var nameCount = memberNamesProperty.Value.Count;
        var memberNames = new string[nameCount];
        for(int x = 0; x < nameCount; x++)
        {
            memberNames[x] = memberNamesProperty.Value[x].ToString();
        }

        var memberIdsProperty = properties.Get<ArrayProperty>("MembersPlayerDataID");
        var idCount = memberNamesProperty.Value.Count;
        var memberIds = new string[idCount];
        for (int x = 0; x < idCount; x++)
        {
            memberIds[x] = memberIdsProperty.Value[x].ToString();
        }

        var tribeLogProperty = properties.Get<ArrayProperty>("TribeLog");
        var lineCount = tribeLogProperty.Value.Count;
        var tribeLogLines = new string[lineCount];
        for (int x = 0; x < lineCount; x++)
        {
            tribeLogLines[x] = tribeLogProperty.Value[x].ToString();
        }
        var logIndex = properties.Get<int>("LogIndex");
        

        return new Tribe
        {
            Id = tribeRecord.Uuid,
            TribeId = tribeId,
            TribeName = tribeName,
            OwnerPlayerDataId = ownerPlayerId,
            MemberIds = memberIds,
            MemberNames = memberNames,
            LogLines = tribeLogLines


        };

    }

}
