using RequireJsNet.Compressor.Parsing;

namespace RequireJsNet.Compressor.Transformations
{
    internal interface IRequireTransformation
    {
        RequireCall RequireCall { get; set; }

        void Execute(ref string script);

        int[] GetAffectedRange();
    }
}