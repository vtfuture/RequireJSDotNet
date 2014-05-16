using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RequireJsNet.Compressor
{
    using RequireJsNet.Configuration;
    using RequireJsNet.Models;

    internal class RequireConfigReader
    {
        private const string ConfigFileName = "RequireJS.config";

        private const string DefaultScriptDirectory = "Scripts";

        private readonly string entryPoint;

        public RequireConfigReader(string projectPath, string packagePath, string entryPointOverride, List<string> filePaths)
        {
            ProjectPath = projectPath;
            FilePaths = filePaths;
            OutputPath = projectPath;
            EntryOverride = entryPointOverride;
            if (!string.IsNullOrWhiteSpace(packagePath))
            {
                OutputPath = packagePath;
            }

            entryPoint = Path.GetFullPath(Path.Combine(projectPath + Path.DirectorySeparatorChar, DefaultScriptDirectory));
        }

        private ConfigurationCollection Configuration { get; set; }

        private string ProjectPath { get; set; }

        private string OutputPath { get; set; }

        private string EntryOverride { get; set; }

        private List<string> FilePaths { get; set; }

        public List<Bundle> ParseConfigs()
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

            var bundles = new List<Bundle>();
            foreach (var bundleDefinition in Configuration.Bundles.BundleEntries.Where(r => !r.IsVirtual))
            {
                var bundle = new Bundle();
                bundle.Output = GetOutputPath(bundleDefinition);
                bundle.Files = bundleDefinition.BundleItems
                                                .Select(r => new FileSpec(this.ResolvePhysicalPath(r.RelativePath), r.CompressionType))
                                                .ToList();
                bundles.Add(bundle);
            }

            return bundles;
        }

        private string ResolvePhysicalPath(string relativePath)
        {
            var filePath = Path.GetFullPath(Path.Combine(ProjectPath, entryPoint, relativePath + ".js"));
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Could not load script" + filePath, filePath);
            }

            return filePath;
        }

        private string GetOutputPath(RequireBundle bundle)
        {
            if (string.IsNullOrEmpty(bundle.OutputPath))
            {
                return Path.GetFullPath(Path.Combine(OutputPath, bundle.Name + ".js"));
            }

            var directory = Path.GetDirectoryName(bundle.OutputPath) ?? string.Empty;
            var fileName = Path.GetFileName(bundle.OutputPath);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = bundle.Name + ".js";
            }

            return Path.GetFullPath(Path.Combine(OutputPath, directory, fileName)); 
        }

        private void FindConfigs()
        {
            if (FilePaths.Any())
            {
                return;
            }

            var files = Directory.GetFiles(ProjectPath, ConfigFileName);
            foreach (var file in files)
            {
                FilePaths.Add(file);
            }

            if (!FilePaths.Any())
            {
                throw new ArgumentException("No Require config files were provided and none were found in the project directory.");    
            }
        }
    }
}
