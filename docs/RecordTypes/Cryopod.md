# Cryopod (CustomItemData)

Cryopods store their creature data as binary blobs inside `CustomItemDatas[0].CustomDataBytes.ByteArrays[n].Bytes`.

## ByteArray slots

| Index | Contents | Compressed | Versions |
|-------|----------|------------|---------|
| `[0]` | Dino + StatusComponent game objects | Yes (zlib → wildcard) | v13+ |
| `[1]` | Saddle item properties | No | v13+ |
| `[2]` | Costume item properties | No | **v14+ only** — v13 clients do not write this slot |

> **Note:** Costume deserialization (`CustomDataBytes[2]`) is only supported for saves created by v14+ clients. v13 cryopods do not include a costume slot.

---

## `CustomDataBytes[0]` — Dino + StatusComponent (compressed)

### Compression layers

The raw blob is compressed in two stages (applied in order when writing, reversed when reading):

1. **Wildcard** — ARK's custom RLE scheme (see `WildcardInflater.cs`)
2. **zlib** (`78 9C` deflate with zlib wrapper)

So decompression order is: **zlib first, then wildcard**.

### Compressed blob header (12 bytes, read before decompressing)

| Offset | Type | Field | Notes |
|--------|------|-------|-------|
| `+0` | `Int32` | `version` | `0x0406` = v13, `0x0407` = v14 |
| `+4` | `Int32` | `zlibSize` | Size of data after zlib decompression (before wildcard) |
| `+8` | `Int32` | `namesOffset` | Byte offset **within the final decompressed payload** where the name table starts |
| `+12` | `byte[]` | payload | zlib-compressed data |

### Decompressed payload layout

```
[if version >= 0x0407]  skip 8 bytes (unknown padding)
Int32                   objectCount
objectCount × ArkGameObject metadata:
    16 bytes            UUID
    FString             blueprint path
    Int32               = 0 (reserved)
    Int32               nameCount
    nameCount × FString names
    byte                fromDataFile (bool)
    Int32               dataFileIndex
    byte                hasRotation (bool)
    [if hasRotation]    ArkRotator (3 × Float/Double)
    Int32               propertiesOffset  ← offset into this same payload
    Int32               = 0 (reserved)
Each object's properties at its propertiesOffset:
    [if version >= 0x0407]  skip 1 byte (zero padding)
    Property list (until "None" terminator)
    Int32               (unknown, skip)
    byte                hasExtraGuid (bool)
    [if hasExtraGuid]   16 bytes UUID
Name table at namesOffset:
    Int32               nameCount
    nameCount × FString name strings (index = i | 0x10000000)
```

---

## `CustomDataBytes[1]` — Saddle item properties

Not compressed. Direct binary property stream.

### Format

```
Int32   archiveVersion
        if archiveVersion == 0x1BEDEAD:
            Int32  archiveVersion  (real version follows the magic)
[if archiveVersion >= 7]  skip 8 bytes
Property list (until "None" terminator)
```

- v13 files: first Int32 = `6` (archiveVersion = 6)
- v14 files: first Int32 = `0x01BEDEAD`, second Int32 = real version

---

## `CustomDataBytes[2]` — Costume item properties *(v14+ only)*

Same format as `CustomDataBytes[1]`. Only present in cryopods written by a v14+ client.
