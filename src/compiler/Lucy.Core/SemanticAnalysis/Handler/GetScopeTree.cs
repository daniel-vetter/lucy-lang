using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Inputs;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record ScopeLayer(ComparableReadOnlyList<ScopeItem> Items, ScopeLayerType Type);
    public enum ScopeLayerType
    {
        Document,
        Function,
        StatementList
    }

    public abstract record ScopeItem();
    public record SubLayerScopeItem(ScopeLayer Layer) : ScopeItem;
    public record SymbolDeclarationScopeItem(string Name) : ScopeItem;
    public record SymbolUseScopeItem(string Name) : ScopeItem;
    
    public static class GetScopeMapHandler
    {
        [GenerateDbExtension] ///<see cref="GetScopeTreeEx.GetScopeTree"/>
        public static ScopeLayer GetScopeTree(IDb db, string documentPath)
        {
            var rootNode = db.GetSyntaxTree(documentPath);
            var items = new ComparableReadOnlyList<ScopeItem>.Builder();
            Traverse(db, rootNode.StatementList, items);
            return new ScopeLayer(items.Build(), ScopeLayerType.Document);
        }

        [GenerateDbExtension] ///<see cref="GetScopeLayerFromFunctionDeclarationEx.GetScopeLayerFromFunctionDeclaration"/>
        public static ScopeLayer GetScopeLayerFromFunctionDeclaration(IDb db, FunctionDeclarationStatementSyntaxNode node)
        {
            var items = new ComparableReadOnlyList<ScopeItem>.Builder();
            foreach (var param in node.ParameterList)
                items.Add(new SymbolDeclarationScopeItem(param.VariableDeclaration.VariableName.Token.Text));

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
                    result.Add(new SubLayerScopeItem(db.GetScopeLayerFromFunctionDeclaration(functionDeclarationStatement)));
                }
                else if (child is StatementListSyntaxNode statementListSyntaxNode)
                {
                    result.Add(new SubLayerScopeItem(db.GetScopeLayerFromStatementList(statementListSyntaxNode)));
                }
                else
                {
                    Traverse(db, child, result);
                }
            }
        }
    }
}
