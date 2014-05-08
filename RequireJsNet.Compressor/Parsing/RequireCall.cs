using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.Parsing
{
    internal enum RequireCallType
    {
        Define,
        Require
    }

    internal class RequireCall
    {
        public RequireCall()
        {
            Children = new List<RequireCall>();
            Dependencies = new List<string>();
        }

        public string Id { get; set; }

        public bool IsModule { get; set; }

        public RequireCallType Type { get; set; }

        public List<string> Dependencies { get; set; }

        public List<RequireCall> Children { get; set; } 
    }
}
