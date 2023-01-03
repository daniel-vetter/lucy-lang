using System;
using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs;

public record GetNodesByNodeIdMap(string DocumentPath);

public static class GetNodesByNodeIdMapEx
{
    public static ImmutableDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> GetNodesByNodeIdMap(this IDb db, string documentPath) => (ImmutableDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode>)db.Query(new GetNodesByNodeIdMap(documentPath));
}

public record GetNodeIdsByTypeMap(string DocumentPath);

public static class GetNodeIdsByTypeMapEx
{
    public static ImmutableDictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>> GetNodeIdsByTypeMap(this IDb db, string documentPath) => (ImmutableDictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>>)db.Query(new GetNodeIdsByTypeMap(documentPath));
}

public record GetParentNodeIdByNodeIdMap(string DocumentPath);

public static class GetParentNodeIdByNodeIdMapEx
{
    public static ImmutableDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?> GetParentNodeIdByNodeIdMap(this IDb db, string documentPath) => (ImmutableDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?>)db.Query(new GetParentNodeIdByNodeIdMap(documentPath));
}
