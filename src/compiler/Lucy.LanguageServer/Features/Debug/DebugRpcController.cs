﻿using System.Diagnostics;
using System.Threading.Tasks;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Infrastructure.RpcServer;

namespace Lucy.App.LanguageServer.Features.Debug;

[Service(Lifetime.Singleton)]
public class DebugRpcController
{
    private readonly DebugViewGenerator _debugViewGenerator;
    private readonly CurrentWorkspace _currentWorkspace;

    public DebugRpcController(DebugViewGenerator debugViewGenerator, CurrentWorkspace currentWorkspace)
    {
        _debugViewGenerator = debugViewGenerator;
        _currentWorkspace = currentWorkspace;
    }

    [JsonRpcFunction("debug/getSyntaxTree")]
    public async Task<string> GetSyntaxTree(RpcGetSyntaxTree input)
    {
        var path = _currentWorkspace.ToWorkspacePath(input.Uri);
        var tree = _currentWorkspace.Analysis.Get<Nodes>().GetSyntaxTree(path);
        return await _debugViewGenerator.Generate(tree);
    }

    [JsonRpcFunction("debug/getScopeTree")]
    public async Task<string> GetScopeTree(RpcGetScopeTree input)
    {
        var path = _currentWorkspace.ToWorkspacePath(input.Uri);
        var tree = _currentWorkspace.Analysis.Get<ScopeTreeBuilder>().GetScopeTree(path);
        return await _debugViewGenerator.Generate(tree);
    }

    [JsonRpcFunction("debug/attachDebugger")]
    public Task AttachDebugger()
    {
        Debugger.Launch(); 
        return Task.CompletedTask;
    }

    [JsonRpcFunction("debug/attachProfiler")]
    public Task AttachProfiler()
    {
        ExternalProfiler.Attach();
        return Task.CompletedTask;
    }

    [JsonRpcFunction("debug/exportProfiler")]
    public Task ExportProfiler()
    {
        Profiler.ExportAndShow();
        return Task.CompletedTask;
    }

    /*
    [JsonRpcFunction("debug/getAssembly")]
    public string GetAssembly()
    {
        if (_currentWorkspace.Workspace == null)
            return "";

        return WinExecutableEmitter.GetAssemblyCode(_currentWorkspace.Workspace);
    }
    */
}

public class RpcGetSyntaxTree
{
    public SystemPath Uri { get; set; } = null!;
}

public class RpcGetScopeTree
{
    public SystemPath Uri { get; set; } = null!;
}