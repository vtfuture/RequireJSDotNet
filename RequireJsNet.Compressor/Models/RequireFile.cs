// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

namespace RequireJsNet.Compressor.Models
{
    internal class RequireFile
    {
        public string Name { get; set; }

        public string Content { get; set; }

        public List<string> Dependencies { get; set; }
    }
}
