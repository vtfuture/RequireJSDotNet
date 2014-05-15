namespace RequireJsNet.Compressor.Transformations
{
    using RequireJsNet.Compressor.Parsing;

    internal interface IRequireTransformation
    {
        object TransformationData { get; set; }

        RequireCall RequireCall { get; set; }

        void Execute(ref string script);

        int[] GetAffectedRange();
    }
}