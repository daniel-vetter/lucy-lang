using Lucy.Core.Model;
using System;
using System.Collections.Generic;

namespace Lucy.Core.Parsing;

internal static class IdAssigner
{
    internal static void Run(string documentPath, DocumentRootSyntaxNodeBuilder rootNode)
    {
        var nodeNameCache = new Dictionary<Type, string>();
        Traverse(rootNode, new NodeIdFactory(documentPath), nodeNameCache);
    }

    private static void Traverse(SyntaxTreeNodeBuilder node, NodeIdFactory nodeIdFactory, Dictionary<Type, string> nodeNameCache)
    {
        var type = node.GetType();
        if (!nodeNameCache.TryGetValue(type, out var nodeName)) 
        {
            var name = node.GetType().Name;
            if (name.EndsWith("SyntaxNodeBuilder"))
                name = name[..^"SyntaxNodeBuilder".Length];
            if (name.EndsWith("NodeBuilder"))
                name = name[..^"NodeBuilder".Length];
            if (name.EndsWith("Builder"))
                name = name[..^"Builder".Length];
            if (name.EndsWith("TriviaNode"))
                name = name[..^"TriviaNode".Length];
            if (name.EndsWith("Node"))
                name = name[..^"Node".Length];
            nodeName = name[0..1].ToLowerInvariant() + name[1..];
            nodeNameCache[type] = nodeName;
        }

        var nodeId = nodeIdFactory.CreateId(nodeName);
        node.SetId(nodeId.DocumentPath, nodeId.NodePath);
        var subFactory = new NodeIdFactory(node.NodeId!);
        foreach (var child in node.GetChildNodes())
        {
            Traverse(child, subFactory, nodeNameCache);
        }
    }

    private class NodeIdFactory
    {
        private readonly string _id = "";
        private readonly string _documentPath;
        private readonly Dictionary<string, int> _counter = new();

        public NodeIdFactory(string documentPath)
        {
            _documentPath = documentPath;
        }

        public NodeIdFactory(IBuilderNodeId<SyntaxTreeNodeBuilder> parent)
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