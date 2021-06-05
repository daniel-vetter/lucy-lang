using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Feature.LanguageServer.RpcController;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Tests
{
    public static class LanguageServerInitalizationExtension
    {
        public static async Task<RpcInitializeResult> Initialze(this LanguageServer server)
        {
            server.Get<IFileSystem>().CreateDirectory(new SystemPath("C:\\workspace"));

            var input = new RpcInitializeParams
            {
                Capabilities = new RpcClientCapabilities
                {
                    TextDocument = new()
                    {
                        Hover = new()
                        {
                            ContentFormat = new[] { RpcMarkupKind.Markdown }
                        }
                    }
                },
                RootUri = new SystemPath("C:\\workspace")
            };

            var result = await server.Get<ServerLifecycleRpcController>().Initialize(input);
            await server.Get<ServerLifecycleRpcController>().Initialized();

            return result;
        }
    }
}
