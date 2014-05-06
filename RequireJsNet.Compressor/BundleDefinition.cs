using System.Collections.Generic;

namespace RequireJsNet.Compressor
{
    internal class BundleDefinition
    {
        public string Name { get; set; }

        public List<BundleItem> Items { get; set; }

        public bool IsVirtual { get; set; }

        public string OutputPath { get; set; }

        public List<string> Includes { get; set; }

        public bool ParsedIncludes { get; set; }
    }
}
