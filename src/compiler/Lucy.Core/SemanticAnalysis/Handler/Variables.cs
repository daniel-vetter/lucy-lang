using System;
using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    internal static class VariablesHandler
    {
        /// <summary>
        /// Returns the best matching variable reference target
        /// </summary>
        /// <see cref="GetBestFunctionCallTargetEx.GetBestFunctionCallTarget"/>
        [DbQuery]
        public static INodeId<SyntaxTreeNode>? GetBestVariableReferenceTarget(IDb db, INodeId<VariableReferenceExpressionSyntaxNode> nodeId)
        {
            var node = db.GetNodeById(nodeId);
            var symbolDeclarations = db.GetSymbolDeclarations(node.Token.NodeId);
            return symbolDeclarations.FirstOrDefault()?.DeclaringNodeId;
        }
    }
}
