using System.Collections.Generic;

namespace RequireJsNet.Compressor
{
    internal class Bundle
    {
        public string Output { get; set; }

        public List<FileSpec> Files { get; set; }
    }
}
