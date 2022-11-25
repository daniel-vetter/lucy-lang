using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record Import(NodeId NodeId, string Path, ImportValidationResult ValidState);
    public enum ImportValidationResult
    {
        Ok,
        CouldNotResolve,
        InvalidPath
    }

    static public class GetImportsHandler
    {
        [GenerateDbExtension] ///<see cref="GetImportsEx.GetImports"/>
        public static ComparableReadOnlyList<Import> GetImports(IDb db, string documentPath)
        {
            var importStatementsIds = db.GetNodeIdsByType<ImportStatementSyntaxNode>(documentPath);
            var importStatements = importStatementsIds.Select(x => db.GetNodeById(x) as ImportStatementSyntaxNode).ToList();
            var documentList = db.GetDocumentList().ToHashSet();
            var currentDir = GetDirectoryFrom(documentPath);

            var result = new ComparableReadOnlyList<Import>.Builder();

            foreach (var importStatement in importStatements)
            {
                var path = NormalizePath(CombinePath(currentDir, importStatement.Path.Value)) + ".lucy";
                if (path == null)
                    result.Add(new Import(importStatement.NodeId, importStatement.Path.Value, ImportValidationResult.InvalidPath));
                else if (!documentList.Contains(path))
                    result.Add(new Import(importStatement.NodeId, path, ImportValidationResult.CouldNotResolve));
                else result.Add(new Import(importStatement.NodeId, path, ImportValidationResult.Ok));
            }

            return result.Build();
        }

        private static string GetDirectoryFrom(string path)
        {
            var index = path.LastIndexOf('/');
            if (index == 0)
                return "/";
            return path[..index];
        }

        private static string CombinePath(string basePath, string toAdd)
        {
            if (basePath.Length == 0)
                throw new Exception("Invalid base path");

            if (toAdd.Length > 0 && toAdd[0] == '/') 
                return toAdd;

            if (basePath[^1] == '/')
                return $"{basePath}{toAdd}";
            else
                return $"{basePath}/{toAdd}";
        }

        private static string? NormalizePath(string path)
        {
            if (path.Length == 0) throw new Exception("Invalid path");
            if (path[0] != '/') throw new Exception("Invalid path");

            var entries = path.Split('/').ToList();

            entries.RemoveAll(x => x == ".");

            for (int i=1;i<entries.Count;i++)
            {
                if (entries[i-1] != ".." && entries[i] == "..")
                {
                    entries.RemoveAt(i-1);
                    entries.RemoveAt(i-1);
                }
            }

            return string.Join("/", entries);
        }
    }
}
