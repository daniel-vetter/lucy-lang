using Lucy.Assembler.Operations;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class StatementReader
    {
        public static ImmutableArray<AssemblerStatement> ReadStatementList(this AsmReader reader)
        {
            var l = ImmutableArray.CreateBuilder<AssemblerStatement>();

            while (true)
            {
                if (reader.TryReadStatement(out var statement))
                {
                    l.Add(statement);
                    continue;
                }

                reader.TryReadWhitespace();
                if (reader.IsDone)
                    break;

                throw new Exception("Could not parse: " + reader.Context);
            }

            return l.ToImmutable();
        }

        public static bool TryReadRemaining(this AsmReader reader, [NotNullWhen(true)] out string? remaining)
        {
            using var t = reader.BeginTransaction();
            reader.TryReadWhitespace();

            var len = 0;
            while (reader.Peek(len) != '\0')
                len++;
                
            if (len == 0)
            {
                remaining = null;
                return false;
            }

            remaining = reader.Read(len);
            return true;
            
        }

        public static bool TryReadStatement(this AsmReader reader, [NotNullWhen(true)] out AssemblerStatement? result)
        {
            using var t = reader.BeginTransaction();
            result = null;

            if (reader.TryReadLabel(out var label))
            {
                t.Commit();
                result = label;
                return true;
            }

            if (reader.TryReadKeyword("mov") && reader.TryReadTwoOperands(out var movOp1, out var movOp2))
            {
                t.Commit();
                result = new Mov(movOp1, movOp2);
                return true;
            }

            if (reader.TryReadKeyword("add") && reader.TryReadTwoOperands(out var addOp1, out var addOp2))
            {
                t.Commit();
                result = new Add(addOp1, addOp2);
                return true;
            }

            if (reader.TryReadKeyword("sub") && reader.TryReadTwoOperands(out var subOp1, out var subOp2))
            {
                t.Commit();
                result = new Add(subOp1, subOp2);
                return true;
            }

            return false;
        }
    }
}
