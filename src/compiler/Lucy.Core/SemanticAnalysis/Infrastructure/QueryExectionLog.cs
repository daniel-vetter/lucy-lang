using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucy.Core.SemanticAnalysis.Infrastructure
{
    public class QueryExectionLog
    {
        public QueryExectionLog(TimeSpan totalDuration, RecordedEntry rootEntry, ImmutableArray<RecordedCalculation> calculations)
        {
            TotalDuration = totalDuration;
            RootEntry = rootEntry;
            Calculations = calculations;
        }

        public TimeSpan TotalDuration { get; }
        public RecordedEntry RootEntry { get; }
        public ImmutableArray<RecordedCalculation> Calculations { get; }

        public ImmutableArray<RecordedEntry> GetAllEntries()
        {
            var hash = new HashSet<RecordedEntry>();
            void Walk(RecordedEntry entry)
            {
                hash.Add(entry);
                foreach (var dep in entry.Dependencies)
                    Walk(dep);
            }
            Walk(RootEntry);
            return hash.ToImmutableArray();
        }
    }

    public class RecordedEntry
    {
        public RecordedEntry(IQuery query, object? result, bool isInput, ImmutableArray<RecordedEntry> dependencies, RecordedCalculation? calculation)
        {
            Query = query;
            Result = result;
            IsInput = isInput;
            Dependencies = dependencies;
            Calculation = calculation;
        }

        public IQuery Query { get; }
        public object? Result { get; }
        public bool IsInput { get; }
        public ImmutableArray<RecordedEntry> Dependencies { get; }
        public RecordedCalculation? Calculation { get; }
    }

    public class RecordedCalculation
    {
        public RecordedCalculation(int index, IQuery query, TimeSpan exlusiveHandlerExecutionTime, TimeSpan inclusiveHandlerExecutionTime, TimeSpan overheadExecutionTime, ResultType resultType)
        {
            Index = index;
            Query = query;
            ExlusiveHandlerExecutionTime = exlusiveHandlerExecutionTime;
            InclusiveHandlerExecutionTime = inclusiveHandlerExecutionTime;
            OverheadExecutionTime = overheadExecutionTime;
            ResultType = resultType;
        }

        public int Index { get; }
        public IQuery Query { get; }
        public TimeSpan ExlusiveHandlerExecutionTime { get; }
        public TimeSpan InclusiveHandlerExecutionTime { get; }
        public TimeSpan OverheadExecutionTime { get; }
        public ResultType ResultType { get; }
    }

    public enum ResultType
    {
        InitialCalculation,
        WasTheSame,
        HasChanged
    }
}
