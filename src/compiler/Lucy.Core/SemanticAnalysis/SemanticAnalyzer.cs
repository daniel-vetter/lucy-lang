using Lucy.Core.ProjectManagement;
using System;
using System.Collections.Immutable;
using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis;

public class SemanticAnalyzer : IDisposable
{
    private readonly IDisposable _workspaceEventSubscription;
    private readonly CacheEngine _cacheEngine;

    public SemanticAnalyzer(Workspace workspace)
    {
        _workspaceEventSubscription = workspace.AddEventHandler(OnWorkspaceEvent);
        _cacheEngine = new CacheEngine(new QueryGroupCollection().AddFromCurrentAssembly(), GetQueryListener());

        AddWorkspaceAsInputs(workspace);
    }

    private IQueryListener? GetQueryListener()
    {
        var targetPath = Environment.GetEnvironmentVariable("LUCY_CORE_EXPORT_SEMANTIC_DB_LOG");
        if (string.IsNullOrWhiteSpace(targetPath))
            return null;

        return new QueryMetricsExporter(targetPath);
    }

    public T Get<T>() => _cacheEngine.Get<T>();

    private void AddWorkspaceAsInputs(Workspace workspace)
    {
        var input = _cacheEngine.Get<SemanticAnalysisInput>();
        input.ParsedCodeFiles = workspace.Documents.Values
            .OfType<CodeWorkspaceDocument>()
            .ToImmutableDictionary(x => x.Path, static x => Map(x.ParserResult));
    }

    private static ParsedCodeFile Map(ParserResult parserResult)
    {
        return new ParsedCodeFile(
            parserResult.RootNode,
            parserResult.NodesByNodeId,
            parserResult.NodeIdsByType,
            parserResult.ParentNodeIdsByNodeId
        );
    }

    private void OnWorkspaceEvent(IWorkspaceEvent @event)
    {
        var input = _cacheEngine.Get<SemanticAnalysisInput>();

        switch (@event)
        {
            case DocumentAdded documentAdded:
            {
                if (documentAdded.Document is CodeWorkspaceDocument codeFile)
                    input.ParsedCodeFiles = input.ParsedCodeFiles.Add(documentAdded.Document.Path, Map(codeFile.ParserResult));

                break;
            }
            case DocumentChanged documentChanged:
            {
                if (documentChanged.NewDocument is CodeWorkspaceDocument codeFile)
                    input.ParsedCodeFiles = input.ParsedCodeFiles.SetItem(codeFile.Path, Map(codeFile.ParserResult));

                break;
            }
            case DocumentRemoved documentRemoved:
            {
                if (documentRemoved.Document is CodeWorkspaceDocument codeFile)
                    input.ParsedCodeFiles = input.ParsedCodeFiles.Remove(codeFile.Path);

                break;
            }
        }
    }

    public void Dispose()
    {
        _workspaceEventSubscription.Dispose();
    }
}

public record ParsedCodeFile(
    DocumentRootSyntaxNode RootNode,
    ImmutableDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> NodesByNodeId,
    ImmutableDictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>> NodeIdsByType,
    ImmutableDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?> ParentNodeIdByNodeIds
);

[QueryGroup]
public class SemanticAnalysisInput
{
    public virtual ImmutableDictionary<string, ParsedCodeFile> ParsedCodeFiles { get; set; } = ImmutableDictionary<string, ParsedCodeFile>.Empty;
}