using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RequireJsNet.Models
{
    public class ShimEntry
    {
        public string For { get; set; }
        public string Exports { get; set; }
        [XmlElement("add")]
        public List<RequireDependency> Dependencies { get; set; } 
    }
}
