// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;

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
                    return new JsonWriter(path, options);
                default:
                    throw new Exception("Unknown config writer.");
            }
        }
    }
}
