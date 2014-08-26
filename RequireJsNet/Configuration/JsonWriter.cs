using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }
    }
}
