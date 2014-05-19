using System.Collections.Generic;
using System.Linq;

using RequireJsNet.Models;
using System;

namespace RequireJsNet.Configuration
{
    internal class ConfigMerger
    {
        private readonly List<ConfigurationCollection> collections;
        private readonly ConfigurationCollection finalCollection = new ConfigurationCollection();

        private readonly ConfigLoaderOptions options;

        public ConfigMerger(List<ConfigurationCollection> collections, ConfigLoaderOptions options)
        {
            this.options = options;
            this.collections = collections;
            finalCollection.Paths = new RequirePaths();
            finalCollection.Paths.PathList = new List<RequirePath>();
            finalCollection.Shim = new RequireShim();
            finalCollection.Shim.ShimEntries = new List<ShimEntry>();
            finalCollection.Map = new RequireMap();
            finalCollection.Map.MapElements = new List<RequireMapElement>();
            finalCollection.Bundles = new RequireBundles();
            finalCollection.Bundles.BundleEntries = new List<RequireBundle>();
            finalCollection.AutoBundles = new AutoBundles();
            finalCollection.AutoBundles.Bundles = new List<AutoBundle>();
        }

        public ConfigurationCollection GetMerged()
        {
            foreach (var coll in collections)
            {
                MergePaths(coll);
                MergeShims(coll);
                MergeMaps(coll);
                this.MergeAutoBundles(coll);
            }

            if (options.ProcessBundles)
            {
                this.MergeBundles(this.collections);    
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

        private void MergeAutoBundles(ConfigurationCollection collection)
        {
            var finalAutoBundles = finalCollection.AutoBundles.Bundles;
            foreach (var autoBundle in collection.AutoBundles.Bundles)
            {
                var existing = finalAutoBundles.Where(r => r.Id == autoBundle.Id).FirstOrDefault();
                if (existing != null)
                {
                    foreach (var include in autoBundle.Includes)
                    {
                        existing.Includes.Add(include);
                    }

                    foreach (var exclude in autoBundle.Excludes)
                    {
                        existing.Excludes.Add(exclude);
                    }
                }
                else
                {
                    finalAutoBundles.Add(autoBundle);
                }
            }
        }

        private void MergeBundles(List<ConfigurationCollection> collection)
        {
            this.MergeExistingBundles(collection);
            this.ResolveDefaultBundles();
            this.ResolveBundleItemsRelativePaths();
            this.ResolveBundleIncludes();
            this.EnsureNoDuplicatesInBundles();
        }

        private void ResolveDefaultBundles()
        {
            var groupedPaths =
                finalCollection.Paths.PathList.Where(r => !string.IsNullOrWhiteSpace(r.DefaultBundle))
                    .GroupBy(r => r.DefaultBundle)
                    .Select(r => new
                    {
                        Bundle = r.Key,
                        Items = r.ToList()
                    }).ToList();

            foreach (var bundleGroup in groupedPaths)
            {
                var itemList = bundleGroup.Items.Select(r => new BundleItem
                {
                    CompressionType = "standard",
                    ModuleName = r.Key
                }).ToList();

                var existingBundle = finalCollection.Bundles.BundleEntries.Where(r => r.Name == bundleGroup.Bundle).FirstOrDefault();
                if (existingBundle == null)
                {
                    finalCollection.Bundles.BundleEntries.Add(new RequireBundle
                    {
                        Includes = new List<string>(),
                        IsVirtual = true,
                        BundleItems = itemList,
                        Name = bundleGroup.Bundle
                    });
                }
                else
                {
                    existingBundle.BundleItems = itemList.Concat(existingBundle.BundleItems).ToList();
                }
            }
        }

        private void ResolveBundleIncludes()
        {
            var rootBundles = finalCollection.Bundles.BundleEntries.Where(r => !r.Includes.Any()).ToList();
            if (!rootBundles.Any())
            {
                throw new Exception("Could not find any bundle with no dependency. Check your config for cyclic dependencies.");
            }

            rootBundles.ForEach(r => r.ParsedIncludes = true);
            var maxIterations = 500;
            var currentIt = 0;
            while (finalCollection.Bundles.BundleEntries.Where(r => !r.ParsedIncludes).Any())
            {
                // shouldn't really happen, but we'll use this as a safeguard against an endless loop for the moment
                if (currentIt > maxIterations)
                {
                    throw new Exception("Maximum number of iterations exceeded. Check your config for cyclick dependencies");
                }

                // get all the bundles that have parents with resolved dependencies and haven't been resolved themselves
                var parsableBundles = GetBundlesWithResolvedParents();

                // we've checked earlier if there are any bundles that haven't been parsed
                // if there are bundles that haven't been parsed but there aren't any we can parse, something went wrong
                if (!parsableBundles.Any())
                {
                    throw new Exception("Could not parse bundle includes. Check your config for cyclic dependencies.");
                }

                foreach (var bundle in parsableBundles)
                {
                    // store a reference to the old list
                    var oldItemList = bundle.BundleItems;

                    // instantiate a new one so that when we're done we can append the old scripts
                    bundle.BundleItems = new List<BundleItem>();
                    var parents = bundle.Includes.Select(r => GetBundleByName(r)).ToList();
                    foreach (var parent in parents)
                    {
                        bundle.BundleItems.AddRange(parent.BundleItems);
                    }

                    bundle.BundleItems.AddRange(oldItemList);
                    bundle.BundleItems = bundle.BundleItems.GroupBy(r => r.RelativePath).Select(r => r.FirstOrDefault()).ToList();
                    bundle.ParsedIncludes = true;
                }

                currentIt++;
            }
        }

        private void ResolveBundleItemsRelativePaths()
        {
            foreach (var item in finalCollection.Bundles.BundleEntries.SelectMany(r => r.BundleItems))
            {
                var finalName = item.ModuleName;

                // this will only go 1 level deep, other cases should be taken into account
                if (finalCollection.Paths.PathList.Where(r => r.Key == finalName).Any())
                {
                    var finalEl = finalCollection.Paths.PathList.Where(r => r.Key == finalName).FirstOrDefault();
                    if (finalEl == null)
                    {
                        throw new Exception("Could not find path item with name = " + finalName);
                    }

                    finalName = finalEl.Value;
                }

                item.RelativePath = finalName;
            }
        }

        private List<RequireBundle> GetBundlesWithResolvedParents()
        {
            var allBundles = finalCollection.Bundles.BundleEntries;
            var result = new List<RequireBundle>();
            foreach (var bundle in allBundles.Where(r => !r.ParsedIncludes))
            {
                // for each include, get its bundle.ParsedIncludes property
                // select those that don't have their parents resolved
                // if any such items exist, it means that the item's parents haven't been resolved
                var parentsResolved = !bundle.Includes
                                        .Select(r => GetBundleByName(r).ParsedIncludes)
                                        .Where(r => !r)
                                        .Any();
                if (parentsResolved)
                {
                    result.Add(bundle);
                }
            }

            return result;
        }

        private RequireBundle GetBundleByName(string name)
        {
            var result = finalCollection.Bundles.BundleEntries.Where(r => r.Name == name).FirstOrDefault();
            if (result == null)
            {
                throw new Exception("Could not find bundle with name " + name);
            }

            return result;
        }

        private void EnsureNoDuplicatesInBundles()
        {
            foreach (var requireBundle in finalCollection.Bundles.BundleEntries)
            {
                requireBundle.BundleItems = requireBundle.BundleItems
                                                            .GroupBy(r => r.ModuleName)
                                                            .Select(r => r.FirstOrDefault())
                                                            .ToList();
            }
        }

        private void MergeExistingBundles(List<ConfigurationCollection> collection)
        {
            foreach (var configuration in collection)
            {
                foreach (var bundle in configuration.Bundles.BundleEntries)
                {
                    var existingBundle = finalCollection.Bundles.BundleEntries.Where(r => r.Name == bundle.Name).FirstOrDefault();
                    if (existingBundle == null)
                    {
                        existingBundle = bundle;
                        finalCollection.Bundles.BundleEntries.Add(existingBundle);
                    }
                    else
                    {
                        // add without checking for duplicates, we'll filter them out later
                        existingBundle.BundleItems.AddRange(bundle.BundleItems);
                    }
                }
            }
            
        }
    }
}
