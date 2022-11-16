using Lucy.Core.Model;

namespace Lucy.Core.ProjectManagement
{
    public class CodeFile : WorkspaceDocument
    {
        public CodeFile(string path, string content, DocumentRootSyntaxNodeBuilder syntaxTree) : base(path, content)
        {
            SyntaxTree = syntaxTree;
        }

        public DocumentRootSyntaxNodeBuilder SyntaxTree { get; }
    }
}
