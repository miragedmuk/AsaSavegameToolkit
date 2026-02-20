# Save Header Record

[SaveHeader_0.bin](../parsed-maps/Ragnarok_WP/custom/SaveHeader_0.bin)
```
       00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
00000: 0E 00 0A 02 00 00 F5 03 00 00 60 46 02 00 7C BB
00010: 39 E0 9B D7 97 41 17 00 00 00 94 00 00 00 0C 00
00020: 00 00 52 61 67 6E 61 72 6F 6B 5F 57 50 00 FF FF
00030: FF FF 29 00 00 00 52 61 67 6E 61 72 6F 6B 5F 57
00040: 50 5F 4D 61 69 6E 47 72 69 64 5F 4C 36 5F 58 30
00050: 5F 59 30 5F 44 4C 35 34 42 30 46 41 34 42 00 FF
00060: FF FF FF...
```

The save header is the first record in every ARK save file and contains metadata about the save file format, game time, and critical lookup tables.

## Binary Structure

```
0E 00                                                  [Int16] SaveVersion: 14
0A 02 00 00                                            [Int32] Unknown1
F5 03 00 00                                            [Int32] Unknown2
60 46 02 00                                            [UInt32] NameTableOffset: 149088
7C BB 39 E0 9B D7 97 41                                [Double] GameTime: 100001528.05637926
17 00 00 00                                            [Int32] Unknown3
            
[## Data Files Array]            
94 00 00 00                                            [Int32] Count: 148
0C 00 00 00                                              [Int32] Length: 12
52 61 67 6E 61 72 6F 6B 5F 57 50 00                      [Byte[]] UTF8 Bytes: "Ragnarok_WP"
FF FF FF FF                                              [Int32] Terminator: -1
                
29 00 00 00                                              [Int32] Length: 41
52 61 67 6E 61 72 6F 6B 5F 57 50 5F 4D 61 69 6E          [Byte[]] UTF8 Bytes: "Ragnarok_WP_MainGrid_L6_X0_Y0_DL54B0FA4B"            
47 72 69 64 5F 4C 36 5F 58 30 5F 59 30 5F 44 4C
35 34 42 30 46 41 34 42 00
FF FF FF FF                                              [Int32] Terminator: -1
                
    ... (146 more data file entries)            
00 00 00 00 00 00 00 00                                [Int64] Unknown4

[## Name Table at offset 149088, 0x024660]
F9 04 00 00                                            [Int32] Count

E9 33 1E 37                                              [Int32] Key: 924726249                                                                                   
4A 00 00 00                                              [Int32] Length: 74                                                                                   
2F 47 61 6D 65 2F 4D 6F 64 73 2F 52 61 67 6E 61          [Byte[]] UTF8 Bytes: "/Game/Mods/Ragnarok/Ragnarok_WP.Ragnarok_WP:PersistentLevel.NPCZoneVolume"                                                                                                                       
72 6F 6B 2F 52 61 67 6E 61 72 6F 6B 5F 57 50 2E                                                                                                                               
52 61 67 6E 61 72 6F 6B 5F 57 50 3A 50 65 72 73                                                                                                                              
69 73 74 65 6E 74 4C 65 76 65 6C 2E 4E 50 43 5A                                                                                                                              
6F 6E 65 56 6F 6C 75 6D 65 00                                                                                                                              
                                                                                                                              
88 C5 B8 D4                                              [Int32] Key: -726088312                                                                                   
4E 00 00 00                                              [Int32] Length: 78                                                                                       
2F 47 61 6D 65 2F 4D 6F 64 73 2F 52 61 67 6E 61          [Byte[]] UTF8 Bytes: /Game/Mods/Ragnarok/Ragnarok_WP.Ragnarok_WP:PersistentLevel.NPCZoneManager51400                                                                                                                      
72 6F 6B 2F 52 61 67 6E 61 72 6F 6B 5F 57 50 2E                                                                                                                              
52 61 67 6E 61 72 6F 6B 5F 57 50 3A 50 65 72 73                                                                                                                              
69 73 74 65 6E 74 4C 65 76 65 6C 2E 4E 50 43 5A                                                                                                                              
6F 6E 65 4D 61 6E 61 67 65 72 35 31 34 00                                                                                                                              
```

## Field Descriptions

### SaveVersion
Save file format version. ASA uses version 14+ (UE5.5).
- **Type**: Int16
- **Value**: 14 for current ARK: Survival Ascended saves
- **Note**: Versions below 14 are not supported (legacy ARK: Survival Evolved)

### NameTableOffset
Absolute file offset to the name table in bytes.
- **Type**: UInt32
- **Purpose**: Points to where the name table begins in the file

### GameTime
Current in-game time in seconds since the world was created.
- **Type**: Double
- **Example**: 100001130.73 ≈ 115.7 days of in-game time
- **Note**: Not real-world time; affected by day/night cycle length multipliers

### Data Files
List of data file names referenced in the save. The first entry is typically the main map name, followed by sublevel/grid files.
- **Type**: String Array
- **Format**: Each entry has:
  - Length (Int32) - includes null terminator
  - UTF-8 string bytes
  - Null terminator (0x00)
  - What seems to be an initialized jagged array of empty objects:
    ```
      [
        [],
        [],
        [ [], [], [] ]
      ]
    ```
  - Terminator integer (0xFFFFFFFF)

**Example entries**:
- `"Ragnarok_WP"` - Main map
- `"Ragnarok_WP_MainGrid_L6_X0_Y0_DL54B0FA4B"` - Grid sublevel file
- `"Ragnarok_WP_MainGrid_L6_X0_Y0_DL4D20BA40"` - Another grid sublevel

### Name Table
Dictionary mapping integer IDs to string names. Used throughout the save file to reduce duplication.
- **Location**: At byte offset specified by NameTableOffset
- **Format**:
  - Count (Int32) - number of entries
  - For each entry:
    - NameIndex (Int32) - the lookup ID
    - String - the name value

The name table contains thousands of entries including:
- Property names (e.g., "BodyColors", "bIsBaby", "CurrentWeapon")
- Class names (e.g., "PlayerPawnTest_Male_C", "Dilo_AIController_BP_C")
- Type names (e.g., "IntProperty", "StructProperty", "BoolProperty")
- Asset paths (e.g., "/Script/Engine", "/Game/PrimalEarth/...")

## JSON Representation

[SaveHeader_0.json](../parsed-maps/Ragnarok_WP/custom/SaveHeader_0.json)
```json
{
  "SaveVersion": 14,
  "NameTableOffset": 149600,
  "GameTime": 100001130.73265652,
  "DataFiles": [
    "Ragnarok_WP",
    "Ragnarok_WP_MainGrid_L6_X0_Y0_DL54B0FA4B",
    "Ragnarok_WP_MainGrid_L6_X0_Y0_DL4D20BA40",
    "...148 total entries..."
  ],
  "NameTable": {
    "162342434": "None",
    "945117242"  : "InheritWeightMutable[1]",    
    "-455136177" : "Bison_Character_BP_C" ,
    "...thousands of entries..."
  }
}
```

## Usage

The save header must be read first before any other records can be parsed, as it provides:
1. **Save version** - determines which parsing logic to use
2. **Name table** - required to decode property names, class names, and types throughout the file
3. **Data files** - maps file indices to actual level/grid names
4. **Game time** - current world timestamp

## Related Code

- [AsaSaveHeader.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Records\AsaSaveHeader.cs) - C# implementation
- [AsaName.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Objects\AsaName.cs) - Name lookup wrapper
