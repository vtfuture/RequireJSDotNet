// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

namespace RequireJsNet.Models
{
    internal class ConfigurationCollection
    {
        public string FilePath { get; set; }

        public RequirePaths Paths { get; set; }

        public RequireShim Shim { get; set; }

        public RequireMap Map { get; set; }

        public RequireBundles Bundles { get; set; }

        public AutoBundles AutoBundles { get; set; }

        public List<CollectionOverride> Overrides { get; set; }
    }
}
