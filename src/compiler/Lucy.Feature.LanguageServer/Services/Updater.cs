using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service]
    public class Updater
    {
        private readonly CurrentWorkspace _currentWorkspace;
        private readonly DiagnosticsUpdater _diagnosticsUpdater;
        private readonly SyntaxTreeDumper _syntaxTreeDumper;

        public Updater(CurrentWorkspace currentWorkspace, DiagnosticsUpdater diagnosticsUpdater, SyntaxTreeDumper syntaxTreeDumper)
        {
            _currentWorkspace = currentWorkspace;
            _diagnosticsUpdater = diagnosticsUpdater;
            _syntaxTreeDumper = syntaxTreeDumper;
        }

        public async Task Update()
        {
            /*
            _currentWorkspace.Update();
            await _diagnosticsUpdater.Update(compiledWorkspace);
            await _syntaxTreeDumper.Update(compiledWorkspace);
            */
        }
    }
}
