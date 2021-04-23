using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using Lucy.App.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer;

namespace Lucy.App.Features
{
    [Service]
    class LanguageServerCommand : ICliCommand
    {
        private readonly LanguageServerFacade _languageServerFacade;

        public LanguageServerCommand(LanguageServerFacade languageServerFacade)
        {
            _languageServerFacade = languageServerFacade;
        }

        public void Register(CommandLineBuilder builder)
        {
            builder.AddCommand(new Command("language-server", "Start the cli in language-server mode")
            {
                Handler = CommandHandler.Create(async () =>
                {
                    await _languageServerFacade.Run();
                })
            });
        }
    }
}
