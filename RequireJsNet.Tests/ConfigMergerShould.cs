using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Configuration;
using RequireJsNet.Models;
using RequireJsNet.Tests.DataCreation;
using RequireJsNet.Tests.Extensions;

using Xunit;

namespace RequireJsNet.Tests
{
    public class ConfigMergerShould
    {
        [Fact]
        public void UnitePathsWhenKeysAreDifferent()
        {
            var jqueryPath = new RequirePath { Key = "jquery", Value = "jquery-1.06.2" };
            var amplifyPath = new RequirePath { Key = "amplify", Value = "amplify-10.3.5" };

            var firstCollection = ConfigurationCreators.CreateCollectionWithPaths(jqueryPath);
            var secondCollection = ConfigurationCreators.CreateCollectionWithPaths(amplifyPath);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateCollectionWithPaths(jqueryPath, amplifyPath);

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void OverridePathsWithSameKey()
        {
            var jqueryPath = new RequirePath { Key = "jquery", Value = "jquery-1.06.2" };
            var altJqueryPath = new RequirePath { Key = "jquery", Value = "jquery-1.05.3" };

            var firstCollection = ConfigurationCreators.CreateCollectionWithPaths(jqueryPath);
            var secondCollection = ConfigurationCreators.CreateCollectionWithPaths(altJqueryPath);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateCollectionWithPaths(altJqueryPath);

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

            var firstCollection = ConfigurationCreators.CreateCollectionWithShims(jqueryShim);
            var secondCollection = ConfigurationCreators.CreateCollectionWithShims(amplifyShim);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateCollectionWithShims(jqueryShim, amplifyShim);

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void MergeShimsWithSameKey()
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

            var firstCollection = ConfigurationCreators.CreateCollectionWithShims(jqueryShim);
            var secondCollection = ConfigurationCreators.CreateCollectionWithShims(altJqueryShim);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
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

            var expected = ConfigurationCreators.CreateCollectionWithShims(expectedShim);

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void OverrideExportsValueForShimsWithSameKey()
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

            var firstCollection = ConfigurationCreators.CreateCollectionWithShims(jqueryShim);
            var secondCollection = ConfigurationCreators.CreateCollectionWithShims(altJqueryShim);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            Assert.Equal("jlo", merged.Shim.ShimEntries.FirstOrDefault().Exports);
        }

        [Fact]
        public void CreateSingleMapListForDifferentScripts()
        {
            var scriptAMap = new RequireMapElement 
                                 { 
                                     For = "scriptA", 
                                     Replacements = new List<RequireReplacement>
                                                        {
                                                            new RequireReplacement { OldKey = "jquery", NewKey = "jquery.min"}
                                                        }
                                 };

            var scriptBMap = new RequireMapElement
                                 {
                                     For = "scriptB",
                                     Replacements = new List<RequireReplacement>
                                                        {
                                                            new RequireReplacement { OldKey = "jquery", NewKey = "jquery.custom"}
                                                        }
                                 };


            var firstCollection = ConfigurationCreators.CreateCollectionWithMaps(scriptAMap);
            var secondCollection = ConfigurationCreators.CreateCollectionWithMaps(scriptBMap);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expectedCollection = ConfigurationCreators.CreateEmptyCollection();
            expectedCollection.Map.MapElements.Add(scriptAMap);
            expectedCollection.Map.MapElements.Add(scriptBMap);

            CustomAssert.JsonEquals(expectedCollection, merged);
        }

        [Fact]
        public void UniteMapElementsForSingleScript()
        {
            var firstMap = new RequireMapElement
                                 {
                                     For = "scriptA",
                                     Replacements = new List<RequireReplacement>
                                                        {
                                                            new RequireReplacement { OldKey = "jquery", NewKey = "jquery.min" }
                                                        }
                                 };

            var secondMap = new RequireMapElement
                                {
                                    For = "scriptA",
                                    Replacements = new List<RequireReplacement>
                                                       {
                                                           new RequireReplacement { OldKey = "amplify", NewKey = "amplify.min" }
                                                       }
                                };

            var firstCollection = ConfigurationCreators.CreateCollectionWithMaps(firstMap);
            var secondCollection = ConfigurationCreators.CreateCollectionWithMaps(secondMap);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expectedCollection = ConfigurationCreators.CreateEmptyCollection();
            expectedCollection.Map.MapElements.Add(new RequireMapElement
                                                       {
                                                           For = "scriptA",
                                                           Replacements = new List<RequireReplacement>
                                                                              {
                                                                                  new RequireReplacement { OldKey = "jquery", NewKey = "jquery.min" },
                                                                                  new RequireReplacement { OldKey = "amplify", NewKey = "amplify.min" }
                                                                              }
                                                       });

            CustomAssert.JsonEquals(expectedCollection, merged);
        }

        [Fact]
        public void OverrideMapReplacementOfSameDependencyForSingleScript()
        {
            var firstMap = new RequireMapElement
                               {
                                   For = "scriptA",
                                   Replacements = new List<RequireReplacement>()
                                                      {
                                                        new RequireReplacement { OldKey = "jquery", NewKey = "jquery.min" }
                                                      }
                               };

            var secondMap = new RequireMapElement
                                {
                                    For = "scriptA",
                                    Replacements = new List<RequireReplacement>
                                                       {
                                                        new RequireReplacement { OldKey = "jquery", NewKey = "jquery.custom" }   
                                                       }
                                };

            var firstCollection = ConfigurationCreators.CreateCollectionWithMaps(firstMap);
            var secondCollection = ConfigurationCreators.CreateCollectionWithMaps(secondMap);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expectedCollection = ConfigurationCreators.CreateEmptyCollection();
            expectedCollection.Map.MapElements.Add(new RequireMapElement
                                                       {
                                                           For = "scriptA",
                                                           Replacements = new List<RequireReplacement>
                                                                              {
                                                                                  new RequireReplacement { OldKey = "jquery", NewKey = "jquery.custom"}
                                                                              }
                                                       });

            CustomAssert.JsonEquals(expectedCollection, merged);
        }

        [Fact]
        public void CreateSingleAutoBundleListForDifferentBundleIds()
        {
            var firstBundle = new AutoBundle
                                 {
                                     Id = "bundleA",
                                     OutputPath = @"\Scripts\bundleA.js",
                                     Includes = new List<AutoBundleItem> { new AutoBundleItem { File = "jquery" } },
                                     Excludes = new List<AutoBundleItem> { new AutoBundleItem { File = "jquery" } }
                                 };
            var secondBundle = new AutoBundle
                                   {
                                       Id = "bundleB",
                                       OutputPath = @"\Scripts\bundleB.js",
                                       Includes = new List<AutoBundleItem> { new AutoBundleItem { File = "jquery" } },
                                       Excludes = new List<AutoBundleItem> { new AutoBundleItem { File = "jquery" } }
                                   };

            var firstCollection = ConfigurationCreators.CreateCollectionWithAutoBundles(firstBundle);
            var secondCollection = ConfigurationCreators.CreateCollectionWithAutoBundles(secondBundle);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expectedCollection = ConfigurationCreators.CreateEmptyCollection();
            expectedCollection.AutoBundles.Bundles = new List<AutoBundle>
                                                         {
                                                             firstBundle,
                                                             secondBundle
                                                         };

            CustomAssert.JsonEquals(expectedCollection, merged);
        }

        [Fact]
        public void CreateSingleAutoBundleItemForSameBundleId()
        {
            var firstBundle = new AutoBundle
                                  {
                                      Id = "bundleA",
                                      OutputPath = @"\Scripts\bundleA.js",
                                      Includes = new List<AutoBundleItem> { new AutoBundleItem { File = "jquery" } },
                                      Excludes = new List<AutoBundleItem> { new AutoBundleItem { File = "bootstrap" } }
                                  };

            var secondBundle = new AutoBundle
                                   {
                                       Id = "bundleA",
                                       OutputPath = @"\Scripts\bundleA.js",
                                       Includes = new List<AutoBundleItem> { new AutoBundleItem { File = "amplify" } },
                                       Excludes = new List<AutoBundleItem> { new AutoBundleItem { File = "bforms" } }
                                   };

            var firstCollection = ConfigurationCreators.CreateCollectionWithAutoBundles(firstBundle);
            var secondCollection = ConfigurationCreators.CreateCollectionWithAutoBundles(secondBundle);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expectedCollection = ConfigurationCreators.CreateEmptyCollection();
            expectedCollection.AutoBundles.Bundles = new List<AutoBundle>
                                                        {
                                                                new AutoBundle
                                                                {
                                                                    Id = "bundleA",
                                                                    OutputPath = @"\Scripts\bundleA.js",
                                                                    Includes = new List<AutoBundleItem>
                                                                                    {
                                                                                        new AutoBundleItem { File = "jquery" },
                                                                                        new AutoBundleItem { File = "amplify" }
                                                                                    },
                                                                    Excludes = new List<AutoBundleItem>
                                                                                    {
                                                                                        new AutoBundleItem { File = "bootstrap" },
                                                                                        new AutoBundleItem { File = "bforms" }
                                                                                    }
                                                                } 
                                                        };

            CustomAssert.JsonEquals(expectedCollection, merged);
        }

        [Fact]
        public void OverrideOutputPathForAutoBundleWithSameId()
        {
            var firstBundle = new AutoBundle
            {
                Id = "bundleA",
                OutputPath = @"\Scripts\bundleA.js",
                Includes = new List<AutoBundleItem>(),
                Excludes = new List<AutoBundleItem>()
            };

            var secondBundle = new AutoBundle
            {
                Id = "bundleA",
                OutputPath = @"\Scripts\bundleB.js",
                Includes = new List<AutoBundleItem>(),
                Excludes = new List<AutoBundleItem>()
            };

            var firstCollection = ConfigurationCreators.CreateCollectionWithAutoBundles(firstBundle);
            var secondCollection = ConfigurationCreators.CreateCollectionWithAutoBundles(secondBundle);

            var merger = ConfigurationCreators.CreateDefaultConfigMerger(firstCollection, secondCollection);
            var merged = merger.GetMerged();

            var expectedCollection = ConfigurationCreators.CreateEmptyCollection();
            expectedCollection.AutoBundles.Bundles = new List<AutoBundle>
                                                        {
                                                                new AutoBundle
                                                                {
                                                                    Id = "bundleA",
                                                                    OutputPath = @"\Scripts\bundleB.js",
                                                                    Includes = new List<AutoBundleItem>(),
                                                                    Excludes = new List<AutoBundleItem>()
                                                                }
                                                        };

            CustomAssert.JsonEquals(expectedCollection, merged);
        }

        [Fact]
        public void CreateSingleBundleListForDifferentIds()
        {
            var bundleA = new RequireBundle
                              {
                                  Name = "bundleA",
                              };
            var bundleB = new RequireBundle()
                              {
                                  Name = "bundleB",
                              };

            var firstConfig = ConfigurationCreators.CreateCollectionWithBundles(bundleA);
            var secondConfig = ConfigurationCreators.CreateCollectionWithBundles(bundleB);

            var merger = ConfigurationCreators.CreateBundleProcessingConfigMerger(firstConfig, secondConfig);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateCollectionWithBundles(bundleA, bundleB);

            CustomAssert.JsonEquals(expected, merged);
        }


        [Fact]
        public void CreateSingleBundleItemForSameId()
        {
            var bundleA = new RequireBundle()
                              {
                                  Name = "bundleA" 
                              };
            var secondBundleA = new RequireBundle
                              {
                                  Name = "bundleA"
                              };

            var firstConfig = ConfigurationCreators.CreateCollectionWithBundles(bundleA);
            var secondConfig = ConfigurationCreators.CreateCollectionWithBundles(secondBundleA);

            var merger = ConfigurationCreators.CreateBundleProcessingConfigMerger(firstConfig, secondConfig);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateEmptyCollection();
            expected.Bundles.BundleEntries = new List<RequireBundle>
                                                 {
                                                     new RequireBundle
                                                         {
                                                             Name = "bundleA",
                                                             ParsedIncludes = true
                                                         }
                                                 };

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void OverrideOutputPathForSameId()
        {
            var bundleA = new RequireBundle()
            {
                Name = "bundleA",
                OutputPath = "bundleA"
            };
            var secondBundleA = new RequireBundle
            {
                Name = "bundleA",
                OutputPath = "bundleAgain"
            };

            var firstConfig = ConfigurationCreators.CreateCollectionWithBundles(bundleA);
            var secondConfig = ConfigurationCreators.CreateCollectionWithBundles(secondBundleA);

            var merger = ConfigurationCreators.CreateBundleProcessingConfigMerger(firstConfig, secondConfig);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateEmptyCollection();
            expected.Bundles.BundleEntries = new List<RequireBundle>
                                                 {
                                                     new RequireBundle
                                                         {
                                                             Name = "bundleA",
                                                             ParsedIncludes = true,
                                                             OutputPath = "bundleAgain"
                                                         }
                                                 };

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void SetVirtualToFalseForResultingBundleIfAnyHasFalse()
        {
            var bundleA = new RequireBundle()
            {
                Name = "bundleA",
                IsVirtual = true
            };
            var secondBundleA = new RequireBundle
            {
                Name = "bundleA",
                IsVirtual = false
            };

            var firstConfig = ConfigurationCreators.CreateCollectionWithBundles(bundleA);
            var secondConfig = ConfigurationCreators.CreateCollectionWithBundles(secondBundleA);

            var merger = ConfigurationCreators.CreateBundleProcessingConfigMerger(firstConfig, secondConfig);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateEmptyCollection();
            expected.Bundles.BundleEntries = new List<RequireBundle>
                                                 {
                                                     new RequireBundle
                                                         {
                                                             Name = "bundleA",
                                                             ParsedIncludes = true,
                                                             IsVirtual = false
                                                         }
                                                 };

            CustomAssert.JsonEquals(expected, merged);
        }

        [Fact]
        public void UnifyBundleItemsForSameId()
        {
            var bundleA = new RequireBundle
                              {
                                  Name = "bundleA",
                                  BundleItems = new List<BundleItem> { new BundleItem { ModuleName = "jquery" } }
                              };
            var bundleB = new RequireBundle()
                              {
                                  Name = "bundleA",
                                  BundleItems = new List<BundleItem> { new BundleItem { ModuleName = "amplify" } }
                              };

            var firstConfig = ConfigurationCreators.CreateCollectionWithBundles(bundleA);
            var secondConfig = ConfigurationCreators.CreateCollectionWithBundles(bundleB);

            var merger = ConfigurationCreators.CreateBundleProcessingConfigMerger(firstConfig, secondConfig);
            var merged = merger.GetMerged();

            var expected = ConfigurationCreators.CreateEmptyCollection();
            expected.Bundles.BundleEntries = new List<RequireBundle>
                                                 {
                                                     new RequireBundle
                                                         {
                                                             Name = "bundleA",
                                                             ParsedIncludes = true,
                                                             BundleItems = new List<BundleItem>
                                                                               {
                                                                                   new BundleItem { ModuleName = "jquery", RelativePath = "jquery"},
                                                                                   new BundleItem { ModuleName = "amplify", RelativePath = "amplify" }
                                                                               }
                                                         }
                                                 };

            CustomAssert.JsonEquals(expected, merged);
        }
    }
}
