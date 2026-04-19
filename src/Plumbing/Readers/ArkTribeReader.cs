using AsaSavegameToolkit.Plumbing.Properties;
using AsaSavegameToolkit.Plumbing.Records;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace AsaSavegameToolkit.Plumbing.Readers
{
    public class ArkTribeReader
    {
        private readonly string _saveDirectory;
        private readonly ILogger _logger;
        private readonly AsaReaderSettings _settings;
        


        public ArkTribeReader(string saveDirectory, ILogger? logger = default, AsaReaderSettings? settings = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(saveDirectory);

            if (!Directory.Exists(saveDirectory))
            {               
                throw new DirectoryNotFoundException($"Save path not found: {saveDirectory}");
            }

            _saveDirectory = saveDirectory;
            _logger = logger ?? NullLogger.Instance;
            _settings = settings ?? AsaReaderSettings.None;
        }

        public List<ArkFileRecord> Read()
        {
            var tribeFiles = Directory.EnumerateFiles(_saveDirectory, "*.arktribe");
            var tribeBag = new List<ArkFileRecord>();
            var exceptions = new ConcurrentBag<Exception>();
            Parallel.ForEach(tribeFiles, new ParallelOptions { MaxDegreeOfParallelism = _settings.MaxCores }, filePath =>
            {
                try
                {
                    var parsed = ReadTribeFile(filePath);
                    
                    if (parsed != null)
                    {
                        tribeBag.Add(parsed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read tribe file {FilePath}", filePath);
                }
            }
            );
            return tribeBag.ToList();


        }

        private ArkFileRecord? ReadTribeFile(string filePath)
        {
            string mapName = "Unknown Map";
            var timestamp = File.GetLastWriteTimeUtc(filePath);
            using var stream = File.OpenRead(filePath);
            using var archive = new AsaArchive(NullLogger.Instance, stream, filePath);

            // Header sequence
            var fileVersion = archive.ReadInt32(); // tribeVersion
            
            if (fileVersion < 7) throw new AsaDataException($"Unsupported .arktribe version: {fileVersion}");
            archive.SaveVersion = (short)fileVersion;
            archive.IsArkFile = true;


            _ = archive.ReadBytes(12); // ID, Save Count, Table Offset
            _ = archive.ReadBytes(16); // GUID

            var fileType = archive.ReadString();
            _ = archive.ReadInt32(); 

            var names = archive.ReadStringArray(); // Map Name table strings
            if (names != null && names.Length > 3)
            {
                mapName = names[3]; // Map name
            }

            _ = archive.ReadInt32(); // shouldBeZero
            _ = archive.ReadBytes(16); // Padding/Struct ID
            _ = archive.ReadBytes(1);  // Separator

            //Properties
            var properties = ReadProperties(archive);

            return new ArkFileRecord(timestamp, filePath, mapName, fileVersion, properties);

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
