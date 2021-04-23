using System;
using System.Linq;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer.Services;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service]
    internal class ServerLifecycleRpcController
    {
        private readonly CurrentRpcConnection _currentRpcConnection;
        private readonly CurrentWorkspace _currentWorkspace;
        private readonly Updater _updater;

        public ServerLifecycleRpcController(CurrentRpcConnection currentRpcConnection, CurrentWorkspace currentWorkspace, Updater updater)
        {
            _currentRpcConnection = currentRpcConnection;
            _currentWorkspace = currentWorkspace;
            _updater = updater;
        }

        [JsonRpcFunction("initialize", deserializeParamterIntoSingleObject: true)]
        public async Task<RpcInitializeResult> Initialize(RpcInitializeParams request)
        {
            var result = new RpcInitializeResult();

            //General server info
            result.ServerInfo = new RpcServerInfo
            {
                Name = "Lucy language server",
                Version = GetType().Assembly.GetName().Version?.ToString() ?? ""
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

            /*
            result.Capabilities.CompletionProvider = new CompletionOptions
            {
                TriggerCharacters = new string[] { "." }
            };
            */

            if (request.RootUri != null)
            {
                await _currentWorkspace.Init(request.RootUri ?? throw new Exception("No root uri provided"));
                
            }

            return result;
        }

        [JsonRpcFunction("initialized")]
        public async Task Initialized()
        {
            await _updater.Update();
        }

        [JsonRpcFunction("shutdown")]
        public void Shutdown()
        {
            _ = _currentRpcConnection.Shutdown();
        }
    }
}
