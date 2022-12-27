using System;
using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs;

public record GetNodesByNodeIdMap(string DocumentPath) : IQuery<GetNodesByNodeIdMapResult>;
public record GetNodesByNodeIdMapResult(ImmutableDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> Map);

public static class GetNodesByNodeIdMapEx
{
    public static ImmutableDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> GetNodesByNodeIdMap(this IDb db, string documentPath) => db.Query(new GetNodesByNodeIdMap(documentPath)).Map;
}

public record GetNodeIdsByTypeMap(string DocumentPath) : IQuery<GetNodeIdsByTypeMapResult>;
public record GetNodeIdsByTypeMapResult(ImmutableDictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>> Map);

public static class GetNodeIdsByTypeMapEx
{
    public static ImmutableDictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>> GetNodeIdsByTypeMap(this IDb db, string documentPath) => db.Query(new GetNodeIdsByTypeMap(documentPath)).Map;
}

public record GetParentNodeIdByNodeIdMap(string DocumentPath) : IQuery<GetParentNodeIdByNodeIdMapResult>;
public record GetParentNodeIdByNodeIdMapResult(ImmutableDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?> Map);

public static class GetParentNodeIdByNodeIdMapEx
{
    public static ImmutableDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?> GetParentNodeIdByNodeIdMap(this IDb db, string documentPath) => db.Query(new GetParentNodeIdByNodeIdMap(documentPath)).Map;
}
