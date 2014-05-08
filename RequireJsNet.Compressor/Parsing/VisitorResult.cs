using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.Parsing
{
    internal class VisitorResult
    {
        public VisitorResult()
        {
            RequireCalls = new List<RequireCall>();    
        }

        public List<RequireCall> RequireCalls { get; set; } 
    }
}
