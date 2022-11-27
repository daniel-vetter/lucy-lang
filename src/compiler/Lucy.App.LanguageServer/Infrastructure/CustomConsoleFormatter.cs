using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Lucy.App.LanguageServer.Infrastructure;

internal class CustomConsoleFormatter : ConsoleFormatter
{
    public CustomConsoleFormatter() : base("Custom")
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var time = DateTime.Now.ToString("HH:mm:ss.fff");

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
        
        var msg = FormatMessage(logEntry);
        
        if (logEntry.Exception == null)
            textWriter.WriteLine($"{time} {color}{severity}{resetColor} {msg}");
        else
        {
            textWriter.WriteLine($"{time} {color}{severity}{resetColor} {msg}");
            textWriter.WriteLine();
            textWriter.WriteLine(logEntry.Exception);
            textWriter.WriteLine();
        }
    }

    private static string FormatMessage<TState>(LogEntry<TState> logEntry)
    {
        if (logEntry.State is IReadOnlyCollection<KeyValuePair<string, object>> collection)
        {
            var values = new Dictionary<string, object>(collection);
            if (values.TryGetValue("{OriginalFormat}", out var templateObj))
            {
                var template = templateObj.ToString() ?? "";
                foreach (var (key, value) in values)
                {
                    if (key == "{OriginalFormat}")
                        continue;

                    var str = $"\u001b[1m\u001b[36m{value.ToString()}\u001b[39m\u001b[22m";
                    template = template.Replace("{" + key + "}", str);
                }

                return template;
            }
        }

        return logEntry.Formatter(logEntry.State, logEntry.Exception);
    }
}