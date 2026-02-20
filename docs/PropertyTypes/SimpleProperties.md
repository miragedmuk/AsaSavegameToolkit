# Simple Properties

| Type               | .NET Type | Size    | Signed | Range/Description           |
| ------------------ | --------- | ------- | ------ | --------------------------- |
| **Int8Property**   | SByte     | 1 byte  | true   | -128 to 127                 |
| **ByteProperty**   | Byte      | 1 bytes | false  | 0 to 255                    |
| **Int16Property**  | Int16     | 2 bytes | true   | -32,768 to 32,767           |
| **UInt16Property** | UInt16    | 2 bytes | false  | 0 to 65,535                 |
| **IntProperty**    | Int32     | 4 bytes | true   | -2.1B to 2.1B               |
| **UInt32Property** | UInt32    | 4 bytes | false  | 0 to 4.3B                   |
| **Int64Property**  | Int64     | 8 bytes | true   | -9.2 * 10^18 to 9.2 * 10^18 |
| **UInt64Property** | UInt64    | 8 bytes | false  | 0 to 1.8 × 10^19            |
| **FloatProperty**  | Single    | 4 bytes | true   | 32-bit floating point       |
| **DoubleProperty** | Double    | 8 bytes | true   | 64-bit floating point       |

## Binary Structure

### Standard Format

```
                                           [Property Header]
70 5D A9 F4 00 00 00 00                    [Name] Property Name: NumNPCSpawned(0)
71 B2 FA F3 00 00 00 00                    [Name] Property Type: IntProperty(0)
00 00 00 00                                [Int32] SubType count: 0
04 00 00 00                                [Int32] Data size: 4
00                                         [Byte] Flags: 0
15 00 00 00                                [Int32] Value: 21
```

## Value Representation Examples

### Integer Values (IntProperty)
```
15 00 00 00 = 21
E8 03 00 00 = 1000
FF FF FF FF = -1
00 00 00 80 = -2,147,483,648
```

### Floating Point Values (FloatProperty)
```
00 00 00 00 = 0.0
00 00 80 3F = 1.0
00 00 00 40 = 2.0
DB 0F 49 40 = 3.14159 (π)
```

### Double Values (DoubleProperty)
```
00 00 00 00 00 00 00 00 = 0.0
00 00 00 00 00 00 F0 3F = 1.0
18 2D 44 54 FB 21 09 40 = π
```

## JSON Representation

```json
// Integer
{"Name": "NumNPCSpawned", "Type": "IntProperty", "Index": 0, "Value": 21}
{"Name": "Temperature", "Type": "FloatProperty", "Index": 0, "Value": 72.5}
{"Name": "GameTime", "Type": "DoubleProperty", "Index": 0, "Value": 100001130.73265652}
{"Name": "PlayerId", "Type": "UInt32Property", "Index": 0, "Value": 2147366052}
{"Name": "CharacterLevel", "Type": "UInt16Property", "Index": 0, "Value": 105}
```

## Related Code

- [AsaPropertyRegistry.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Properties\AsaPropertyRegistry.cs) - Property parsing logic
- [AsaProperty.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Properties\AsaProperty.cs) - Property wrapper class
- [GameObject.md](../RecordTypes/GameObject.md) - Example usage in game objects
