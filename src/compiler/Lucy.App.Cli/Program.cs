using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Lucy.App.Cli.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;

namespace Lucy.App.Cli
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var sp = new ServiceCollection()
                .AddServicesFromCurrentAssembly()
                .BuildServiceProvider();

            var cmdBuilder = new CommandLineBuilder(new RootCommand("Lucy command line interface"))
                .UseVersionOption()
                .UseHelp()
                .UseEnvironmentVariableDirective()
                .UseParseDirective()
                .UseDebugDirective()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseTypoCorrections()
                .UseParseErrorReporting()
                .UseExceptionHandler()
                .CancelOnProcessTermination()
                .UseCustomExceptionHandler();

            sp.GetServices<ICliCommand>()
                .ToList()
                .ForEach(x => x.Register(cmdBuilder));
            
            return await cmdBuilder.Build().InvokeAsync(args);
        }
    }
}
