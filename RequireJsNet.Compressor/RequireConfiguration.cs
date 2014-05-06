using System.Collections.Generic;

namespace RequireJsNet.Compressor
{
    internal class RequireConfiguration
    {
        public RequireConfiguration()
        {
            Paths = new List<PathItem>();
            Bundles = new List<BundleDefinition>();
        }

        public string EntryPoint { get; set; }

        public List<PathItem> Paths { get; set; }

        public List<BundleDefinition> Bundles { get; set; }
    }
}
