using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System;
using System.Collections.Generic;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors
{
    public record GetDublicateDeclarations(string DocumentPath) : IQuery<GetDublicateVariableDeclarationsResult>;
    public record GetDublicateVariableDeclarationsResult(ComparableReadOnlyList<Error> Errors);

    public record Error(NodeId NodeId, string Message);

    public class GetDublicateVariableDeclarationsHandler : QueryHandler<GetDublicateDeclarations, GetDublicateVariableDeclarationsResult>
    {
        public override GetDublicateVariableDeclarationsResult Handle(IDb db, GetDublicateDeclarations query)
        {
            var root = db.Query(new GetScopeTree(query.DocumentPath)).DocumentScope;
            var result = new ComparableReadOnlyList<Error>.Builder();
            Traverse(db, root, new HashSet<string>(), result);
            return new GetDublicateVariableDeclarationsResult(result.Build());
        }

        private void Traverse(IDb db, ScopeItem scopeItem, HashSet<string> knownNames, ComparableReadOnlyList<Error>.Builder errors)
        {
            var node = db.Query(new GetNodeById(scopeItem.NodeId)).Node;
            if (node is ImmutableFunctionDeclarationStatementSyntaxNode functionDeclarationStatementSyntaxNode)
            {
                var name = functionDeclarationStatementSyntaxNode.FunctionName.Token.Text;
                if (knownNames.Contains(name))
                {
                    errors.Add(new Error(node.NodeId, $"A symbol named '{name}' was already defined in this or a parent scope."));
                }
                else
                {
                    knownNames.Add(name);
                }
            }
            else if (node is ImmutableFunctionDeclarationParameterSyntaxNode functionDeclarationParameterSyntaxNode)
            {
                var name = functionDeclarationParameterSyntaxNode.VariableDeclaration.VariableName.Token.Text;
                if (knownNames.Contains(name))
                {
                    errors.Add(new Error(node.NodeId, $"A symbol named '{name}' was already defined in this or a parent scope."));
                }
                else
                {
                    knownNames.Add(name);
                }
            }
            else if (node is ImmutableDocumentRootSyntaxNode)
            {

            }
            else throw new NotImplementedException("Unsupported node type: " + node.GetType().Name);

            foreach (var subScope in scopeItem.Items)
            {
                Traverse(db, subScope, knownNames, errors);
            }
        }
    }
}
