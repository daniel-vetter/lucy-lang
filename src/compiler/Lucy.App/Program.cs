using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Lucy.App.Infrastructure.Cli;
using Lucy.App.Infrastructure.Output;
using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer;
using Microsoft.Extensions.DependencyInjection;

namespace Lucy.App
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var sp = new ServiceCollection()
                .AddServicesFromCurrentAssembly()
                .AddLanguageServerFeatureModule()
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
                .UseCustomExceptionHandler()
                .UseJsonLoggingGlobalOption(sp.GetRequiredService<IOutput>());

            sp.GetServices<ICliCommand>()
                .ToList()
                .ForEach(x => x.Register(cmdBuilder));
            
            return await cmdBuilder.Build().InvokeAsync(args);
        }
    }
}
