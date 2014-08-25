// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

using Jint.Parser.Ast;

using RequireJsNet.Compressor.Models;

namespace RequireJsNet.Compressor.Parsing
{
    internal enum RequireCallType
    {
        Define,
        Require
    }

    internal class RequireCall
    {
        public RequireCall()
        {
            Children = new List<RequireCall>();
            Dependencies = new List<string>();
        }

        public string Id { get; set; }

        public NodeWithChildren ParentNode { get; set; }

        public bool IsModule { get; set; }

        public RequireCallType Type { get; set; }

        public List<string> Dependencies { get; set; }

        public List<RequireCall> Children { get; set; }

        public Literal SingleDependencyNode { get; set; }

        public Literal ModuleIdentifierNode { get; set; }

        public ArrayExpression DependencyArrayNode { get; set; }

        public FunctionExpression ModuleDefinitionNode { get; set; }
    }
}
