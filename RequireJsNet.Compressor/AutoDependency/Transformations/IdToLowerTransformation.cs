// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using Jint.Parser.Ast;

using RequireJsNet.Compressor.Parsing;

namespace RequireJsNet.Compressor.Transformations
{
    internal class IdToLowerTransformation : IRequireTransformation
    {
        public RequireCall RequireCall { get; set; }

        protected string IdentifierName { get; set; }

        public static IdToLowerTransformation Create(RequireCall call)
        {
            return new IdToLowerTransformation
            {
                RequireCall = call
            };
        }

        public void Execute(ref string script)
        {
            var range = this.GetAffectedRange();

            var preDeps = script.Substring(0, range[0]);
            var postDeps = script.Substring(range[1], script.Length - range[1]);
            var deps = script.Substring(range[0], range[1] - range[0]).ToLower();

            script = preDeps + deps + postDeps;
        }

        public int[] GetAffectedRange()
        {
            var call = RequireCall.ParentNode.Node.As<CallExpression>();

            // since there's no range for the argument list itself and we might not have an identifier at all,
            // just return something that positions it where it should be in the execution pipeline
            var calleeEnd = call.Callee.Range[1];
            return new int[] { calleeEnd, calleeEnd + 1 };
        }
    }
}