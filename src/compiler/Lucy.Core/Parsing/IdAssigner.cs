using Lucy.Core.Model;
using System.Collections.Immutable;
using System.Linq;

namespace Lucy.Core.Parsing;

internal static class IdAssigner
{
    /// <summary>
    /// Gives every node a new id. If a node already has an id, this will fail
    /// </summary>
    internal static void AssignNewIds(string documentPath, SyntaxTreeNode node)
    {
        node.AssignNewNodeId(documentPath);
        foreach (var child in node.GetChildNodes())
            AssignNewIds(documentPath, child);
    }

    /// <summary>
    /// Assigns every node in <paramref name="newTree"/> a id from the old tree. Nodes that already have a id will be skipped.
    /// Tries to find the correlating nodes from the old tree so the ids stay stable. 
    /// </summary>
    internal static void ReassignIdsFromPreviousTreeOrCreateNewOnes(DocumentRootSyntaxNode oldTree, DocumentRootSyntaxNode newTree, ImmutableArray<SyntaxTreeNode> candidates)
    {
        // Assumption: The oldTree is a full processed tree. So all nodes have a node id
        // Assumption: The newTree is new tree where some ids are missing. We only traverse down nodes that have a missing id
        //             If a node already has an id, it was reused from the parser cache and therefore all its child nodes will have ids

        // Since there can only be one root node, we can just move to id to the new tree. 
        if (newTree.NodeId.IsMissing())
            newTree.AssignExistingNodeId(oldTree.NodeId);

        var documentPath = oldTree.NodeId.DocumentPath;

        // To prevent assigning a id more than once, we keep track of all node ids
        // where we know they are not part of the new tree.
        var remainingCandidates = candidates.ToHashSet();
        
        Traverse(oldTree, newTree);

        void Traverse(SyntaxTreeNode leftNode, SyntaxTreeNode rightNode)
        {
            foreach(var nodeWithoutId in rightNode.GetChildNodes().Where(x => x.NodeId.IsMissing()))
            {
                // Find all node which ids could be reused
                var possibleCandidates = leftNode.GetChildNodes()
                    .Where(remainingCandidates.Contains)
                    .Where(x => x.GetType() == nodeWithoutId.GetType())
                    .ToImmutableArray();

                // Find the best one
                var match = FindMatch(nodeWithoutId, possibleCandidates);

                if (match != null)
                {
                    // We found a match, so we remove them from the candidate list and assign the id to the new node
                    // Since this was a new node, 
                    remainingCandidates.Remove(match);
                    nodeWithoutId.AssignExistingNodeId(match.NodeId);
                    Traverse(match, nodeWithoutId);
                }
                else
                {
                    // We could not find a good node as id source, so we just create a new id
                    // Since we can no longer match the new node to an old node, all further lookups will fail.
                    // Therefore we can also just assign new node ids to all child nodes. AssignNewIds will do that recursively.
                    nodeWithoutId.AssignNewNodeId(documentPath);
                    AssignNewIds(documentPath, nodeWithoutId);
                }
            }
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private static SyntaxTreeNode? FindMatch(SyntaxTreeNode nodeWithoutId, ImmutableArray<SyntaxTreeNode> possibleCandidates)
    {
        // Maybe find some better match algorithm. But its good enough for now.
        // Still, needs to be tested how often the wrong node is picked.
        return possibleCandidates.FirstOrDefault();
    }
}