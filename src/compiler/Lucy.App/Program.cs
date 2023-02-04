using Lucy.App.Cli;
using Lucy.App.LanguageServer;

if (args.Contains("--language-server"))
    return await LanguageServerApp.Main();
return await CliApp.Main(args);