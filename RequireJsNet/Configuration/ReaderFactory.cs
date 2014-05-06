using System;
using System.IO;

namespace RequireJsNet.Configuration
{
    internal static class ReaderFactory
    {
        public static IConfigReader CreateReader(string path)
        {
            var type = GetReaderType(path);
            switch (type)
            {
                    case ReaderType.Xml:
                        return new XmlReader(path);
                    case ReaderType.Json:
                        throw new NotImplementedException("No reader for json config implemented.");
                    default:
                    throw new Exception("Unknown config reader.");
            }
        }

        private static ReaderType GetReaderType(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
            {
                throw new Exception(string.Format("No extension specified for file {0}.", path));
            }
            extension = extension.ToLowerInvariant();
            switch (extension)
            {
                case ".config":
                    return ReaderType.Xml;
                case ".xml":
                    return ReaderType.Xml;
                case ".json":
                    return ReaderType.Json;
                default:
                    throw new Exception(string.Format("Unknown file extension {0}.", extension));
            }
        }
    }
}
