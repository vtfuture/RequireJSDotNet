using System.Collections.Generic;

namespace RequireJsNet.Models
{
    internal class AutoBundle
    {
        public string Id { get; set; }

        public string OutputPath { get; set; }

        public List<AutoBundleItem> Includes { get; set; }

        public List<AutoBundleItem> Excludes { get; set; }

        public string ContainingConfig { get; set; }
    }
}
