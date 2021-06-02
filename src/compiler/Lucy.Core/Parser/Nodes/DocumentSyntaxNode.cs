﻿using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Statements;
using Lucy.Core.Parser.Nodes.Trivia;
using System.Collections.Generic;

namespace Lucy.Core.Parser.Nodes
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
