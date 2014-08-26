// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace RequireJsNet.Models
{
    public class JsonRequireDeps
    {
        [JsonProperty(PropertyName = "deps")]
        public List<string> Dependencies { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "exports", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Exports { get; set; }
    }
}