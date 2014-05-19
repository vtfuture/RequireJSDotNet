using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Models
{
    internal class AutoBundle
    {
        public string Id { get; set; }

        public List<AutoBundleItem> Includes { get; set; }

        public List<AutoBundleItem> Excludes { get; set; }
    }
}
