namespace RequireJsNet.Compressor.Transformations
{
    using RequireJsNet.Compressor.Parsing;

    internal class ToDefineTransformation : IRequireTransformation
    {
        public object TransformationData { get; set; }

        public RequireCall RequireCall { get; set; }

        public void Execute(ref string script)
        {
            throw new System.NotImplementedException();
        }

        public int[] GetAffectedRange()
        {
            return null;
        }
    }
}
