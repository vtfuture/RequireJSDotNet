using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Models
{
    public class BundleItem
    {
        public string ModuleName { get; set; }

        public string CompressionType { get; set; }

        public string RelativePath { get; set; }
    }
}
