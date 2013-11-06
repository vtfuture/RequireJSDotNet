using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor
{
    internal class BundleItem
    {
        public string ModuleName { get; set; }
        public string PhysicalPath { get; set; }
        public string CompressionType { get; set; }
    }
}
