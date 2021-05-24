using Lucy.Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.Core.ProjectManagement
{
    public class Workspace
    {
        Dictionary<string, TextDocument> _documents = new Dictionary<string, TextDocument>();

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
                ws.AddDocument(new TextDocument(subPath, file.Content));
            }
            return ws;
        }

        public void AddDocument(TextDocument document)
        {
            _documents.Add(document.Path, document);
        }

        public void AddOrUpdateDocument(TextDocument document)
        {
            _documents[document.Path] = document;
        }

        public void RemoveDocument(string path)
        {
            _documents.Remove(path);
        }

        public void ReplaceDocument(TextDocument document)
        {
            if (!_documents.ContainsKey(document.Path))
                throw new Exception("Workspace does not contain a document with path: " + document.Path);
            _documents[document.Path] = document;
        }

        public void Change(string path, Range1D range, string content)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("Workspace does not contain a document with path: " + path);
            _documents[path] = document.Change(range, content);
        }

        public void Change(string path, Range2D range, string content)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("Workspace does not contain a document with path: " + path);
            _documents[path] = document.Change(range, content);
        }

        public bool ContainsFile(string path)
        {
            return _documents.ContainsKey(path);
        }

        public TextDocument? Get(string path)
        {
            if (_documents.TryGetValue(path, out var document))
                return document;
            return null;
        }

        public ImmutableArray<TextDocument> Documents
        {
            get
            {
                return _documents.Values.ToImmutableArray();
            }
        }
    }
}
