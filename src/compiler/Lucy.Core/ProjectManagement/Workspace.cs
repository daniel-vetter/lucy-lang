﻿using Lucy.Common;
using Lucy.Core.Parsing;
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
        private Dictionary<string, WorkspaceDocument> _documents = new();
        private ImmutableList<string> _paths = ImmutableList<string>.Empty;

        private Subscriptions<IWorkspaceEvent> _eventSubscriptions = new();

        public IEnumerable<WorkspaceDocument> Documents => _documents.Values;
        public ImmutableList<string> Paths => _paths;

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
                ws.AddDocument(new CodeFile(subPath, file.Content, Parser.Parse(subPath, file.Content)));
            }
            return ws;
        }

        public void AddDocument(WorkspaceDocument document)
        {
            _documents.Add(document.Path, document);
            _paths = _paths.Add(document.Path);
            _eventSubscriptions.Publish(new DocumentAdded(document));
        }

        public void AddOrUpdateDocument(WorkspaceDocument document)
        {
            if (_documents.TryGetValue(document.Path, out var oldDocument))
            {
                _documents[document.Path] = document;
                _eventSubscriptions.Publish(new DocumentChanged(oldDocument, document));
            }
            else
            {
                AddDocument(document);
            }
        }

        public void RemoveDocument(string path)
        {
            if (!_documents.TryGetValue(path, out var document))
                throw new Exception("This workspace does not contain a document with the path: " + path);
            _documents.Remove(path);
            _paths.Remove(path);
            _eventSubscriptions.Publish(new DocumentRemoved(document));
        }

        public bool ContainsDocument(string path) => _documents.ContainsKey(path);

        public WorkspaceDocument? Get(string path)
        {
            if (_documents.TryGetValue(path, out var document))
                return document;
            return null;
        }
    }

    public interface IWorkspaceEvent { }
    public record DocumentAdded(WorkspaceDocument Document) : IWorkspaceEvent;
    public record DocumentChanged(WorkspaceDocument OldDocument, WorkspaceDocument NewDocument) : IWorkspaceEvent;
    public record DocumentRemoved(WorkspaceDocument Document) : IWorkspaceEvent;
}
