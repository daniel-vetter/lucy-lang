using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class RangeCalculation
    {
        [GenerateDbExtension] ///<see cref="GetNodeAtPositionEx.GetNodeAtPosition"/>
        public static SyntaxTreeNode? GetNodeAtPosition(IDb db, string documentPath, Position1D position)
        {
            var root = db.GetSyntaxTree(documentPath);
            var result = FindNodeFromPosition(db, position, root, 0);
            if (result == null)
                return null;
            return result.Node;
        }

        [GenerateDbExtension] ///<see cref="GetRangeFromNodeEx.GetRangeFromNode"/>
        public static Range1D? GetRangeFromNode(IDb db, SyntaxTreeNode node)
        {
            var root = db.GetSyntaxTree(node.NodeId.DocumentPath);
            return FindRangeFromNode(db, root, node, 0);
        }

        private static Range1D? FindRangeFromNode(IDb db, SyntaxTreeNode parentNode, SyntaxTreeNode target, int offset)
        {
            if (parentNode.NodeId.Equals(target.NodeId))
            {
                var start = offset;
                var end = start + db.GetNodeRangeLength(parentNode);
                return new Range1D(new Position1D(start), new Position1D(end));
            }

            foreach(var child in parentNode.GetChildNodes())
            {
                var result = FindRangeFromNode(db, child, target, offset);
                if (result != null)
                    return result;
                offset += db.GetNodeRangeLength(child);
            }

            return null;
        }

        private static SearchResult? FindNodeFromPosition(IDb db, Position1D position, SyntaxTreeNode startNode, int offset)
        {
            if (!startNode.GetChildNodes().Any())
            {
                var start = offset;
                var end = start + db.GetNodeRangeLength(startNode);
                return new SearchResult(startNode, new Range1D(new Position1D(start), new Position1D(end)));
            }
            
            foreach (var child in startNode.GetChildNodes())
            {
                var nodeLenght = db.GetNodeRangeLength(child);
                if (position.Position >= offset && position.Position < + offset + nodeLenght)
                {
                    return FindNodeFromPosition(db, position, child, offset);
                }
                offset += nodeLenght;
            }

            return null;
        }

        private record SearchResult(SyntaxTreeNode Node, Range1D Range);

        [GenerateDbExtension] ///<see cref="GetStatementListRangeLengthEx.GetStatementListRangeLength"/>
        public static int GetNodeRangeLength(IDb db, SyntaxTreeNode node)
        {
            return GetLengthOf(db, node);
        }

        private static int GetLengthOf(IDb db, SyntaxTreeNode node)
        {
            int length = 0;

            if (node is TokenNode token)
            {
                length += token.Text.Length;
            }

            foreach (var child in node.GetChildNodes())
            {
                if (child is StatementSyntaxNode statementNode)
                {
                    length += db.GetNodeRangeLength(statementNode);
                }
                else
                    length += GetLengthOf(db, child);
            }

            return length;
        }
    }
}
