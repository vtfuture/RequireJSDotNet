using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class JsonWriter : IConfigWriter
    {
        private readonly ConfigLoaderOptions options;

        public JsonWriter(string path, ConfigLoaderOptions options)
        {
            this.options = options;
            Path = path;
        }

        public string Path { get; private set; }

        public void WriteConfig(ConfigurationCollection conf)
        {
            dynamic obj = new ExpandoObject();
            if (conf.Paths != null && conf.Paths.PathList != null && conf.Paths.PathList.Any())
            {
                obj.Paths = conf.Paths.PathList.ToDictionary(r => r.Key, r => r.Value);
            }

            if (conf.Overrides != null && conf.Overrides.Any())
            {
                obj.Overrides = conf.Overrides.ToDictionary(
                    r => r.BundleId,
                    r => new
                             {
                                 Paths = r.Paths.PathList.ToDictionary(x => x.Key, x => x.Value),
                                 BundledScripts = r.BundledScripts
                             });
            }

            File.WriteAllText(
                Path, 
                JsonConvert.SerializeObject(
                            obj,
                            new JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented,
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }));
        }
    }
}
