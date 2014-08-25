using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsDotNet.Compressor.Models
{
    internal class RequireFile
    {
        public string Name { get; set; }

        public string Content { get; set; }

        public List<string> Dependencies { get; set; }
    }
}
