using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsaSavegameToolkit.Plumbing.Readers
{
    public class ArkProfileReader
    {
        private readonly string _saveDirectory;
        private readonly ILogger _logger;
        private readonly AsaReaderSettings _settings;

        public ArkProfileReader(string saveDirectory, ILogger? logger = default, AsaReaderSettings? settings = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(saveDirectory);

            if (!Directory.Exists(saveDirectory))
            {
                throw new FileNotFoundException("Save path not found", saveDirectory);
            }

            _saveDirectory = saveDirectory;
            _logger = logger ?? NullLogger.Instance;
            _settings = settings ?? AsaReaderSettings.None;
        }

        public List<GameObjectRecord> Read()
        {
            var profileFiles = Directory.EnumerateFiles(_saveDirectory, "*.arkprofile");
            var profileBag = new List<GameObjectRecord>();
            var exceptions = new List<Exception>();
            foreach (var filePath in profileFiles)
            {
                try
                {
                    var parsed = ReadProfileFile(filePath);
                    if (parsed != null)
                    {
                        profileBag.Add(parsed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read profile file {FilePath}", filePath);
                }
            }
            return profileBag.ToList();
        }

        private GameObjectRecord ReadProfileFile(string filePath)
        {
            string mapName = "Unknown Map";
            var timestamp = File.GetLastWriteTimeUtc(filePath);
            using var stream = File.OpenRead(filePath);
            using var archive = new AsaArchive(NullLogger.Instance, stream, filePath);

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

            var properties = ReadProperties(archive);

            ObjectTypeFlags objectType = ObjectTypeFlags.Actor;

            return new GameObjectRecord(Guid.NewGuid(),new Primitives.FName(0,0,classPath),names,properties,0,objectType,default);

        }

        public static List<Property> ReadProperties(AsaArchive archive)
        {

            var results = new List<Property>();
            while (true)
            {
                var prop = Property.Read(archive);
                if (prop == null) break;
                results.Add(prop);
            }
            return results;
        }

    }
}
