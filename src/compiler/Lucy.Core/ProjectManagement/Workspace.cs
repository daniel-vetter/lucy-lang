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
                ws.AddOrUpdateFile(subPath, file.Content);
            }
            return ws;
        }

        public void AddOrUpdateFile(string path, string content)
        {
            if (path.EndsWith(".lucy"))
            {
                var document = new CodeFile(path, content, Parser.Parse(path, content));
                if (_documents.TryGetValue(path, out var oldDocument))
                {
                    _documents = _documents.SetItem(document.Path, document);
                    _eventSubscriptions.Publish(new DocumentChanged(oldDocument, document));
                }
                else
                {
                    _documents = _documents.Add(document.Path, document);
                    _eventSubscriptions.Publish(new DocumentAdded(document));
                }
            }
            else
                throw new NotSupportedException("Could not determin type of workspace file: " + path);
        }

        public void RemoveFile(string path)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("This workspace does not contain a document with the path: " + path);
            _documents.Remove(path);
            _eventSubscriptions.Publish(new DocumentRemoved(document));
        }

        public bool ContainsFile(string path) => _documents.TryGetValue(path, out var document);

        public CodeFile GetCodeFile(string path)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("This workspace does not contain a code file with path: " + path);

            if (document is not CodeFile cf)
                throw new Exception("This workspace does contain a file with path " + path + " but it is not a code file.");

            return cf;
        }
    }

    public interface IWorkspaceEvent { }
    public record DocumentAdded(WorkspaceDocument Document) : IWorkspaceEvent;
    public record DocumentChanged(WorkspaceDocument OldDocument, WorkspaceDocument NewDocument) : IWorkspaceEvent;
    public record DocumentRemoved(WorkspaceDocument Document) : IWorkspaceEvent;
}
