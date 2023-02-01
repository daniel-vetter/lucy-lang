using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    [QueryGroup]
    public class ScopeTreeBuilder
    {
        private readonly Nodes _nodes;
        private readonly Imports _imports;
        [Inject] private readonly Functions _functions = null!;

        public ScopeTreeBuilder(Nodes nodes, Imports imports)
        {
            _nodes = nodes;
            _imports = imports;
        }
        
        public virtual Scope GetScopeTree(string documentPath)
        {
            var rootNode = _nodes.GetSyntaxTree(documentPath);

            var importScopeEntries = new ComparableReadOnlyList<ScopeEntry>.Builder();
            foreach (var import in _imports.GetImports(documentPath).Valid)
            foreach (var function in _functions.GetTopLevelFunctions(import.Path))
                importScopeEntries.Add(new SymbolDeclaration(function.Name.Text, function.Name.NodeId, function.NodeId));
            
            importScopeEntries.Add(GetScopeFromStatementList(rootNode.StatementList.NodeId));
            return new Scope(importScopeEntries.Build());
        }

        protected virtual Scope GetScopeFromStatementList(INodeId<StatementListSyntaxNode> nodeId)
        {
            var sl = _nodes.GetNodeById(nodeId);
            var result = new ComparableReadOnlyList<ScopeEntry>.Builder();
            TraverseStatementList(sl, result);
            return new Scope(result.Build());
        }

        protected virtual Scope GetScopeFromFunctionDeclaration(INodeId<FunctionDeclarationStatementSyntaxNode> nodeId)
        {
            var fd = _nodes.GetNodeById(nodeId);
            var result = new ComparableReadOnlyList<ScopeEntry>.Builder();

            foreach (var parameter in fd.ParameterList)
                result.Add(new SymbolDeclaration(parameter.VariableDefinition.VariableName.Text,
                    parameter.VariableDefinition.VariableName.NodeId, parameter.NodeId));

            if (fd.Body != null)
                TraverseStatementList(fd.Body, result);

            return new Scope(result.Build());
        }
        
        private void TraverseStatementList(StatementListSyntaxNode sl, ComparableReadOnlyList<ScopeEntry>.Builder result)
        {
            void Traverse(SyntaxTreeNode node, ComparableReadOnlyList<ScopeEntry>.Builder result)
            {
                switch (node)
                {
                    case FunctionDeclarationStatementSyntaxNode fd:
                        result.Add(GetScopeFromFunctionDeclaration(fd.NodeId));
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
                    Traverse(child, result);
            }

            foreach (var statement in sl.Statements)
            {
                if (statement is FunctionDeclarationStatementSyntaxNode fd)
                    result.Add(new SymbolDeclaration(fd.FunctionName.Text, fd.FunctionName.NodeId, fd.NodeId));
            }

            Traverse(sl, result);
        }
    }

    public abstract record ScopeEntry;
    public abstract record NamedScopeEntry(string Name, INodeId<TokenNode> NameTokenNodeId) : ScopeEntry;

    public record Scope(ComparableReadOnlyList<ScopeEntry> Entries) : ScopeEntry;
    public record SymbolUsage(string Name, INodeId<TokenNode> NameTokenNodeId) : NamedScopeEntry(Name, NameTokenNodeId);
    public record SymbolDeclaration(string Name, INodeId<TokenNode> NameTokenNodeId, INodeId<SyntaxTreeNode> DeclaringNodeId) : NamedScopeEntry(Name, NameTokenNodeId);
    
}
