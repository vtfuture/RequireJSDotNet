// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Linq;
using Jint.Parser.Ast;
using RequireJsNet.Compressor.Parsing;
using RequireJsNet.Helpers;

namespace RequireJsNet.Compressor.Transformations
{
    internal class NormalizeDepsTransformation : IRequireTransformation
    {
        public RequireCall RequireCall { get; set; }

        public static NormalizeDepsTransformation Create(RequireCall requireCall)
        {
            return new NormalizeDepsTransformation
            {
                RequireCall = requireCall
            };
        }

        public void Execute(ref string script)
        {
            var dependencies =
                RequireCall.DependencyArrayNode.Elements.Where(r => r.Type == SyntaxNodes.Literal).Cast<Literal>();

            foreach (var dependency in dependencies)
            {
                var range = new int[] {dependency.Range[0] + 1, dependency.Range[1] - 1};

                var preDeps = script.Substring(0, range[0]);
                var postDeps = script.Substring(range[1], script.Length - range[1]);
                var deps = script.Substring(range[0], range[1] - range[0]).ToModuleName();
                script = preDeps + deps + postDeps;
            }
        }

        public int[] GetAffectedRange()
        {
            return RequireCall.DependencyArrayNode.Range;
        }
    }
}
