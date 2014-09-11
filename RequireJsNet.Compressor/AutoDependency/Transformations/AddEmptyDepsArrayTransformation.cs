using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jint.Parser.Ast;

using RequireJsNet.Compressor.Parsing;
using RequireJsNet.Compressor.Transformations;

namespace RequireJsNet.Compressor.AutoDependency.Transformations
{
    internal class AddEmptyDepsArrayTransformation : IRequireTransformation
    {
        public RequireCall RequireCall { get; set; }

        public static AddEmptyDepsArrayTransformation Create(RequireCall call)
        {
            return new AddEmptyDepsArrayTransformation
            {
                RequireCall = call
            };
        }

        public void Execute(ref string script)
        {
            var call = RequireCall.ParentNode.Node.As<CallExpression>();
            var lastArg = call.Arguments.Last();
            
            var beforeInsertPoint = script.Substring(0, lastArg.Range[0]);
            var afterInsertPoint = script.Substring(lastArg.Range[0], script.Length - lastArg.Range[0]);
            script = beforeInsertPoint + "[]," + afterInsertPoint;
        }

        public int[] GetAffectedRange()
        {
            var call = RequireCall.ParentNode.Node.As<CallExpression>();

            // since there's no range for the argument list itself and we might not have an identifier at all,
            // just return something that positions it where it should be in the execution pipeline
            var calleeEnd = call.Callee.Range[1];

            // added + 1 to the range so that this gets executed before AddIdentifierTransformation
            return new int[] { calleeEnd + 1, calleeEnd + 2 };
        }
    }
}
