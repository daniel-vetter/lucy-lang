using Lucy.App.Infrastructure.Output;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace Lucy.App.Infrastructure.Cli
{
    internal static class JsonLoggingGlobalOption
    {
        public static CommandLineBuilder UseJsonLoggingGlobalOption(this CommandLineBuilder commandLineBuilder, IOutput output)
        {
            commandLineBuilder.AddGlobalOption(new Option("--json", "Write all log messages as newline delimited json to stdout"))
                .UseMiddleware(x =>
                {
                    if (x.ParseResult.HasOption("--json"))
                        output.JsonMode = true;
                });
            return commandLineBuilder;
        }
    }
}
