using System;
using System.IO;

namespace RequireJsDotNet.Configuration
{
    internal static class ReaderFactory
    {
        public static IConfigReader CreateReader(string path, ConfigLoaderOptions options)
        {
            var type = ConfigHelpers.GetReaderType(path);
            switch (type)
            {
                    case ConfigType.Xml:
                        return new XmlReader(path, options);
                    case ConfigType.Json:
                        throw new NotImplementedException("No reader for json config implemented.");
                    default:
                    throw new Exception("Unknown config reader.");
            }
        }
    }
}
