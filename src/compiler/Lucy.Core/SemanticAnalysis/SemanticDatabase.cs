using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Diagnostics;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis
{
    public class SemanticDatabase : IDb, IDisposable
    {
        private readonly Workspace _workspace;
        private IDisposable _workspaceEventSubscription;
        private IDisposable? _exporterEventSubscription;
        private Db _db = new();
        
        public SemanticDatabase(Workspace workspace, string? traceOutputDir = null)
        {
            _workspaceEventSubscription = workspace.AddEventHandler(OnWorkspaceEvent);
            _workspace = workspace;

            RegisterHandler();
            AddWorkspaceAsInputs(workspace);
            RegisterTraceListener(traceOutputDir);
        }

        private void RegisterTraceListener(string? graphOutputDir)
        {
            if (graphOutputDir != null)
            {
                var exporter = new GraphExport(graphOutputDir);
                _exporterEventSubscription = _db.AddEventHandler(exporter.ProcessDbEvent);
            }
        }

        private void AddWorkspaceAsInputs(Workspace workspace)
        {
            
            _db.SetInput(new GetDocumentList(), new GetDocumentListResult(_workspace.Documents.Keys.ToComparableReadOnlyList()));
            foreach (var codeFile in workspace.Documents.Values.OfType<CodeFile>())
            {
                _db.SetInput(new GetSyntaxTree(codeFile.Path), new GetSyntaxTreeResult(codeFile.SyntaxTree.Build()));
            }
                
        }

        private void RegisterHandler()
        {
            var handlerTypes = typeof(SemanticDatabase)
                .Assembly
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(QueryHandler)) && x.IsAbstract == false)
                .ToArray();

            foreach (var type in handlerTypes)
            {
                _db.RegisterHandler((QueryHandler)(Activator.CreateInstance(type) ?? throw new Exception("Could not create handler")));
            }
        }

        public void Dispose()
        {
            _workspaceEventSubscription.Dispose();
            _exporterEventSubscription?.Dispose();
        }

        public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
        {
            var sw = Stopwatch.StartNew();
            var result = _db.Query(query);
            return result;
        }

        private void OnWorkspaceEvent(IWorkspaceEvent @event)
        {
            if (@event is DocumentAdded documentAdded)
            {
                _db.SetInput(new GetDocumentList(), new GetDocumentListResult(_workspace.Documents.Keys.ToComparableReadOnlyList()));
                if (documentAdded.Document is CodeFile codeFile)
                {
                    _db.SetInput(new GetSyntaxTree(codeFile.Path), new GetSyntaxTreeResult(codeFile.SyntaxTree.Build()));
                }

                else
                    throw new NotSupportedException("Unsupported workspace document: " + documentAdded.Document.GetType().Name);
            }
            if (@event is DocumentChanged documentChanged)
            {
                if (documentChanged.NewDocument is CodeFile codeFile)
                {
                    _db.SetInput(new GetSyntaxTree(codeFile.Path), new GetSyntaxTreeResult(codeFile.SyntaxTree.Build()));
                }
            }
            if (@event is DocumentRemoved documentRemoved)
            {
                _db.SetInput(new GetDocumentList(), new GetDocumentListResult(_workspace.Documents.Keys.ToComparableReadOnlyList()));
                _db.RemoveInput(new GetSyntaxTree(documentRemoved.Document.Path));
            }
        }
    }
}
