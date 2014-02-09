using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Models
{
    internal class RequireMapElement
    {
        public string For { get; set; }
        public List<RequireReplacement> Replacements { get; set; }
    }
}
