using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Models
{
    public class RequirePackage
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "main")]
        public string Main { get; set; }

        [JsonProperty(PropertyName = "location", NullValueHandling = NullValueHandling.Ignore)]
        public string Location { get; set; }

        public RequirePackage(string name, string main = "main", string location = null)
        {
            this.Name = name;
            this.Main = main ?? "main";
            this.Location = location;
        }
    }
}
