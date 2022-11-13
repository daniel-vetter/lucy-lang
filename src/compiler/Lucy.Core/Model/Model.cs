using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucy.Core.Model
{
    /*
    public abstract class SyntaxTreeNode
    {
        public NodeId NodeId { get; set; } = NodeId.Uninitalized;
        public List<string> SyntaxErrors { get; set; } = new List<string>();

        public abstract IEnumerable<SyntaxTreeNode> GetChildNodes();
    }

    public abstract record FlatSyntaxTreeNode
    {
        public NodeId NodeId { get; init; } = NodeId.Uninitalized;
        public ImmutableArray<string> SyntaxErrors { get; init; } = ImmutableArray<string>.Empty;

        public abstract IEnumerable<NodeId> GetChildNodeIds();
    }

    public abstract class ImmutableSyntaxTreeNode
    {
        public ImmutableSyntaxTreeNode(NodeId nodeId, ImmutableArray<string> syntaxErrors)
        {
            NodeId = nodeId;
            SyntaxErrors = syntaxErrors;
        }

        public NodeId NodeId { get; }
        public ImmutableArray<string> SyntaxErrors { get; }
    }
    */


    public class NodeId<TNodeType> : NodeId
    {
        public NodeId(string documentPath, string nodePath) : base(documentPath, nodePath)
        {
        }
    }

    public class NodeId
    {
        public NodeId(string documentPath, string nodePath)
        {
            DocumentPath = documentPath;
            NodePath = nodePath;
        }

        private static NodeId _unitialized = new NodeId("!", "Uninitalized");
        public static NodeId Uninitalized => _unitialized;

        public string DocumentPath { get; }
        public string NodePath { get; }

        public string GetFullHash() => ToString();

        public override bool Equals(object? obj)
        {
            return obj is NodeId id && DocumentPath == id.DocumentPath && NodePath == id.NodePath;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DocumentPath, NodePath);
        }

        public override string ToString() => $"{DocumentPath}!{NodePath}";

        public NodeId<T> ToTyped<T>() => new NodeId<T>(DocumentPath, NodePath);
    }
}
