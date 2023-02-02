using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    [QueryGroup]
    public class Variables
    {
        private readonly Nodes _nodes;
        private readonly SymbolResolver _symbolResolver;

        public Variables(Nodes nodes, SymbolResolver symbolResolver)
        {
            _nodes = nodes;
            _symbolResolver = symbolResolver;
        }
        
        /// <summary>
        /// Returns the best matching variable reference target
        /// </summary>
        public virtual INodeId<SyntaxTreeNode>? GetBestVariableReferenceTarget(INodeId<VariableReferenceExpressionSyntaxNode> nodeId)
        {
            var node = _nodes.GetNodeById(nodeId);
            var symbolDeclarations = _symbolResolver.GetSymbolDeclarations(node.Token.NodeId);
            return symbolDeclarations.FirstOrDefault()?.DeclaringNodeId;
        }
    }
}
