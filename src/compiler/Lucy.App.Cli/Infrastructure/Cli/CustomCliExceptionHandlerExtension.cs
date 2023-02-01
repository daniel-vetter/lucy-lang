using System.CommandLine.Builder;
using Spectre.Console;

namespace Lucy.App.Cli.Infrastructure.Cli
{
    internal static class CustomCliExceptionHandlerExtension
    {
        public static CommandLineBuilder UseCustomExceptionHandler(this CommandLineBuilder commandLineBuilder)
        {
            return commandLineBuilder.UseExceptionHandler((e, ctx) =>
            {
                if (e is CliException)
                {
                    AnsiConsole.WriteException(e);
                    ctx.ResultCode = -1;
                }
                else
                {
                    AnsiConsole.WriteException(e);
                    ctx.ResultCode = -2;
                }
            });
        }
    }
}