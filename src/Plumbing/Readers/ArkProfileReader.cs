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
    public class ArkProfileReader: ArkFileReader
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

        public List<ArkFileRecord> Read()
        {
            var profileFiles = Directory.EnumerateFiles(_saveDirectory, "*.arkprofile");
            var profileBag = new List<ArkFileRecord>();
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

        private ArkFileRecord ReadProfileFile(string filePath)
        {
            string mapName = "Unknown Map";
            var timestamp = File.GetLastWriteTimeUtc(filePath);
            using var stream = new MemoryStream(File.ReadAllBytes(filePath));
            using var archive = new AsaArchive(NullLogger.Instance, stream, filePath);

            // Header sequence
            var fileVersion = archive.ReadInt32(); // tribeVersion
            if (fileVersion != 7) throw new AsaDataException($"Unsupported .arkprofile version: {fileVersion}");


            _ = archive.ReadBytes(12); // ID, Save Count, Table Offset
            _ = archive.ReadBytes(16); // GUID

            var fileType = archive.ReadString();
            _ = archive.ReadInt32(); // Name Table count

            var names = archive.ReadStringArray(); // Map Name table strings
            if (names != null && names.Length > 3)
            {
                mapName = names[3]; // Map name
            }

            _ = archive.ReadBytes(12); // padding / unknown
            _ = archive.ReadByte();

            var buffCount = archive.ReadInt32(); // Number of "buffs" or similar objects attached to the player. Often 0, but can be 1 or more.

            for (int buffLoop = 0; buffLoop < buffCount; buffLoop++)
            {
                var unknown = archive.ReadBytes(19);

                var a = archive.ReadString(); // Buff name
                var b = archive.ReadInt32(); // instance
                var c = archive.ReadInt32(); // flags
                var d = archive.ReadString(); // value
                archive.ReadBytes(17); // Additional metadata pointers           
            }

            var unknown2 = archive.ReadInt32();


            var properties = ReadProperties(archive);

            return new ArkFileRecord(timestamp, filePath, mapName, properties);
        }

    }
}
