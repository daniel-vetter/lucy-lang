using System;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public abstract record ScopeEntry;
    public record SubScope(ComparableReadOnlyList<ScopeEntry> Entries) : ScopeEntry;
    public record VariableDeclarationScopeEntry(INodeId<VariableDefinitionSyntaxNode> NodeId, string Name) : ScopeEntry;
    public record FunctionDeclarationScopeEntry(INodeId<FunctionDeclarationStatementSyntaxNode> NodeId, string Name, SubScope SubScope) : ScopeEntry;
    public record IdentifierUsed(INodeId<TokenNode> TokenNode, string Name) : ScopeEntry;

    public static class ScopeHandler
    {
        [DbQuery]
        public static SubScope CreateScopeTree(IDb db, string documentPath)
        {
            var root = db.GetSyntaxTree(documentPath);
            var scopeRoot = db.CreateScopeFromStatementList(root.StatementList);
            return scopeRoot;
        }


        [DbQuery]
        public static SubScope CreateScopeFromStatementList(IDb db, StatementListSyntaxNode node)
        {
            var result = new ComparableReadOnlyList<ScopeEntry>.Builder();

            void Traverse(SyntaxTreeNode subNode)
            {
                switch (subNode)
                {
                    case VariableDeclarationStatementSyntaxNode vd:
                        result.Add(new VariableDeclarationScopeEntry(vd.VariableDefinition.NodeId, vd.VariableDefinition.VariableName.Text));
                        break;
                    case FunctionDeclarationStatementSyntaxNode fd:
                        result.Add(db.CreateScopeFromFunctionDeclaration(fd));
                        break;
                    case StatementListSyntaxNode sl:
                        result.Add(db.CreateScopeFromStatementList(sl));
                        break;
                    case FunctionCallExpressionSyntaxNode fc:
                        result.Add(new IdentifierUsed(fc.FunctionName.NodeId, fc.FunctionName.Text));
                        foreach (var child in subNode.GetChildNodes())
                            Traverse(child);
                        break;
                    default:
                    {
                        foreach (var child in subNode.GetChildNodes())
                            Traverse(child);
                        break;
                    }
                }
            }
            
            foreach (var child in node.GetChildNodes())
            {
                Traverse(child);
            }
            
            return new SubScope(result.Build());
        }

        [DbQuery]
        public static FunctionDeclarationScopeEntry CreateScopeFromFunctionDeclaration(IDb db, FunctionDeclarationStatementSyntaxNode node)
        {
            var result = new ComparableReadOnlyList<ScopeEntry>.Builder();
            foreach (var parameter in node.ParameterList)
            {
                result.Add(new VariableDeclarationScopeEntry(parameter.VariableDefinition.NodeId, parameter.VariableDefinition.VariableName.Text));
            }

            if (node.Body != null)
                result.AddRange(db.CreateScopeFromStatementList(node.Body).Entries);
            
            return new FunctionDeclarationScopeEntry(node.NodeId, node.FunctionName.Text, new SubScope(result.Build()));
        }
    }
}
