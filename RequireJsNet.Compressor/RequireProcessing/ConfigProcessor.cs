using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor
{
    using System.IO;

    using RequireJsNet.Models;

    internal abstract class ConfigProcessor
    {
        protected const string ConfigFileName = "RequireJS.config";

        protected const string DefaultScriptDirectory = "Scripts";

        protected string EntryPoint { get; set; }

        protected ConfigurationCollection Configuration { get; set; }

        protected string ProjectPath { get; set; }

        protected string OutputPath { get; set; }

        protected string EntryOverride { get; set; }

        protected List<string> FilePaths { get; set; }

        public abstract List<Bundle> ParseConfigs();

        protected string ResolvePhysicalPath(string relativePath)
        {
            var filePath = Path.GetFullPath(Path.Combine(ProjectPath, this.EntryPoint, relativePath + ".js"));
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Could not load script" + filePath, filePath);
            }

            return filePath;
        }

        protected string GetOutputPath(RequireBundle bundle)
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

        protected void FindConfigs()
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

        protected string GetEntryPointPath()
        {
            return Path.GetFullPath(Path.Combine(ProjectPath + Path.DirectorySeparatorChar, DefaultScriptDirectory));
        }
    }
}
