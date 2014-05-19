using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.RequireProcessing
{
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
            return new List<Bundle>();
        }
    }
}
