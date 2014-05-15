namespace RequireJsNet.Compressor.Transformations
{
    using System;

    using RequireJsNet.Compressor.Parsing;

    internal class AddIdentifierTransformation : IRequireTransformation
    {
        public object TransformationData { get; set; }

        public RequireCall RequireCall { get; set; }

        public void Execute(ref string script)
        {
            throw new NotImplementedException();
        }

        public int[] GetAffectedRange()
        {
            return null;
        }
    }
}