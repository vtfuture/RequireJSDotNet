// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

namespace RequireJsNet.Compressor.Parsing
{
    internal class ScriptLine
    {
        public string LineText { get; set; }

        public int StartingIndex { get; set; }

        public int NewLineLength { get; set; }
    }
}
