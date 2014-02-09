using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RequireJsNet.Models
{
    internal class RequireShim
    {
        public List<ShimEntry> ShimEntries { get; set; }
    }
}
