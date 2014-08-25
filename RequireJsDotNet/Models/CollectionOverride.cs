using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsDotNet.Models
{
    internal class CollectionOverride
    {
        public RequirePaths Paths { get; set; }

        public RequireShim Shim { get; set; }

        public RequireMap Map { get; set; }

        public string BundleId { get; set; }

        public List<string> BundledScripts { get; set; }
    }
}
