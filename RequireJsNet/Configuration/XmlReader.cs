using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class XmlReader : IConfigReader
    {
        private readonly string path;

        public XmlReader(string path)
        {
            this.path = path;
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public ConfigurationCollection ReadConfig()
        {
            using (var stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var doc = XDocument.Load(stream);
                var collection = new ConfigurationCollection();
                collection.FilePath = Path;
                collection.Paths = GetPaths(doc.Root);
                collection.Shim = GetShim(doc.Root);
                collection.Map = GetMap(doc.Root);
                return collection;    
            }
        }

        private RequirePaths GetPaths(XElement root)
        {
            var paths = new RequirePaths();
            paths.PathList = new List<RequirePath>();
            var pathEl = root.Descendants("paths").FirstOrDefault();
            if (pathEl != null)
            {
                paths.PathList = pathEl.Descendants("path")
                                        .Select(r => new RequirePath
                                                    {
                                                        Key = r.Attribute("key").Value,
                                                        Value = r.Attribute("value").Value
                                                    }).ToList();
            }

            return paths;
        }

        private RequireShim GetShim(XElement root)
        {
            var shim = new RequireShim();
            shim.ShimEntries = new List<ShimEntry>();
            var shimEl = root.Descendants("shim").FirstOrDefault();
            if (shimEl != null)
            {
                shim.ShimEntries = shimEl.Descendants("dependencies")
                                        .Select(ShimEntryReader)
                                        .ToList();
            }

            return shim;
        }

        private ShimEntry ShimEntryReader(XElement element)
        {
            return new ShimEntry
            {
                Exports = element.Attribute("exports") != null ? element.Attribute("exports").Value : string.Empty,
                For = element.Attribute("for").Value,
                Dependencies = DependenciesReader(element)
            };
        }

        private List<RequireDependency> DependenciesReader(XElement element)
        {
            return element.Descendants("add")
                        .Select(x => new RequireDependency
                        {
                            Dependency = x.Attribute("dependency").Value
                        }).ToList();
        }

        private RequireMap GetMap(XElement root)
        {
            var map = new RequireMap();
            map.MapElements = new List<RequireMapElement>();
            var mapEl = root.Descendants("map").FirstOrDefault();
            if (mapEl != null)
            {
                map.MapElements = mapEl.Descendants("replace")
                                        .Select(MapElementReader)
                                        .ToList();
            }

            return map;
        }

        private RequireMapElement MapElementReader(XElement element)
        {
            return new RequireMapElement
                   {
                       For = element.Attribute("for").Value,
                       Replacements = ReplacementsReader(element)
                   };
        }

        private List<RequireReplacement> ReplacementsReader(XElement element)
        {
            return element.Descendants("add")
                            .Select(x => new RequireReplacement
                                         {
                                             NewKey = x.Attribute("new").Value,
                                             OldKey = x.Attribute("old").Value
                                         }).ToList();
        }
    }
}
