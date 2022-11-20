using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{   
    public static class GetNodeMapHandler
    {
        [GenerateDbExtension] ///<see cref="GetNodeByIdMapEx.GetNodeByIdMap"/>
        public static ComparableReadOnlyDictionary<NodeId, SyntaxTreeNode> GetNodeByIdMap(IDb db, string documentPath)
        {
            return db.GetNodeList(db.GetSyntaxTree(documentPath)).ToComparableReadOnlyDictionary(x => x.NodeId, x => x);
        }

        [GenerateDbExtension] ///<see cref="GetNodeByIdEx.GetNodeById"/>
        public static SyntaxTreeNode GetNodeById(IDb db, NodeId nodeId)
        {
            return db.GetNodeByIdMap(nodeId.DocumentPath)[nodeId];
        }

        [GenerateDbExtension] ///<see cref="GetNodesByTypeMapEx.GetNodesByTypeMap"/>
        public static ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<SyntaxTreeNode>> GetNodesByTypeMap(IDb db, string documentPath)
        {
            return db.GetNodeList(db.GetSyntaxTree(documentPath))
                .GroupBy(x => x.GetType())
                .ToComparableReadOnlyDictionary(x => x.Key, x => x.ToComparableReadOnlyList());
        }

        [GenerateDbExtension] ///<see cref="GetNodesByTypeEx.GetNodesByType"/>
        public static ComparableReadOnlyList<SyntaxTreeNode> GetNodesByType(IDb db, string documentPath, Type type)
        {
            if (db.GetNodesByTypeMap(documentPath).TryGetValue(type, out var list))
                return list;
            return new ComparableReadOnlyList<SyntaxTreeNode>();
        }

        public static ComparableReadOnlyList<T> GetNodesByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
        {
            return db.GetNodesByType(documentPath, typeof(T)).Cast<T>().ToComparableReadOnlyList();
        }

        [GenerateDbExtension] ///<see cref="GetNodeListEx.GetNodeList"/>
        public static ComparableReadOnlyList<SyntaxTreeNode> GetNodeList(IDb db, SyntaxTreeNode node)
        {
            static void Traverse(IDb db, SyntaxTreeNode node, ComparableReadOnlyList<SyntaxTreeNode>.Builder nodes)
            {
                nodes.Add(node);

                if (node is StatementListSyntaxNode statementList)
                {
                    foreach (var child in node.GetChildNodes())
                        nodes.AddRange(db.GetNodeList(child));
                }
                else
                {
                    foreach (var child in node.GetChildNodes())
                        Traverse(db, child, nodes);
                }
            }

            var list = new ComparableReadOnlyList<SyntaxTreeNode>.Builder();
            Traverse(db, node, list);
            return list.Build();
        }
    }
}
