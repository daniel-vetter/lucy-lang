using Disassembler.Assembling;
using Disassembler.Assembling.Operations;
using Disassembler.ContainerFormats.PE;
using Lucy.Core.Parser.Nodes.Expressions.Unary;
using System.Linq;
using System.Text;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    internal class StringConstantToAssemblerConverter
    {
        internal static void Run(StringConstantExpressionSyntaxNode sc, AsmConvertContext ctx)
        {
            var bytes = Encoding.UTF8.GetBytes(sc.Value).Concat(new byte[] { 0x00 }).ToArray();
            var entry = new StringDataEntry(ctx.Data.EntryCount);

            ctx.Data.Add(bytes, new AddressExport(entry));
            ctx.Assembler.Add(new Mov(Register.EAX, new Immediate(OperandSize.S32, 0, new AddressImport(entry, AddressType.AbsoluteVirtualAddress))));
        }

        private record StringDataEntry(int Index);
    }
}
