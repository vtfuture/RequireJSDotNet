// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.Linq;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class ConfigOverrider
    {
        public void Override(ConfigurationCollection collection, string entryPoint)
        {
            if (collection.Overrides == null || !collection.Overrides.Any())
            {
                return;
            }

            var relevantOverrides = this.GetRelevantOverrides(collection, entryPoint);
            var merged = this.MergeOverrides(relevantOverrides);
            this.ApplyOverride(collection, merged);
        }

        private List<CollectionOverride> GetRelevantOverrides(ConfigurationCollection collection, string entryPoint)
        {
            var result = collection.Overrides.ToList();

            // if there's a single bundle, just return it
            if (result.Count == 1)
            {
                return result;
            }
            
            // otherwise put the bundle containing the entrypoint last, if such a bundle exists, so that it gets priority
            var entryBundle = result.Where(r => r.BundledScripts.Where(x => x.ToLower() == entryPoint.ToLower()).Any()).FirstOrDefault();
            if (entryBundle != null)
            {
                result.Remove(entryBundle);
                result.Add(entryBundle);    
            }

            return result;
        }

        private void ApplyOverride(ConfigurationCollection collection, CollectionOverride collOverride)
        {
            this.ApplyMapOverride(collection.Map, collOverride.Map);
            this.ApplyPathsOverride(collection.Paths, collOverride.Paths);
            this.ApplyShimOverride(collection.Shim, collOverride.Shim);
        }

        // TODO: these functions are very similar to the merge ones in ConfigMerger,
        // maybe do something about this
        private void ApplyMapOverride(RequireMap originalMap, RequireMap overrideMap)
        {
            foreach (var mapEl in overrideMap.MapElements)
            {
                var existing = originalMap.MapElements.Where(r => r.For.ToLower() == mapEl.For.ToLower()).FirstOrDefault();
                if (existing != null)
                {
                    var finalReplacements = existing.Replacements
                                                        .Union(mapEl.Replacements)
                                                        .GroupBy(r => r.OldKey)
                                                        .Select(r => r.FirstOrDefault())
                                                        .ToList();
                    existing.Replacements = finalReplacements;
                }
                else
                {
                    originalMap.MapElements.Add(mapEl);
                }
            }
        }

        private void ApplyPathsOverride(RequirePaths originalPaths, RequirePaths overridePaths)
        {
            foreach (var pathEl in overridePaths.PathList)
            {
                var existing = originalPaths.PathList.Where(r => r.Key.ToLower() == pathEl.Key.ToLower()).FirstOrDefault();
                if (existing != null)
                {
                    existing.Value = pathEl.Value;
                }
                else
                {
                    originalPaths.PathList.Add(pathEl);
                }

                var existingValue = originalPaths.PathList.Where(r => r.Value.ToLower() == pathEl.Key.ToLower()).FirstOrDefault();
                if (existingValue != null)
                {
                    existingValue.Value = pathEl.Value;
                }
            }
        }

        private void ApplyShimOverride(RequireShim originalShim, RequireShim overrideShim)
        {
            // no need to implement this, at least not for now
        }
        

        // last one wins, there might be a better way to do this that would result in loading the minimum amount of bundles,
        // but those cases should be rare if you've configured them properly
        private CollectionOverride MergeOverrides(List<CollectionOverride> overrides)
        {
            var result = overrides.FirstOrDefault();
            overrides.Remove(result);
            foreach (var collOverride in overrides)
            {
                this.ApplyMapOverride(result.Map, collOverride.Map);
                this.ApplyPathsOverride(result.Paths, collOverride.Paths);
                this.ApplyShimOverride(result.Shim, collOverride.Shim);
            }

            return result;
        }
    }
}
