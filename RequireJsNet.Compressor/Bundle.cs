using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor
{
    internal class Bundle
    {
        public string Output { get; set; }
        public List<FileSpec> Files { get; set; }
    }
}
