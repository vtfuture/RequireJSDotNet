using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Configuration;

namespace RequireJsNet.ConfigConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!args.Any())
            {
                return;
            }

            var path = args[0];

            var reader = ReaderFactory.CreateReader(path, new ConfigLoaderOptions { ProcessAutoBundles = true, ProcessBundles = true });
            var config = reader.ReadConfig();

            var outPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + ".out" + ".json";

            var writer = WriterFactory.CreateWriter(
                outPath,
                new ConfigLoaderOptions { ProcessAutoBundles = true, ProcessBundles = true });
            writer.WriteConfig(config);
        }
    }
}
