using Lucy.App.LanguageServer.Features.Diagnostics;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;

namespace Lucy.App.LanguageServer.Features;

[Service(Lifetime.Singleton)]
public class ServerLifecycleRpcController
{
    private readonly JsonRpcServer _jsonRpcServer;
    private readonly CurrentWorkspace _currentWorkspace;
    private readonly DiagnosticsReporter _diagnosticsReporter;

    public ServerLifecycleRpcController(JsonRpcServer jsonRpcServer, CurrentWorkspace currentWorkspace, DiagnosticsReporter diagnosticsReporter)
    {
        _jsonRpcServer = jsonRpcServer;
        _currentWorkspace = currentWorkspace;
        _diagnosticsReporter = diagnosticsReporter;
    }

    [JsonRpcFunction("initialize", deserializeParameterIntoSingleObject: true)]
    public async Task<RpcInitializeResult> Initialize(RpcInitializeParams request)
    {
        var result = new RpcInitializeResult
        {
            //General server info
            ServerInfo = new RpcServerInfo
            {
                Name = "Lucy language server",
                Version = GetType().Assembly.GetName().Version?.ToString() ?? ""
            }
        };

        //Only support hover if markdown is supported
        var supportedHoverMarkupKind = request.Capabilities.TextDocument?.Hover?.ContentFormat ?? Array.Empty<RpcMarkupKind>();
        if (supportedHoverMarkupKind.Contains(RpcMarkupKind.Markdown))
        {
            result.Capabilities.HoverProvider = true;
        }

        //Enable document synchronization
        result.Capabilities.TextDocumentSync = new RpcTextDocumentSyncOptions
        {
            Change = RpcTextDocumentSyncKind.Incremental,
            OpenClose = true
        };

        // Initializing these properties with empty object is enough to tell the client that these features are supported
        result.Capabilities.DocumentLinkProvider = new RpcDocumentLinkOptions();
        result.Capabilities.CompletionProvider = new RpcCompletionOptions();
        result.Capabilities.DefinitionProvider = true;
        //Enable signature help
        result.Capabilities.SignatureHelpProvider = new RpcSignatureHelpOptions
        {
            TriggerCharacters = new[] { "(", "," }
        };
            
        if (request.RootUri != null)
        {
            await _currentWorkspace.Load(request.RootUri);
        }

        return result;
    }

    [JsonRpcFunction("initialized")]
    public async Task Initialized()
    {
        await _diagnosticsReporter.Report();
    }

    [JsonRpcFunction("shutdown")]
    public void Shutdown()
    {
        // Awaiting the returned task would lead to a a deadlock.
        // When stop is called, the JsonRpcServer will wait for all running handlers to complete,
        // but the current handler would never complete because it is waiting for the JsonRpcServer to shut down.
        _ = _jsonRpcServer.Stop();
    }
}