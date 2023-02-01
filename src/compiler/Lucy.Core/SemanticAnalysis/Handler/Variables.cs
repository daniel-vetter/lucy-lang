using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    [QueryGroup]
    public class Variables
    {
        private readonly Nodes _nodes;
        private readonly Symbols _symbols;

        public Variables(Nodes nodes, Symbols symbols)
        {
            _nodes = nodes;
            _symbols = symbols;
        }
        
        /// <summary>
        /// Returns the best matching variable reference target
        /// </summary>
        public virtual INodeId<SyntaxTreeNode>? GetBestVariableReferenceTarget(INodeId<VariableReferenceExpressionSyntaxNode> nodeId)
        {
            var node = _nodes.GetNodeById(nodeId);
            var symbolDeclarations = _symbols.GetSymbolDeclarations(node.Token.NodeId);
            return symbolDeclarations.FirstOrDefault()?.DeclaringNodeId;
        }
    }
}
