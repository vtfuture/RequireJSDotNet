// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;

namespace RequireJsNet.Compressor.Models
{
    using Jint.Parser.Ast;

    internal class NodeWithChildren
    {
        public NodeWithChildren()
        {
            Children = new List<NodeWithChildren>();    
        }

        public SyntaxNode Node { get; set; }

        public List<NodeWithChildren> Children { get; set; }

        public NodeWithChildren Parent { get; set; }
    }
}
