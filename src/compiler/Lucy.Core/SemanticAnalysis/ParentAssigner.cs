using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;

namespace Lucy.Core.SemanticAnalysis
{
    internal class ParentAssigner
    {
        internal static void Run(SyntaxTreeNode node)
        {
            foreach(var child in node.GetChildNodes())
            {
                child.SetAnnotation(new ParentNodeAnnotation(node));
                Run(child);
            }
        }
    }

    public static class ParentNodeExtension
    {
        public static SyntaxTreeNode? GetParent(this SyntaxTreeNode node) => node.GetAnnotation<ParentNodeAnnotation>()?.Node;
    }

    internal class ParentNodeAnnotation
    {
        public ParentNodeAnnotation(SyntaxTreeNode node) => Node = node;
        public SyntaxTreeNode Node { get; set; }
    }
}
