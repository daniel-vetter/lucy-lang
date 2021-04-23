using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Disassembler.Infrastructure.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text;

namespace Disassembler.Assembling
{
    public class Assembler
    {
        private readonly List<Operation> _ops = new();

        public Assembler(OperandSize defaultOperandSize)
        {
            DefaultOperandSize = defaultOperandSize;
        }

        public void Add(Operation operation) => _ops.Add(operation);

        public AssemblerResult Process()
        {
            var memoryBlock = new MemoryBlock();
            var writer = new MachineCodeWriter(DefaultOperandSize, memoryBlock);

            foreach (var op in _ops)
                op.Write(writer);

            memoryBlock.Address = 0;
            return new AssemblerResult(memoryBlock, writer.Issues);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var op in _ops)
                sb.AppendLine(op.ToString());
            return sb.ToString();
        }

        public ImmutableArray<Operation> Operations => _ops.ToImmutableArray();

        public OperandSize DefaultOperandSize { get; }
    }

    public class AssemblerResult
    {
        public AssemblerResult(MemoryBlock data, ImmutableArray<AssemblerIssue> issues)
        {
            Data = data;
            Issues = issues;
        }

        public MemoryBlock Data { get; }
        public ImmutableArray<AssemblerIssue> Issues { get; }
    }

    public record AssemblerIssue(AssemblerIssueSeverity Severity, string Message);

    public enum AssemblerIssueSeverity
    {
        Warning,
        Error
    }

    public class AssemblerException : Exception
    {
        public AssemblerException()
        {
        }

        public AssemblerException(string message) : base(message)
        {
        }

        public AssemblerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AssemblerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
