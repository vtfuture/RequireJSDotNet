// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.IO;

namespace RequireJsNet.Configuration
{
    internal static class ConfigHelpers
    {
        public static ConfigType GetReaderType(string path)
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
                    return ConfigType.Xml;
                case ".xml":
                    return ConfigType.Xml;
                case ".json":
                    return ConfigType.Json;
                default:
                    throw new Exception(string.Format("Unknown file extension {0}.", extension));
            }
        }
    }
}
