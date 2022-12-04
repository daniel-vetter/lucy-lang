using Lucy.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lucy.Core.Parsing;

internal static class IdAssigner
{
    internal static void Run(string documentPath, DocumentRootSyntaxNodeBuilder rootNode)
    {
        var nodeNameCache = new Dictionary<Type, string>();
        rootNode.SetId(documentPath, "root");
        Traverse(documentPath, rootNode, new StringBuilder("root"), nodeNameCache);
    }

    private static void Traverse(string documentPath, SyntaxTreeNodeBuilder node, StringBuilder sb,
        Dictionary<Type, string> nodeNameCache)
    {
        sb.Append('.');
        var start = sb.Length;
        var dict = new Dictionary<string, int>();
        foreach (var childNode in node.GetChildNodes())
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
                nodeName = name[..1].ToLowerInvariant() + name[1..];
                nodeNameCache[type] = nodeName;
            }

            sb.Length = start;
            sb.Append(nodeName);
            sb.Append('[');
            dict.TryGetValue(nodeName, out var index);
            sb.Append(index);
            dict[nodeName] = index + 1;
            sb.Append(']');
            childNode.SetId(documentPath, sb.ToString());
            Traverse(documentPath, childNode, sb, nodeNameCache);
        }
    }
}