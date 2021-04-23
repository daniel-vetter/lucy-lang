using System.Collections.Generic;
using Lucy.Core.Model;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Trivia;
using Lucy.Core.Parser.Nodes.Statements;
using Lucy.Core.ProjectManagement;

namespace Lucy.Core.Parser
{
    public class CodeParser
    {
        public static DocumentSyntaxNode Parse(TextDocument textDocument)
        {
            var code = new Code(textDocument.Content);
            var statementList = StatementListSyntaxNode.ReadStatementsWithoutBlock(code);

            var trailingTrivia = TriviaListNode.Read(code);
            if (!code.IsDone)
            {
                var tokenStr = code.Read().ToString();
                code.ReportError("Unexpected token '" + tokenStr + "'", code.Position - 1);
            }

            return new DocumentSyntaxNode(textDocument.Version, statementList, trailingTrivia, code.Issues);
        }
    }

    public class DocumentSyntaxNode : SyntaxNode
    {
        public DocumentSyntaxNode(int version, StatementListSyntaxNode statementList, TriviaListNode trailingTrivia, List<Issue> issues)
        {
            Version = version;
            StatementList = statementList;
            TrailingTrivia = trailingTrivia;
            Issues = issues;
        }

        public int Version { get; }
        public StatementListSyntaxNode StatementList { get; }
        public TriviaListNode TrailingTrivia { get; }
        public List<Issue> Issues { get; }
    }
}
