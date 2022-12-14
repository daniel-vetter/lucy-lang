using System.Text;

namespace Lucy.Core.Model;

// ReSharper disable once UnusedTypeParameter
public interface INodeId<out T> where T : SyntaxTreeNode
{
    public string DocumentPath { get; }
    public byte[] NodePath { get; }
}

// ReSharper disable once UnusedTypeParameter
public interface IBuilderNodeId<out T>  where T : SyntaxTreeNodeBuilder
{
    public string DocumentPath { get; }
    public byte[] NodePath { get; }
}

public sealed class NodeId<TNode, TBuilder> : NodeId, INodeId<TNode>, IBuilderNodeId<TBuilder> where TNode: SyntaxTreeNode where TBuilder : SyntaxTreeNodeBuilder
{
    public NodeId(string documentPath, byte[] nodePath) : base(documentPath, nodePath)
    {
    }
}

public class NodeId : IHashable
{
    protected NodeId(string documentPath, byte[] nodePath)
    {
        if (documentPath.Length == 0 || (documentPath[0] != '/' && documentPath[0] != '!'))
            throw new ArgumentException("Invalid document path");

        DocumentPath = documentPath;
        NodePath = nodePath;
        _hash = nodePath;

        var hc = new HashCode();
        hc.AddBytes(_hash);
        _hashCode = hc.ToHashCode();
    }
    
    public string DocumentPath { get; }
    public byte[] NodePath { get; }
    
    private readonly int _hashCode;
    private readonly byte[] _hash;

    public byte[] GetFullHash() => _hash;

    public override bool Equals(object? obj)
    {
        return obj is NodeId id && NodePath.SequenceEqual(id.NodePath);
    }

    public override int GetHashCode()
    {
        return _hashCode;
    }

    public override string ToString()
    {
        var parts = new List<string>();
        var span = NodePath.AsSpan();
        var index = span.IndexOf((byte) 0);
        var block = index == -1 ? span[0..] : span[0..(index)];
        parts.Add(Encoding.UTF8.GetString(block));
        span = span[(index+1)..];
        
        while (true)
        {
            var num = BitConverter.ToInt32(span[..4]);
            span= span[4..];
            index = span.IndexOf((byte)0);
            var name = Encoding.UTF8.GetString(index == -1 ? span : span[..index]);
            parts.Add(name + "[" + num + "]");
            if (index == -1)
                break;

            span = span[(index+1)..];
        }
        
        return string.Join(".", parts);
    }

    public static bool operator ==(NodeId? id1, NodeId? id2)
    {
        if (ReferenceEquals(id1, id2)) return true;
        if (ReferenceEquals(id1, null)) return false;
        if (ReferenceEquals(id2, null)) return false;
        return id1.Equals(id2);
    }

    public static bool operator !=(NodeId? id1, NodeId? id2)
    {
        return !(id1 == id2);
    }
}