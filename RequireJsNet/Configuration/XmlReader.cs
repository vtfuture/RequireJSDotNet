using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class XmlReader : IConfigReader
    {
        private readonly string _path;
        public string Path { get { return _path; } }

        public XmlReader(string path)
        {
            _path = path;
        }

        public ConfigurationCollection ReadConfig()
        {
            RequireXmlConfig config;
            using (var reader = new FileStream(Path, FileMode.Open))
            {
                var serializer = new XmlSerializer(typeof(RequireXmlConfig));
                config = serializer.Deserialize(reader) as RequireXmlConfig;
            }
            if (config == null)
            {
                throw new Exception(string.Format("Could not read configuration from {0}. Please check that the file is valid.",
                                                    Path));
            }
            return MapToCollection(config);
        }

        private ConfigurationCollection MapToCollection(RequireXmlConfig config)
        {
            var collection = new ConfigurationCollection
            {
                FilePath = Path
            };

            collection.Paths = config.Paths ?? new RequirePaths();
            collection.Shim = config.Shim ?? new RequireShim();

            collection.Paths.PathList = collection.Paths.PathList ?? new List<RequirePath>();
            collection.Paths.PathList = collection.Paths.PathList.Where(r => r != null).ToList();

            collection.Shim.ShimEntries = collection.Shim.ShimEntries ?? new List<ShimEntry>();
            collection.Shim.ShimEntries = collection.Shim.ShimEntries.Where(r => r != null).ToList();
            return collection;
        }
    }
}
