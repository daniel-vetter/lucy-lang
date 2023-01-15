using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class ScopeHandler
    {
        [DbQuery]
        public static Scope GetScopeTree(IDb db, string documentPath)
        {
            var rootNode = db.GetSyntaxTree(documentPath);

            var importScopeEntries = new ComparableReadOnlyList<ScopeEntry>.Builder();
            foreach (var import in db.GetImports(documentPath).Valid)
            foreach (var function in db.GetTopLevelFunctions(import.Path))
                importScopeEntries.Add(new SymbolDeclaration(function.Name.Text, function.Name.NodeId, function.NodeId));
            
            importScopeEntries.Add(db.GetScopeFromStatementList(rootNode.StatementList.NodeId));
            return new Scope(importScopeEntries.Build());
        }

        [DbQuery]
        public static Scope GetScopeFromStatementList(IDb db, INodeId<StatementListSyntaxNode> nodeId)
        {
            var sl = db.GetNodeById(nodeId);
            var result = new ComparableReadOnlyList<ScopeEntry>.Builder();
            TraverseStatementList(db, sl, result);
            return new Scope(result.Build());
        }

        [DbQuery]
        public static Scope GetScopeFromFunctionDeclaration(IDb db, INodeId<FunctionDeclarationStatementSyntaxNode> nodeId)
        {
            var fd = db.GetNodeById(nodeId);
            var result = new ComparableReadOnlyList<ScopeEntry>.Builder();

            foreach (var parameter in fd.ParameterList)
                result.Add(new SymbolDeclaration(parameter.VariableDefinition.VariableName.Text,
                    parameter.VariableDefinition.VariableName.NodeId, parameter.NodeId));

            if (fd.Body != null)
                TraverseStatementList(db, fd.Body, result);

            return new Scope(result.Build());
        }
        
        private static void TraverseStatementList(IDb db, StatementListSyntaxNode sl, ComparableReadOnlyList<ScopeEntry>.Builder result)
        {
            static void Traverse(IDb db, SyntaxTreeNode node, ComparableReadOnlyList<ScopeEntry>.Builder result)
            {
                switch (node)
                {
                    case FunctionDeclarationStatementSyntaxNode fd:
                        result.Add(db.GetScopeFromFunctionDeclaration(fd.NodeId));
                        return;
                    case VariableDeclarationStatementSyntaxNode vd:
                        result.Add(new SymbolDeclaration(vd.VariableDefinition.VariableName.Text, vd.VariableDefinition.VariableName.NodeId, vd.NodeId));
                        break;
                    case VariableReferenceExpressionSyntaxNode vr:
                        result.Add(new SymbolUsage(vr.Token.Text, vr.Token.NodeId));
                        break;
                    case FunctionCallExpressionSyntaxNode fc:
                        result.Add(new SymbolUsage(fc.FunctionName.Text, fc.FunctionName.NodeId));
                        break;
                }

                foreach (var child in node.GetChildNodes()) 
                    Traverse(db, child, result);
            }

            foreach (var statement in sl.Statements)
            {
                if (statement is FunctionDeclarationStatementSyntaxNode fd)
                    result.Add(new SymbolDeclaration(fd.FunctionName.Text, fd.FunctionName.NodeId, fd.NodeId));
            }

            Traverse(db, sl, result);
        }
    }

    public abstract record ScopeEntry;
    public abstract record NamedScopeEntry(string Name, INodeId<TokenNode> NameTokenNodeId) : ScopeEntry;

    public record Scope(ComparableReadOnlyList<ScopeEntry> Entries) : ScopeEntry;
    public record SymbolUsage(string Name, INodeId<TokenNode> NameTokenNodeId) : NamedScopeEntry(Name, NameTokenNodeId);
    public record SymbolDeclaration(string Name, INodeId<TokenNode> NameTokenNodeId, INodeId<SyntaxTreeNode> DeclaringNodeId) : NamedScopeEntry(Name, NameTokenNodeId);
    
}
