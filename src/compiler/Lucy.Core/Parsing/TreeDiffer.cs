using System.Collections.Immutable;
using System.Linq;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing;

public static class TreeDiffer
{
    public static ImmutableArray<TreeDiff> Diff(DocumentRootSyntaxNode leftTree, DocumentRootSyntaxNode rightTree)
    {
        var result = ImmutableArray.CreateBuilder<TreeDiff>();

        void Traverse(SyntaxTreeNode left, SyntaxTreeNode right)
        {
            var leftChildren = left.GetChildNodes().ToDictionary(x => x.NodeId, x => x);
            var rightChildren = right.GetChildNodes().ToDictionary(x => x.NodeId, x => x);

            foreach (var removedId in leftChildren.Keys.Where(x => !rightChildren.ContainsKey(x)))
                result.Add(new NodeDetached(right, leftChildren[removedId]));

            foreach (var rightNode in rightChildren.Values)
            {
                if (!leftChildren.TryGetValue(rightNode.NodeId, out var leftNode))
                {
                    result.Add(new NodeAttached(right, rightChildren[rightNode.NodeId]));
                    continue;
                }

                if (ReferenceEquals(leftNode, rightNode))
                    continue;

                result.Add(new NodeReplaced(right, leftNode, rightNode));
                Traverse(leftNode, rightNode);
            }
        }

        if (leftTree.NodeId != rightTree.NodeId)
        {
            result.Add(new NodeDetached(null, leftTree));
            result.Add(new NodeAttached(null, rightTree));
        }
        else if (!ReferenceEquals(leftTree, rightTree))
            result.Add(new NodeReplaced(null, leftTree, rightTree));

        Traverse(leftTree, rightTree);

        return result.ToImmutable();
    }
}

public record TreeDiff;
public record NodeAttached(SyntaxTreeNode? Parent, SyntaxTreeNode Node) : TreeDiff;
public record NodeDetached(SyntaxTreeNode? Parent, SyntaxTreeNode Node) : TreeDiff;
public record NodeReplaced(SyntaxTreeNode? Parent, SyntaxTreeNode OldNode, SyntaxTreeNode NewNode) : TreeDiff;