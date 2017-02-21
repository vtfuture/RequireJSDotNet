using RequireJsNet.Compressor.AutoDependency;
using RequireJsNet.Compressor.Tests.Helpers;
using RequireJsNet.Configuration;
using RequireJsNet.Models;
using System.Collections.Generic;
using Xunit;

namespace RequireJSNet.Compressor.Tests
{
    public class ExpandPathsShould
    {
        readonly ConfigurationCollection configuration;

        public ExpandPathsShould()
        {
            configuration = ReadJson(new TestFileReader());
        }

        [Fact]
        public void LeaveRelativePaths()
        {
            var expected = "relative/url";
            var actual = ScriptProcessor.ExpandPaths("relative/url", configuration);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExpandsExactMatches()
        {
            var expected = "/somewhere/jquery-1.10.2/jquery";
            var actual = ScriptProcessor.ExpandPaths("jquery", configuration);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoCaseInsensitiveMatches()
        {
            var expected = "/somewhere/jquery-1.10.2/jquery";
            var actual = ScriptProcessor.ExpandPaths("JQuERy", configuration);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExpandPartialMatches()
        {
            var expected = "jqueryui";
            var actual = ScriptProcessor.ExpandPaths("jqueryui", configuration);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExpandsStartOfPath()
        {
            var expected = "/somewhere/jquery-1.10.2/jquery/subfile";
            var actual = ScriptProcessor.ExpandPaths("jquery/subfile", configuration);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExpandMatchesInsidePaths()
        {
            var expected = "my/jquery/alternative";
            var actual = ScriptProcessor.ExpandPaths("my/jquery/alternative", configuration);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExpandUrlsToNull()
        {
            var actual = ScriptProcessor.ExpandPaths("external_jquery", configuration);
            Assert.Null(actual);
        }

        [Fact]
        public void ExpandLocalPathWhenAvailable()
        {
            var expected = "/jquery";
            var actual = ScriptProcessor.ExpandPaths("jquery_with_fallback", configuration);
            Assert.Equal(expected, actual);
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
