using System.CommandLine.Builder;

namespace Lucy.App.Cli.Infrastructure.Cli
{
    internal interface ICliCommand
    {
        void Register(CommandLineBuilder builder);
    }
}
