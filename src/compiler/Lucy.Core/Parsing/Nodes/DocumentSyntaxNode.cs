using Lucy.Core.Parsing.Nodes.Statements;
using Lucy.Core.Parsing.Nodes.Trivia;
using Lucy.Core.Parsing;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes
{
    public class DocumentSyntaxNode : SyntaxTreeNode
    {
        public DocumentSyntaxNode(StatementListSyntaxNode statementList, List<TriviaNode> trailingTrivia)
        {
            StatementList = statementList;
            TrailingTrivia = trailingTrivia;
        }

        public StatementListSyntaxNode StatementList { get; }
        public List<TriviaNode> TrailingTrivia { get; }

        public static DocumentSyntaxNode ReadDocumentSyntaxNode(Code code)
        {
            var statementList = StatementListSyntaxNode.ReadStatementsWithoutBlock(code);
            var trailingTrivia = TriviaNode.ReadList(code);

            return new DocumentSyntaxNode(statementList, trailingTrivia);
        }
    }
}
