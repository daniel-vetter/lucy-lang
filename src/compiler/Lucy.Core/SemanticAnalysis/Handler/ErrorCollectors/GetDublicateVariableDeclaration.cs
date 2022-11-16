using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
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
            var root = db.GetScopeTree(query.DocumentPath);
            var result = new ComparableReadOnlyList<Error>.Builder();
            Traverse(db, root, new HashSet<string>(), result);
            return new GetDublicateVariableDeclarationsResult(result.Build());
        }

        private void Traverse(IDb db, ScopeItem scopeItem, HashSet<string> knownNames, ComparableReadOnlyList<Error>.Builder errors)
        {
            var node = db.GetNodeById(scopeItem.NodeId);
            if (node is FunctionDeclarationStatementSyntaxNode functionDeclarationStatementSyntaxNode)
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
            else if (node is FunctionDeclarationParameterSyntaxNode functionDeclarationParameterSyntaxNode)
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
            else if (node is DocumentRootSyntaxNode)
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
