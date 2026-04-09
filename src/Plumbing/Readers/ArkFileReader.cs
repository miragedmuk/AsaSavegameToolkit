using AsaSavegameToolkit.Plumbing.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsaSavegameToolkit.Plumbing.Readers
{
    public abstract class ArkFileReader
    {

        public static List<ArkProperty> ReadProperties(AsaArchive archive)
        {
            var results = new List<ArkProperty>();
            while (true)
            {
                var prop = ReadProperty(archive);
                if (prop == null) break;
                results.Add(prop);
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
                        _ = archive.ReadBytes(8); // separator
                        break;
                    default:
                        size = archive.ReadInt32();
                        index = archive.ReadInt32();
                        _ = archive.ReadByte(); // separator
                        break;
                }

            }

            return type switch
            {
                "IntProperty" => new ArkProperty<int> { Name = name, Value = archive.ReadInt32(), Index = index },
                "StrProperty" => new ArkProperty<string> { Name = name, Value = archive.ReadString(), Index = index },
                "UInt32Property" => new ArkProperty<uint> { Name = name, Value = archive.ReadUInt32(), Index = index },
                "BoolProperty" => new ArkProperty<bool> { Name = name, Value = archive.ReadByte() != 0, Index = index },
                "ArrayProperty" => ReadArrayProperty(archive, name),
                "StructProperty" => ReadStructProperty(archive, name),
                _ => null // Add fallback/logging here if needed
            };
        }

        private static ArkProperty? ReadStructProperty(AsaArchive archive, string name)
        {
            var someInt = archive.ReadInt32();
            var structName = archive.ReadString();
            var someOtherInt = archive.ReadInt32();
            var structPath = archive.ReadString();
            _ = archive.ReadBytes(8); // struct id / padding
            _ = archive.ReadByte(); // separator

            var propertyList = new List<ArkProperty>();
            propertyList = ReadProperties(archive);
            return new ArkProperty<List<ArkProperty>>() { Name = name, Index = 0, Value = propertyList };
        }

        private static ArkProperty? ReadArrayProperty(AsaArchive archive, string name)
        {
            var arrayIndex = archive.ReadInt32();
            var arrayType = archive.ReadString();

            if (arrayType == "StructProperty")
            {
                return ReadStructArray(archive, name, arrayIndex);
            }

            // Simple Type Arrays (String, UInt, etc)
            _ = archive.ReadBytes(8);
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
                _ => null
            };
        }

        private static ArkProperty ReadStructArray(AsaArchive archive, string name, int index)
        {
            _ = archive.ReadInt32(); // unknown
            var structKey = archive.ReadString();
            _ = archive.ReadInt32(); // unknown
            _ = archive.ReadString(); // structType / path
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
