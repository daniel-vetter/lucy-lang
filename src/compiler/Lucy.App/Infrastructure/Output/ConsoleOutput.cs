using System;
using System.IO;
using Lucy.Common.ServiceDiscovery;
using Newtonsoft.Json;

namespace Lucy.App.Infrastructure.Output
{
    internal interface IOutput
    {
        InputOutput GetInputOutputStreamsExclusive();
        bool JsonMode { get; set; }
        void WriteLine(OutputSeverity outputSeverity, string text);
    }

    internal static class OutputEx
    {
        internal static void WriteLine(this IOutput output, OutputSeverity outputSeverity, string text) => output.WriteLine(outputSeverity, text);
        internal static void WriteDebugLine(this IOutput output, string text) => output.WriteLine(OutputSeverity.Debug, text);
        internal static void WriteInfoLine(this IOutput output, string text) => output.WriteLine(OutputSeverity.Info, text);
        internal static void WriteWarningLine(this IOutput output, string text) => output.WriteLine(OutputSeverity.Warning, text);
        internal static void WriteErrorLine(this IOutput output, string text) => output.WriteLine(OutputSeverity.Error, text);
    }

    public enum OutputSeverity
    {
        Debug,
        Info,
        Warning,
        Error
    }

    [Service]
    internal class ConsoleOutput : IOutput
    {
        private readonly ConsoleColor _defaultColor;
        private bool _disabled;

        public ConsoleOutput()
        {
            _defaultColor = Console.ForegroundColor;
        }

        public InputOutput GetInputOutputStreamsExclusive()
        {
            if (_disabled)
                throw new Exception("IO stream a already locked by someone else");

            _disabled = true;
            return new InputOutput(Console.OpenStandardInput(), Console.OpenStandardOutput(), () => _disabled = false);
        }

        public bool JsonMode { get; set; }

        public void WriteLine(OutputSeverity outputSeverity, string text)
        {
            if (_disabled)
                return;

            var output = outputSeverity == OutputSeverity.Error && JsonMode == false
                ? Console.Error
                : Console.Out;

            if (JsonMode)
            {
                output.WriteLine(JsonConvert.SerializeObject(new 
                {
                    severity = outputSeverity.ToString().ToLowerInvariant(),
                    message = text
                }));
                return;
            }

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = outputSeverity switch
            {
                OutputSeverity.Debug => ConsoleColor.Gray,
                OutputSeverity.Info => _defaultColor,
                OutputSeverity.Warning => ConsoleColor.Yellow,
                OutputSeverity.Error => ConsoleColor.Red,
                _ => throw new Exception("Unsupported output severity: " + outputSeverity)
            };

            output.WriteLine(text);

            Console.ForegroundColor = oldColor;
        }
    }

    public class InputOutput : IDisposable
    {
        private readonly Action _onDispose;

        public InputOutput(Stream input, Stream output, Action onDispose)
        {
            Input = input;
            Output = output;
            _onDispose = onDispose;
        }

        public Stream Input { get; }
        public Stream Output { get; }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
