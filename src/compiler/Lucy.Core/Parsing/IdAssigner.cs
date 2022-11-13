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
            var nodeNameCache = new Dictionary<Type, string>();
            Traverse(rootNode, new NodeIdFactory(documentPath), nodeNameCache);
        }

        private static void Traverse(SyntaxTreeNode node, NodeIdFactory nodeIdFactory, Dictionary<Type, string> nodeNameCache)
        {
            var type = node.GetType();
            if (!nodeNameCache.TryGetValue(type, out var nodeName)) 
            {
                var name = node.GetType().Name;
                if (name.EndsWith("SyntaxNode"))
                    name = name.Substring(0, name.Length - "SyntaxNode".Length);
                if (name.EndsWith("TriviaNode"))
                    name = name.Substring(0, name.Length - "TriviaNode".Length);
                if (name.EndsWith("Node"))
                    name = name.Substring(0, name.Length - "Node".Length);
                nodeName = name[0..1].ToLowerInvariant() + name[1..];
                nodeNameCache[type] = nodeName;
            }
            
            node.NodeId = nodeIdFactory.CreateId(nodeName);
            var subFactory = new NodeIdFactory(node.NodeId);
            foreach (var child in node.GetChildNodes())
            {
                Traverse(child, subFactory, nodeNameCache);
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
                _counter[name] = count + 1;
                return new NodeId(_documentPath, _id.Length == 0 ? $"{name}[{count}]" : $"{_id}.{name}[{count}]");
            }
        }
    }
}
