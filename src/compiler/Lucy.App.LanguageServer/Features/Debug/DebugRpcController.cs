using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Features.Debug
{
    [Service(Lifetime.Singleton)]
    public class DebugRpcController
    {
        private readonly DebugViewGenerator _debugViewGenerator;

        public DebugRpcController(DebugViewGenerator debugViewGenerator)
        {
            _debugViewGenerator = debugViewGenerator;
        }

        [JsonRpcFunction("debug/getSyntaxTree")]
        public async Task<string> GetDump()
        {
            return await _debugViewGenerator.Generate();
        }

        [JsonRpcFunction("debug/attachDebugger")]
        public Task AttachDebugger()
        {
            Debugger.Launch();
            return Task.CompletedTask;
        }
    }
}
