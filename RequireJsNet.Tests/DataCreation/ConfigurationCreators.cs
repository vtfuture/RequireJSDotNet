using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Configuration;
using RequireJsNet.Models;

namespace RequireJsNet.Tests.DataCreation
{
    internal static class ConfigurationCreators
    {
        public static ConfigurationCollection CreateEmptyCollection()
        {
            var collection = new ConfigurationCollection();
            collection.Paths = new RequirePaths();
            collection.Paths.PathList = new List<RequirePath>();

            collection.AutoBundles = new AutoBundles();
            collection.AutoBundles.Bundles = new List<AutoBundle>();

            collection.Bundles = new RequireBundles();
            collection.Bundles.BundleEntries = new List<RequireBundle>();

            collection.Map = new RequireMap();
            collection.Map.MapElements = new List<RequireMapElement>();

            collection.Overrides = new List<CollectionOverride>();
            collection.Shim = new RequireShim();
            collection.Shim.ShimEntries = new List<ShimEntry>();

            return collection;
        }

        public static ConfigurationCollection CreateCollectionWithPaths(params RequirePath[] paths)
        {
            var collection = CreateEmptyCollection();
            collection.Paths = new RequirePaths();
            collection.Paths.PathList = paths.ToList();
            return collection;
        }

        public static ConfigurationCollection CreateCollectionWithShims(params ShimEntry[] shim)
        {
            var collection = CreateEmptyCollection();
            collection.Shim.ShimEntries = shim.ToList();
            return collection;
        }

        public static ConfigurationCollection CreateCollectionWithMaps(params RequireMapElement[] maps)
        {
            var collection = CreateEmptyCollection();
            collection.Map.MapElements = maps.ToList();

            return collection;
        }

        public static ConfigurationCollection CreateCollectionWithAutoBundles(params AutoBundle[] autoBundles)
        {
            var collection = CreateEmptyCollection();
            collection.AutoBundles.Bundles = autoBundles.ToList();

            return collection;
        }

        public static ConfigurationCollection CreateCollectionWithBundles(params RequireBundle[] bundles)
        {
            var collection = CreateEmptyCollection();
            collection.Bundles.BundleEntries = bundles.ToList();
            return collection;
        }

        public static ConfigMerger CreateDefaultConfigMerger(params ConfigurationCollection[] collections)
        {
            return new ConfigMerger(collections.ToList(), new ConfigLoaderOptions());
        }

        public static ConfigMerger CreateBundleProcessingConfigMerger(params ConfigurationCollection[] collections)
        {
            return new ConfigMerger(
                                collections.ToList(), 
                                new ConfigLoaderOptions
                                    {
                                        LoadOverrides = false,
                                        ProcessAutoBundles = false,
                                        ProcessBundles = true
                                    });   
        }
    }
}
