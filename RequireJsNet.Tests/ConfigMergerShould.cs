using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Configuration;
using RequireJsNet.Models;
using RequireJsNet.Tests.Extensions;

using Xunit;

namespace RequireJsNet.Tests
{
    public class ConfigMergerShould
    {
        [Fact]
        public void UnitePathsWhenDifferentKeys()
        {
            var jqueryPath = new RequirePath { Key = "jquery", Value = "jquery-1.06.2" };
            var amplifyPath = new RequirePath { Key = "amplify", Value = "amplify-10.3.5" };

            var firstCollection = CreateCollectionWithPaths(jqueryPath);
            var secondCollection = CreateCollectionWithPaths(amplifyPath);

            var merger = CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expected = CreateCollectionWithPaths(jqueryPath, amplifyPath);

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void OverridePathsWithSameKey()
        {
            var jqueryPath = new RequirePath { Key = "jquery", Value = "jquery-1.06.2" };
            var altJqueryPath = new RequirePath { Key = "jquery", Value = "jquery-1.05.3" };

            var firstCollection = CreateCollectionWithPaths(jqueryPath);
            var secondCollection = CreateCollectionWithPaths(altJqueryPath);

            var merger = CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expected = CreateCollectionWithPaths(altJqueryPath);

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void UniteShimsWithDifferentKeys()
        {
            var jqueryShim = new ShimEntry
                                 {
                                     For = "jquery",
                                     Dependencies = new List<RequireDependency>
                                                        {
                                                            new RequireDependency { Dependency = "depA" },
                                                            new RequireDependency { Dependency = "depB" }
                                                        }
                                 };

            var amplifyShim = new ShimEntry
                                  {
                                      For = "amplify",
                                      Dependencies = new List<RequireDependency>
                                                         {
                                                             new RequireDependency { Dependency = "depC" },
                                                             new RequireDependency { Dependency = "depD" }
                                                         }
                                  };

            var firstCollection = CreateCollectionWithShim(jqueryShim);
            var secondCollection = CreateCollectionWithShim(amplifyShim);

            var merger = CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expected = CreateCollectionWithShim(jqueryShim, amplifyShim);

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void MergeShismWithSameKey()
        {
            var jqueryShim = new ShimEntry
            {
                For = "jquery",
                Dependencies = new List<RequireDependency>
                                                        {
                                                            new RequireDependency { Dependency = "depA" },
                                                            new RequireDependency { Dependency = "depB" }
                                                        }
            };

            var altJqueryShim = new ShimEntry
            {
                For = "jquery",
                Dependencies = new List<RequireDependency>
                                                         {
                                                             new RequireDependency { Dependency = "depB" },
                                                             new RequireDependency { Dependency = "depD" }
                                                         }
            };

            var firstCollection = CreateCollectionWithShim(jqueryShim);
            var secondCollection = CreateCollectionWithShim(altJqueryShim);

            var merger = CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expectedShim = new ShimEntry
                                   {
                                       For = "jquery",
                                       Dependencies = new List<RequireDependency>
                                                       {
                                                           new RequireDependency { Dependency = "depA" },
                                                           new RequireDependency { Dependency = "depB" },
                                                           new RequireDependency { Dependency = "depD" }
                                                       }
                                   };

            var expected = CreateCollectionWithShim(expectedShim);

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void OverrideExportsForShimsWithSameKey()
        {
            var jqueryShim = new ShimEntry
            {
                For = "jquery",
                Dependencies = new List<RequireDependency>(),
                Exports = "jqu"
            };

            var altJqueryShim = new ShimEntry
            {
                For = "jquery",
                Dependencies = new List<RequireDependency>(),
                Exports = "jlo"
            };

            var firstCollection = CreateCollectionWithShim(jqueryShim);
            var secondCollection = CreateCollectionWithShim(altJqueryShim);

            var merger = CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            Assert.Equal("jlo", merged.Shim.ShimEntries.FirstOrDefault().Exports);
        }

        private ConfigurationCollection CreateEmptyCollection()
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

        private ConfigurationCollection CreateCollectionWithPaths(params RequirePath[] paths)
        {
            var collection = CreateEmptyCollection();
            collection.Paths = new RequirePaths();
            collection.Paths.PathList = paths.ToList();
            return collection;
        }

        private ConfigurationCollection CreateCollectionWithShim(params ShimEntry[] shim)
        {
            var collection = CreateEmptyCollection();
            collection.Shim = new RequireShim();
            collection.Shim.ShimEntries = shim.ToList();
            return collection;
        }

        private ConfigMerger CreateDefaultConfigMerger(params ConfigurationCollection[] collections)
        {
            return new ConfigMerger(collections.ToList(), new ConfigLoaderOptions());   
        }
    }
}
