// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using Yahoo.Yui.Compressor;

namespace RequireJsNet.Compressor
{
    public class CssCompressorTask : CompressorTask
    {
        private readonly ICssCompressor compressor;

        public CssCompressorTask()
            : this(new CssCompressor())
        {
        }

        public CssCompressorTask(ICssCompressor compressor)
            : base(compressor)
        {
            this.compressor = compressor;
        }

        public bool PreserveComments { get; set; }

        public override bool Execute()
        {
            this.compressor.RemoveComments = !PreserveComments;
            return base.Execute();
        }
    }
}
