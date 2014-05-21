using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Configuration
{
    internal static class WriterFactory
    {
        public static IConfigWriter CreateWriter(string path, ConfigLoaderOptions options)
        {
            var type = ConfigHelpers.GetReaderType(path);
            switch (type)
            {
                case ConfigType.Xml:
                    return new XmlWriter(path, options);
                case ConfigType.Json:
                    throw new NotImplementedException("No writer for json config implemented.");
                default:
                    throw new Exception("Unknown config writer.");
            }
        }
    }
}
