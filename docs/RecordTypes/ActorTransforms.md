# Actor Transforms Record

[ActorTransforms_0.bin](../parsed-maps/Ragnarok_WP/custom/ActorTransforms_0.bin)
```
       00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
00000: 73 D0 62 00 39 57 22 45 9F 0E 92 06 74 4E 96 A9
00010: 00 F0 E9 14 46 53 05 C1 00 80 51 F5 C7 14 FD C0
00020: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00030: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
00040: 00 00 80 3F 8A D1 63 00 3D 59 22 45 0B 7D 93 06
00050: 73 4E 96 A9 00 00 E7 14 46 53 05 C1 00 80 28 F6
00060: C7 14 FD C0 00 00 00 00 00 00 00 00 00 00 00 00
...
```

The ActorTransforms record stores position and rotation data for all actors (game objects) that have a physical presence in the world.

## Binary Structure

Each entry in the record consists of:

```
                                           [Transform Entry]
73 D0 62 00 39 57 22 45                    [Guid Part 1] ObjectId
9F 0E 92 06 74 4E 96 A9                    [Guid Part 2] 0062d073-4522-5739-0692-0e9fa9964e74

                                           [Transform Data - 56 bytes]
                                           [Location Vector - 24 bytes]
00 F0 E9 14 46 53 05 C1                        [Double] X: -169704.02093821
00 80 28 F6 C7 14 FD C0                        [Double] Y: 563585.582244
00 00 00 00 00 00 00 00                        [Double] Z: 8064.796875

                                           [Rotation Quaternion - 32 bytes]
00 00 00 00 00 00 00 00                        [Double] X: 0.0
00 00 00 00 00 00 00 00                        [Double] Y: 0.0
00 00 00 00 00 00 00 00                        [Double] Z: 0.0
00 00 80 3F                                    [Double] W: 1.0 (as float, should be double)

... (more entries follow the same pattern)

00 00 00 00 00 00 00 00                    [Guid] Empty GUID signals end of record
00 00 00 00 00 00 00 00
```

## Field Descriptions

### ObjectId
GUID that uniquely identifies the game object. Matches the GUID in the corresponding GameObject record.
- **Type**: Guid (16 bytes)
- **Format**: Binary representation of GUID
- **Example**: `0062d073-4522-5739-0692-0e9fa9964e74`
- **Endianness**: Little-endian by parts (matches .NET Guid.ToByteArray())

### Location (Translation)
3D position vector in Unreal Engine units (centimeters).
- **Type**: Vector (3 x Double = 24 bytes)
- **Components**:
  - **X**: East-West position
  - **Y**: North-South position  
  - **Z**: Height above sea level
- **Example**: `(-169704.02, 563585.58, 8064.80)` = position on the map

### Rotation (Quaternion)
Orientation represented as a quaternion.
- **Type**: Quaternion (4 x Double = 32 bytes)
- **Components**: X, Y, Z, W
- **Default**: `(0, 0, 0, 1)` = no rotation
- **Note**: Quaternions avoid gimbal lock and provide smooth interpolation

**Example rotations**:
```
(0, 0, 0, 1) - No rotation
(0.9998356, 0, 0, 0.0012546) - Small roll rotation
(-0.0039, 0.0074, 0.4679, 0.8838) - Complex 3D rotation
```

### End Marker
An empty GUID (16 bytes of zeros) signals the end of the ActorTransforms record.

## JSON Representation

[ActorTransforms_0.json](../parsed-maps/Ragnarok_WP/custom/ActorTransforms_0.json)
```json
{
  "0062d073-4522-5739-0692-0e9fa9964e74": {
    "X": -169704.02093821,
    "Y": 563585.582244,
    "Z": 8064.796875,
    "Pitch": 0.0,
    "Yaw": 0.0,
    "Roll": 0.0
  },
  "46e86e37-4eaf-c355-4899-b285f84f4bb8": {
    "X": -381204.0,
    "Y": -308662.9375,
    "Z": -25417.3515625,
    "Pitch": 0.0,
    "Yaw": 0.0,
    "Roll": 0.0
  },
  "...many more entries..."
}
```

**Note**: In JSON, the quaternion is converted to Euler angles (Pitch, Yaw, Roll) for easier human readability.

## Transform Details

### Coordinate System
ARK uses Unreal Engine's left-handed Z-up coordinate system:
- **X-axis**: Positive = East, Negative = West
- **Y-axis**: Positive = North, Negative = South
- **Z-axis**: Positive = Up, Negative = Down
- **Units**: Centimeters (100 units = 1 meter)

### Quaternion to Euler Conversion
The binary stores rotation as quaternions (X, Y, Z, W), but for debugging/display they're often converted to Euler angles:
- **Pitch**: Rotation around Y-axis (up/down tilt)
- **Yaw**: Rotation around Z-axis (left/right turn)
- **Roll**: Rotation around X-axis (barrel roll)

### Typical Z Values
Common height values on different maps:
- **Sea level**: ~8000-8100 units
- **Underground**: Negative Z values
- **Flying/Mountains**: Higher positive Z values
- **Ragnarok caves**: Often negative or low Z values

## Usage

ActorTransforms are used to:
1. **Position objects** - Place structures, creatures, and items in the world
2. **Spawn locations** - Determine where creatures and resources appear
3. **Physics calculations** - Provide starting positions for physics simulations
4. **Map rendering** - Display object locations on admin maps

The transform record is separate from GameObject records for performance reasons - transforms can be updated frequently without touching property data.

## Relationship to GameObjects

Each GameObject record contains a GUID. The ActorTransforms record uses the same GUID to associate position/rotation data:

```
GameObject: 0062d073-4522-5739 -> NPCZoneManager29 with properties
ActorTransforms: 0062d073-4522-5739 -> Position  (-169704, 563585, 8064)
                                       Rotation (0, 0, 0, 1)
```

Not all GameObjects have transforms - only those that exist physically in the world.

## Related Code

- [ActorTransformsRecord.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Records\ActorTransformsRecord.cs) - C# implementation
- [AsaTransform.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Structs\AsaTransform.cs) - Transform struct
- [AsaVector.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Structs\AsaVector.cs) - 3D vector
- [AsaQuaternion.cs](d:\repos\AsaSavegameToolkit\src\Serialization\Structs\AsaQuaternion.cs) - Quaternion rotation
