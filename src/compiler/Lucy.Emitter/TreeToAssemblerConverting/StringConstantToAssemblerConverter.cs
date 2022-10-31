using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Assembler.Operations;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using System.Linq;
using System.Text;

namespace Lucy.Emitter.TreeToAssemblerConverting
{
    internal class StringConstantToAssemblerConverter
    {
        internal static void Run(StringConstantExpressionSyntaxNode sc, WinExecutableEmitterContext ctx)
        {
            var bytes = Encoding.UTF8.GetBytes(sc.Value).Concat(new byte[] { 0x00 }).ToArray();
            var entry = new StringDataEntry(ctx.Data.EntryCount);

            ctx.Data.Add(bytes, entry);
            ctx.Assembler.AddOperation(new Mov(Register.EAX, new Immediate(OperandSize.S32, 0, new AddressImport(entry, AddressType.AbsoluteVirtualAddress))));
        }

        private record StringDataEntry(int Index)
        {
            public override string ToString()
            {
                return $"data_string_{Index}";
            }
        }
    }
}
