using RequireJsNet.Compressor.AutoDependency;
using RequireJsNet.Compressor.Tests.Helpers;
using RequireJsNet.Configuration;
using RequireJsNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace RequireJSNet.Compressor.Tests.Tests.CompressorTaskEngine
{
    public class ExecuteShould
    {
        readonly string projectPath;

        public ExecuteShould()
        {
            var executingDirectory = Directory.GetCurrentDirectory();
            var directory = Path.Combine(executingDirectory, @"..\..\TestData\ExecuteShould");
            projectPath = Path.GetFullPath(directory);
        }

        [Fact]
        public void BundleFilesOnSeparateRows()
        {
            //If a bundled file ends with a //# sourceMappingURL comment, the next define() would be commented out
            var yui = new Yahoo.Yui.Compressor.JavaScriptCompressor { Encoding = System.Text.Encoding.UTF8 };
            var taskLog = new Helpers.InMemoryLogAdapter();

            var files = new[] {
                TestData.FileSpecs.Scripts_BundleIncludedDirectory_a(projectPath),
                TestData.FileSpecs.Scripts_BundleIncludedDirectory_b(projectPath)
            };

            var expected = "define(\"bundleincludeddirectory/a\",[],function(){console.log(\"a.js\")});\r\ndefine(\"bundleincludeddirectory/b\",[],function(){console.log(\"b.js\")});\r\n";

            var outputFile = Path.GetTempFileName();
            try
            {
                var taskEngine = new RequireJsNet.Compressor.CompressorTaskEngine(taskLog, yui)
                {
                    CompressionType = "standard",
                    DeleteSourceFiles = false,
                    LineBreakPosition = -1,
                    LoggingType = "none",
                    OutputFile = outputFile,
                    SourceFiles = files
                };
                taskEngine.Execute();

                var actual = File.ReadAllText(outputFile);

                Assert.Equal(expected, actual);
            }
            finally
            {
                File.Delete(outputFile);
            }
        }
    }
}
