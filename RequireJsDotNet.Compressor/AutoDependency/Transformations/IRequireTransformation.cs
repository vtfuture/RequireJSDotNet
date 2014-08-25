namespace RequireJsDotNet.Compressor.Transformations
{
    using RequireJsDotNet.Compressor.Parsing;

    internal interface IRequireTransformation
    {
        RequireCall RequireCall { get; set; }

        void Execute(ref string script);

        int[] GetAffectedRange();
    }
}