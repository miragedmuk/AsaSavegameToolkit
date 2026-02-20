# GameModeCustomBytes Structure

This document describes the structure of the `CustomGameModeBlob` data found in ARK: Survival Ascended save files.

## Overview

The `GameModeCustomBytes` blob contains various game mode data including tribe information, spawn configurations, and typed dictionaries.

## Embedded Tribe / Profile Archives (v7 blobs)

The blob contains an index of embedded sub-archives for tribe and player profile data. These sub-archives use the **old ARK SE archive-version-7 format** (not the ASA v11-v14 format).

### Outer Index Structure

```
[4]  tribeCount
tribeCount × 16 bytes:
    [4]  TribeId    uint32
    [4]  Unknown    int32
    [4]  Offset     int32   (byte offset from start of tribe data block)
    [4]  Size       int32   (byte size of this tribe's blob)
[4]  profileCount
profileCount × 16 bytes:
    [8]  EosId      int64   (Epic Online Services player ID)
    [4]  Offset     int32
    [4]  Size       int32   (0 = deleted entry)
[4]  tribeDataBlockSize
[raw tribe blobs — laid out contiguously at their declared offsets]
[4]  profileDataBlockSize
[raw profile blobs — laid out contiguously at their declared offsets]
```

### V7 Blob Structure (`EmbeddedV7Archive`)

Each tribe/profile blob is a self-contained mini-archive:

```
[4]  saveVersion   = 7
[4]  nameCount     (unused — v7 era field, not a name table)
[4]  propsSize     (properties data block size, skipped during header read)
[4]  objectCount
objectCount × ArkObject header:
    [16] UUID              (Guid, little-endian)
    [*]  className         FString (e.g. "/Script/ShooterGame.PrimalTribeData")
    [4]  isItem            int32 bool
    [4]  nameCount         int32
    nameCount × [*] FString instance names
    [4]  fromDataFile      int32 bool
    [4]  dataFileIndex     int32
    [4]  hasLocation       int32 bool
    if hasLocation:
        [24] FVector       (3 × double, X/Y/Z)
        [12] FRotator      (3 × float, Pitch/Yaw/Roll)
    [4]  propertiesOffset  int32 (byte offset from blob start)
    [4]  (reserved zero)
Properties data (seeked to via propertiesOffset):
    [1]  0x00 padding byte
    [*]  tagged properties, terminated by FString "None"
```

> **Note:** Booleans in ArkObject headers (`isItem`, `fromDataFile`, `hasLocation`) are `int32`, not single bytes — unlike typical C# serialization.

### V7 Property Tag Format

V7 uses the **UE5.5 / v14 FPropertyTypeName structure** (confirmed by Java reference: `useUE55Structure()` returns `true` for `ARK_ARCHIVE >= 7`). The only differences from v14:

| Aspect | v14 | v7 |
|--------|-----|----|
| Name storage | Name-table index (int32 + int32) | Raw FString |
| Struct GUID prefix | Not present | Not present |
| BoolProperty value | Bit in flags byte | Separate `int32` after `size` field |

`ReadFName()` in `AsaArchive` transparently handles v7 by delegating to `ReadString()` when `SaveVersion <= 7`.

### Implementation

- **`EmbeddedV7Object`** — model class for a parsed v7 game object (UUID, class name, names, location, properties)
- **`EmbeddedV7Archive`** — static parser; wraps the blob bytes in a `MemoryStream`, sets `archive.SaveVersion = 7`, reads headers then seeks to each object's properties offset and parses property tags
- **`EmbeddedTribeEntry.Objects`** / **`EmbeddedProfileEntry.Objects`** — `List<EmbeddedV7Object>` replacing the former `RawBlob: byte[]`

### Version guard pattern

Several property readers had `if (SaveVersion < 14)` guards routing to v13-era code. These must be narrowed to `if (SaveVersion is >= 11 and < 14)` so that v7 blobs take the v14 path:

- `PropertyTag.Read()` — now dispatches `<= 7` → `ReadVersion7()`, `>= 11 and < 14` → `ReadPre14()`
- `StructProperty.ReadStructGuid()` — only reads 16-byte GUID for `>= 11 and < 14`
- `ArrayProperty.ReadValue()` — only routes to `ReadValuePre14` for `>= 11 and < 14`

### Status

> **⚠ In progress** — property parsing for nested structs (e.g. `TribeData` containing arrays) is not yet fully working. Tests fail inside `StructProperty.ReadGenericStruct` when reading sub-properties of the top-level `TribeData` struct. Remaining investigation needed before committing.

## Typed Dictionary Structure

One of the key structures found within this blob is a typed heterogeneous dictionary. This appears late in the data structure.

### Format

Each dictionary contains:
1. **Count of keys** (4 bytes) - List of sequential integers
2. **Key list** - Sequential integers (1, 2, 3, ... n)
3. **Entry count** (4 bytes) - Number of key-value entries
4. **Entry array** - Series of 12-byte entries

### Entry Structure (12 bytes each)

```
[Type ID - 4 bytes] [Data Value - 4 bytes] [Sequential Index - 4 bytes]
```

- **Type ID**: A 4-byte identifier/hash indicating how to interpret the data value
- **Data Value**: 4 bytes of data interpreted according to the Type ID
- **Sequential Index**: A sequential integer (0, 1, 2, 3, ...)

### Known Type IDs

| Type ID (hex) | Type ID (decimal) | Interpretation | Notes |
|---------------|-------------------|----------------|-------|
| `A6 52 9B A7` | 2814948006 | Integer or Name Table Index | Most common (~90% of entries) |
| `3B 6C 33 F4` | 4097533995 | Float (often NaN) | Values like `0x7FFF2EC8` are NaN sentinels for "unset" |
| `26 2F 66 41` | 1094041382 | IEEE 754 Float | Valid float values (e.g., 7.915, 4.515, 3.996) |
| `39 CA CE 0D` | 233132089 | Float or Special Value | Values like `0x7FEB4380`, `0x7FFFCA7D` |
| `9A C3 24 4D` | 1294123930 | Unknown | Values like `0x7FFF2EB0` (possibly NaN float) |
| Various singles | - | Unknown | One-off type IDs, purpose unclear |

### Example

```
Offset: ~16440

39 00 00 00   -- Key count (57 keys)

01 00 00 00   -- Keys: 1
02 00 00 00   --       2
...           --       ...
39 00 00 00   --       57

76 01 00 00   -- Entry count (0x176 = 374 entries)

A6 52 9B A7 0E 02 00 00 00 00 00 00   -- Entry 0: Type=Int, Value=0x020E, Index=0
A6 52 9B A7 1B 02 00 00 01 00 00 00   -- Entry 1: Type=Int, Value=0x021B, Index=1
A6 52 9B A7 4B 00 00 00 02 00 00 00   -- Entry 2: Type=Int, Value=0x004B, Index=2
...
3B 6C 33 F4 07 2E FF 7F 0C 00 00 00   -- Entry 12: Type=Float, Value=NaN, Index=12
...
26 2F 66 41 F3 49 FD 40 6A 00 00 00   -- Entry 106: Type=Float, Value=7.915, Index=106
```

### Notes

- Type IDs appear to be internal magic numbers/hashes rather than name table references
- Not found in the save file's NameTable
- NaN float values (`0x7FFF....`) are commonly used as sentinel values for uninitialized properties
- This pattern is typical of Unreal Engine's property serialization system

## TODO

- [ ] Document the structure from the beginning of the blob
- [ ] Identify the purpose of this typed dictionary
- [ ] Map type IDs to their actual property usage in game logic
- [ ] Parse and expose this data through the toolkit API
