using Lucy.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lucy.Core.Parsing
{
    

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
            if (name.EndsWith("TriviaNode"))
                name = name.Substring(0, name.Length - "TriviaNode".Length);
            if (name.EndsWith("Node"))
                name = name.Substring(0, name.Length - "Node".Length);
            name = name[0..1].ToLowerInvariant() + name[1..];

            node.NodeId = nodeIdFactory.CreateId(name);
            var subFactory = new NodeIdFactory(node.NodeId);
            foreach (var child in node.GetChildNodes())
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
