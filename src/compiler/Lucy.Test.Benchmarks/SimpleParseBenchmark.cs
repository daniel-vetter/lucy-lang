using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Lucy.Core.Model;
using Lucy.Core.Parsing;
// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Lucy.Test.Benchmarks
{
    public static class Program
    {


        public static void Main()
        {
            BenchmarkRunner.Run<SimpleParseBenchmark>();
        }
    }

    [MemoryDiagnoser]
    [RPlotExporter]
    public class SimpleParseBenchmark
    {
        [Params(1, 10, 100, 1000, 10000)]
        public int MethodCount;

        private string _code = null!;

        [GlobalSetup]
        public void Setup()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < MethodCount; i++)
            {
                sb.Append($$"""
                fun method{{i + 1}}(): void {
                    method{{i + 1}}()
                    var v = method{{i + 1}}()
                }
                
                """);
            }

            _code = sb.ToString();
        }

        [Benchmark]
        public DocumentRootSyntaxNode InitialParse()
        {
            return Parser.Parse("/doc", _code);
        }
    }

    [MemoryDiagnoser]
    [RPlotExporter]
    public class SimpleBuildBenchmark
    {
        [Params(1, 10, 100, 1000, 10000)]
        public int MethodCount;

        private DocumentRootSyntaxNode _tree = null!;

        [GlobalSetup]
        public void Setup()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < MethodCount; i++)
            {
                sb.Append($$"""
                fun method{{i + 1}}(): void {
                    method{{i + 1}}()
                    var v = method{{i + 1}}()
                }
                
                """);
            }

            _tree = Parser.Parse("/doc", sb.ToString());
        }

        [Benchmark]
        public DocumentRootSyntaxNode SimpleParse()
        {

            return _tree;
        }
    }
}