using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Tests.Extensions
{
    public static class FileHelpers
    {
        public static string ReadTestData(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("File name cannot be empty.");
            }

            var asm = Assembly.GetExecutingAssembly();
            var defaultNamespace = "RequireJsNet.Tests.TestData";
            if (!name.StartsWith(defaultNamespace))
            {
                name = defaultNamespace + "." + name.TrimStart('.');
            }

            using (Stream stream = asm.GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
