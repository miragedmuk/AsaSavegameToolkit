using AsaSavegameToolkit.Plumbing.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsaSavegameToolkit.Plumbing.Records
{
    public class ArkFileRecord
    {
        public DateTime TimestampUtc { get; }
        public string Filename { get; }
        public string MapName { get; }
        public List<Property>? Properties { get; }

        public ArkFileRecord(DateTime timestampUtc, string filename, string mapName, List<Property>? properties)
        {
            TimestampUtc = timestampUtc;
            Filename = filename;
            MapName = mapName;
            Properties = properties;
        }
    }
}
