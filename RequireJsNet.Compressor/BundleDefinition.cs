using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor
{
    internal class BundleDefinition
    {
        public string Name { get; set; }
        public List<BundleItem> Items { get; set; }
        public bool IsVirtual { get; set; }
    }
}
