// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

namespace RequireJsNet.Configuration
{
    internal class ConfigLoaderOptions
    {
        public bool ProcessBundles { get; set; }

        public bool ProcessAutoBundles { get; set; }

        public bool LoadOverrides { get; set; }

        public ConfigCachingPolicy CachingPolicy { get; set; }
    }
}
