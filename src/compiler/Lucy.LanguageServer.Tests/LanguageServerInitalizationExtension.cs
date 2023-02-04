using Lucy.App.LanguageServer.Infrastructure;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Lucy.App.LanguageServer.Features;
using Lucy.App.LanguageServer.Models;

namespace Lucy.App.LanguageServer.Tests
{
    public static class LanguageServerInitalizationExtension
    {
        public static async Task<RpcInitializeResult> Initialze(this LanguageServer server)
        {
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
            await server.Get<ServerLifecycleRpcController>().Initialized();

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
