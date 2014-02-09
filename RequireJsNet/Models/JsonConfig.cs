using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace RequireJsNet.Models
{
    internal class JsonConfig
    {
        [JsonProperty(PropertyName = "baseUrl")]
        public string BaseUrl { get; set; }
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }
        [JsonProperty(PropertyName = "paths")]
        public Dictionary<string, string> Paths { get; set; }
        [JsonProperty(PropertyName = "shim")]
        public Dictionary<string, JsonRequireDeps> Shim { get; set; }
    }

    internal class JsonRequireDeps
    {
        [JsonProperty(PropertyName = "deps")]
        public List<string> Dependencies { get; set; }
        [DefaultValue("")]
        [JsonProperty(PropertyName = "exports", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Exports { get; set; }
    }
}
