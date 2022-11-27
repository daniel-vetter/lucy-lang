using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Lucy.App.LanguageServer.Infrastructure;

internal class CustomConsoleFormatter : ConsoleFormatter
{
    private static readonly Stopwatch _startupTime = Stopwatch.StartNew();

    public CustomConsoleFormatter() : base("Custom")
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        var msg = logEntry.Formatter(logEntry.State, logEntry.Exception);

        var resetColor = "\x1B[39m\x1B[22m";
        var color = logEntry.LogLevel switch
        {
            LogLevel.Information => "\x1B[1m\x1B[32m",
            LogLevel.Warning => "\x1B[1m\x1B[33m",
            LogLevel.Error => "\x1B[1m\x1B[31m",
            LogLevel.Critical => "\x1B[31m",
            _ => "\x1B[37m"
        };

        var severity = logEntry.LogLevel switch
        {
            LogLevel.Information => "Info",
            LogLevel.Warning => "Warn",
            _ => logEntry.LogLevel.ToString()
        };

        if (logEntry.Exception == null)
            textWriter.WriteLine($"{time} {color}{severity}{resetColor} {msg}");
        else
        {
            textWriter.WriteLine($"{time} {color}{severity}{resetColor} {msg} {logEntry.Exception}");
            textWriter.WriteLine();
        }
            
    }
}