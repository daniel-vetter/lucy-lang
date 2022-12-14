using Lucy.Core.Model;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Lucy.Common;

namespace Lucy.Core.Parsing;

internal static class IdAssigner
{
    internal static void Run(string documentPath, DocumentRootSyntaxNodeBuilder rootNode)
    {
        var nodeNameCache = new Dictionary<Type, byte[]>();
        rootNode.SetId(documentPath, Encoding.UTF8.GetBytes(documentPath));
        Traverse(documentPath, rootNode, Encoding.UTF8.GetBytes(documentPath), nodeNameCache);
    }

    private static void Traverse(string documentPath, SyntaxTreeNodeBuilder node, byte[] parentId, Dictionary<Type, byte[]> nodeIdCache)
    {
        Profiler.Start("IdAssign: " + node.NodeId);
        var dict = new Dictionary<byte[], int>();
        foreach (var childNode in node.GetChildNodes())
        {
            var type = childNode.GetType();
            if (!nodeIdCache.TryGetValue(type, out var nodeName))
            {
                Profiler.Start("AddCacheEntry");
                nodeName = Encoding.UTF8.GetBytes(type.Name);
                nodeIdCache[type] = nodeName;
                Profiler.End("AddCacheEntry");
            }

            dict.TryGetValue(nodeName, out var index);
            dict[nodeName] = ++index;
            var indexAsBytes = BitConverter.GetBytes(index);

            var nodeId = new byte[parentId.Length + nodeName.Length + indexAsBytes.Length + 1];
            parentId.CopyTo(nodeId, 0);
            indexAsBytes.CopyTo(nodeId, parentId.Length + 1);
            nodeName.CopyTo(nodeId, parentId.Length + indexAsBytes.Length + 1);
            childNode.SetId(documentPath, nodeId);
            Traverse(documentPath, childNode, nodeId, nodeIdCache);
        }

        Profiler.End("IdAssign: " + node.NodeId);
    }
}