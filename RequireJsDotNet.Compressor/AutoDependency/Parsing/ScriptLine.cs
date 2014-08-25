using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsDotNet.Compressor.Parsing
{
    internal class ScriptLine
    {
        public string LineText { get; set; }

        public int StartingIndex { get; set; }

        public int NewLineLength { get; set; }
    }
}
