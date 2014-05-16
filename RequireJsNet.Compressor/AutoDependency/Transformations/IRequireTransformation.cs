namespace RequireJsNet.Compressor.Transformations
{
    using RequireJsNet.Compressor.Parsing;

    internal interface IRequireTransformation
    {
        RequireCall RequireCall { get; set; }

        void Execute(ref string script);

        int[] GetAffectedRange();
    }
}