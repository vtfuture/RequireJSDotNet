using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RequireJsNet.Models
{
    public class RequirePath
    {
        [XmlAttribute("key")]
        public string Key { get; set; }
        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
