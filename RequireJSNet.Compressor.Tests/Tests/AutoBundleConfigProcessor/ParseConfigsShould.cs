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
                    TestData.FileSpecs.Scripts_BundleIncludedDirectory_a(projectPath),
                    TestData.FileSpecs.Scripts_BundleIncludedDirectory_b(projectPath),
                    TestData.FileSpecs.Scripts_c(projectPath),
                    TestData.FileSpecs.Scripts_b(projectPath),
                    TestData.FileSpecs.Scripts_a(projectPath)
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void BundleIncludeFileWithPath()
        {
            var bundleId = "bundle2";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\BundleIncludedFileWithPath.json" });
            var entrypointOverride = "";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    TestData.FileSpecs.Scripts_BundleIncludedDirectory_a(projectPath)
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void BundleIncludedDirectoryWithPath()
        {
            var bundleId = "bundle5";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\BundleIncludedDirectoryWithPath.json" });
            var entrypointOverride = "";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    TestData.FileSpecs.Scripts_BundleIncludedDirectory_a(projectPath),
                    TestData.FileSpecs.Scripts_BundleIncludedDirectory_b(projectPath)
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
                    TestData.FileSpecs.Scripts_d(projectPath),
                    TestData.FileSpecs.otherscripts_e(projectPath, "'d'")
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
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
                    TestData.FileSpecs.otherscripts_e(projectPath),
                    TestData.FileSpecs.Scripts_MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree_g(projectPath),
                    TestData.FileSpecs.Scripts_MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree_f(projectPath)
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
                    TestData.FileSpecs.Scripts_SupportEC6_h(projectPath)
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void LoadModuleThatExportsDefault()
        {
            var bundleId = "bundle1";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { projectPath + @"\LoadModuleThatExportsDefault.json" });
            var entrypointOverride = "";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    TestData.FileSpecs.Scripts_exportdefault(projectPath)
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

        [Fact]
        public void FindShimWithPath()
        {
            var bundleId = "bundle3";
            var bundleOutputFolder = @"c:\bundles";
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\FindShimWithPath.json" });
            var entrypointOverride = "";

            var expectedBundle = new RequireJsNet.Compressor.Bundle()
            {
                BundleId = bundleId,
                ContainingConfig = filesPaths[0],
                Output = $"{bundleOutputFolder}\\{bundleId}.js",
                Files = new List<RequireJsNet.Compressor.FileSpec>(new[] {
                    TestData.FileSpecs.Scripts_d(projectPath),
                    TestData.FileSpecs.Scripts_shimmed(projectPath)
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(projectPath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
            var actualBundles = compressor.ParseConfigs();

            Assert.Equal(1, actualBundles.Count);
            AssertEqual(expectedBundle, actualBundles[0]);
        }

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
