using Lucy.Common;
using Lucy.Core.Parsing;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.Core.ProjectManagement;

public class Workspace
{
    private ImmutableDictionary<string, WorkspaceDocument> _documents = ImmutableDictionary<string, WorkspaceDocument>.Empty;
    private readonly Subscriptions<IWorkspaceEvent> _eventSubscriptions = new();

    public ImmutableDictionary<string, WorkspaceDocument> Documents => _documents;
    public IDisposable AddEventHandler(Action<IWorkspaceEvent> handler) => _eventSubscriptions.AddHandler(handler);

    public static async Task<Workspace> CreateFromPath(string path)
    {
        var files = Directory.GetFiles(path, "*.lucy", SearchOption.AllDirectories);
        var data = await Task.WhenAll(files.Select(async x => new
        {
            Path = x,
            Content = await File.ReadAllTextAsync(x)
        }));
        
        var ws = new Workspace();
        foreach (var document in data)
            ws.AddDocument(document.Path, document.Content);
        return ws;
    }

    public void AddDocument(string path, string content)
    {
        if (_documents.ContainsKey(path))
            throw new Exception($"A file named '{path}' already exists.");

        if (!path.EndsWith(".lucy"))
            throw new Exception($"Invalid document path: '{path}'");

        var document = new CodeWorkspaceDocument
        {
            Path = path,
            Content = content,
            LineBreakMap = LineBreakMap.CreateFrom(content),
            ParserResult = ParserResult.CreateFrom(path, content)
        };

        _documents = _documents.Add(path, document);
        _eventSubscriptions.Publish(new DocumentAdded(document));
    }

    public void UpdateFile(string path, string content)
    {
        if (!_documents.TryGetValue(path, out var oldDocument))
            throw new Exception($"A file named '{path}' does not exist.");

        if (oldDocument is CodeWorkspaceDocument)
        {
            var newDocument = new CodeWorkspaceDocument
            {
                Path = path,
                Content = content,
                LineBreakMap = LineBreakMap.CreateFrom(content),
                ParserResult = ParserResult.CreateFrom(path, content)
            };

            _documents = _documents.SetItem(path, newDocument);
            _eventSubscriptions.Publish(new DocumentChanged(oldDocument, newDocument));
        }
    }

    public void UpdateFile(string path, Range2D range, string content)
    {
        if (!_documents.TryGetValue(path, out var document))
            throw new Exception($"A file named '{path}' does not exist.");

        UpdateFile(path, document.LineBreakMap.To1D(range), content);
    }

    public void UpdateFile(string path, Range1D range, string content)
    {
        if (!_documents.TryGetValue(path, out var oldDocument))
            throw new Exception($"A file named '{path}' does not exist.");

        if (oldDocument is not CodeWorkspaceDocument codeDoc)
            throw new Exception($"Incremental update of '{path}' is not supported.");

        var updatedParseResult = codeDoc.ParserResult.Update(range, content);
        var updatesLineBreakMap = LineBreakMap.CreateFrom(updatedParseResult.Code);

        var newDocument = new CodeWorkspaceDocument
        {
            Path = path,
            Content = updatedParseResult.Code,
            ParserResult = updatedParseResult,
            LineBreakMap = updatesLineBreakMap
        };

        _documents = _documents.SetItem(path, newDocument);
        _eventSubscriptions.Publish(new DocumentChanged(oldDocument, newDocument));
    }

    public void RemoveFile(string path)
    {
        if (!_documents.TryGetValue(path, out var document))
            throw new Exception($"A file named '{path}' does not exist.");
        _documents = _documents.Remove(path);
        _eventSubscriptions.Publish(new DocumentRemoved(document));
    }

    public bool ContainsFile(string path) => _documents.ContainsKey(path);

    public WorkspaceDocument GetFile(string path)
    {
        if (!_documents.TryGetValue(path, out var document))
            throw new Exception($"This workspace does not contain a file with path: '{path}'");

        return document;
    }
}

public class WorkspaceDocument
{
    public required string Path { get; init; }
    public required string Content { get; init; }
    public required LineBreakMap LineBreakMap { get; init; }
}

public class CodeWorkspaceDocument : WorkspaceDocument
{
    public required ParserResult ParserResult { get; init; }
}

public interface IWorkspaceEvent { }
public record DocumentAdded(WorkspaceDocument Document) : IWorkspaceEvent;
public record DocumentChanged(WorkspaceDocument OldDocument, WorkspaceDocument NewDocument) : IWorkspaceEvent;
public record DocumentRemoved(WorkspaceDocument Document) : IWorkspaceEvent;
