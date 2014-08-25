using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsDotNet.Models
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
