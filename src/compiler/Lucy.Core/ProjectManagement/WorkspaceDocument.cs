using System;
using Lucy.Core.Parsing;

namespace Lucy.Core.ProjectManagement;

public class WorkspaceDocument
{
    public static WorkspaceDocument Create(string documentPath, string content)
    {
        if (!documentPath.EndsWith(".lucy"))
            throw new Exception($"Invalid document path: '{documentPath}'");

        var document = new CodeWorkspaceDocument
        {
            Path = documentPath,
            Content = content,
            LineBreakMap = LineBreakMap.CreateFrom(content),
            ParserResult = Parser.Parse(documentPath, content)
        };

        return document;
    }

    public required string Path { get; init; }
    public required string Content { get; init; }
    public required LineBreakMap LineBreakMap { get; init; }
}

public class CodeWorkspaceDocument : WorkspaceDocument
{
    public required ParserResult ParserResult { get; init; }
}