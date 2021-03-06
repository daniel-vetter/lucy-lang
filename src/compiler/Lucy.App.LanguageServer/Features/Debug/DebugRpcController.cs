using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Compiler;
using Lucy.Infrastructure.RpcServer;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Features.Debug
{
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

        [JsonRpcFunction("debug/getAssembly")]
        public string GetAssembly()
        {
            if (_currentWorkspace.Workspace == null)
                return "";

            return WinExecutableEmitter.GetAssemblyCode(_currentWorkspace.Workspace);
        }
    }
}
