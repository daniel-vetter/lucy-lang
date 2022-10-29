using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis
{
    public class SemanticAnalyzer : IDisposable
    {
        private readonly Workspace _workspace;
        private IDisposable _workspaceEventSubscription;
        private IDisposable? _exporterEventSubscription;
        private Db _db = new();
        
        public SemanticAnalyzer(Workspace workspace, string? graphOutputDir)
        {
            _workspaceEventSubscription = workspace.AddEventHandler(OnWorkspaceEvent);
            _workspace = workspace;

            _db.RegisterHandler(new GetNodesMapHandler());
            _db.RegisterHandler(new GetNodeByIdHandler());

            foreach(var codeFile in workspace.Documents.Values.OfType<CodeFile>())
                _db.SetInput(new GetSyntaxTree(codeFile.Path), new GetSyntaxTreeResult(codeFile.SyntaxTree));

            if (graphOutputDir != null)
            {
                var exporter = new GraphExport(graphOutputDir);
                _exporterEventSubscription = _db.AddEventHandler(exporter.ProcessDbEvent);
            }
        }

        public void Dispose()
        {
            _workspaceEventSubscription.Dispose();
            _exporterEventSubscription?.Dispose();
        }

        public SyntaxTreeNode GetNodeById(NodeId nodeId)
        {
            return _db.Query(new GetNodeById(nodeId)).Node;
        }

        private void OnWorkspaceEvent(IWorkspaceEvent @event)
        {
            if (@event is DocumentAdded documentAdded)
            {
                _db.SetInput(new GetDocumentList(), new GetDocumentListResult(_workspace.Documents.Keys.ToImmutableList()));
                if (documentAdded.Document is CodeFile codeFile)
                    _db.SetInput(new GetSyntaxTree(codeFile.Path), new GetSyntaxTreeResult(codeFile.SyntaxTree));
                else
                    throw new NotSupportedException("Unsupported workspace document: " + documentAdded.Document.GetType().Name);
            }
            if (@event is DocumentChanged documentChanged)
            {
                if (documentChanged.NewDocument is CodeFile codeFile)
                    _db.SetInput(new GetSyntaxTree(codeFile.Path), new GetSyntaxTreeResult(codeFile.SyntaxTree));
            }
            if (@event is DocumentRemoved documentRemoved)
            {
                _db.SetInput(new GetDocumentList(), new GetDocumentListResult(_workspace.Documents.Keys.ToImmutableList()));
                _db.RemoveInput(new GetSyntaxTree(documentRemoved.Document.Path));
            }
        }
    }

    public record GetDocumentList() : IQuery<GetDocumentListResult>;
    public class GetDocumentListResult
    {
        public GetDocumentListResult(ImmutableList<string> paths)
        {
            Paths = paths;
        }

        public ImmutableList<string> Paths { get; }

        public override bool Equals(object? obj) => obj is GetDocumentListResult result && EqualityComparer<ImmutableList<string>>.Default.Equals(Paths, result.Paths);
        public override int GetHashCode() => HashCode.Combine(Paths);
    }

    public record GetSyntaxTree(string DocumentPath) : IQuery<GetSyntaxTreeResult>;
    public record GetSyntaxTreeResult(DocumentRootSyntaxNode RootNode);
}
