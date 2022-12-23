using System.Runtime.CompilerServices;

namespace Lucy.Core.Model;

// ReSharper disable once UnusedTypeParameter
public interface INodeId<out T> where T : SyntaxTreeNode
{
    public string DocumentPath { get; }
}

public static class NodeEx
{
    public static bool IsMissing<T>(this INodeId<T>? nodeId) where T : SyntaxTreeNode
    {
        return nodeId == null;
    }
}

public sealed class NodeId<TNode> : NodeId, INodeId<TNode> where TNode : SyntaxTreeNode
{
    public NodeId(string documentPath) : base(documentPath)
    {
    }
}

public class NodeId
{
    private static int _lastId;
    private static readonly ConditionalWeakTable<NodeId, IdSlot> _idStorage = new();
    private class IdSlot { public int Value { get; set; } }

    protected NodeId(string documentPath)
    {
        if (documentPath.Length == 0 || (documentPath[0] != '/' && documentPath[0] != '!'))
            throw new ArgumentException("Invalid document path");

        DocumentPath = documentPath;
    }

    public string DocumentPath { get; }

    public override string ToString()
    {
        var slot = _idStorage.GetOrCreateValue(this);
        if (slot.Value == 0)
            slot.Value = Interlocked.Increment(ref _lastId);
        return DocumentPath + "!" + slot.Value;
    }
}