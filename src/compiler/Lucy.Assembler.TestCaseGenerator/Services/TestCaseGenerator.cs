using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lucy.Assembler.TestCaseGenerator.Services
{
    public static class TestCaseGenerator
    {
        public static async Task GenerateTestCases(ChannelWriter<StatementTest> target, string operation, int operandCount)
        {
            if (operandCount != 1 && operandCount != 2)
                throw new ArgumentException("Invalid operand count", nameof(operandCount));

            var operands = await Task.Run(() => GenerateOperands().ToArray());
            await Task.Run(async () => await Generate(target, operation, operandCount, operands));
            target.Complete();
        }

        private static async Task Generate(ChannelWriter<StatementTest> target, string operation, int operandCount, Operand[] operands)
        {
            if (operandCount == 1)
            {
                foreach (var operand in operands)
                {
                    await target.WriteAsync(new StatementTest($"{operand} {operand.Text}", 32, null, new List<string>()));
                }
            }

            if (operandCount == 2)
            {
                foreach (var op1 in operands)
                {
                    foreach (var op2 in operands)
                        if (op1.IsMemoryReference == false || op2.IsMemoryReference == false)
                        {
                            await target.WriteAsync(new StatementTest($"{operation} {op1.Text}, {op2.Text}", 32, null, new List<string>()));
                        }
                }
            }
        }

        private static IEnumerable<Operand> GenerateOperands()
        {
            foreach (Register? baseReg in Register.All)
                yield return new Operand($"{baseReg.Name}", false);

            

            foreach (var size in new[] { "byte ", "word ", "dword ", "qword " })
            {
                yield return new Operand($"{size}label", false);
                yield return new Operand($"{size}[label]", true);

                foreach (var n in new[] { sbyte.MaxValue, short.MaxValue, int.MaxValue })
                    yield return new Operand($"{size}[{n}]", true);

                foreach (Register? baseReg in Register.All)
                {
                    foreach (Register? indexReg in Register.FromOperandSize(baseReg.Size).Prepend(null!))
                    {
                        foreach (var scale in new[] { 1, 2, 4, 8 })
                        {
                            foreach (var p in new[] { 0, sbyte.MaxValue, short.MaxValue, int.MaxValue })
                            {
                                var op = new StringBuilder();
                                var type = new StringBuilder();
                                type.Append("memory");
                                op.Append(size);
                                op.Append('[');
                                op.Append(baseReg.Name);
                                type.Append("_base");

                                if (indexReg != null)
                                {
                                    op.Append(" + " + indexReg.Name);
                                    type.Append("_index");
                                }

                                if (scale != 1)
                                {
                                    op.Append(" * " + scale);
                                    type.Append("_scale");
                                }

                                if (p != 0)
                                {
                                    op.Append(" + " + p);
                                    type.Append("_immediate");
                                }

                                op.Append(']');

                                yield return new Operand(op.ToString(), true);
                            }
                        }
                    }
                }
            }
        }
    }

    public record Operand(string Text, bool IsMemoryReference);
    public record StatementTest(string Text, int Bits, string? Binary, List<string> Errors)
    {
        public string? Binary { get; set; } = Binary;
    }
}
