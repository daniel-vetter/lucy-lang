using Lucy.Core.Parsing.Nodes.Statements;
using Lucy.Core.Parsing.Nodes.Trivia;
using System.Collections.Generic;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes
{
    public class DocumentRootSyntaxNodeParser
    {
        public static DocumentRootSyntaxNodeBuilder ReadDocumentSyntaxNode(Code code)
        {
            var statementList = StatementListSyntaxNodeParser.ReadStatementsWithoutBlock(code);
            var trailingTrivia = TriviaNodeParser.ReadList(code);
            
            return new DocumentRootSyntaxNodeBuilder(statementList, trailingTrivia);
        }
    }
}
