using System.Collections.Generic;

namespace RequireJsNet.Models
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
