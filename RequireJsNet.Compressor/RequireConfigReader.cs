using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RequireJsNet.Compressor
{
    internal class RequireConfigReader
    {
        
        private const string ConfigFileName = "RequireJS.config";
        private const string DefaultScriptDirectory = "Scripts";
        private RequireConfiguration Configuration { get; set; }
        private string ProjectPath { get; set; }
        private List<string> FilePaths { get; set; }

        public RequireConfigReader(string projectPath, List<string> filePaths)
        {
            ProjectPath = projectPath;
            FilePaths = filePaths;
            Configuration = new RequireConfiguration
            {
                EntryPoint = Path.Combine(projectPath + Path.DirectorySeparatorChar, DefaultScriptDirectory)
            };
        }

        public List<Bundle> ParseConfigs()
        {

            if (!Directory.Exists(ProjectPath))
            {
                throw new DirectoryNotFoundException("Could not find project directory.");
            }

            FindConfigs();


            foreach (var filePath in FilePaths)
            {
                LoadConfigData(filePath);
            }

            ResolvePhysicalPaths();

            var bundles = new List<Bundle>();
            foreach (var bundleDefinition in Configuration.Bundles)
            {
                var bundle = new Bundle();
                bundle.Output = GetOutputPath(bundleDefinition.Name);
                bundle.Files = bundleDefinition.Items
                                                .Select(r => new FileSpec(r.PhysicalPath, r.CompressionType))
                                                .ToList();
                bundles.Add(bundle);
            }

            return bundles;
        }

        private void ResolvePhysicalPaths()
        {
            foreach (var item in Configuration.Bundles.SelectMany(r => r.Items))
            {
                var finalName = item.ModuleName;

                // this will only go 1 level deep, other cases should be taken into account
                if (Configuration.Paths.ContainsKey(finalName))
                {
                    finalName = Configuration.Paths[finalName];
                }
                item.PhysicalPath = Path.Combine(ProjectPath, Configuration.EntryPoint, finalName + ".js");
                if (!File.Exists(item.PhysicalPath))
                {
                    throw new FileNotFoundException("Could not load script" + item.PhysicalPath, item.PhysicalPath);
                }
            }
        }

        private string GetOutputPath(string bundleName)
        {
            return Path.Combine(ProjectPath, Configuration.EntryPoint, bundleName + ".js");
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

        private void LoadConfigData(string path)
        {
            var doc = XDocument.Load(path);
            if (doc == null || doc.Document == null || doc.Document.Root == null)
            {
                throw new FileLoadException("Could not read config file.", path);
            }
            var entryPointAttr = doc.Document.Root.Attribute("entryPoint");
            if (entryPointAttr != null)
            {
                var entryPoint = entryPointAttr.Value;
                if (!string.IsNullOrWhiteSpace(entryPoint))
                {
                    Configuration.EntryPoint = entryPoint;
                }
            }

            LoadPaths(doc.Document.Root);
            LoadBundles(doc.Document.Root);
        }

        private void LoadPaths(XElement docRoot)
        {
            var pathsElement = docRoot.Document.Root.Descendants("paths").FirstOrDefault();
            if (pathsElement != null)
            {
                var paths = pathsElement.Descendants("path").Select(r => new
                {
                    Key = r.Attribute("key").Value,
                    Value = r.Attribute("value").Value
                });
                foreach (var scriptPath in paths)
                {
                    if (!Configuration.Paths.ContainsKey(scriptPath.Key))
                    {
                        Configuration.Paths.Add(scriptPath.Key, scriptPath.Value);
                    }
                }
            }
        }

        private void LoadBundles(XElement docRoot)
        {
            var bundlesElement = docRoot.Document.Root.Descendants("bundles").FirstOrDefault();
            if (bundlesElement != null)
            {
                var bundles = bundlesElement.Descendants("bundle").Select(r => new BundleDefinition
                {
                    Name = r.Attribute("name").Value,
                    Items = r.Descendants("bundleItem").Select(x => new BundleItem
                    {
                        ModuleName = x.Attribute("path").Value,
                        CompressionType = x.Attribute("compression") != null ? x.Attribute("compression").Value : ""
                    }).ToList()
                });

                foreach (var bundle in bundles)
                {
                    if (!Configuration.Bundles.Where(r => r.Name == bundle.Name).Any())
                    {
                        Configuration.Bundles.Add(bundle);
                    }
                }
            }
        }

    }
}
