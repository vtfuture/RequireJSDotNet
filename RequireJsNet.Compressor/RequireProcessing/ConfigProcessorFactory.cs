using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.RequireProcessing
{
    internal static class ConfigProcessorFactory
    {
        public static ConfigProcessor Create(bool autoBundles, string projectPath, string packagePath, string entryPointOverride, List<string> filePaths)
        {
            if (autoBundles)
            {
                return new AutoBundleConfigProcessor(projectPath, packagePath, entryPointOverride, filePaths);
            }

            return new SimpleBundleConfigProcessor(projectPath, packagePath, entryPointOverride, filePaths);
        }
    }
}
