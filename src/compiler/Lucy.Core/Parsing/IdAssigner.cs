using Lucy.Core.Helper;
using Lucy.Core.Parsing.Nodes;
using System;
using System.Collections.Generic;

namespace Lucy.Core.Parsing
{
    public class NodeId
    {
        private NodeId()
        {
            DocumentPath = "";
            NodePath = "";
        }

        public NodeId(string documentPath, string nodePath)
        {
            DocumentPath = documentPath;
            NodePath = nodePath;
        }

        private static NodeId _unitialized = new NodeId("!", "Uninitalized");
        public static NodeId Uninitalized => _unitialized;

        public string DocumentPath { get; }
        public string NodePath { get; }

        public override bool Equals(object? obj)
        {
            if (DocumentPath == "" && NodePath == "")
                throw new Exception("Can not compare unitialized node id.");

            return obj is NodeId id && DocumentPath == id.DocumentPath && NodePath == id.NodePath;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DocumentPath, NodePath);
        }

        public override string ToString() => $"{DocumentPath}!{NodePath}";
    }

    internal class IdAssigner
    {
        internal static void Run(string documentPath, DocumentRootSyntaxNode rootNode)
        {
            Traverse(rootNode, new NodeIdFactory(documentPath));
        }

        private static void Traverse(SyntaxTreeNode node, NodeIdFactory nodeIdFactory)
        {
            var name = node.GetType().Name;
            if (name.EndsWith("SyntaxNode"))
                name = name.Substring(0, name.Length - "SyntaxNode".Length);
            node.NodeId = nodeIdFactory.CreateId(name);
            var subFactory = new NodeIdFactory(node.NodeId);
            foreach(var child in node.GetChildNodes())
            {
                Traverse(child, subFactory);
            }
        }

        private class NodeIdFactory
        {
            private readonly string _id = "";
            private readonly string _documentPath;
            private Dictionary<string, int> _counter = new();

            public NodeIdFactory(string documentPath)
            {
                _documentPath = documentPath;
            }

            public NodeIdFactory(NodeId parent)
            {
                _id = parent.NodePath;
                _documentPath = parent.DocumentPath;
            }

            public NodeId CreateId(string name)
            {
                _counter.TryGetValue(name, out var count);
                var nameWithCounter = $"{name}[{count}]";
                _counter[name] = count + 1;
                return new NodeId(_documentPath, _id.Length == 0 ? nameWithCounter : $"{_id}.{nameWithCounter}");
            }
        }
    }
}
