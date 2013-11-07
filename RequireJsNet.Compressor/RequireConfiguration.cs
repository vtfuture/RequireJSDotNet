using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor
{
    internal class RequireConfiguration
    {
        public string EntryPoint { get; set; }
        public List<PathItem> Paths { get; set; }
        public List<BundleDefinition> Bundles { get; set; }

        public RequireConfiguration()
        {
            Paths = new List<PathItem>();
            Bundles = new List<BundleDefinition>();
        }
    }
}
