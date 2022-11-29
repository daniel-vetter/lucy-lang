using System.Text;

namespace Lucy.Core.Model;

// ReSharper disable once UnusedTypeParameter
public interface INodeId<out T> where T : SyntaxTreeNode
{
    public string DocumentPath { get; }
    public string NodePath { get; }

    public bool IsRoot { get; }
    public INodeId<SyntaxTreeNode> Parent { get; }
}

// ReSharper disable once UnusedTypeParameter
public interface IBuilderNodeId<out T>  where T : SyntaxTreeNodeBuilder
{
    public string DocumentPath { get; }
    public string NodePath { get; }

    public bool IsRoot { get; }
    public INodeId<SyntaxTreeNode> Parent { get; }
}

public class NodeId<T> : NodeId, INodeId<T> where T: SyntaxTreeNode
{
    public NodeId(string documentPath, string nodePath) : base(documentPath, nodePath)
    {
    }
}

public class BuilderNodeId<T> : NodeId, IBuilderNodeId<T> where T : SyntaxTreeNodeBuilder
{
    public BuilderNodeId(string documentPath, string nodePath) : base(documentPath, nodePath)
    {
    }
}

public class NodeId : IHashable
{
    public NodeId(string documentPath, string nodePath)
    {
        if (documentPath.Length == 0 || (documentPath[0] != '/' && documentPath[0] != '!'))
            throw new ArgumentException("Invalid document path");

        DocumentPath = documentPath;
        NodePath = nodePath;
        _str = DocumentPath + "!" + NodePath;
        _hash = Encoding.UTF8.GetBytes(_str);
    }
    
    public string DocumentPath { get; }
    public string NodePath { get; }

    private readonly string _str;
    private readonly byte[] _hash;

    public byte[] GetFullHash() => _hash;

    public override bool Equals(object? obj)
    {
        return obj is NodeId id && DocumentPath == id.DocumentPath && NodePath == id.NodePath;
    }

    public override int GetHashCode()
    {
        return _str.GetHashCode();
    }

    public override string ToString() => _str;

    public bool IsRoot => NodePath.IndexOf('.') == -1;
    public INodeId<SyntaxTreeNode> Parent
    {
        get
        {
            var lastIndex = NodePath.LastIndexOf('.');
            if (lastIndex == -1)
                throw new Exception("Current node is already the root node id.");
            return new NodeId<SyntaxTreeNode>(DocumentPath, NodePath[..lastIndex]);
        }
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