using System;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace Lucy.Core.Model
{
    public class NodeId<TNodeType> : NodeId
    {
        public NodeId(string documentPath, string nodePath) : base(documentPath, nodePath)
        {
        }
    }

    public class NodeId : IHashable
    {
        public NodeId(string documentPath, string nodePath)
        {
            DocumentPath = documentPath;
            NodePath = nodePath;
            _str = DocumentPath + "!" + NodePath;
        }

        private static NodeId _unitialized = new NodeId("!", "Uninitalized");
        public static NodeId Uninitalized => _unitialized;

        public string DocumentPath { get; }
        public string NodePath { get; }

        private string _str;

        public string GetFullHash() => _str;

        public override bool Equals(object? obj)
        {
            return obj is NodeId id && DocumentPath == id.DocumentPath && NodePath == id.NodePath;
        }

        public override int GetHashCode()
        {
            return _str.GetHashCode();
        }

        public override string ToString() => _str;

        public NodeId<T> ToTyped<T>() => new NodeId<T>(DocumentPath, NodePath);
    }
}
