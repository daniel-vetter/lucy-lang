using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Inputs;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record ScopeTree(ScopeLayer RootLayer)
    {
        private Dictionary<ScopeLayer, ScopeLayer>? _parentLayers;

        public ScopeLayer? GetParentLayer(ScopeLayer layer)
        {
            if (_parentLayers == null)
                _parentLayers = BuildIndex();

            if (!_parentLayers.TryGetValue(layer, out var parentLayer))
                return null;

            return parentLayer;
        }

        private Dictionary<ScopeLayer, ScopeLayer> BuildIndex()
        {
            var dict = new Dictionary<ScopeLayer, ScopeLayer>();

            void Traverse(ScopeLayer layer)
            {
                foreach (var subLayer in layer.Items.OfType<SubLayerScopeItem>())
                {
                    Traverse(subLayer.Layer);
                    dict.Add(subLayer.Layer, layer);
                }
            }

            Traverse(RootLayer);
            return dict;
        }
    }

    public record ScopeLayer(ComparableReadOnlyList<ScopeItem> Items, ScopeLayerType Type);
    public enum ScopeLayerType
    {
        Document,
        Function,
        StatementList
    }

    public enum SymbolType
    {
        Function,
        Variable
    }

    public abstract record ScopeItem();
    public record SubLayerScopeItem(ScopeLayer Layer) : ScopeItem;
    public record SymbolDeclarationScopeItem(string Name, NodeId NodeId, NodeId IdentfierTokenNodeId, SymbolType SymbolType) : ScopeItem;
    public record SymbolUseScopeItem(string Name, NodeId ExpressionNodeId, NodeId IdentifierTokenNodeId, SymbolType SymbolType) : ScopeItem;
    
    public static class GetScopeMapHandler
    {
        [GenerateDbExtension] ///<see cref="GetScopeTreeEx.GetScopeTree"/>
        public static ScopeTree GetScopeTree(IDb db, string documentPath)
        {
            var rootNode = db.GetSyntaxTree(documentPath);
            var items = new ComparableReadOnlyList<ScopeItem>.Builder();
            Traverse(db, rootNode.StatementList, items);
            return new ScopeTree(new ScopeLayer(items.Build(), ScopeLayerType.Document));
        }

        [GenerateDbExtension] ///<see cref="GetScopeLayerFromFunctionDeclarationEx.GetScopeLayerFromFunctionDeclaration"/>
        public static ScopeLayer GetScopeLayerFromFunctionDeclaration(IDb db, FunctionDeclarationStatementSyntaxNode node)
        {
            var items = new ComparableReadOnlyList<ScopeItem>.Builder();
            foreach (var param in node.ParameterList)
                items.Add(new SymbolDeclarationScopeItem(
                    param.VariableDeclaration.VariableName.Token.Text,
                    param.VariableDeclaration.NodeId,
                    param.VariableDeclaration.VariableName.Token.NodeId,
                    SymbolType.Function));

            if (node.Body is not null)
                foreach(var statement in node.Body.Statements)
                    Traverse(db, statement, items);

            return new ScopeLayer(items.Build(), ScopeLayerType.Function);
        }

        [GenerateDbExtension] ///<see cref="GetScopeLayerFromFunctionDeclarationEx.GetScopeLayerFromFunctionDeclaration"/>
        public static ScopeLayer GetScopeLayerFromStatementList(IDb db, StatementListSyntaxNode node)
        {
            var items = new ComparableReadOnlyList<ScopeItem>.Builder();
            Traverse(db, node, items);
            return new ScopeLayer(items.Build(), ScopeLayerType.StatementList);
        }

        private static void Traverse(IDb db, SyntaxTreeNode node, ComparableReadOnlyList<ScopeItem>.Builder result)
        {
            foreach (var child in node.GetChildNodes())
            {
                if (child is FunctionDeclarationStatementSyntaxNode functionDeclarationStatement)
                {
                    result.Add(new SymbolDeclarationScopeItem(
                        functionDeclarationStatement.FunctionName.Token.Text,
                        functionDeclarationStatement.NodeId,
                        functionDeclarationStatement.FunctionName.Token.NodeId,
                        SymbolType.Function)
                    );

                    result.Add(new SubLayerScopeItem(db.GetScopeLayerFromFunctionDeclaration(functionDeclarationStatement)));
                }
                else if (child is StatementListSyntaxNode statementListSyntaxNode)
                {
                    result.Add(new SubLayerScopeItem(db.GetScopeLayerFromStatementList(statementListSyntaxNode)));
                }
                else if (child is FunctionCallExpressionSyntaxNode functionCallExpressionSyntaxNode)
                {
                    result.Add(new SymbolUseScopeItem(
                        functionCallExpressionSyntaxNode.FunctionName.Token.Text,
                        functionCallExpressionSyntaxNode.NodeId,
                        functionCallExpressionSyntaxNode.FunctionName.Token.NodeId,
                        SymbolType.Function));
                }
                else
                {
                    Traverse(db, child, result);
                }
            }
        }
    }
}
