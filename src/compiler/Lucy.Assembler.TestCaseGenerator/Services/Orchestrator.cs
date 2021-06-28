using Spectre.Console;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lucy.Assembler.TestCaseGenerator.Services
{
    public static class Orchestrator
    {
        public static async Task Run(string operation, int operandCount, int bits, bool writeSqliteDb)
        {
            var generatorQueue = Channel.CreateBounded<StatementTest>(1_000_000);
            var processorQueue = Channel.CreateBounded<StatementTest>(1_000_000);

            var tracker = new Tracker();
            var sw = Stopwatch.StartNew();

            var cts = new CancellationTokenSource();
            Task display = ShowStatus(generatorQueue, processorQueue, tracker, cts.Token);

            var generator = TestCaseGenerator.GenerateTestCases(generatorQueue.Writer, operation, operandCount);
            var processor = TestCaseProcessor.Run(generatorQueue.Reader, processorQueue.Writer, bits);
            var writer = TestCaseWriter.Run(processorQueue.Reader, tracker, bits, operation, writeSqliteDb);
            await Task.WhenAll(generator, processor, writer, processorQueue.Reader.Completion);

            cts.Cancel();
            await display;
            AnsiConsole.MarkupLine($"  [green]Success. Took: {sw.Elapsed}[/]");
        }

        private static Task ShowStatus(Channel<StatementTest> generatorQueue, Channel<StatementTest> processorQueue, Tracker tracker, CancellationToken ct)
        {
            return AnsiConsole.Live(new Table()).StartAsync(async ctx =>
            {
                void Display()
                {
                    var table = new Table();
                    table.Border(TableBorder.Simple);
                    table.HideHeaders();
                    table.AddColumns("Queue", "Count");
                    table.AddRow("Buffered test cases", generatorQueue.Reader.Count.ToString("n0").PadLeft(10));
                    table.AddRow("Buffered write jobs", processorQueue.Reader.Count.ToString("n0").PadLeft(10));
                    table.AddRow("Written", tracker.Done.ToString("n0").PadLeft(10));
                    ctx.UpdateTarget(table);
                    ctx.Refresh();
                }

                while (!ct.IsCancellationRequested)
                {
                    Display();
                    await Task.Delay((int)(1000 / 30.0), ct).ContinueWith(_ => { });
                }
                Display();
            });
        }
    }


    public record StatementTestBatch(ImmutableArray<StatementTest> Tests, int Bits);
}
