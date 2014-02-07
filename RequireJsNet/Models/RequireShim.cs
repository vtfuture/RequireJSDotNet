using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RequireJsNet.Models
{
    public class RequireShim
    {
        [XmlElement("dependencies")]
        public List<ShimEntry> ShimEntries { get; set; }
    }
}
