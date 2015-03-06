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
    public class ConfigurationCollection
    {
        public string FilePath { get; set; }

        [JsonProperty(PropertyName = "paths")]
        public RequirePaths Paths { get; set; }

        [JsonProperty(PropertyName = "shim")]
        public RequireShim Shim { get; set; }

        [JsonProperty(PropertyName = "map")]
        public RequireMap Map { get; set; }

        [JsonProperty(PropertyName = "bundles")]
        public RequireBundles Bundles { get; set; }

        [JsonProperty(PropertyName = "autoBundles")]
        public AutoBundles AutoBundles { get; set; }

        [JsonProperty(PropertyName = "overrides")]
        public List<CollectionOverride> Overrides { get; set; }
    }
}
