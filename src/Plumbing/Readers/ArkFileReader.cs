using AsaSavegameToolkit.Plumbing.Primitives;
using AsaSavegameToolkit.Plumbing.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsaSavegameToolkit.Plumbing.Readers
{
    public abstract class ArkFileReader
    {
        private static string lastPropertyName = string.Empty;

        public static List<ArkProperty> ReadProperties(AsaArchive archive)
        {
            var results = new List<ArkProperty>();
            while (true)
            {
                var propStart = archive.Position;
                var prop = ReadProperty(archive);
                if (prop == null) break;
                results.Add(prop);
                lastPropertyName = prop.Name;
            }
            return results;
        }

        private static ArkProperty? ReadProperty(AsaArchive archive)
        {
            var name = archive.ReadString();
            if (string.IsNullOrEmpty(name) || name == "None") return null;

            var type = archive.ReadString();

            // Simple properties share a header; Structs/Arrays handle their own headers
            int size = 0, index = 0;


            if (type != "StructProperty" && type != "ArrayProperty")
            {

                switch (type)
                {
                    case "BoolProperty":
                        index = archive.ReadInt32();
                        _ = archive.ReadByte(); // separator
                        break;
                    default:
                        index = archive.ReadInt32();
                        size = archive.ReadInt32();

                        _ = archive.ReadByte(); // separator
                        break;
                }

                if (name == lastPropertyName)
                {
                    index = archive.ReadInt32();
                }

            }


            return type switch
            {
                "IntProperty" => new ArkProperty<int> { Name = name, Value = archive.ReadInt32(), Index = index },
                "StrProperty" => new ArkProperty<string> { Name = name, Value = archive.ReadString(), Index = index },
                "NameProperty" => new ArkProperty<string> { Name = name, Value = archive.ReadString(), Index = index },
                "UInt16Property" => new ArkProperty<uint> { Name = name, Value = archive.ReadUInt16(), Index = index },
                "UInt32Property" => new ArkProperty<uint> { Name = name, Value = archive.ReadUInt32(), Index = index },
                "UInt64Property" => new ArkProperty<ulong> { Name = name, Value = archive.ReadUInt64(), Index = index },
                "Int64Property" => new ArkProperty<long> { Name = name, Value = archive.ReadInt64(), Index = index },
                "BoolProperty" => new ArkProperty<bool> { Name = name, Value = archive.ReadInt32() != 0, Index = index },
                "FloatProperty" => new ArkProperty<float> { Name = name, Value = archive.ReadFloat(), Index = index },
                "DoubleProperty" => new ArkProperty<double> { Name = name, Value = archive.ReadDouble(), Index = index },
                "ByteProperty" =>  ReadByteProperty(archive,name,index,size),
                "ObjectProperty" => ReadObjectProperty(archive, name, index),
                "ArrayProperty" => ReadArrayProperty(archive, name),
                "StructProperty" => ReadStructProperty(archive, name),
                _ => null // Add fallback/logging here if needed
            };
        }

        private static ArkProperty ReadByteProperty(AsaArchive archive, string name, int index, int size)
        {
            if(size== 1)
            {
                return new ArkProperty<byte> { Name = name, Value = archive.ReadByte(), Index = index };
            }

            archive.Position -= 5;
            var enumType = archive.ReadString();
            var enumSize = archive.ReadInt32();
            var enumPath = archive.ReadString();

            var a1 = archive.ReadInt32();
            var a2 = archive.ReadInt32();
            _ = archive.ReadByte();

            var enumValue = archive.ReadString();

            return new ArkProperty<string> { Name = name, Value = enumValue, Index = index };

        }

        private static ArkProperty<string> ReadObjectProperty(AsaArchive archive, string name, int index)
        {
            _ = archive.ReadInt32();
            return new ArkProperty<string>() { Name = name, Value = archive.ReadString(), Index = index };

        }

        private static ArkProperty? ReadStructProperty(AsaArchive archive, string name)
        {
            var someInt = archive.ReadInt32();
            var structName = archive.ReadString();

            var someOtherInt = archive.ReadInt32();
            var structPath = archive.ReadString();

            var structIndex = archive.ReadInt32();
            var structSizeBytes = archive.ReadInt32();

            _ = archive.ReadByte(); // separator
            var dataStartPos = archive.Position;

            switch (structName)
            {
                case "UniqueNetIdRepl":

                    byte hasType = archive.ReadByte();
                    if (hasType != 0)
                    {
                        string netIdType = archive.ReadString(); // e.g., "EOS" or "Steam"
                        ulong netId = archive.ReadUInt64();
                        _ = archive.ReadBytes(9);
                        return new ArkProperty<ulong> { Name = name, Value = netId, Index = 0 };
                    }
                    return null;
                case "LinearColor":
                    if (name == lastPropertyName)
                        structIndex = archive.ReadInt32();

                    var r = archive.ReadFloat();
                    var g = archive.ReadFloat();
                    var b = archive.ReadFloat();
                    var a = archive.ReadFloat();

                    return new ArkProperty<(float R, float G, float B, float A)> { Name = name, Value = (r, g, b, a), Index = structIndex };
                case "IntPoint":
                    var p1 = archive.ReadInt32();
                    var p2= archive.ReadInt32();

                    return new ArkProperty<string> { Name = name, Value = $"{p1},{p2}", Index = structIndex };
                case "Vector":
                    var x = archive.ReadDouble();
                    var y = archive.ReadDouble();
                    var z = archive.ReadDouble();
                    return new ArkProperty<(double X, double Y, double Z)> { Name = name, Value = (x, y, z), Index = structIndex };
            }


            var propertyList = new List<ArkProperty>();
            propertyList = ReadProperties(archive);
            return new ArkProperty<List<ArkProperty>>() { Name = name, Index = 0, Value = propertyList };
        }

        private static ArkProperty? ReadArrayProperty(AsaArchive archive, string name)
        {
            var arrayIndex = archive.ReadInt32();
            var arrayType = archive.ReadString();

            switch (arrayType)
            {
                case "StructProperty":
                    return ReadStructArray(archive, name, arrayIndex);
            }


            // Simple Type Arrays (String, UInt, etc)
            var dataIndex = archive.ReadInt32();
            var dataSize = archive.ReadInt32();

            _ = archive.ReadByte();
            var count = archive.ReadInt32();

            return arrayType switch
            {
                "StrProperty" => new ArkProperty<string[]>
                {
                    Name = name,
                    Index = arrayIndex,
                    Value = Enumerable.Range(0, count).Select(_ => archive.ReadString()).ToArray()
                },
                "UInt32Property" => new ArkProperty<uint[]>
                {
                    Name = name,
                    Index = arrayIndex,
                    Value = Enumerable.Range(0, count).Select(_ => archive.ReadUInt32()).ToArray()
                },
                "DoubleProperty" => new ArkProperty<double[]>
                {
                    Name = name,
                    Index = arrayIndex,
                    Value = Enumerable.Range(0, count).Select(_ => archive.ReadDouble()).ToArray()
                },
                "FloatProperty" => new ArkProperty<float[]>
                {
                    Name = name,
                    Index = arrayIndex,
                    Value = Enumerable.Range(0, count).Select(_ => archive.ReadFloat()).ToArray()
                },
                "NameProperty" => new ArkProperty<string[]>
                {
                    Name = name,
                    Index = arrayIndex,
                    Value = Enumerable.Range(0, count).Select(_ => archive.ReadString()).ToArray()
                },
                "ByteProperty" => new ArkProperty<byte[]>
                {
                    Name = name,
                    Index = arrayIndex,
                    Value = Enumerable.Range(0, count).Select(_ => archive.ReadByte()).ToArray()

                },
                "ObjectProperty" => new ArkProperty<string[]>
                {
                    Name = name,
                    Index = arrayIndex,
                    Value = Enumerable.Range(0, count).Select(_ =>
                    {
                        var objectValue = string.Empty;

                        if (dataSize > 20)
                        {
                            archive.ReadBytes(4);
                            objectValue = archive.ReadString();
                            // Path reference 
                            return objectValue;
                        }
                        else
                        {
                            archive.Position -= 8;
                           
                            objectValue = archive.ReadGuid().ToString();

                        }

                        return objectValue;
                    }).ToArray()
                },
                _ => null
            };
        }


        private static ArkProperty ReadStructArray(AsaArchive archive, string name, int index)
        {
            _ = archive.ReadInt32(); // unknown
            var structKey = archive.ReadString();
            _ = archive.ReadInt32(); // unknown
            var structPath = archive.ReadString(); // structType / path
            _ = archive.ReadBytes(8); // struct id / padding
            _ = archive.ReadByte(); // separator

            var structCount = archive.ReadInt32();
            var structList = new List<ArkProperty>();

            for (int i = 0; i < structCount; i++)
            {
                structList.Add(new ArkProperty<List<ArkProperty>>
                {
                    Name = structKey,
                    Index = i,
                    Value = ReadProperties(archive)
                });
            }

            return new ArkProperty<List<ArkProperty>> { Name = name, Index = index, Value = structList };
        }

    }
}
