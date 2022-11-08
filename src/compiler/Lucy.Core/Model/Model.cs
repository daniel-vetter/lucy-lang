using System;
using System.Collections.Generic;

namespace Lucy.Core.Model
{
    public abstract class SyntaxTreeNode
    {
        public NodeId NodeId { get; set; } = NodeId.Uninitalized;
        public List<string> SyntaxErrors { get; set; } = new List<string>();

        public abstract IEnumerable<SyntaxTreeNode> GetChildNodes();
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
    }
}
