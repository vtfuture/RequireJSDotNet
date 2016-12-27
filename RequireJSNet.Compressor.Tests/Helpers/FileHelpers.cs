using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RequireJSNet.Compressor.Tests.Helpers
{
    public static class FileHelpers
    {
        public static string ReadTestData(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("File name cannot be empty.", nameof(name));

            var asm = Assembly.GetExecutingAssembly();
            var defaultNamespace = "RequireJSNet.Compressor.Tests.TestData";
            if (!name.StartsWith(defaultNamespace))
                name = defaultNamespace + "." + name.TrimStart('.');

            var stream = asm.GetManifestResourceStream(name);
            if (stream == null)
                throw new FileNotFoundException(name);

            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }
    }
}
