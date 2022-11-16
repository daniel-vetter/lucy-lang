using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class GetNodesByTypesHandler
    {
        public static ComparableReadOnlyList<T> GetNodesByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
        {
            return db.GetNodesByType(documentPath, typeof(T)).Cast<T>().ToComparableReadOnlyList();
        }

        [GenerateDbExtension] ///<see cref="GetNodesByTypeEx.GetNodexByType"/>
        public static ComparableReadOnlyList<SyntaxTreeNode> GetNodesByType(IDb db, string documentPath, Type type)
        {
            var nodesByType = db.GetNodeMap(documentPath).NodesByType;
            if (nodesByType.TryGetValue(type, out var nodes))
                return nodes;
            return new ComparableReadOnlyList<SyntaxTreeNode>();
        }
    }
}
