// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

using Newtonsoft.Json;

namespace RequireJsNet.Models
{
    public class JsonRequireOptions
    {
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "pageOptions")]
        public Dictionary<string, object> PageOptions { get; set; }

        [JsonProperty(PropertyName = "websiteOptions")]
        public Dictionary<string, object> WebsiteOptions { get; set; }
    }
}
