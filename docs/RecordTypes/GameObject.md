# Game object binary

[0062d073-4522-5739-0692-0e9fa9964e74.bin](../parsed-maps/Ragnarok_WP/game/0062d073-4522-5739-0692-0e9fa9964e74.bin)
```
       00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
00000: 8F 04 EE 1D 00 00 00 00 00 00 00 00 01 00 00 00
00010: 11 00 00 00 4E 50 43 5A 6F 6E 65 4D 61 6E 61 67
00020: 65 72 32 39 00 00 00 00 00 02 00 BC 96 C3 15 00
00030: 00 00 00 0D D9 8C 90 00 00 00 00 00 00 00 00 08
00040: 00 00 00 00 83 3D EE AA 95 D7 97 41 70 5D A9 F4
00050: 00 00 00 00 71 B2 FA F3 00 00 00 00 00 00 00 00
00060: 04 00 00 00 00 15 00 00 00 A9 01 5B B1 00 00 00
00070: 00 0D D9 8C 90 00 00 00 00 00 00 00 00 08 00 00
00080: 00 00 38 79 EF B3 95 D7 97 41 22 26 AD 09 00 00
00090: 00 00 01 00 00 00 73 D0 62 00 39 57 22 45 9F 0E
000A0: 92 06 74 4E 96 A9
```

This game object has the binary structure:
```
                                       [Name] ClassName: NPCZoneManager(0)
8F 04 EE 1D                              [Int32] Name: NPCZoneManager from NameTable
00 00 00 00                              [Int32] Index: 0
00 00 00 00                            [Int32]
                                       [String[]] Names
01 00 00 00                              [Int32] Count: 1
                                         [String]
11 00 00 00                              [Int32] Length: 17
4E 50 43 5A 6F 6E 65 4D 61 6E            [Byte[]] UTF8 Bytes: NPCZoneManager29{nul}
61 67 65 72 32 39 00
00 00 00 00                            [Int32] DataFileIndex: 0
02                                     [Byte] Flags1: 2               
00                                     [Byte] Flags2: 0     

BC 96 C3 15 00 00 00 00                [Name] Property Name: FirstUpdateTime(0)
0D D9 8C 90 00 00 00 00                [Name] Property Type: DoubleProperty(0)
00 00 00 00                              [Int32] SubType count
08 00 00 00                            [Int32] Property size: 8
00                                     [Byte] Flags: 0 (flags with bit 1 are followed by another Int32 with an offset to the proprety's bytes)
83 3D EE AA 95 D7 97 41                [Double] Value: 100001130.73265652

70 5D A9 F4 00 00 00 00                [Name] Property Name: NumNPCSpawned(0)
71 B2 FA F3 00 00 00 00                [Name] Property Type: IntProperty(0)
00 00 00 00                              [Int32] SubType count: 0
04 00 00 00                            [Int32] Property size: 4
00                                     [Byte] Flags: 0
15 00 00 00                            [Int32] Value: 21

A9 01 5B B1 00 00 00 00                [Name] Property Name: LastIncreaseNPCTime(0)
0D D9 8C 90 00 00 00 00                [Name] Property Type: DoubleProperty(0)
00 00 00 00                              [Int32] SubType count: 0                     
08 00 00 00                            [Int32] Property size: 8                     
00                                     [Byte] Flags: 0            
38 79 EF B3 95 D7 97 41                [Double] Value: 100001132.98386085

22 26 AD 09 00 00 00 00                [Name] Property Name: None

                                       [Guid[]]
01 00 00 00                            [Int32] Length: 1
73 D0 62 00 39 57 22 45                [Guid] Value: 0062d073-4522-5739-0692-0e9fa9964e74
9F 0E 92 06 74 4E 96 A9
```

We serialize that in json as:

[0062d073-4522-5739-0692-0e9fa9964e74.json](../parsed-maps/Ragnarok_WP/game/0062d073-4522-5739-0692-0e9fa9964e74.json)
```json
{
    "Guid": "0062d073-4522-5739-0692-0e9fa9964e74", 
    "ClassName": {"Name": "NPCZoneManager", "Instance": 0}, 
    "IsItem": false, 
    "Names": [ {"Name": "NPCZoneManager29", "Instance": 0} ], 
    "Properties": [
        {"Name": "FirstUpdateTime"    , "Type": "DoubleProperty", "Index": 0, "Value": 100001130.73265652}, 
        {"Name": "NumNPCSpawned"      , "Type": "IntProperty"   , "Index": 0, "Value": 21                }, 
        {"Name": "LastIncreaseNPCTime", "Type": "DoubleProperty", "Index": 0, "Value": 100001132.98386085}  
    ], 
    "DataFileIndex": 0, 
    "PropertyOffset": 43, 
    "TailGuid": "0062d073-4522-5739-0692-0e9fa9964e74", 
    "ExtraData": "AQAAAHPQYgA5VyJFnw6SBnROlqk=", 
    "ClassString": "NPCZoneManager", 
    "ParentNames": []
}
```
