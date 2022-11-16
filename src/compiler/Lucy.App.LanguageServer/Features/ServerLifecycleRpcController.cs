using System;
using System.Linq;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service(Lifetime.Singleton)]
    public class ServerLifecycleRpcController
    {
        private readonly JsonRpcServer _jsonRpcServer;
        private readonly CurrentWorkspace _currentWorkspace;

        public ServerLifecycleRpcController(JsonRpcServer jsonRpcServer, CurrentWorkspace currentWorkspace)
        {
            _jsonRpcServer = jsonRpcServer;
            _currentWorkspace = currentWorkspace;
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
        public void Initialized()
        {
        }

        [JsonRpcFunction("shutdown")]
        public void Shutdown()
        {
            _ = _jsonRpcServer.Stop();
        }
    }
}
