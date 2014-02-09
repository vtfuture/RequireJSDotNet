using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RequireJsNet.Models
{
    internal class ShimEntry
    {
        public string For { get; set; }
        public string Exports { get; set; }
        public List<RequireDependency> Dependencies { get; set; } 
    }
}
