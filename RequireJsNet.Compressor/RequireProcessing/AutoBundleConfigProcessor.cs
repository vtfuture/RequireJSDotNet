using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.RequireProcessing
{
    using System.IO;

    using RequireJsNet.Compressor.AutoDependency;
    using RequireJsNet.Compressor.Helpers;
    using RequireJsNet.Configuration;
    using RequireJsNet.Models;

    internal class AutoBundleConfigProcessor : ConfigProcessor
    {
        public AutoBundleConfigProcessor(string projectPath, string packagePath, string entryPointOverride, List<string> filePaths)
        {
            ProjectPath = projectPath;
            FilePaths = filePaths;
            OutputPath = projectPath;
            EntryOverride = entryPointOverride;
            if (!string.IsNullOrWhiteSpace(packagePath))
            {
                OutputPath = packagePath;
            }

            EntryPoint = this.GetEntryPointPath();
        }

        public override List<Bundle> ParseConfigs()
        {
            if (!Directory.Exists(ProjectPath))
            {
                throw new DirectoryNotFoundException("Could not find project directory.");
            }

            FindConfigs();

            var loader = new ConfigLoader(
                FilePaths,
                new ExceptionThrowingLogger(),
                new ConfigLoaderOptions { ProcessBundles = true });

            Configuration = loader.Get();
            
            foreach (var bundle in Configuration.AutoBundles.Bundles)
            {
                var files = new List<string>();
                foreach (var include in bundle.Includes)
                {
                    if (!string.IsNullOrEmpty(include.File))
                    {
                        files.Add(this.ResolvePhysicalPath(include.File));
                    }
                    else if(!string.IsNullOrEmpty(include.Directory))
                    {
                        var absDirectory = this.GetAbsoluteDirectory(include.Directory);

                        // not using filter for this since we're going to use the one the user provided in the future
                        var dirFiles = Directory.GetFiles(absDirectory, "*", SearchOption.AllDirectories).Where(r => Path.GetExtension(r) == ".js").ToList();
                        files.AddRange(dirFiles);
                    }
                }

                files = files.Distinct().ToList();

                foreach (var file in files)
                {
                    var fileText = File.ReadAllText(file);
                    var relativePath = PathHelpers.GetRelativePath(file, EntryPoint + Path.DirectorySeparatorChar);
                    var processor = new ScriptProcessor(relativePath, fileText, Configuration);
                    processor.Process();
                    var result = processor.ProcessedString;
                }

                var outputPath = this.GetOutputPath(bundle.OutputPath, bundle.Id);
            }

            return new List<Bundle>();
        }

        private string GetAbsoluteDirectory(string relativeDirectory)
        {
            relativeDirectory = relativeDirectory.Replace("/", "\\");
            if (relativeDirectory.StartsWith("\\"))
            {
                relativeDirectory = relativeDirectory.Substring(1);
            }
            return Path.Combine(EntryPoint + Path.DirectorySeparatorChar, relativeDirectory);
        }
    }
}
