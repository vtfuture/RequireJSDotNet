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
        readonly string scriptsBasePath;

        public ParseConfigsShould()
        {
            var executingDirectory = Directory.GetCurrentDirectory();
            var directory = Path.Combine(executingDirectory, @"..\..\TestData\ParseConfigsShould");
            scriptsBasePath = Path.GetFullPath(directory);
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
            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(scriptsBasePath, "", "", null, System.Text.Encoding.UTF8);
            Assert.Throws<ArgumentNullException>(() =>
            {
                compressor.ParseConfigs();
            });
        }

        [Fact]
        public void ThrowIfConfigIsNotProvided()
        {
            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(scriptsBasePath, "", "", new List<string>(), System.Text.Encoding.UTF8);
            Assert.Throws<ArgumentException>(() =>
            {
                compressor.ParseConfigs();
            });
        }

        [Fact]
        public void FindConfigs()
        {
            var filesPaths = new List<string>(new[] { @"TestData\ParseConfigsShould\LoadAComplexDependencyTree.json" });
            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(scriptsBasePath, "", "", filesPaths, System.Text.Encoding.UTF8);
            compressor.ParseConfigs();
            //TODO: We should find multiple files
            //TODO: We should find the override file if it exists
        }

        [Fact]
        public void BundleIncludedDirectory()
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
                    new RequireJsNet.Compressor.FileSpec(scriptsBasePath + @"\Scripts\BundleIncludedDirectory\a.js", null)
                    {
                        FileContent ="define('bundleincludeddirectory/a', [], function () {\r\ndeclare(function () {\r\n    console.log('a.js');\r\n});\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(scriptsBasePath + @"\Scripts\BundleIncludedDirectory\b.js", null)
                    {
                        FileContent ="define('bundleincludeddirectory/b', [], function () {\r\ndeclare(function () {\r\n    console.log('b.js');\r\n});\r\n});"
                    },
                    new RequireJsNet.Compressor.FileSpec(scriptsBasePath + @"\Scripts\a.js", null)
                    {
                        FileContent ="define('a', [], function () {\r\ndeclare(function () {\r\n    console.log('file-a.js');\r\n});\r\n});"
                    }
                })
            };

            var compressor = new RequireJsNet.Compressor.RequireProcessing.AutoBundleConfigProcessor(scriptsBasePath, bundleOutputFolder, entrypointOverride, filesPaths, System.Text.Encoding.UTF8);
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
