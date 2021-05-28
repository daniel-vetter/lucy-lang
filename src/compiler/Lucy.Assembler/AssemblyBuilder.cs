using Lucy.Assembler.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Text;

namespace Lucy.Assembler
{
    public class AssemblyBuilder
    {
        private readonly List<AssemblerStatement> _statements = new();

        public AssemblyBuilder(OperandSize defaultOperandSize)
        {
            DefaultOperandSize = defaultOperandSize;
        }

        public void AddOperation(Operation operation) => _statements.Add(operation);
        public void AddEmptyLine(string? comment) => _statements.Add(new EmptyLine(comment));
        public void AddSpacer()
        {
            if (_statements.Count == 0)
                return;

            if (_statements[_statements.Count - 1] is EmptyLine el && el.Comment == null)
                return;

            _statements.Add(new EmptyLine(null));
        }

        public void AddLabel(object key, string? comment = null)
        {
            _statements.Add(new Label(key, comment));
        }

        public AssemblerResult Process()
        {
            var memoryBlock = new MemoryBlock();
            var writer = new MachineCodeWriter(DefaultOperandSize, memoryBlock);

            foreach (var statement in _statements)
            {
                if (statement is Operation op)
                    op.Write(writer);
                else if (statement is Label lb)
                    writer.WriteAnnotaton(lb.Key);
            }
            
            var issues = writer.Issues;
            if (_statements.Count == 0)
                issues = issues.Add(new AssemblerIssue(AssemblerIssueSeverity.Warning, "Operation list is empty"));

            memoryBlock.Address = 0;
            return new AssemblerResult(memoryBlock, issues);
        }

        public string CreateAssemblerCode()
        {
            var writer = new AssemblyWriter();
            foreach (var statement in _statements)
                statement.Write(writer);

            return writer.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var op in _statements)
                sb.AppendLine(op.ToString());
            return sb.ToString();
        }

        public ImmutableArray<AssemblerStatement> Statements => _statements.ToImmutableArray();

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

    public record EntryPointAnnotation();
}
