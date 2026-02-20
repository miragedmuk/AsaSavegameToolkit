# Array property header
B4 2F 71 9B 00 00 00 00          DinoAncestorsMale
6B C5 FD 58 00 00 00 00          ArrayProperty
00 01 00 00                      Size: 256
00 00 00 00                      Index: 0
70 C6 A4 FA 00 00 00 00          Subtype: StructProperty 
00                               Flags: 0

# Array property value
01 00 00 00                      Count: 1

## Entry 1 - Looks like a full property definition.
I can't tell if array properties hold a list of inner properties or a list of inner raw values
This array of struct has a property as its element

### Array Entry 1 Property Header
B4 2F 71 9B 00 00 00 00          Name: DinoAncestorsMale - Struct named after the array its in
70 C6 A4 FA 00 00 00 00          Type: StructProperty
CB 00 00 00                      Size: 203
00 00 00 00                      Index: 0
EB B4 F0 9B 00 00 00 00          Subtype: DinoAncestorsEntry
00                               Flags: 0

### Array Entry 1 PRoperty Value
00 00 00 00
00 00 00 00
00 00 00 00
00 00 00 00 

#### Struct Property 1 Header
C0 07 D0 88 00 00 00 00          Name: MaleName 
80 FF 9F A8 00 00 00 00          Type: StrProperty 
19 00 00 00                      Size: 25
00 00 00 00                      Index: 0
00                               Flags: 0

#### Struct Property 1 Value
15 00 00 00 54 72 69 63 65 72 61 74 6F 70 73 20 2D 20 4C 76 6C 20 36 30 00    "Triceratops - Lvl 60"

#### Struct Property 2 Header
2D B6 B5 47 00 00 00 00          Name: MaleDinoID1
5B 9E 09 B5 00 00 00 00          Type: UInt32Property
04 00 00 00                      Size: 4
00 00 00 00                      Index 0
00                               Flags: 0

#### Struct Property 2 Value
4D D7 B0 00                      Value: 8424632832775739213

ED 4E EA 74 00 00 00 00          Name: MaleDinoID2 
5B 9E 09 B5 00 00 00 00          Type: UInt32Property
04 00 00 00                      Size: 4
00 00 00 00                      Index: 0
00                               Flags: 0

AA 12 08 03                      Value: 50860714

07 A0 A3 70 00 00 00 00          Name: FemaleName
80 FF 9F A8 00 00 00 00          Type: StrProperty
04 00 00 00                      Size: 4
00 00 00 00                      Index: 0
00                               Flags: 0

00 00 00 00                      Value: 0

03 59 58 ED 00 00 00 00          Name: FemaleDinoID1
5B 9E 09 B5 00 00 00 00          Type: UInt32Property
04 00 00 00                      Size: 4
00 00 00 00                      Index: 0
00                               Flags: 0

00 00 00 00                      Value: 0

C3 A1 07 DE 00 00 00 00          Name: FemaleDinoID2
5B 9E 09 B5 00 00 00 00          Type: UInt32Property
04 00 00 00                      Size: 4
00 00 00 00                      Index: 0
00                               Flags: 0

00 00 00 00                      Value: 0

22 26 AD 09 00 00 00 00          None