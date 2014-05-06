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

            Configuration = new RequireConfiguration
            {
                EntryPoint = Path.GetFullPath(Path.Combine(projectPath + Path.DirectorySeparatorChar, DefaultScriptDirectory))
            };
        }

        private RequireConfiguration Configuration { get; set; }

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


            foreach (var filePath in FilePaths)
            {
                LoadConfigData(filePath);
            }

            ResolveDefaultBundles();

            ResolvePhysicalPaths();

            ResolveBundleIncludes();

            var bundles = new List<Bundle>();
            foreach (var bundleDefinition in Configuration.Bundles.Where(r => !r.IsVirtual))
            {
                var bundle = new Bundle();
                bundle.Output = GetOutputPath(bundleDefinition);
                bundle.Files = bundleDefinition.Items
                                                .Select(r => new FileSpec(r.PhysicalPath, r.CompressionType))
                                                .ToList();
                bundles.Add(bundle);
            }

            return bundles;
        }

        private void ResolveDefaultBundles()
        {
            var groupedPaths =
                Configuration.Paths.Where(r => !string.IsNullOrWhiteSpace(r.DefaultBundle))
                    .GroupBy(r => r.DefaultBundle)
                    .Select(r => new
                    {
                        Bundle = r.Key,
                        Items = r.ToList()
                    }).ToList();
            foreach (var bundleGroup in groupedPaths)
            {
                var itemList = bundleGroup.Items.Select(r => new BundleItem
                {
                    CompressionType = "standard",
                    ModuleName = r.Key
                }).ToList();
                var existingBundle = Configuration.Bundles.Where(r => r.Name == bundleGroup.Bundle).FirstOrDefault();
                if (existingBundle == null)
                {
                    Configuration.Bundles.Add(new BundleDefinition
                    {
                        Includes = new List<string>(),
                        IsVirtual = true,
                        Items = itemList,
                        Name = bundleGroup.Bundle
                    });
                }
                else
                {
                    existingBundle.Items = itemList.Concat(existingBundle.Items).ToList();
                }
            }
        }


        private void ResolveBundleIncludes()
        {
            var rootBundles = Configuration.Bundles.Where(r => !r.Includes.Any()).ToList();
            if (!rootBundles.Any())
            {
                throw new Exception("Could not find any bundle with no dependency. Check your config for cyclic dependencies.");
            }

            rootBundles.ForEach(r => r.ParsedIncludes = true);
            var maxIterations = 500;
            var currentIt = 0;
            while (Configuration.Bundles.Where(r => !r.ParsedIncludes).Any())
            {
                // shouldn't really happen, but we'll use this as a safeguard against an endless loop for the moment
                if (currentIt > maxIterations)
                {
                    throw new Exception("Maximum number of iterations exceeded. Check your config for cyclick dependencies");
                }

                // get all the bundles that have parents with resolved dependencies and haven't been resolved themselves
                var parsableBundles = GetBundlesWithResolvedParents();

                // we've checked earlier if there are any bundles that haven't been parsed
                // if there are bundles that haven't been parsed but there aren't any we can parse, something went wrong
                if (!parsableBundles.Any())
                {
                    throw new Exception("Could not parse bundle includes. Check your config for cyclic dependencies.");
                }
                
                foreach (var bundle in parsableBundles)
                {
                    // store a reference to the old list
                    var oldItemList = bundle.Items;

                    // instantiate a new one so that when we're done we can append the old scripts
                    bundle.Items = new List<BundleItem>();
                    var parents = bundle.Includes.Select(r => GetBundleByName(r)).ToList();
                    foreach (var parent in parents)
                    {
                        bundle.Items.AddRange(parent.Items);
                    }

                    bundle.Items.AddRange(oldItemList);
                    bundle.Items = bundle.Items.GroupBy(r => r.PhysicalPath).Select(r => r.FirstOrDefault()).ToList();
                    bundle.ParsedIncludes = true;
                }

                currentIt++;
            }
        }

        private BundleDefinition GetBundleByName(string name)
        {
            var result = Configuration.Bundles.Where(r => r.Name == name).FirstOrDefault();
            if (result == null)
            {
                throw new Exception("Could not find bundle with name " + name);
            }

            return result;
        }

        private List<BundleDefinition> GetBundlesWithResolvedParents()
        {
            var allBundles = Configuration.Bundles;
            var result = new List<BundleDefinition>();
            foreach (var bundle in allBundles.Where(r => !r.ParsedIncludes))
            {
                // for each include, get its bundle.ParsedIncludes property
                // select those that don't have their parents resolved
                // if any such items exist, it means that the item's parents haven't been resolved
                var parentsResolved = !bundle.Includes
                                        .Select(r => GetBundleByName(r).ParsedIncludes)
                                        .Where(r => !r)
                                        .Any();
                if (parentsResolved)
                {
                    result.Add(bundle);
                }
            }

            return result;
        }

        private void ResolvePhysicalPaths()
        {
            foreach (var item in Configuration.Bundles.SelectMany(r => r.Items))
            {
                var finalName = item.ModuleName;

                // this will only go 1 level deep, other cases should be taken into account
                if (Configuration.Paths.Where(r => r.Key == finalName).Any())
                {
                    var finalEl = Configuration.Paths.Where(r => r.Key == finalName).FirstOrDefault();
                    if (finalEl == null)
                    {
                        throw new Exception("Could not find path item with name = " + finalName);
                    }

                    finalName = finalEl.Value;
                }

                item.PhysicalPath = Path.GetFullPath(Path.Combine(ProjectPath, Configuration.EntryPoint, finalName + ".js"));
                if (!File.Exists(item.PhysicalPath))
                {
                    throw new FileNotFoundException("Could not load script" + item.PhysicalPath, item.PhysicalPath);
                }
            }
        }

        private string GetOutputPath(BundleDefinition bundle)
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

            if (!string.IsNullOrWhiteSpace(EntryOverride))
            {
                Configuration.EntryPoint = EntryOverride;
            }

            LoadPaths(doc.Document.Root);
            LoadBundles(doc.Document.Root);
        }

        private void LoadPaths(XElement docRoot)
        {
            var pathsElement = docRoot.Document.Root.Descendants("paths").FirstOrDefault();
            if (pathsElement != null)
            {
                var paths = pathsElement.Descendants("path").Select(r => new PathItem
                {
                    Key = r.Attribute("key").Value,
                    Value = r.Attribute("value").Value,
                    DefaultBundle = ReadStringAttribute(r.Attribute("bundle"))
                });
                foreach (var scriptPath in paths)
                {
                    if (!Configuration.Paths.Where(r => r.Key == scriptPath.Key).Any())
                    {
                        Configuration.Paths.Add(scriptPath);
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
                    IsVirtual = ReadBooleanAttribute(r.Attribute("virtual")),
                    OutputPath = ReadStringAttribute(r.Attribute("outputPath")),
                    Includes = ReadStringListAttribute(r.Attribute("includes")),
                    Items = r.Descendants("bundleItem").Select(x => new BundleItem
                    {
                        ModuleName = x.Attribute("path").Value,
                        CompressionType = ReadStringAttribute(x.Attribute("compression"))
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

        private List<string> ReadStringListAttribute(XAttribute attribute)
        {
            if (attribute == null)
            {
                return new List<string>();
            }

            var result = attribute.Value.Split(',').Select(r => r.Trim()).Distinct().ToList();
            return result;
        }

        private string ReadStringAttribute(XAttribute attribute)
        {
            if (attribute == null)
            {
                return string.Empty;
            }

            return attribute.Value;
        }

        private bool ReadBooleanAttribute(XAttribute attribute)
        {
            if (attribute == null)
            {
                return false;
            }

            return Convert.ToBoolean(attribute.Value);
        }
    }
}
