using System.Runtime.CompilerServices;

namespace Lucy.Core.Model;

// ReSharper disable once UnusedTypeParameter
public interface INodeId<out T> where T : SyntaxTreeNode
{
    public string DocumentPath { get; }
}

public sealed class NodeId<TNode> : NodeId, INodeId<TNode> where TNode : SyntaxTreeNode
{
    public NodeId(string documentPath) : base(documentPath)
    {
    }
}

public static class NodeEx
{
    public static bool IsMissing<T>(this INodeId<T>? nodeId) where T: SyntaxTreeNode
    {
        return nodeId == null;
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
        slot.Value = Interlocked.Increment(ref _lastId);
        return DocumentPath + "!" + slot.Value;
    }
}

public readonly struct NodeId2<T> where T : SyntaxTreeNode
{
    public int Value { get; }

    public NodeId2(int value)
    {
        Value = value;
    }



    public static implicit operator NodeId2<T>(T node)
    {
        return new NodeId2<T>(0);
    }
}