using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Lucy.App.Cli.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;

namespace Lucy.App.Cli;

public static class CliApp
{
    public static async Task<int> Main(string[] args)
    {
        var sp = new ServiceCollection()
            .AddServicesFromCurrentAssembly()
            .BuildServiceProvider();

        var b = new CommandLineBuilder(new RootCommand("Lucy command line interface"))
            .UseVersionOption()
            .UseHelp()
            .UseEnvironmentVariableDirective()
            .UseParseDirective()
            .UseSuggestDirective()
            .RegisterWithDotnetSuggest()
            .UseTypoCorrections()
            .UseParseErrorReporting()
            .UseExceptionHandler()
            .CancelOnProcessTermination()
            .UseCustomExceptionHandler();

        sp.GetServices<ICliCommand>()
            .ToList()
            .ForEach(x => x.Register(b));
            
        return await b.Build().InvokeAsync(args);
    }
}