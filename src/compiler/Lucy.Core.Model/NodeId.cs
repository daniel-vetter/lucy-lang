﻿using System.Text;

namespace Lucy.Core.Model;

// ReSharper disable once UnusedTypeParameter
public interface INodeId<out T> where T : SyntaxTreeNode
{
    public string DocumentPath { get; }
    public string NodePath { get; }
}

// ReSharper disable once UnusedTypeParameter
public interface IBuilderNodeId<out T>  where T : SyntaxTreeNodeBuilder
{
    public string DocumentPath { get; }
    public string NodePath { get; }
}

public sealed class NodeId<TNode, TBuilder> : NodeId, INodeId<TNode>, IBuilderNodeId<TBuilder> where TNode: SyntaxTreeNode where TBuilder : SyntaxTreeNodeBuilder
{
    public NodeId(string documentPath, string nodePath) : base(documentPath, nodePath)
    {
    }
}

public class NodeId : IHashable
{
    protected NodeId(string documentPath, string nodePath)
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