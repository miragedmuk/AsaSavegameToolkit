using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Readers;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsaSavegameToolkit.Plumbing.Records
{
    public class ArkProfileRecord
    {
        public static GameObjectRecord Read(AsaArchive archive, Guid uuid)
        {
            string mapName = string.Empty;

            // Header sequence
            var fileVersion = archive.ReadInt32(); // tribeVersion
            if (fileVersion < 7) throw new AsaDataException($"Unsupported .arkprofile version: {fileVersion}");

            archive.SaveVersion = (short)fileVersion; // Set save version for correct parsing
            archive.IsArkFile = true; // Mark as Ark file for correct handling

            var gameVersion = archive.ReadInt32();
            var packageVersion = archive.ReadInt32();
            var objectDescriptorCount = archive.ReadInt32();

            _ = archive.ReadBytes(16);

            var classPath = archive.ReadString(); // e.g., "/Game/PrimalEarth/..."
            var isActor = archive.ReadInt32();    // Usually 0 or 1

            var names = archive.ReadStringArray(); // Map Name table strings
            if (names != null && names.Length > 3)
            {
                mapName = names[3]; // Map name
            }

            _ = archive.ReadBytes(12);
            var dataOffset = archive.ReadInt64(); //property start

            archive.Position = dataOffset;
            _ = archive.ReadByte(); // Property list start marker (0x00)

            var properties = Property.ReadList(archive);

            ObjectTypeFlags objectType = ObjectTypeFlags.Actor;

            return new GameObjectRecord(uuid, new Primitives.FName(0, 0, classPath), names, properties, 0, objectType, default);
        }

        public static GameObjectRecord ReadFromFile(string filePath, Guid uuid)
        {

            var timestamp = File.GetLastWriteTimeUtc(filePath);
            using var archive = new AsaArchive(NullLogger.Instance, File.ReadAllBytes(filePath), filePath);

            return Read(archive, uuid);
        }
    }
}
