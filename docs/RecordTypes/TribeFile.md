# Tribe File Record

Tribe files (.arktribe) store tribe data independently from the main save file. They are used for tribe uploads, downloads, and cross-server tribe transfers.

## File Format

Tribe files have the extension `.arktribe` and follow this naming pattern:
```
<TribeID>.arktribe
```

Example: `1234567890.arktribe`

## Binary Structure

The tribe file structure is identical to tribe entries in GameModeCustomBytes, but stored as a standalone file:

```
                                           [Tribe Container]
00 00 00 00                                [Int32] Version: 0 (or flags)
05 00 00 00                                [Int32] ObjectCount: 5

                                           [Objects - repeated ObjectCount times]
                                           [AsaObject structures...]

                                           [Properties - for each object]
                                           [Property lists for each object above...]
```

## Typical Tribe Objects

A tribe file typically contains these objects:
1. **PrimalTribeData** - Main tribe information
2. **TestGameMode_C** (or similar) - Game mode reference
3. **PersistentLevel** - Level reference
4. **Map name** - Current map reference (e.g., "TheIsland_WP")
5. **Asset paths** - Various game asset references

## Tribe Data Properties (Common)

The PrimalTribeData object contains properties such as:

### Identity & Ownership
- **TribeName** (StrProperty) - Name of the tribe
- **OwnerPlayerDataID** (UInt32Property) - Tribe owner's player ID
- **TribeID** (IntProperty) - Unique tribe identifier

### Membership
- **MembersPlayerDataID** (ArrayProperty[UInt32]) - All tribe member player IDs
- **TribeAdmins** (ArrayProperty[UInt32]) - Admin member player IDs
- **MembersPlayerName** (ArrayProperty[StrProperty]) - Member names
- **MembersConfigs** (ArrayProperty[StructProperty]) - Member configurations

### Governance
- **SetGovernmentDinoOwnership** (BoolProperty) - Enable shared dino ownership
- **TribeGovernPINCode** (IntProperty) - PIN code for governance
- **NumTribeDinos** (IntProperty) - Count of tribe-owned dinos
- **bSetGovernment** (BoolProperty) - Governance enabled flag

### Activity & Logs
- **TribeLog** (ArrayProperty[StrProperty]) - Tribe log entries
- **LogIndex** (IntProperty) - Current log index

### Assets & Ownership
- **TribeData** (ArrayProperty[ByteProperty]) - Binary tribe data
- **TribeDinoCharacterIDs** (SetProperty[ObjectProperty]) - Owned dino GUIDs
- **TribeStructureIDs** (ArrayProperty[IntProperty]) - Owned structure IDs
- **PlayerRankGroups** (MapProperty) - Rank/permission groups

### Timestamps
- **LastUploadTime** (DoubleProperty) - Time of last upload
- **LastDownloadTime** (DoubleProperty) - Time of last download

## JSON Representation

```json
{
  "Path": "SavedArks/Tribes/1234567890.arktribe",
  "Timestamp": "2026-02-10T12:34:56Z",
  "Tribe": {
    "Objects": [
      {
        "ClassName": "PrimalTribeData",
        "Names": ["PrimalTribeData_1234567890"],
        "Properties": [
          {"Name": "TribeName", "Type": "StrProperty", "Value": "Alpha Tribe"},
          {"Name": "OwnerPlayerDataID", "Type": "UInt32Property", "Value": 987654321},
          {"Name": "TribeID", "Type": "IntProperty", "Value": 1234567890},
          {"Name": "MembersPlayerDataID", "Type": "ArrayProperty", "Value": [987654321, 123456789]},
          {"Name": "TribeLog", "Type": "ArrayProperty", "Value": [
            "Day 1, 12:00: Bob joined the tribe",
            "Day 2, 14:30: Alice was promoted to admin",
            "Day 3, 08:15: Tamed a Level 150 Rex"
          ]},
          ...
        ]
      },
      {
        "ClassName": "TestGameMode_C",
        "Names": ["TestGameMode_1234567876"],
        "Properties": []
      },
      {
        "ClassName": "PersistentLevel",
        "Names": ["PersistentLevel"],
        "Properties": []
      },
      {
        "ClassName": "TheIsland_WP",
        "Names": ["TheIsland_WP"],
        "Properties": []
      },
      {
        "ClassName": "/Game/Maps/TheIslandSubMaps/TheIsland",
        "Names": ["/Game/Maps/TheIslandSubMaps/TheIsland"],
        "Properties": []
      }
    ]
  }
}
```

## Usage Scenarios

### Tribe Upload
When a tribe is uploaded from a server:
1. Server creates a .arktribe file with current tribe data
2. File is available for download by tribe owner
3. Uploaded tribe remains on the server until deleted

### Tribe Download
When a tribe owner downloads a tribe to a new server:
1. Player selects downloaded .arktribe file
2. Server reads and validates the tribe data
3. Tribe is recreated on the new server with new TribeID
4. Original .arktribe file may be consumed or retained

### Cross-Server Transfer
Tribes can move between clusters:
1. Upload on Server A creates .arktribe
2. Download on Server B imports the tribe
3. Members can join the tribe on the new server

## Reading Tribe Files

To parse a tribe file:

```csharp
// Open the file
using var stream = File.OpenRead("1234567890.arktribe");
using var archive = new AsaArchive(stream, saveHeader);

// Read tribe data
bool usePropertiesOffset = false; // Standalone files use false
var tribeFile = AsaTribeFile.Read(archive, filePath, usePropertiesOffset);

// Access tribe properties
var tribeName = tribeFile.Tribe.Properties
    .FirstOrDefault(p => p.Name == "TribeName")?.Value;
```

## Differences from GameModeCustomBytes

| Feature | .arktribe File | GameModeCustomBytes |
|---------|---------------|---------------------|
| **Location** | Separate file | Embedded in save |
| **Purpose** | Upload/download | Current session |
| **Properties Offset** | Not used (false) | May be used (varies) |
| **Headers** | None | Has tribe headers |
| **Multiple Tribes** | One per file | Multiple in record |

## File Location

Tribe files are typically stored in:
```
<SavedArksRoot>/Saved/SavedArks/<ServerName>/Tribes/<TribeID>.arktribe
```

Or in upload/download directories:
```
<SavedArksRoot>/Saved/SavedArks/<ServerName>/TribeUploads/<TribeID>.arktribe
<SavedArksRoot>/Saved/SavedArks/<ServerName>/TribeDownloads/<TribeID>.arktribe
```

## Validation & Security

Tribe files should be validated before import to prevent:
- **Duplication exploits** - Cloning tribe assets
- **Data corruption** - Invalid property values
- **Permission escalation** - Unauthorized admin access
- **Asset injection** - Adding items/dinos not earned

Servers typically:
1. Verify file format and structure
2. Check digital signatures (if implemented)
3. Validate property values against server limits
4. Check for conflicts with existing tribes
5. Log tribe imports for audit purposes

## Related Code

- [AsaTribeFile.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Records\AsaTribeFile.cs) - Tribe file container
- [AsaTribe.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Objects\AsaTribe.cs) - Tribe data parser
- [GameModeCustomBytes.md](./GameModeCustomBytes.md) - Related embedded format
