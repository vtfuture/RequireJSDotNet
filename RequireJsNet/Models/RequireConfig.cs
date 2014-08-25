// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

namespace RequireJsNet.Models
{
    internal class RequireConfig
    {
        public string BaseUrl { get; set; }

        public string Locale { get; set; }

        public RequirePaths Paths { get; set; }

        public RequireShim Shim { get; set; }
    }
}
