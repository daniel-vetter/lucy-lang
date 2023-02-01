using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using System;
using System.Linq;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler;

public record DocumentImports(
    ComparableReadOnlyList<Import> Valid, 
    ComparableReadOnlyList<Import> Invalid
);

public record Import(
    INodeId<ImportStatementSyntaxNode> ImportStatementNodeId,
    INodeId<TokenNode> ImportPathTokenNodeId,
    string Path
);

[QueryGroup]
public class Imports
{
    private readonly Nodes _nodes;

    public Imports(Nodes nodes)
    {
        _nodes = nodes;
    }

    /// <summary>
    /// Returns a list of all file imports of a specific document.
    /// </summary>
    /// <param name="documentPath">The document to analyze</param>
    public virtual DocumentImports GetImports(string documentPath)
    {
        var importStatementsIds = _nodes.GetNodeIdsByType<ImportStatementSyntaxNode>(documentPath);
        var importStatements = importStatementsIds.Select(_nodes.GetNodeById).ToList();
        var documentList = _nodes.GetDocumentList();
        var currentDir = GetDirectoryFrom(documentPath);

        var validList = new ComparableReadOnlyList<Import>.Builder();
        var invalidList = new ComparableReadOnlyList<Import>.Builder();

        foreach (var importStatement in importStatements)
        {
            var path = NormalizePath(CombinePath(currentDir, importStatement.Path.Value)) + ".lucy";

            if (documentList.Contains(path))
                validList.Add(new Import(importStatement.NodeId, importStatement.Path.Str.NodeId, path));
            else
                invalidList.Add(new Import(importStatement.NodeId, importStatement.Path.Str.NodeId, path));
        }

        return new DocumentImports(validList.Build(), invalidList.Build());
    }

    private static string GetDirectoryFrom(string path)
    {
        var index = path.LastIndexOf('/');
        return index == 0 ? "/" : path[..index];
    }

    private static string CombinePath(string basePath, string toAdd)
    {
        if (basePath.Length == 0)
            throw new Exception("Invalid base path");

        if (toAdd.Length > 0 && toAdd[0] == '/')
            return toAdd;

        return basePath[^1] == '/'
            ? $"{basePath}{toAdd}"
            : $"{basePath}/{toAdd}";
    }

    private static string NormalizePath(string path)
    {
        if (path.Length == 0) throw new Exception("Invalid path");
        if (path[0] != '/') throw new Exception("Invalid path");

        var entries = path.Split('/').ToList();

        entries.RemoveAll(x => x == ".");

        for (var i = 1; i < entries.Count; i++)
        {
            if (entries[i - 1] == ".." || entries[i] != "..")
                continue;

            entries.RemoveAt(i - 1);
            entries.RemoveAt(i - 1);
        }

        return string.Join("/", entries);
    }
}