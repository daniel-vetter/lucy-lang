using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis;

public class SemanticDatabase : IDb, IDisposable
{
    private readonly Workspace _workspace;
    private readonly IDisposable _workspaceEventSubscription;
    private readonly Db _db = new();

    public SemanticDatabase(Workspace workspace)
    {
        _workspaceEventSubscription = workspace.AddEventHandler(OnWorkspaceEvent);
        _workspace = workspace;

        RegisterHandler();
        AddWorkspaceAsInputs(workspace);
    }
    
    private void AddWorkspaceAsInputs(Workspace workspace)
    {
        _db.SetInput(new GetDocumentList(), _workspace.Documents.Keys.ToComparableReadOnlyList());
        foreach (var codeFile in workspace.Documents.Values.OfType<CodeWorkspaceDocument>())
        {
            _db.SetInput(new GetSyntaxTree(codeFile.Path), codeFile.ParserResult.RootNode);
            _db.SetInput(new GetNodesByNodeIdMap(codeFile.Path), codeFile.ParserResult.NodesById);
            _db.SetInput(new GetNodeIdsByTypeMap(codeFile.Path), codeFile.ParserResult.NodeIdsByType);
            _db.SetInput(new GetParentNodeIdByNodeIdMap(codeFile.Path), codeFile.ParserResult.ParentNodeIdsByNodeId);
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
    }

    public object Query(object query)
    {
        return _db.Query(query);
    }

    private void OnWorkspaceEvent(IWorkspaceEvent @event)
    {
        if (@event is DocumentAdded documentAdded)
        {
            _db.SetInput(new GetDocumentList(), _workspace.Documents.Keys.ToComparableReadOnlyList());
            if (documentAdded.Document is CodeWorkspaceDocument codeFile)
            {
                _db.SetInput(new GetSyntaxTree(codeFile.Path), codeFile.ParserResult.RootNode);
                _db.SetInput(new GetNodesByNodeIdMap(codeFile.Path), codeFile.ParserResult.NodesById);
                _db.SetInput(new GetNodeIdsByTypeMap(codeFile.Path), codeFile.ParserResult.NodeIdsByType);
                _db.SetInput(new GetParentNodeIdByNodeIdMap(codeFile.Path), codeFile.ParserResult.ParentNodeIdsByNodeId);
            }
            else
                throw new NotSupportedException("Unsupported workspace document: " + documentAdded.Document.GetType().Name);
        }
        if (@event is DocumentChanged documentChanged)
        {
            if (documentChanged.NewDocument is CodeWorkspaceDocument codeFile)
            {
                _db.SetInput(new GetSyntaxTree(codeFile.Path), codeFile.ParserResult.RootNode);
                _db.SetInput(new GetNodesByNodeIdMap(codeFile.Path), codeFile.ParserResult.NodesById);
                _db.SetInput(new GetNodeIdsByTypeMap(codeFile.Path), codeFile.ParserResult.NodeIdsByType);
                _db.SetInput(new GetParentNodeIdByNodeIdMap(codeFile.Path), codeFile.ParserResult.ParentNodeIdsByNodeId);
            }
        }
        if (@event is DocumentRemoved documentRemoved)
        {
            _db.SetInput(new GetDocumentList(), _workspace.Documents.Keys.ToComparableReadOnlyList());
            _db.RemoveInput(new GetSyntaxTree(documentRemoved.Document.Path));
            _db.RemoveInput(new GetNodesByNodeIdMap(documentRemoved.Document.Path));
            _db.RemoveInput(new GetNodeIdsByTypeMap(documentRemoved.Document.Path));
            _db.RemoveInput(new GetParentNodeIdByNodeIdMap(documentRemoved.Document.Path));
        }
    }
}