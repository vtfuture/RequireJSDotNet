using RequireJsNet.Compressor.AutoDependency;
using RequireJsNet.Compressor.Tests.Helpers;
using RequireJsNet.Configuration;
using RequireJsNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace RequireJSNet.Compressor.Tests.Tests.AutoBundleConfigProcessor
{
    public class ParseConfigsShould
    {
        readonly string projectPath;

        public ParseConfigsShould()
        {
            var executingDirectory = Directory.GetCurrentDirectory();
            var directory = Path.Combine(executingDirectory, @"..\..\TestData\ParseConfigsShould");
            projectPath = Path.GetFullPath(directory);
        }

        [Fact]
        public void ThrowIfProjectPathDoesntExist()
        {
            var projectPath = @"C:\NonExistingDirectory";
            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, "", "", null, System.Text.Encoding.UTF8);
            Assert.Throws<DirectoryNotFoundException>(() =>
            {
                compressor.ParseConfigs();
            });
        }

        [Fact]
        public void ThrowIfFilesPathsAreEmpty()
        {
            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, "", "", null, System.Text.Encoding.UTF8);
            Assert.Throws<ArgumentNullException>(() =>
            {
                compressor.ParseConfigs();
            });
        }

        [Fact]
        public void ThrowIfConfigIsNotProvided()
        {
            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, "", "", new List<string>(), System.Text.Encoding.UTF8);
            Assert.Throws<ArgumentException>(() =>
            {
                compressor.ParseConfigs();
            });
        }

        [Fact]
        public void FindConfigs()
        {
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\LoadAComplexDependencyTree.json" });
            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, "", "", filesPaths, System.Text.Encoding.UTF8);
            compressor.ParseConfigs();
            //TODO: We should find multiple files
            //TODO: We should find the override file if it exists
        }

        [Fact]
        public void BundleComplexStructure()
        {
            var bundleId = "bundle1";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\LoadAComplexDependencyTree.json" });
            var entrypointOverride = "";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\BundleIncludedDirectory\a.js", null)
                    {
                        FileContent ="define('bundleincludeddirectory/a', [],function () {\r\n    console.log('a.js');\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\BundleIncludedDirectory\b.js", null)
                    {
                        FileContent ="define('bundleincludeddirectory/b', [],function () {\r\n    console.log('b.js');\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\c.js", null)
                    {
                        FileContent ="define('c', [],function () {\r\n    console.log('file-c.js');\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\b.js", null)
                    {
                        FileContent ="define('b', [\"c\", \"d\"], function (c, d) {\r\n    console.log('file-b.js');\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\a.js", null)
                    {
                        FileContent ="define('a', [\"require\", \"exports\", \"b\"], function (require, exports, b) {\r\n    console.log('file-a.js');\r\n});"
                    }
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void BundleIncludeWithPath()
        {
            var bundleId = "bundle2";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\BundleIncludeWithPath.json" });
            var entrypointOverride = "";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\BundleIncludedDirectory\a.js", null)
                    {
                        FileContent ="define('bundleincludeddirectory/a', [],function () {\r\n    console.log('a.js');\r\n});"
                    }
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void MakeCorrectRelativePathWhenEntryPointEndsWithDirSeparator()
        {
            var bundleId = "bundle4";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\UseCorrectModuleRelativePathWhenOutsideBasePath.json" });
            var entrypointOverride = projectPath + @"\Scripts\";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\d.js", null)
                    {
                        FileContent ="define('d', [],function () {\r\n    console.log('file-d.js');\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\otherscripts\e.js", null)
                    {
                        FileContent ="define('externalfile', ['d'], function () {\r\nconsole.log('file-e.js');\r\n});"
                    }
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree()
        {
            var bundleId = "bundle5";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree.json" });
            var entrypointOverride = projectPath + @"\Scripts\";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\otherscripts\e.js", null)
                    {
                        FileContent ="define('externalfile', [], function () {\r\nconsole.log('file-e.js');\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree\g.js", null)
                    {
                        FileContent ="define('makecorrectrelativepathwhendependedfilerequiresupwardsintree/g', ['externalfile'],function () {\r\n    console.log('file-g.js');\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree\f.js", null)
                    {
                        FileContent ="define('makecorrectrelativepathwhendependedfilerequiresupwardsintree/f', ['./g'],function () {\r\n    console.log('starting-f.js');\r\n});"
                    }
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void SupportEC6()
        {
            var bundleId = "bundle5";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\SupportEC6.json" });
            var entrypointOverride = projectPath + @"\Scripts\";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\SupportEC6\h.js", null)
                    {
                        FileContent ="define('supportec6/h', ['require', 'exports'],function (require, exports) {\r\n    function default_1() { console.log('my fn'); };\r\n    Object.defineProperty(exports, \"__esModule\", { value: true });\r\n    exports.default = default_1;\r\n});"
                    }
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        //        [Fact]
        //        public void LoadModuleThatExportsDefault()
        //        {
        //            var bundleId = "bundle6";
        //            var bundleOutputFolder = @"c:\bundles";
        //            var filesPaths = new List<string>(new[] { projectPath + @"\LoadModuleThatExportsDefault.json" });
        //            var entrypointOverride = "";

        //            var expectedBundle = new RequireJsNet.Compressor.Bundle()
        //            {
        //                BundleId = bundleId,
        //                ContainingConfig = filesPaths[0],
        //                Output = $"{bundleOutputFolder}\\{bundleId}.js",
        //                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
        //                    new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\exportdefault.js", null)
        //                    {
        //                        FileContent  =
        //@"define('exportdefault', [],function () {
        //    ""use strict"";
        //    function default_1(a, b) {
        //        return a + b;
        //        }
        //        Object.defineProperty(exports, ""__esModule"", { value: true });
        //    exports.default = default_1;
        //});"
        //                    }
        //                })
        //            };

        //            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
        //            var actualBundles = compressor.ParseConfigs();

        //            Assert.Equal(1, actualBundles.Count);
        //            AssertEqual(expectedBundle, actualBundles[0]);
        //        }


        #region Assertion Helpers

        private static void AssertEqual(RequireJsNet.Compressor.Bundle expected, RequireJsNet.Compressor.Bundle actual)
        {
            Assert.Equal(expected.BundleId, actual.BundleId);
            Assert.Equal(expected.ContainingConfig, actual.ContainingConfig);
            Assert.Equal(expected.Output, actual.Output);

            Assert.Equal(expected.Files.Count, actual.Files.Count);
            for (var i = 0; i < expected.Files.Count; i++)
            {
                AssertEqual(expected.Files[i], actual.Files[i]);
            }
        }

        private static void AssertEqual(RequireJsNet.Compressor.FileSpec expected, RequireJsNet.Compressor.FileSpec actual)
        {
            Assert.Equal(expected.CompressionType, actual.CompressionType);
            Assert.Equal(expected.FileContent, actual.FileContent);
            Assert.Equal(expected.FileName, actual.FileName);
        }

        #endregion Assertion Helpers
    }
}
