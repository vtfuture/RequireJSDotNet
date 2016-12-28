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
    internal class AddDepsTransformation : IRequireTransformation
    {
        public RequireCall RequireCall { get; set; }
        private List<string> Dependencies;

        public static AddDepsTransformation Create(RequireCall call, List<string> deps)
        {
            return new AddDepsTransformation
            {
                RequireCall = call,
                Dependencies = deps
            };
        }

        public void Execute(ref string script)
        {
            if (!Dependencies.Any())
                return;

            var call = RequireCall.ParentNode.Node.As<CallExpression>();
            var dependencyArgument = call.Arguments[call.Arguments.Count - 2];

            var insertPoint = dependencyArgument.Range[1] - 1;

            var beforeInsertPoint = script.Substring(0, insertPoint);
            var afterInsertPoint = script.Substring(insertPoint, script.Length - insertPoint);

            var codeToInject = "'" + string.Join("','", Dependencies.ToArray()) + "'";

            if (beforeInsertPoint.TrimEnd().Last() != '[')
                codeToInject = "," + codeToInject;

            script = beforeInsertPoint + codeToInject + afterInsertPoint;
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
