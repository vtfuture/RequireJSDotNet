using System;
using System.Collections.Generic;
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
        }

        public ConfigurationCollection GetMerged()
        {
            foreach (var coll in collections)
            {
                MergePaths(coll);
                MergeShims(coll);    
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
                                                            .Select(r => r.FirstOrDefault())
                                                            .ToList();
                }
            }
        }

        


    }
}
