using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucy.Core.SemanticAnalysis
{
    public class SemanticAnalyzer : IDisposable
    {
        private readonly Workspace _workspace;
        private IDisposable _workspaceEventSubscription;
        private Db _db = new();

        public SemanticAnalyzer(Workspace workspace)
        {
            _workspaceEventSubscription = workspace.AddEventHandler(OnWorkspaceEvent);
            _workspace = workspace;
        }

        public void Dispose()
        {
            _workspaceEventSubscription.Dispose();
        }

        private void OnWorkspaceEvent(IWorkspaceEvent @event)
        {
            if (@event is DocumentAdded documentAdded)
            {
                _db.SetInput(new DocumentListQuery(), new DocumentListResult(_workspace.Paths));
                if (documentAdded.Document is CodeFile codeFile)
                    _db.SetInput(new DocumentSyntaxTreeQuery(codeFile.Path), new DocumentSyntaxTreeResult(codeFile.SyntaxTree));
                else
                    throw new NotSupportedException("Unsupported workspace document: " + documentAdded.Document.GetType().Name);
            }
            if (@event is DocumentChanged documentChanged)
            {
                if (documentChanged.NewDocument is CodeFile codeFile)
                    _db.SetInput(new DocumentSyntaxTreeQuery(codeFile.Path), new DocumentSyntaxTreeResult(codeFile.SyntaxTree));
            }
            if (@event is DocumentRemoved documentRemoved)
            {
                _db.SetInput(new DocumentListQuery(), new DocumentListResult(_workspace.Paths));
                _db.RemoveInput(new DocumentSyntaxTreeQuery(documentRemoved.Document.Path));
            }
        }
    }

    public record DocumentListQuery() : IQuery<DocumentListResult>;
    public class DocumentListResult
    {
        public DocumentListResult(ImmutableList<string> paths)
        {
            Paths = paths;
        }

        public ImmutableList<string> Paths { get; }

        public override bool Equals(object? obj) => obj is DocumentListResult result && EqualityComparer<ImmutableList<string>>.Default.Equals(Paths, result.Paths);
        public override int GetHashCode() => HashCode.Combine(Paths);
    }

    public record DocumentSyntaxTreeQuery(string Path) : IQuery<DocumentSyntaxTreeResult>;
    public record DocumentSyntaxTreeResult(DocumentRootSyntaxNode RootNode);
}
