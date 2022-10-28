using Lucy.Core.Parsing.Nodes;

namespace Lucy.Core.ProjectManagement
{
    public class CodeFile : WorkspaceDocument
    {
        public CodeFile(string path, string content, DocumentRootSyntaxNode syntaxTree) : base(path, content)
        {
            SyntaxTree = syntaxTree;
        }

        public DocumentRootSyntaxNode SyntaxTree { get; }
    }
}
