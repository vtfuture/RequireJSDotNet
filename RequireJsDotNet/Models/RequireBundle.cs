using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsDotNet.Models
{
    public class RequireBundle
    {
        public RequireBundle()
        {
            Includes = new List<string>();    
            BundleItems = new List<BundleItem>();
        }

        public string Name { get; set; }

        public bool IsVirtual { get; set; }

        public string OutputPath { get; set; }

        public List<string> Includes { get; set; }

        public List<BundleItem> BundleItems { get; set; }

        public bool ParsedIncludes { get; set; }
    }
}
