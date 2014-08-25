// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

namespace RequireJsNet.Models
{
    internal class CollectionOverride
    {
        public RequirePaths Paths { get; set; }

        public RequireShim Shim { get; set; }

        public RequireMap Map { get; set; }

        public string BundleId { get; set; }

        public List<string> BundledScripts { get; set; }
    }
}
