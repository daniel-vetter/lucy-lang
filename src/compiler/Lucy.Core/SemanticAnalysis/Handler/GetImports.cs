using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public record DocumentImports(ComparableReadOnlyList<Import> Valid, ComparableReadOnlyList<Import> Invalid);
public record Import(INodeId<SyntaxTreeNode> ImportStatementNodeId, INodeId<TokenNode> ImportPathTokenNodeId, string Path);

public static class GetImportsHandler
{
    [DbQuery] ///<see cref="GetImportsEx.GetImports"/>
    public static DocumentImports GetImports(IDb db, string documentPath)
    {
        var importStatementsIds = db.GetNodeIdsByType<ImportStatementSyntaxNode>(documentPath);
        var importStatements = importStatementsIds.Select(db.GetNodeById).ToList();
        var documentList = db.GetDocumentList().ToHashSet();
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

        for (var i=1;i<entries.Count;i++)
        {
            if (entries[i - 1] == ".." || entries[i] != "..") 
                continue;

            entries.RemoveAt(i-1);
            entries.RemoveAt(i-1);
        }

        return string.Join("/", entries);
    }
}