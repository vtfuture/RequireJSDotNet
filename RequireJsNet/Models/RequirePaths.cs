using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RequireJsNet.Models
{
    public class RequirePaths
    {
        [XmlElement("path")]
        public List<RequirePath> PathList { get; set; }
    }
}
