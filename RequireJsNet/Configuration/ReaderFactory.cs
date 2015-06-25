// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RequireJsNet.Helpers;

namespace RequireJsNet.Configuration
{
    internal static class ReaderFactory
    {
        private static HashStore<JsonReader> readerStore = new HashStore<JsonReader>();

        public static IConfigReader CreateReader(string path, ConfigLoaderOptions options)
        {
            var type = ConfigHelpers.GetReaderType(path);
            switch (type)
            {
                    case ConfigType.Xml:
                        return new XmlReader(path, options);
                    case ConfigType.Json:
                        return GetJsonReaderInstance(path, options);
                    default:
                    throw new Exception("Unknown config reader.");
            }
        }

        private static JsonReader GetJsonReaderInstance(string path, ConfigLoaderOptions options)
        {
            var policy = options == null ? ConfigCachingPolicy.None : options.CachingPolicy;
            if (policy == ConfigCachingPolicy.None)
            {
                return new JsonReader(path, options);
            }

            return readerStore.GetOrSet(ComputeHashForFile(path, policy), () => new JsonReader(path, options));
        }

        private static string ComputeHashForFile(string path, ConfigCachingPolicy policy)
        {
            switch (policy)
            {
                case ConfigCachingPolicy.ByFileContent:
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(path))
                        {
                            var fileMd5 = md5.ComputeHash(stream);
                            return BitConverter.ToString(fileMd5);
                        }
                    }
                case ConfigCachingPolicy.ByFileModified:
                    var fileInfo = new FileInfo(path);
                    return fileInfo.LastWriteTimeUtc.ToBinary().ToString();
                case ConfigCachingPolicy.Permanent:
                    return path;
            }

            throw new InvalidOperationException("Cannot calculate file hash for ConfigCachinePolicy.None");
        } 

    }
}
