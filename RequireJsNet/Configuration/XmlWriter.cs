// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class XmlWriter : IConfigWriter
    {
        private readonly ConfigLoaderOptions options;

        public XmlWriter(string path, ConfigLoaderOptions options)
        {
            this.options = options;
            Path = path;
        }

        public string Path { get; private set; }

        public void WriteConfig(ConfigurationCollection conf)
        {
            var paths = this.GetPaths(conf.Paths);
            var overrides = this.GetOverrides(conf.Overrides);
            var document = new XDocument(new XElement("configuration", paths, overrides));
            File.WriteAllText(Path, document.ToString());
        }

        public XElement GetPaths(RequirePaths paths)
        {
            if (paths == null || !paths.PathList.Any())
            {
                return null;
            }

            var pathsEl = new XElement("paths", paths.PathList.Select(r => new XElement("path", new XAttribute("key", r.Key), new XAttribute("value", r.Value))));

            return pathsEl;
        }

        public List<XElement> GetOverrides(List<CollectionOverride> overrides)
        {
            if (overrides == null || !overrides.Any())
            {
                return null;
            }

            var elementList = new List<XElement>();
            foreach (var collectionOverride in overrides)
            {
                var el = new XElement(
                    "collectionOverride",
                    new XAttribute("bundleId", collectionOverride.BundleId),
                    this.GetPaths(collectionOverride.Paths),
                    collectionOverride.BundledScripts.Select(r => new XElement("bundledScript", new XAttribute("path", r))));
                elementList.Add(el);
            }

            return elementList;
        }


    }
}
