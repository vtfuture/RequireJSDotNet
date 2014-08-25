// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.IO;
using System.Linq;

using RequireJsNet.Configuration;

namespace RequireJsNet.Compressor
{
    internal class SimpleBundleConfigProcessor : ConfigProcessor
    {
        public SimpleBundleConfigProcessor(string projectPath, string packagePath, string entryPointOverride, List<string> filePaths)
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

            var bundles = new List<Bundle>();
            foreach (var bundleDefinition in Configuration.Bundles.BundleEntries.Where(r => !r.IsVirtual))
            {
                var bundle = new Bundle();
                bundle.Output = GetOutputPath(bundleDefinition.OutputPath, bundleDefinition.Name);
                bundle.Files = bundleDefinition.BundleItems
                                                .Select(r => new FileSpec(this.ResolvePhysicalPath(r.RelativePath), r.CompressionType))
                                                .ToList();
                bundles.Add(bundle);
            }

            return bundles;
        }
    }
}
