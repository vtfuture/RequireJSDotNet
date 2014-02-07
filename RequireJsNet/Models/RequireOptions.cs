using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Models
{
    internal class RequireOptions
    {
        public string Locale { get; set; }
        public dynamic PageOptions { get; set; }
        public dynamic GlobalObject { get; set; }
    }
}
