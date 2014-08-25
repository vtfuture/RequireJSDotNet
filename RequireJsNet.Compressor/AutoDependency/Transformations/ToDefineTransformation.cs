using Jint.Parser.Ast;

using RequireJsNet.Compressor.Parsing;

namespace RequireJsNet.Compressor.Transformations
{
    internal class ToDefineTransformation : IRequireTransformation
    {
        public RequireCall RequireCall { get; set; }

        public static ToDefineTransformation Create(RequireCall requireCall)
        {
            return new ToDefineTransformation
                       {
                           RequireCall = requireCall
                       };
        }

        public void Execute(ref string script)
        {
            var range = this.GetAffectedRange();

            var preIdentifier = script.Substring(0, range[0]);
            var postIdentifier = script.Substring(range[1], script.Length - range[1]);
            script = preIdentifier + "define" + postIdentifier;
            RequireCall.Type = RequireCallType.Define;
        }

        public int[] GetAffectedRange()
        {
            var node = RequireCall.ParentNode.Node.As<CallExpression>();
            var identifier = node.Callee.As<Identifier>();
            return identifier.Range;
        }
    }
}
