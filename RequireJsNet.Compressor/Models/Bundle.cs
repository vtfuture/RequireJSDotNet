// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

namespace RequireJsNet.Compressor
{
    internal class Bundle
    {
        public string Output { get; set; }

        public List<FileSpec> Files { get; set; }

        public string ContainingConfig { get; set; }

        public string BundleId { get; set; }
    }
}
