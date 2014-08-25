using Yahoo.Yui.Compressor;

namespace RequireJsDotNet.Compressor
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
