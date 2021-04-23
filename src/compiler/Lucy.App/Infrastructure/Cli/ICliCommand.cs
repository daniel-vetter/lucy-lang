using System.CommandLine.Builder;

namespace Lucy.App.Infrastructure.Cli
{
    internal interface ICliCommand
    {
        void Register(CommandLineBuilder builder);
    }
}
