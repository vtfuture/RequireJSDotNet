using Newtonsoft.Json;

namespace RequireJsDotNet.Models
{
    internal class JsonRequireOptions
    {
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "pageOptions")]
        public dynamic PageOptions { get; set; }

        [JsonProperty(PropertyName = "websiteOptions")]
        public dynamic WebsiteOptions { get; set; }
    }
}
