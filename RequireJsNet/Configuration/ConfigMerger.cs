using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class ConfigMerger
    {
        private readonly List<ConfigurationCollection> collections;
        private readonly ConfigurationCollection finalCollection = new ConfigurationCollection();

        public ConfigMerger(List<ConfigurationCollection> collections)
        {
            this.collections = collections;
            finalCollection.Paths = new RequirePaths();
            finalCollection.Paths.PathList = new List<RequirePath>();
            finalCollection.Shim = new RequireShim();
            finalCollection.Shim.ShimEntries = new List<ShimEntry>();
            finalCollection.Map = new RequireMap();
            finalCollection.Map.MapElements = new List<RequireMapElement>();
        }

        public ConfigurationCollection GetMerged()
        {
            foreach (var coll in collections)
            {
                MergePaths(coll);
                MergeShims(coll);
                MergeMaps(coll);
            }
            
            return finalCollection;
        }

        private void MergePaths(ConfigurationCollection collection)
        {
            var finalPaths = finalCollection.Paths.PathList;
            foreach (var path in collection.Paths.PathList)
            {
                var existing = finalPaths.Where(r => r.Key == path.Key).FirstOrDefault();
                if (existing != null)
                {
                    existing.Value = path.Value;
                }
                else
                {
                    finalPaths.Add(path);
                }
            }
        }

        private void MergeShims(ConfigurationCollection collection)
        {
            var finalShims = finalCollection.Shim.ShimEntries;
            foreach (var shim in collection.Shim.ShimEntries)
            {
                var existingKey = finalShims.Where(r => r.For == shim.For).FirstOrDefault();
                if (existingKey != null)
                {
                    existingKey.Exports = shim.Exports;
                    existingKey.Dependencies.AddRange(shim.Dependencies);
                    // distinct by Dependency
                    existingKey.Dependencies = existingKey.Dependencies
                                                            .GroupBy(r => r.Dependency)
                                                            .Select(r => r.LastOrDefault())
                                                            .ToList();
                }
                else
                {
                    finalShims.Add(shim);
                }
            }
        }

        private void MergeMaps(ConfigurationCollection collection)
        {
            var finalMaps = finalCollection.Map.MapElements;
            foreach (var map in collection.Map.MapElements)
            {
                var existingKey = finalMaps.Where(r => r.For == map.For).FirstOrDefault();
                if (existingKey != null)
                {
                    existingKey.Replacements.AddRange(map.Replacements);
                    existingKey.Replacements = existingKey.Replacements
                                                        .GroupBy(r => r.OldKey)
                                                        .Select(r => r.LastOrDefault())
                                                        .ToList();
                }
                else
                {
                    finalMaps.Add(map);
                }
            }
        }


    }
}
