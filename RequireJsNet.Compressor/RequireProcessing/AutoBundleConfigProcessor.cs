// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using RequireJsNet.Compressor.AutoDependency;
using RequireJsNet.Compressor.Models;
using RequireJsNet.Configuration;
using RequireJsNet.Helpers;
using RequireJsNet.Models;

namespace RequireJsNet.Compressor.RequireProcessing
{
    internal class AutoBundleConfigProcessor : ConfigProcessor
    {
        private Encoding encoding;

        public AutoBundleConfigProcessor(string projectPath, string packagePath, string entryPointOverride, List<string> filePaths, Encoding encoding)
        {
            ProjectPath = projectPath;
            FilePaths = filePaths;
            OutputPath = projectPath;
            EntryOverride = entryPointOverride;
            this.encoding = encoding;
            if (!string.IsNullOrWhiteSpace(packagePath))
            {
                OutputPath = packagePath;
            }

            EntryPoint = this.GetEntryPointPath();
        }

        public override List<Bundle> ParseConfigs()
        {
            var bundles = new List<Bundle>();
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
                var bundleResult = new Bundle
                                       {
                                           Files = new List<FileSpec>(),
                                           Output = this.GetOutputPath(bundle.OutputPath, bundle.Id),
                                           ContainingConfig = bundle.ContainingConfig,
                                           BundleId = bundle.Id
                                       };
                bundles.Add(bundleResult);

                var tempFileList = new List<RequireFile>();

                foreach (var include in bundle.Includes)
                {
                    // check if the file path is actually an URL
                    if (!string.IsNullOrEmpty(include.File) && !include.File.Contains("?"))
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

                var fileQueue = new Queue<string>();
                this.EnqueueFileList(tempFileList, fileQueue, files);
                

                while (fileQueue.Any())
                {
                    var file = fileQueue.Dequeue();
                    var fileText = File.ReadAllText(file, encoding);
                    var relativePath = PathHelpers.GetRelativePath(file, EntryPoint + Path.DirectorySeparatorChar);
                    var processor = new ScriptProcessor(relativePath, fileText, Configuration);
                    processor.Process();
                    var result = processor.ProcessedString;
                    var dependencies = processor.Dependencies.Select(r => this.ResolvePhysicalPath(r)).Where(r => r != null).Distinct().ToList();
                    tempFileList.Add(new RequireFile
                                         {
                                             Name = file,
                                             Content = result,
                                             Dependencies = dependencies
                                         });

                    this.EnqueueFileList(tempFileList, fileQueue, dependencies);
                }

                while (tempFileList.Any())
                {
                    var addedFiles = bundleResult.Files.Select(r => r.FileName).ToList();
                    var noDeps = tempFileList.Where(r => !r.Dependencies.Any()
                                                        || r.Dependencies.All(x => addedFiles.Contains(x))).ToList();
                    if (!noDeps.Any())
                    {
                        noDeps = tempFileList.ToList();
                    }

                    foreach (var requireFile in noDeps)
                    {
                        bundleResult.Files.Add(new FileSpec(requireFile.Name, string.Empty) { FileContent = requireFile.Content });
                        tempFileList.Remove(requireFile);
                    }    
                }
            }

            this.WriteOverrideConfigs(bundles);

            return bundles;
        }

        private void WriteOverrideConfigs(List<Bundle> bundles)
        {
            var groupings = bundles.GroupBy(r => r.ContainingConfig).ToList();
            foreach (var grouping in groupings)
            {
                var path = RequireJsNet.Helpers.PathHelpers.GetOverridePath(grouping.Key);
                var writer = WriterFactory.CreateWriter(path, null);
                var collection = this.ComposeCollection(grouping.ToList());
                writer.WriteConfig(collection);    
            }
        }

        private ConfigurationCollection ComposeCollection(List<Bundle> bundles)
        {
            var conf = new ConfigurationCollection();
            conf.Overrides = new List<CollectionOverride>();
            foreach (var bundle in bundles)
            {
                var scripts = bundle.Files.Select(r => PathHelpers.GetRequireRelativePath(EntryPoint, r.FileName)).ToList();
                var paths = new RequirePaths
                                {
                                    PathList = new List<RequirePath>()
                                };
                foreach (var script in scripts)
                {
                    paths.PathList.Add(new RequirePath
                                           {
                                               Key = script,
                                               Value = PathHelpers.GetRequireRelativePath(EntryPoint, bundle.Output)
                                           });
                }

                var over = new CollectionOverride
                               {
                                   BundleId = bundle.BundleId,
                                   BundledScripts = scripts,
                                   Paths = paths
                               };
                conf.Overrides.Add(over);
            }

            return conf;
        }

        private void EnqueueFileList(List<RequireFile> fileList, Queue<string> queue, List<string> files)
        {
            foreach (var file in files)
            {
                if (!fileList.Where(r => r.Name.ToLower() == file.ToLower()).Any()
                    && !queue.Where(r => r.ToLower() == file.ToLower()).Any())
                {
                    queue.Enqueue(file);
                }
            }
        }

        private string GetAbsoluteDirectory(string relativeDirectory)
        {
            string entry = this.EntryPoint;

            relativeDirectory = relativeDirectory.Replace("/", "\\");
            if (relativeDirectory.StartsWith("\\"))
            {
                relativeDirectory = relativeDirectory.Substring(1);
            }

            return Path.Combine(entry + Path.DirectorySeparatorChar, relativeDirectory);
        }
    }
}
