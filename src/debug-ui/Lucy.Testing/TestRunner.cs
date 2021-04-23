using Lucy.Testing.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lucy.Testing
{
    public static partial class TestRunner
    {
        private static Task? _running;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(10);

        public static IDisposable OnProgress(Action<TestProgress> onProgress) => Progress.OnProgress(onProgress);

        public static void Run()
        {
            if (_running != null)
                throw new Exception("TestRunner is already running");

            _running = Task.Run(async () =>
            {
                try
                {
                    Progress.Publish(new TestRunStarted());
                    await RunTests();
                    Progress.Publish(new TestRunCompleted(null));
                }
                catch (Exception e)
                {
                    Progress.Publish(new TestRunCompleted(e.Message));
                }
                _running = null;
            });
        }

        private static async Task RunTests()
        {
            Progress.Publish(new CompilingCompiler());
            await Compiler.CompileCurrentCompiler();
            Progress.Publish(new DiscoveringTests());
            var testCases = await TestCaseDiscovery.FindTestCases();
            Progress.Publish(new RunningTests(testCases));

            var tasks = new List<Task>();

            foreach (var test in testCases)
                tasks.Add(Run(test));

            await Task.WhenAll(tasks);
        }

        private static async Task Run(TestCase test)
        {
            await _semaphore.WaitAsync();
            Progress.Publish(new TestStarted(test));

            try
            {
                var result = await Compiler.Run(
                    mainFile: "main.lucy",
                    workingDirectory: test.Directory
                );

                var testConditions = ImmutableArray.CreateBuilder<TestResult>();

                if (test.ExpectedErrorMessages.Any())
                {
                    var errorMessages = result.ErrorOutput.Split("\n").Select(x => x.Trim()).ToArray();
                    var missingErrorMessages = test.ExpectedErrorMessages.Where(x => !errorMessages.Contains(x)).ToArray();

                    if (missingErrorMessages.Any())
                        testConditions.Add(new TestResult("Compilation", "Missing error Messages:\n" + string.Join("\n", missingErrorMessages.Select(x => " - " + x).ToArray()) + "\n\n"));
                    else
                        testConditions.Add(new TestResult("Compilation", null));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(result.ErrorOutput))
                        testConditions.Add(new TestResult("Compilation",  null));
                    else
                        testConditions.Add(new TestResult("Compilation", "Compiler wrote to error stream:\n-----------------------\n" + result.ErrorOutput + "\n-----------------------\n"));
                }
                
                if (string.IsNullOrWhiteSpace(result.ErrorOutput))
                {
                    if (test.ExpectedOutput != null && result.StandardOutput != test.ExpectedOutput)
                    {
                        testConditions.Add(new TestResult("Runtime", "Expected output did not match. Expected:\n-----------------------\n" + test.ExpectedOutput + "\n-----------------------\n\nGot:\n-----------------------\n" + result.StandardOutput + "\n-----------------------\n"));
                    }
                    else
                    {
                        testConditions.Add(new TestResult("Runtime", null));
                    }
                }
                
                Progress.Publish(new TestCompleted(test, testConditions.ToImmutable()));
            }
            catch (Exception e)
            {
                Progress.Publish(new TestCompleted(test, ImmutableArray.Create(new TestResult("Test runner", "Test runner crashed:\n\n" + e.Message))));
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public abstract record TestProgress();
    public record TestRunStarted() : TestProgress;
    public record CompilingCompiler : TestProgress;
    public record DiscoveringTests : TestProgress;
    public record RunningTests(ImmutableArray<TestCase> TestCases) : TestProgress();
    public record TestStarted(TestCase TestCase) : TestProgress;
    public record TestCompleted(TestCase TestCase, ImmutableArray<TestResult> Results) : TestProgress;
    public record TestResult(string Name, string? Error);
    public record TestRunCompleted(string? Error) : TestProgress();

    public record TestCase(string Name, string Directory, string? ExpectedOutput, ImmutableArray<string> ExpectedErrorMessages, bool ParseOnly);
}
