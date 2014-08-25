// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.Text;

namespace RequireJsNet.Compressor.RequireProcessing
{
    internal static class ConfigProcessorFactory
    {
        public static ConfigProcessor Create(bool autoBundles, string projectPath, string packagePath, string entryPointOverride, List<string> filePaths, Encoding encoding)
        {
            if (autoBundles)
            {
                return new AutoBundleConfigProcessor(projectPath, packagePath, entryPointOverride, filePaths, encoding);
            }

            return new SimpleBundleConfigProcessor(projectPath, packagePath, entryPointOverride, filePaths);
        }
    }
}
