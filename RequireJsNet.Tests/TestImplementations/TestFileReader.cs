using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Configuration;
using RequireJsNet.Tests.Extensions;

namespace RequireJsNet.Tests.TestImplementations
{
    class TestFileReader : IFileReader
    {
        private string callingMember;

        private string callingClass;

        public TestFileReader([CallerMemberName] string callerMember = null, [CallerFilePath] string callerPath = null)
        {
            callingMember = callerMember;
            callingClass = Path.GetFileNameWithoutExtension(callerPath);
        }

        public string FilePath
        {
            get
            {
                return callingClass + "." + callingMember + ".json";
            }
        }

        public string ReadFile(string path = null)
        {
            var finalPath = path ?? FilePath;
            return FileHelpers.ReadTestData(finalPath);
        }
    }
}
