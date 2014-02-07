using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RequireJsNet.Models
{
    [XmlRoot("configuration")]
    public class RequireXmlConfig
    {
        [XmlElement("paths")]
        public RequirePaths Paths { get; set; }

        [XmlElement("shim")]
        public RequireShim Shim { get; set; }
        
    }
}
