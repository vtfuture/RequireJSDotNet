// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RequireJsNet.Models;
using System.Text.RegularExpressions;

namespace RequireJsNet.Compressor
{
    internal abstract class ConfigProcessor
    {
        protected const string ConfigFileName = "RequireJS.*";

        protected const string DefaultScriptDirectory = "Scripts";

        protected string EntryPoint { get; set; }

        protected ConfigurationCollection Configuration { get; set; }

        protected string ProjectPath { get; set; }

        protected string OutputPath { get; set; }

        protected string EntryOverride { get; set; }

        protected List<string> FilePaths { get; set; }

        public abstract List<Bundle> ParseConfigs();

        internal protected string ResolvePhysicalPath(string relativePath, string directory = "")
        {
            var protocolRegex = @"^\w+://";
            if (Regex.IsMatch(relativePath, protocolRegex))
                return null;

            if (relativePath.Contains("?"))
                return null;

            string entry = this.EntryPoint;
            if (!string.IsNullOrEmpty(EntryOverride))
                entry = this.EntryOverride;

            var modulePath = resolveModulePath(relativePath, entry, directory);

            string filePath;
            try
            {
                filePath = Path.GetFullPath(modulePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load script " + modulePath + ": " + ex.Message, ex);
            }


            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Could not load script " + filePath, filePath);
            }

            return filePath;
        }

        private string resolveModulePath(string relativePath, string entry, string directory)
        {
            relativePath = relativePath.Replace("/", @"\");

            if (!relativePath.Contains(".js"))
                relativePath += ".js";

            if (relativePath.StartsWith(@"\"))
                return Path.Combine(this.ProjectPath, relativePath.Substring(1));

            return Path.Combine(directory ?? entry, relativePath);
        }

        protected string GetOutputPath(string outputPath, string bundleName)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                return Path.GetFullPath(Path.Combine(OutputPath, bundleName + ".js"));
            }

            var directory = Path.GetDirectoryName(outputPath) ?? string.Empty;
            var fileName = Path.GetFileName(outputPath);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = bundleName + ".js";
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
            foreach (var file in files.Where(r => !r.ToLower().Contains(".override.")))
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
            if (!string.IsNullOrWhiteSpace(this.EntryOverride))
            {
                return this.EntryOverride;
            }

            return Path.GetFullPath(Path.Combine(ProjectPath + Path.DirectorySeparatorChar, DefaultScriptDirectory));
        }
    }
}
