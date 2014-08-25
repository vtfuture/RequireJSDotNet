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
