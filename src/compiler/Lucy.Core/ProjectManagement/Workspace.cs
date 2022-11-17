using Lucy.Common;
using Lucy.Core.Parsing;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.Core.ProjectManagement
{

    public class Workspace
    {
        private ImmutableDictionary<string, WorkspaceDocument> _documents = ImmutableDictionary<string, WorkspaceDocument>.Empty;
        private Subscriptions<IWorkspaceEvent> _eventSubscriptions = new();
        public ImmutableDictionary<string, WorkspaceDocument> Documents => _documents;
        public IDisposable AddEventHandler(Action<IWorkspaceEvent> handler) => _eventSubscriptions.AddHandler(handler);

        public static async Task<Workspace> CreateFromPath(string path)
        {
            var files = Directory.GetFiles(path, "*.lucy", SearchOption.AllDirectories);
            var tasks = files.Select(async x => new
            {
                Path = x,
                Content = await File.ReadAllTextAsync(x)
            });

            var ws = new Workspace();
            foreach (var file in await Task.WhenAll(tasks))
            {
                var subPath = file.Path.Substring(path.Length).Replace("\\", "/");
                ws.AddFile(subPath, file.Content);
            }
            return ws;
        }

        public void AddFile(string path, string content)
        {
            if (_documents.ContainsKey(path))
                throw new Exception("A file named '" + path + "' already exists.");

            if (path.EndsWith(".lucy"))
            {
                var document = new CodeFile(path, content, Parser.Parse(path, content));
                _documents = _documents.Add(document.Path, document);
                _eventSubscriptions.Publish(new DocumentAdded(document));
            }
            else
                throw new NotSupportedException("Could not determin type of workspace file: " + path);
        }

        public void UpdateFile(string path, string content)
        {
            if (!_documents.TryGetValue(path, out var oldDocument))
                throw new Exception("A file named '" + path + "' does not exist.");

            var document = new CodeFile(path, content, Parser.Parse(path, content));
            _documents = _documents.SetItem(document.Path, document);
            _eventSubscriptions.Publish(new DocumentChanged(oldDocument, document));
        }

        public void UpdateFile(string path, Range2D range, string content)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("A file named '" + path + "' does not exist.");

            UpdateFile(path, document.ConvertTo1D(range), content);
        }

        public void UpdateFile(string path, Range1D range, string content)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("A file named '" + path + "' does not exist.");

            if (document is not CodeFile codeFile)
                throw new Exception($"Incrental update of '{path}' is not supported.");

            var pre = codeFile.Content.Substring(0, range.Start.Position);
            var post = codeFile.Content.Substring(range.End.Position);

            UpdateFile(path, pre + content + post);
        }

        public void RemoveFile(string path)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("A file named '" + path + "' does not exist.");
            _documents.Remove(path);
            _eventSubscriptions.Publish(new DocumentRemoved(document));
        }

        public bool ContainsFile(string path) => _documents.TryGetValue(path, out var document);

        public WorkspaceDocument GetFile(string path)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("This workspace does not contain a file with path: " + path);

            return document;
        }
    }

    public interface IWorkspaceEvent { }
    public record DocumentAdded(WorkspaceDocument Document) : IWorkspaceEvent;
    public record DocumentChanged(WorkspaceDocument OldDocument, WorkspaceDocument NewDocument) : IWorkspaceEvent;
    public record DocumentRemoved(WorkspaceDocument Document) : IWorkspaceEvent;
}
