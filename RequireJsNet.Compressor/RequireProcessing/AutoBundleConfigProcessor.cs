using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.RequireProcessing
{
    using System.Diagnostics;
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
                                           Output = this.GetOutputPath(bundle.OutputPath, bundle.Id)
                                       };
                bundles.Add(bundleResult);

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

                var fileQueue = new Queue<string>();
                this.EnqueueFileList(bundleResult, fileQueue, files);
                

                while (fileQueue.Any())
                {
                    var file = fileQueue.Dequeue();
                    var fileText = File.ReadAllText(file);
                    var relativePath = PathHelpers.GetRelativePath(file, EntryPoint + Path.DirectorySeparatorChar);
                    var processor = new ScriptProcessor(relativePath, fileText, Configuration);
                    processor.Process();
                    var result = processor.ProcessedString;
                    bundleResult.Files.Add(new FileSpec(file, string.Empty) { FileContent = result });
                    var dependencies = processor.Dependencies.Select(r => this.ResolvePhysicalPath(r)).Distinct().ToList();
                    this.EnqueueFileList(bundleResult, fileQueue, dependencies);
                }
            }

            return bundles;
        }

        private void EnqueueFileList(Bundle bundle, Queue<string> queue, List<string> files)
        {
            foreach (var file in files)
            {
                if (!bundle.Files.Where(r => r.FileName.ToLower() == file.ToLower()).Any()
                    && !queue.Where(r => r.ToLower() == file.ToLower()).Any())
                {
                    queue.Enqueue(file);
                }
            }
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
