using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.ResxToJs.Models
{
    internal class ResourceInfo
    {
        public string CultureShort { get; set; }

        public string ResxPath { get; set; }

        public string ProjectedJsPath { get; set; }

        public Dictionary<string, string> Resources { get; set; }
    }
}
