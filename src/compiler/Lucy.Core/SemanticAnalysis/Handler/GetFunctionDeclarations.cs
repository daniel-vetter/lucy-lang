﻿using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class GetFunctionDeclarationsHandler
    {
        [GenerateDbExtension] ///<see cref="GetFunctionDeclarationsEx.GetFunctionDeclarations"/>
        public static ComparableReadOnlyList<FunctionDeclarationStatementSyntaxNode> GetFunctionDeclarations(IDb db, string documentPath)
        {
            var r = db.Query(new GetNodesByType(documentPath, typeof(FunctionDeclarationStatementSyntaxNode)));
            return r.Nodes.Cast<FunctionDeclarationStatementSyntaxNode>().ToComparableReadOnlyList();
        }
    }
}
