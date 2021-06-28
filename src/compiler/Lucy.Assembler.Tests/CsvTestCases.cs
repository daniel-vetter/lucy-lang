using CsvHelper;
using Lucy.Assembler.ContainerFormats.Flat;
using Lucy.Assembler.Parsing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.Assembler.Tests
{
    public class CsvTestCases
    {
        [TestCaseSource(nameof(GetTestCases))][Ignore("No csv files provided yet")]
        public async Task RunCsvTests(CsvTestCaseFile file)
        {
            var results = await RunTestCaseFile(file);
            ValidateResults(results);
        }

        private void ValidateResults(ImmutableArray<CsvTestResult> results)
        {
            if (results.Length == 0)
                Assert.Fail("Not tests found.");

            var mismatches = results.Where(x => x.ExpectedResult != x.ReceivedResult).ToArray();

            if (mismatches.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{mismatches.Length} of {results.Length} statements did not result in the expected binary result:");
                sb.AppendLine();
                foreach (var mismatch in mismatches)
                    sb.AppendLine($"{mismatch.Operation,-50} Expected: {mismatch.ExpectedResult,-20} Received: {mismatch.ReceivedResult,-20}");
                
                Assert.Fail(sb.ToString());
            }

            TestContext.WriteLine(results.Length + " tests succeeded.");
        }

        private static async Task<ImmutableArray<CsvTestResult>> RunTestCaseFile(CsvTestCaseFile file)
        {
            using var streamReader = new StreamReader(file.Path);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            var testResults = ImmutableArray.CreateBuilder<CsvTestResult>();
            await foreach (var testCase in csvReader.GetRecordsAsync<CsvTestCase>())
            {
                var module = AsmParser.Parse("label: \n" + testCase.Operation, OperandSize.FromBits(file.Bits));

                var resultBytes = FlatBinaryBuilder.Build(module, OperandSize.FromBits(file.Bits));
                var resultHex = BitConverter.ToString(resultBytes).Replace("-", "");

                testResults.Add(new CsvTestResult(testCase.Operation, testCase.Binary, resultHex));
            }
            return testResults.ToImmutable();
        }

        private static IEnumerable<TestCaseData> GetTestCases()
        {
            var files = TestCaseDataHelper.GetPaths("*.csv");

            foreach(var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);

                ushort bits;
                if (name.EndsWith("16")) bits = 16;
                else if (name.EndsWith("32")) bits = 32;
                else if (name.EndsWith("64")) bits = 64;
                else throw new Exception("Invalid csv test case name: " + name);

                var op = name.Substring(0, name.Length - 2);

                yield return new TestCaseData(new CsvTestCaseFile(file, op, bits)).SetName($"{op} - {bits}");
            }
        }
    }

    public record CsvTestCaseFile(string Path, string Operation, ushort Bits);
    public record CsvTestCase(string Operation, string Binary);
    public record CsvTestResult(string Operation, string ExpectedResult, string ReceivedResult);
}
