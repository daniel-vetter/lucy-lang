using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;

namespace Lucy.Core.SemanticAnalysis
{
    internal class ParentAssigner
    {
        internal static void Run(SyntaxNode node)
        {
            foreach(var child in node.GetChildNodes())
            {
                child.Node.SetAnnotation(new ParentNodeAnnotation(node));
                Run(child.Node);
            }
        }
    }

    public static class ParentNodeExtension
    {
        public static SyntaxNode? GetParent(this SyntaxNode node) => node.GetAnnotation<ParentNodeAnnotation>()?.Node;
    }

    internal class ParentNodeAnnotation
    {
        public ParentNodeAnnotation(SyntaxNode node) => Node = node;
        public SyntaxNode Node { get; set; }
    }
}
