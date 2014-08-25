using System.Collections.Generic;

namespace RequireJsNet.Compressor.Models
{
    internal class RequireFile
    {
        public string Name { get; set; }

        public string Content { get; set; }

        public List<string> Dependencies { get; set; }
    }
}
