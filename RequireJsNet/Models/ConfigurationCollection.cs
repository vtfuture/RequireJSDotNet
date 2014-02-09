using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Models
{
    internal class ConfigurationCollection
    {
        public string FilePath { get; set; }
        public RequirePaths Paths { get; set; }
        public RequireShim Shim { get; set; }
        public RequireMap Map { get; set; }
    }
}
