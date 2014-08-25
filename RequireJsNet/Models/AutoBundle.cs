// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

namespace RequireJsNet.Models
{
    internal class AutoBundle
    {
        public string Id { get; set; }

        public string OutputPath { get; set; }

        public List<AutoBundleItem> Includes { get; set; }

        public List<AutoBundleItem> Excludes { get; set; }

        public string ContainingConfig { get; set; }
    }
}
