using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucy.Common;
using Lucy.Core.Parsing;

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
        var documents = await Task.WhenAll(files.Select(async x =>
        {
            var documentPath = x.ToString()[path.Length..].Replace("\\", "/");
            var content = await File.ReadAllTextAsync(x);

            return WorkspaceDocument.Create(documentPath, content);
        }));
        
        var ws = new Workspace();
        foreach (var document in documents)
            ws.AddDocument(document);
        return ws;
    }

    public void AddDocument(WorkspaceDocument document)
    {
        if (_documents.ContainsKey(document.Path))
            throw new Exception($"A file named '{document.Path}' already exists.");

        _documents = _documents.Add(document.Path, document);
        _eventSubscriptions.Publish(new DocumentAdded(document));
    }

    public void UpdateFile(WorkspaceDocument document)
    {
        if (!_documents.TryGetValue(document.Path, out var oldDocument))
            throw new Exception($"A file named '{document.Path}' does not exist.");
        
        _documents = _documents.SetItem(document.Path, document);
        _eventSubscriptions.Publish(new DocumentChanged(oldDocument, document));
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

        var updatedParseResult = Parser.Update(codeDoc.ParserResult, range, content);
        var updatesLineBreakMap = LineBreakMap.CreateFrom(updatedParseResult.Reader.Code);

        var newDocument = new CodeWorkspaceDocument
        {
            Path = path,
            Content = updatedParseResult.Reader.Code,
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

public interface IWorkspaceEvent { }
public record DocumentAdded(WorkspaceDocument Document) : IWorkspaceEvent;
public record DocumentChanged(WorkspaceDocument OldDocument, WorkspaceDocument NewDocument) : IWorkspaceEvent;
public record DocumentRemoved(WorkspaceDocument Document) : IWorkspaceEvent;
