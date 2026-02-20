# Profile File Record

Profile files (.arkprofile) store player character data independently from the main save file. They are used for character uploads, downloads, and cross-server character transfers.

## File Format

Profile files have the extension `.arkprofile` and follow this naming pattern:
```
<EpicOnlineServicesID>.arkprofile
```

Example: `0011223344556677.arkprofile` (EOS ID in hex format)

## Binary Structure

The profile file structure is identical to profile entries in GameModeCustomBytes, but stored as a standalone file:

```
                                           [Profile Container]
00 00 00 00                                [Int32] Version: 0 (or flags)
02 00 00 00                                [Int32] ObjectCount: 2

                                           [Objects - repeated ObjectCount times]
                                           [AsaObject structures...]

                                           [Properties - for each object]
                                           [Property lists for each object above...]
```

## Typical Profile Objects

A profile file typically contains these objects:
1. **PrimalPlayerDataBP_C** - Main player character data
2. **Map reference** - Current or last map (e.g., "TheIsland_WP")

## Player Data Properties (Common)

The PrimalPlayerDataBP_C object contains properties such as:

### Identity
- **PlayerName** (StrProperty) - Character name
- **PlayerId** (UInt32Property) - Unique player identifier
- **UniqueID** (StructProperty[UniqueNetIdRepl]) - Network unique ID
- **SavedPlayerDataVersion** (IntProperty) - Data format version

### Character Stats
- **CharacterLevel** (UInt16Property) - Current level (1-180+)
- **CharacterExperience** (FloatProperty) - Current XP
- **TotalEngramPoints** (IntProperty) - Total engram points available
- **PlayerStatsConfig** (ArrayProperty[IntProperty]) - Stat point allocations per stat

### Appearance
- **MyPlayerCharacterConfig** (StructProperty) - Character customization:
  - **BodyColors** (ArrayProperty[StructProperty[LinearColor]]) - Body color regions
  - **RawBoneModifiers** (ArrayProperty[FloatProperty]) - Bone scale adjustments
  - **FacialHairIndex** (IntProperty) - Facial hair style
  - **HeadHairIndex** (IntProperty) - Head hair style
  - **HairColor** (StructProperty[LinearColor]) - Hair color
  - **SkinTone** (FloatProperty) - Skin tone value

### Progression
- **UnlockedEngrams** (ArrayProperty[ObjectProperty]) - Learned engrams (blueprints)
- **DefaultItemSlotClasses** (ArrayProperty[ObjectProperty]) - Default hotbar layout
- **PerMapExplorerNoteUnlocks** (MapProperty) - Explorer notes found per map
- **TamedDinoCount** (IntProperty) - Lifetime tamed dino count
- **DinoKillCount** (IntProperty) - Lifetime dino kills
- **PlayerKillCount** (IntProperty) - Lifetime player kills

### Spawn & Respawn
- **SpawnRegionIndex** (IntProperty) - Last spawn region
- **RespawnLocIndices** (ArrayProperty[IntProperty]) - Available respawn points
- **LastRespawnLocation** (StructProperty[Vector]) - Last death location

### Multiplayer
- **TribeName** (StrProperty) - Current tribe name (if in tribe)
- **TribeID** (IntProperty) - Current tribe ID
- **bIsServerAdmin** (BoolProperty) - Admin privileges

### Cosmetics & Items
- **UnlockedCosmetics** (ArrayProperty) - Cosmetic item unlocks (skins, emotes)
- **PerMapPlayerDataItems** (MapProperty) - Per-map stored items/data

### Achievements & Progression
- **AchievementUnlockTimes** (MapProperty) - Achievement unlock timestamps
- **PlayerStatTotals** (ArrayProperty[IntProperty]) - Lifetime stat totals
- **TotalTimePlayed** (FloatProperty) - Total playtime in seconds

## JSON Representation

```json
{
  "Path": "SavedArks/Players/0011223344556677.arkprofile",
  "Timestamp": "2026-02-10T12:34:56Z",
  "Profile": {
    "Objects": [
      {
        "ClassName": "PrimalPlayerDataBP_C",
        "Names": ["PrimalPlayerData_0011223344556677"],
        "Properties": [
          {"Name": "PlayerName", "Type": "StrProperty", "Value": "SurvivorBob"},
          {"Name": "CharacterLevel", "Type": "UInt16Property", "Value": 105},
          {"Name": "CharacterExperience", "Type": "FloatProperty", "Value": 1234567.5},
          {"Name": "TotalEngramPoints", "Type": "IntProperty", "Value": 3500},
          {
            "Name": "MyPlayerCharacterConfig",
            "Type": "StructProperty",
            "Value": {
              "BodyColors": [
                {"R": 0.8, "G": 0.6, "B": 0.4, "A": 1.0},
                {"R": 0.2, "G": 0.1, "B": 0.05, "A": 1.0},
                ...
              ],
              "FacialHairIndex": 3,
              "HeadHairIndex": 12
            }
          },
          {
            "Name": "UnlockedEngrams",
            "Type": "ArrayProperty",
            "Value": [
              "/Game/PrimalEarth/CoreBlueprints/Items/Structures/Stone/PrimalItemStructure_StoneWall.PrimalItemStructure_StoneWall",
              "/Game/PrimalEarth/CoreBlueprints/Items/Armor/Metal/PrimalItemArmor_MetalHelmet.PrimalItemArmor_MetalHelmet",
              ...
            ]
          },
          {"Name": "TamedDinoCount", "Type": "IntProperty", "Value": 47},
          {"Name": "DinoKillCount", "Type": "IntProperty", "Value": 1523},
          ...
        ]
      },
      {
        "ClassName": "TheIsland_WP",
        "Names": ["TheIsland_WP"],
        "Properties": []
      }
    ]
  }
}
```

## Character Customization Details

### BodyColors Array
The character has multiple coloreable regions (typically 6):
- Index 0: Skin tone
- Index 1: Hair color
- Index 2: Eye color
- Index 3: Tattoo/paint color 1
- Index 4: Tattoo/paint color 2
- Index 5: Tattoo/paint color 3

Each color is a LinearColor struct with R, G, B, A values (0.0 to 1.0).

### RawBoneModifiers
Bone modifiers adjust character proportions:
- Head size
- Upper face depth
- Lower face depth
- Hand size
- Upper arm length/thickness
- Lower arm length/thickness
- Torso height/width
- Leg length/thickness
- Foot size

Values typically range from -1.0 to 1.0.

## Usage Scenarios

### Character Upload
When a character is uploaded from a server:
1. Server creates a .arkprofile file with current character data
2. File is available for download by character owner
3. Character data remains on the server until deleted

### Character Download
When a player downloads a character to a new server:
1. Player selects downloaded .arkprofile file
2. Server reads and validates the character data
3. Character spawns on the new server
4. Original .arkprofile file may be consumed or retained

### Cross-Server Transfer
Characters can move between clusters:
1. Upload on Server A creates .arkprofile
2. Download on Server B imports the character
3. Character retains level, engrams, and appearance

## Reading Profile Files

To parse a profile file:

```csharp
// Open the file
using var stream = File.OpenRead("0011223344556677.arkprofile");
using var archive = new AsaArchive(stream, saveHeader);

// Read profile data
var profileFile = AsaProfileFile.Read(archive, filePath);

// Access profile properties
var playerName = profileFile.Profile.Properties
    .FirstOrDefault(p => p.Name == "PlayerName")?.Value;
var level = profileFile.Profile.Properties
    .FirstOrDefault(p => p.Name == "CharacterLevel")?.Value;
```

## Differences from GameModeCustomBytes

| Feature | .arkprofile File | GameModeCustomBytes |
|---------|------------------|---------------------|
| **Location** | Separate file | Embedded in save |
| **Purpose** | Upload/download | Current session |
| **Properties Offset** | Not used (false) | Not used for profiles |
| **Headers** | None | Has profile headers |
| **Multiple Profiles** | One per file | Multiple in record |
| **EOS ID** | In filename | In header entry |

## File Location

Profile files are typically stored in:
```
<SavedArksRoot>/Saved/SavedArks/<ServerName>/Players/<EOS_ID>.arkprofile
```

Or in upload/download directories:
```
<SavedArksRoot>/Saved/SavedArks/<ServerName>/PlayerUploads/<EOS_ID>.arkprofile
<SavedArksRoot>/Saved/SavedArks/<ServerName>/PlayerDownloads/<EOS_ID>.arkprofile
```

## Validation & Security

Profile files should be validated before import to prevent:
- **Level exploits** - Importing overpowered characters
- **Item duplication** - Cloning items/engrams
- **Stat manipulation** - Invalid stat allocations
- **Achievement fraud** - Unearned achievements
- **Cross-map exploits** - Bringing map-specific items

Servers typically:
1. Verify file format and structure
2. Check level and stat limits
3. Validate engram unlocks against level
4. Remove invalid or restricted items
5. Check for conflicts with existing characters
6. Log character imports for audit purposes

## Transfer Restrictions

Many servers limit what can transfer:
- **Max level cap** - Characters above certain level rejected
- **Item restrictions** - Some items stripped on transfer
- **Engram limits** - Map-specific engrams may be removed
- **Cosmetic restrictions** - Event items may not transfer
- **Cooldown periods** - Time limits between transfers

## Character Progression Tracking

Profile data includes detailed progression metrics:
- **Time played** - Total game time
- **Distance traveled** - Walk/swim/fly distances
- **Resources harvested** - Counts per resource type
- **Crafting stats** - Items crafted counters
- **Combat stats** - Kills, deaths, damage dealt/taken
- **Taming stats** - Dinos tamed, taming efficiency records

## Related Code

- [AsaProfileFile.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Records\AsaProfileFile.cs) - Profile file container
- [AsaProfile.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Objects\AsaProfile.cs) - Profile data parser
- [GameModeCustomBytes.md](./GameModeCustomBytes.md) - Related embedded format
