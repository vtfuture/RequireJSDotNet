using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
