// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

namespace RequireJsNet.Models
{
    internal class ShimEntry
    {
        public string For { get; set; }

        public string Exports { get; set; }

        public List<RequireDependency> Dependencies { get; set; } 
    }
}
