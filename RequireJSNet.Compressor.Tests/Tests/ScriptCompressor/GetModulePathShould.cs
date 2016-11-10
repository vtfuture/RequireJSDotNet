using RequireJsNet.Compressor.AutoDependency;
using RequireJsNet.Compressor.Tests.Helpers;
using RequireJsNet.Configuration;
using RequireJsNet.Models;
using System.Collections.Generic;
using Xunit;

namespace RequireJSNet.Compressor.Tests
{
    public class GetModulePathShould
    {
        ScriptProcessor processor;

        public GetModulePathShould()
        {
            var configuration = ReadJson(new TestFileReader());
            processor = new ScriptProcessor("", "", configuration);
        }

        [Fact]
        public void LeaveRelativePaths()
        {
            var expected = "relative/url";
            var actual = processor.GetModulePath("relative/url");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExpandsExactMatches()
        {
            var expected = "/somewhere/jquery-1.10.2/jquery";
            var actual = processor.GetModulePath("jquery");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoCaseInsensitiveMatches()
        {
            var expected = "/somewhere/jquery-1.10.2/jquery";
            var actual = processor.GetModulePath("JQuERy");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExpandPartialMatches()
        {
            var expected = "jqueryui";
            var actual = processor.GetModulePath("jqueryui");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExpandsStartOfPath()
        {
            var expected = "/somewhere/jquery-1.10.2/jquery/subfile";
            var actual = processor.GetModulePath("jquery/subfile");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NotExpandMatchesInsidePaths()
        {
            var expected = "my/jquery/alternative";
            var actual = processor.GetModulePath("my/jquery/alternative");
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
