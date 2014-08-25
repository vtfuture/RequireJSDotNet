using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace RequireJsDotNet.Models
{
    internal class JsonRequireDeps
    {
        [JsonProperty(PropertyName = "deps")]
        public List<string> Dependencies { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "exports", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Exports { get; set; }
    }
}