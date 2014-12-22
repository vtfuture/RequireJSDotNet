using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Configuration;
using RequireJsNet.Models;
using RequireJsNet.Tests.DataCreation;
using RequireJsNet.Tests.Extensions;
using RequireJsNet.Tests.TestImplementations;

using Xunit;

namespace RequireJsNet.Tests
{
    public class JsonReaderShould
    {
        [Fact]
        public void NotThrowExceptionOnEmptyJson()
        {
            var reader = ReadJson(new TestFileReader());

            Assert.True(true);
        }

        [Fact]
        public void ReadCompactedPath()
        {
            var config = ReadJson(new TestFileReader());

            var expected = ConfigurationCreators.CreateCollectionWithPaths(new RequirePath
                                                                               {
                                                                                   Key = "jquery",
                                                                                   Value = "jquery-1.10.2"
                                                                               });

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadExpandedPath()
        {
            var config = ReadJson(new TestFileReader());

            var expected = ConfigurationCreators.CreateCollectionWithPaths(new RequirePath
                                                                               {
                                                                                   Key = "jquery-validate",
                                                                                   Value = "jquery.validate",
                                                                                   DefaultBundle = "jqValidate"
                                                                               });

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadShimWithExports()
        {
            var config = ReadJson(new TestFileReader());
            var shim = new ShimEntry
                           {
                               For = "jquery-validate",
                               Dependencies =
                                   new List<RequireDependency>
                                       {
                                           new RequireDependency { Dependency = "jquery" }
                                       },
                               Exports = "jqVal"
                           };
            var expected = ConfigurationCreators.CreateCollectionWithShims(shim);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadShimWithoutExports()
        {
            var config = ReadJson(new TestFileReader());
            var shim = new ShimEntry
                           {
                               For = "jquery-validate-unobtrusive",
                               Dependencies =
                                   new List<RequireDependency>
                                       {
                                           new RequireDependency
                                               {
                                                   Dependency = "jquery"
                                               },
                                           new RequireDependency
                                               {
                                                   Dependency = "jquery-validate"
                                               }
                                       },
                           };
            var expected = ConfigurationCreators.CreateCollectionWithShims(shim);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadMap()
        {
            var config = ReadJson(new TestFileReader());
            var map = new RequireMapElement
                          {
                              For = "controllers/root/home/complexLoad",
                              Replacements =
                                  new List<RequireReplacement>
                                      {
                                          new RequireReplacement
                                              {
                                                  OldKey = "req1",
                                                  NewKey = "req2"
                                              }
                                      }
                          };
            var expected = ConfigurationCreators.CreateCollectionWithMaps(map);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadBundleWithCompactedItems()
        {
            var config = ReadJson(new TestFileReader());
            var bundle = new RequireBundle
                             {
                                 Name = "jqueryBundle",
                                 BundleItems =
                                     new List<BundleItem>
                                         {
                                             new BundleItem
                                                 {
                                                     ModuleName = "jquery"
                                                 }
                                         }
                             };
            var expected = ConfigurationCreators.CreateCollectionWithBundles(bundle);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadBundleWithExpandedItems()
        {
            var config = ReadJson(new TestFileReader());
            var bundle = new RequireBundle
                             {
                                 Name = "full",
                                 BundleItems =
                                     new List<BundleItem>
                                         {
                                             new BundleItem
                                                 {
                                                     ModuleName = "bootstrap",
                                                     CompressionType = "none"
                                                 },
                                             new BundleItem
                                                 {
                                                     ModuleName = "amplify",
                                                     CompressionType = "standard"
                                                 }
                                         }
                             };
            var expected = ConfigurationCreators.CreateCollectionWithBundles(bundle);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadBundleWithIncludes()
        {
            var config = ReadJson(new TestFileReader());
            var bundle = new RequireBundle
                             {
                                 Name = "full",
                                 Includes = new List<string>
                                                {
                                                    "jqueryBundle", "jqvalUnobtrusive", "jqValidate"
                                                }
                             };
            var expected = ConfigurationCreators.CreateCollectionWithBundles(bundle);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadBundleWithOutputPath()
        {
            var config = ReadJson(new TestFileReader());
            var bundle = new RequireBundle { Name = "jqueryBundle", OutputPath = @"Bundles\full.min.js" };
            var expected = ConfigurationCreators.CreateCollectionWithBundles(bundle);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadBundleWithSpecifiedVirtual()
        {
            var config = ReadJson(new TestFileReader());
            var bundle = new RequireBundle { Name = "jqueryBundle", IsVirtual = true };
            var expected = ConfigurationCreators.CreateCollectionWithBundles(bundle);

            CustomAssert.JsonEquals(expected, config);
        }

        [Fact]
        public void ReadAutoBundles()
        {
            var config = ReadJson(new TestFileReader());
            var autoBundle = new AutoBundle
                                 {
                                     Id = "full",
                                     OutputPath = @"bundles\auto\",
                                     Includes =
                                         new List<AutoBundleItem>
                                             {
                                                 new AutoBundleItem
                                                     {
                                                         Directory = @"\controllers\Root\",
                                                     },
                                                 new AutoBundleItem
                                                     {
                                                         File = "jquery-1.10.2"
                                                     }
                                             },
                                     Excludes = new List<AutoBundleItem>(),
                                     ContainingConfig = "JsonReaderShould.ReadAutoBundles.json"
                                 };
            var expected = ConfigurationCreators.CreateCollectionWithAutoBundles(autoBundle);

            CustomAssert.JsonEquals(expected, config);
        }

        private ConfigurationCollection ReadJson(TestFileReader reader)
        {
            var jsonReader = new JsonReader(
                                            reader.FilePath, 
                                            new ConfigLoaderOptions
                                                {
                                                    ProcessAutoBundles = true,
                                                    ProcessBundles = true,
                                                    LoadOverrides = true
                                                }, 
                                            reader);
            var config = jsonReader.ReadConfig();
            config.FilePath = null;
            if (config.Bundles == null)
            {
                config.Bundles = new RequireBundles { BundleEntries = new List<RequireBundle>() };
            }

            return config;
        }

    }
}
