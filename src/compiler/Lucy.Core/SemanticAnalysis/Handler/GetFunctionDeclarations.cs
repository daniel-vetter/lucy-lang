﻿using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetFunctionDeclarations(string DocumentPath) : IQuery<GetFunctionDeclarationsResult>;
    public record GetFunctionDeclarationsResult(ComparableReadOnlyList<NodeId> Ids);

    public class GetFunctionDeclarationsHandler : QueryHandler<GetFunctionDeclarations, GetFunctionDeclarationsResult>
    {
        public override GetFunctionDeclarationsResult Handle(IDb db, GetFunctionDeclarations query)
        {
            var r = db.Query(new GetNodesByType(query.DocumentPath, typeof(ImmutableFunctionDeclarationStatementSyntaxNode)));
            return new GetFunctionDeclarationsResult(r.Nodes.ToComparableReadOnlyList());
        }
    }
}
