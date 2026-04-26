using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using AsaSavegameToolkit.Plumbing.Utilities;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

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


    public override string ToString() => PlayerName ?? string.Empty;

    /// <summary>
    /// Creates a new Player instance from a record.
    /// </summary>
    public static Player? Create(GameObjectRecord profileRecord, ActorTransform? transform)
    {
        //arkprofile data
        var myDataProperties = profileRecord.Properties.Get<StructProperty>("MyData")?.Value as List<Property>;
        if(myDataProperties == null)
        {           
            return null;
        }        
        
        var linkedPlayerDataId = myDataProperties.Get<ulong>("PlayerDataID");
        var playerName = myDataProperties.Get<string>("PlayerName");
        var tribeId = myDataProperties.Get<int>("TribeID");
        var uniqueNetIdProperty = (FUniqueNetIdRepl)myDataProperties.Get<StructProperty>("UniqueID").Value;
        var uniqueNetId = Convert.ToHexString(uniqueNetIdProperty.Id);
        var savedNetAddress = myDataProperties.Get<string>("SavedNetworkAddress");
        var loginTime = myDataProperties.Get<double>("LoginTime");
        var lastLoginTime = myDataProperties.Get<double>("LastLoginTime");
        var numOfDeaths = myDataProperties.Get<float>("NumOfDeaths");
        var spawnDayNumber = myDataProperties.Get<int>("SpawnDayNumber");
        var spawnDayTime = myDataProperties.Get<float>("SpawnDayTime");

        List<string> heardVoices = new List<string>();
        var voiceOvers = myDataProperties.Get<ArrayProperty>("HeardVoiceOvers")?.Value;
        if (voiceOvers != null)
        {
            foreach(FName voiceHeard in voiceOvers)
            {
                heardVoices.Add(voiceHeard.Name);
            }
        }


        var persistentConfigProperties = myDataProperties.Get<StructProperty>("MyPersistentCharacterStats")?.Value as List<Property>;
        if (persistentConfigProperties != null)
        {
            var experiencePoints = persistentConfigProperties.Get<float>("CharacterStatusComponent_ExperiencePoints");
            var totalEngramPoints = persistentConfigProperties.Get<int>("PlayerState_TotalEngramPoints");

            var headHairGrowth = persistentConfigProperties.Get<float>("PercentageOfHeadHairGrowth");
            var facialHairGrowth = persistentConfigProperties.Get<float>("PercentageOfFacialHairGrowth");
            var headHairStyle = persistentConfigProperties.Get<byte>("HeadHairIndex");
            var headCostmetic = persistentConfigProperties.Get<long>("HeadHairCustomCosmeticModID");
            var facialHairStyle = persistentConfigProperties.Get<byte>("FacialHairIndex");
            var facialCosmetic = persistentConfigProperties.Get<long>("FacialHairCustomCosmeticModID");
            var eyebrowStyle = persistentConfigProperties.Get<byte>("EyebrowIndex");
            var eyebrowCosmetic = persistentConfigProperties.Get<long>("EyebrowCustomCosmeticModID");

            var totalSkillPoints = persistentConfigProperties.Get<int>("PlayerState_TotaSkillPoints");
            var freeSkillPoints = persistentConfigProperties.Get<int>("PlayerState_FreeSkillPoints");


            byte[] playerStats = new byte[12];
            var levelsApplied = 0;
            for (int i = 0; i < playerStats.Length; i++)
            {
                playerStats[i] = persistentConfigProperties.Get<byte>("CharacterStatusComponent_NumberOfLevelUpPointsApplied", i);
                levelsApplied += playerStats[i];
            }

            List<string> learnedEngrams = new List<string>();
            var engramsUnlocked = persistentConfigProperties.Get<ArrayProperty>("PlayerState_EngramBlueprints")?.Value;
            if (engramsUnlocked != null)
            {
                foreach (ObjectReference engram in engramsUnlocked)
                {
                    learnedEngrams.Add(engram.Value);
                }
            }

            string[] equippedItems = new string[10];
            for (int i = 0; i < equippedItems.Length; i++)
            {
                var equippedItem = persistentConfigProperties.Get<ObjectProperty>("PlayerState_DefaultItemSlotClasses", i)?.Value?.ToString();
                equippedItems[i] = equippedItem != null ? equippedItem : string.Empty;
            }


            List<string> namedExplorerNotesFound = new List<string>();
            var namedExplorerNotesUnlocked = persistentConfigProperties.Get<ArrayProperty>("PerMapNamedExplorerNoteUnlocks")?.Value;
            if (namedExplorerNotesUnlocked != null)
            {
                foreach (FName namedNote in namedExplorerNotesUnlocked)
                {
                    namedExplorerNotesFound.Add(namedNote.Name);
                }
            }

            var skillUnlocksArray = persistentConfigProperties.Get<ArrayProperty>("SkillUnlocks")?.Value;
            var skillRanksArray = persistentConfigProperties.Get<ArrayProperty>("SkillRanks")?.Value;

            Dictionary<string, byte> unlockedSkills = new Dictionary<string, byte>(); //skill,rank
            if (skillUnlocksArray != null)
            {
                for (int x = 0; x < skillUnlocksArray.Count; x++)
                {
                    FName skillUnlock = (FName)skillUnlocksArray[x];
                    unlockedSkills.Add(skillUnlock.Name, (byte)skillRanksArray[x]);

                }
            }

            var currentMilestonesArray = persistentConfigProperties.Get<ArrayProperty>("CurrentMilestones")?.Value; //NameProperty
            var currentMilestonesProgressArray = persistentConfigProperties.Get<ArrayProperty>("MilestoneProgress")?.Value; //FloatProperty
            Dictionary<string, float> currentMilestones = new Dictionary<string, float>(); //milestone,progress
            if (currentMilestonesArray != null)
            {
                for (int x = 0; x < currentMilestonesArray.Count; x++)
                {
                    FName milesStone = (FName)currentMilestonesArray[x];
                    currentMilestones.Add(milesStone.Name, (float)currentMilestonesProgressArray[x]);
                }
            }

            List<string> completedMilestones = new List<string>();
            var milestonesCompleted = persistentConfigProperties.Get<ArrayProperty>("CompletedMilestones")?.Value;
            if (milestonesCompleted != null)
            {
                foreach (FName milestone in milestonesCompleted)
                {
                    completedMilestones.Add(milestone.Name);
                }
            }
        }

        var persistentStatusProperties = myDataProperties.Get<StructProperty>("MyPlayerCharacterConfig")?.Value as List<Property>;
        if (persistentStatusProperties != null)
        {
            string characterName = persistentStatusProperties.Get<string>("PlayerCharacterName");

            var headHairGrowth = persistentStatusProperties.Get<float>("PercentOfFullHeadHairGrowth");
            var headHairStyle = persistentStatusProperties.Get<byte>("HeadHairIndex");
            var headCostmetic = persistentStatusProperties.Get<long>("HeadHairCustomCosmeticModID");
            var facialHairStyle = persistentStatusProperties.Get<byte>("FacialHairIndex");
            var facialCosmetic = persistentStatusProperties.Get<long>("FacialHairCustomCosmeticModID");
            var eyebrowStyle = persistentStatusProperties.Get<byte>("EyebrowIndex");
            var eyebrowCosmetic = persistentStatusProperties.Get<long>("EyebrowCustomCosmeticModID");
            
            Color[] bodyColors = new Color[3];
            for(int i = 0; i< bodyColors.Length; i++)
            {
                var bodyColorStruct = (FLinearColor?)persistentStatusProperties.Get<StructProperty>("BodyColors", i)?.Value;
                if (bodyColorStruct != null)
                {
                    bodyColors[i] = Color.FromArgb(
                        (int)(bodyColorStruct.Value.A * 255),
                        (int)(bodyColorStruct.Value.R * 255),
                        (int)(bodyColorStruct.Value.G * 255),
                        (int)(bodyColorStruct.Value.B * 255));
                }
                else
                {
                    bodyColors[i] = Color.Empty;
                }
            }


        }












        return new Player
        {
            PlayerName = playerName,
            TribeId = (int)linkedPlayerDataId,
            PlayerDataId = (long)linkedPlayerDataId,
            Location = transform?.Location,
            Rotation = transform?.Rotation
        };

    }

    internal void IngestStatusRecord(GameObjectRecord statusComponent)
    {
        var playerLevel = 1;
        if (statusComponent != null)
        {
            for (int i = 0; i < 12; i++)
            {
                playerLevel += statusComponent.Properties.Get<byte>($"NumberOfLevelUpPointsApplied", i);
            }
        }
    }

    internal void IngestCharacterRecord(GameObjectRecord characterComponent)
    {

    }

}
