using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Feature.LanguageServer.RpcController;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Tests
{
    public static class LanguageServerInitalizationExtension
    {
        public static async Task<RpcInitializeResult> Initialze(this LanguageServer server)
        {
            server.Get<IFileSystem>().CreateDirectory(server.GetWorkspacePath());

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
                RootUri = server.GetWorkspacePath()
            };

            var result = await server.Get<ServerLifecycleRpcController>().Initialize(input);
            server.Get<ServerLifecycleRpcController>().Initialized();

            return result;
        }

        public static SystemPath GetWorkspacePath(this LanguageServer server)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new SystemPath("C:\\workspace");

            return new SystemPath("/workspace");
        }
    }
}
