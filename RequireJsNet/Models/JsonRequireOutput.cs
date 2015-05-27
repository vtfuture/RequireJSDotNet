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
    public class JsonRequireOutput
    {
        [JsonProperty(PropertyName = "baseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "urlArgs")]
        public string UrlArgs { get; set; }

        [JsonProperty(PropertyName = "waitSeconds")]
        public int WaitSeconds { get; set; }

        [JsonProperty(PropertyName = "paths")]
        public Dictionary<string, string> Paths { get; set; }

        [JsonProperty(PropertyName = "shim")]
        public Dictionary<string, JsonRequireDeps> Shim { get; set; }

        [JsonProperty(PropertyName = "map")]
        public Dictionary<string, Dictionary<string, string>> Map { get; set; }
    }
}
