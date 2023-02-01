using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Infrastructure
{
    public class QueryMetricsExporter : IQueryListener
    {
        private readonly string _path;
        private int _index;

        public QueryMetricsExporter(string path)
        {
            _path = path;
            PrepareDirectory();
        }

        private void PrepareDirectory()
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            foreach (var file in Directory.GetFiles(_path, "QME*.txt"))
                File.Delete(file);
        }

        public void OnQueryExecuted(CacheEngineSnapshot snapshot)
        {
            _ = Task.Run(async () => await Export(++_index, snapshot));
        }

        private async Task Export(int index, CacheEngineSnapshot snapshot)
        {
            var sb = new StringBuilder();
            
            if (snapshot.LastQueryMetrics != null) 
                WriteCalculationsToStringBuilder(snapshot.LastQueryMetrics.Calculations, sb);

            WriteCachedQueriesByTypeToStringBuilder(snapshot.CachedQueriesByType, sb);

            await File.WriteAllTextAsync(GetFileNameFromSnapshot(index, snapshot), sb.ToString());
        }

        private string GetFileNameFromSnapshot(int index, CacheEngineSnapshot snapshot)
        {
            var fileName = $"QME{index:000}";
            if (snapshot.LastQueryMetrics != null)
                fileName += snapshot.LastQueryMetrics.RootQuery.GetType().Name;
            fileName += ".txt";
            fileName = Path.Combine(_path, fileName);
            return fileName;
        }

        private void WriteCachedQueriesByTypeToStringBuilder(ImmutableDictionary<Type,int> cachedQueriesByType, StringBuilder sb)
        {
            var rows = new List<ImmutableArray<string>> {ImmutableArray.Create("Query", "Count")};
            rows.AddRange(cachedQueriesByType
                .OrderByDescending(x => x.Value)
                .Select(x => ImmutableArray.Create(x.Key.Name, x.Value.ToString()))
            );
            sb.AppendLine(ConvertToTable(rows.ToImmutableArray()));
        }

        private static void WriteCalculationsToStringBuilder(ImmutableArray<RecordedCalculation> recordedCalculations, StringBuilder sb)
        {
            var summary = recordedCalculations
                .GroupBy(x => x.Query.GetType().Name)
                .Select(x =>
                {
                    var totalTime = x
                        .Select(y => y.ExecutionTime)
                        .Aggregate(TimeSpan.Zero, (a, b) => a + b)
                        .TotalMilliseconds;

                    var totalCount = x.Count();
                    var initCount = x.Count(y => y.ResultType == ResultType.InitialCalculation);
                    var changedCount = x.Count(y => y.ResultType == ResultType.HasChanged);
                    var sameCount = x.Count(y => y.ResultType == ResultType.WasTheSame);

                    return new
                    {
                        Type = x.Key,
                        Count = totalCount,
                        InitCount = initCount,
                        ChangedResultCount = changedCount,
                        SameCount = sameCount,
                        TotalTime = totalTime,
                        AvgTime = totalTime / totalCount
                    };
                })
                .OrderByDescending(x => x.TotalTime)
                .ToArray();

            var rows = new List<ImmutableArray<string>> {ImmutableArray.Create("Query", "Count", "I", "C", "S", "Total Time", "Avg. Time")};
            foreach (var entry in summary)
            {
                rows.Add(ImmutableArray.Create(
                    entry.Type,
                    entry.Count.ToString(CultureInfo.InvariantCulture),
                    entry.InitCount.ToString(CultureInfo.InvariantCulture),
                    entry.ChangedResultCount.ToString(CultureInfo.InvariantCulture),
                    entry.SameCount.ToString(CultureInfo.InvariantCulture),
                    entry.TotalTime.ToString(CultureInfo.InvariantCulture),
                    entry.AvgTime.ToString(CultureInfo.InvariantCulture)
                ));
            }

            sb.AppendLine(ConvertToTable(rows.ToImmutableArray()));
            sb.AppendLine();
        }

        private static string ConvertToTable(ImmutableArray<ImmutableArray<string>> rows, bool addDivider = true)
        {
            static int[] GetMaxWidthOfEachColumn(ImmutableArray<ImmutableArray<string>> rows)
            {
                var widths = new List<int>();
                for (var i = 0; i < rows[0].Length; i++)
                    widths.Add(0);

                foreach (var row in rows)
                {
                    for (var i = 0; i < rows[0].Length; i++)
                        if (widths[i] < row[i].Length)
                            widths[i] = row[i].Length;
                }

                return widths.ToArray();
            }

            var widths = GetMaxWidthOfEachColumn(rows);

            if (addDivider)
                rows = rows.Insert(1, widths.Select(x => new string('-', x)).ToImmutableArray());

            var sb = new StringBuilder();
            foreach (var row in rows)
            {
                for (var colIndex = 0; colIndex < row.Length; colIndex++)
                {
                    var val = row[colIndex];
                    sb.Append(val.PadRight(widths[colIndex]));
                    if (colIndex < row.Length - 1)
                        sb.Append(addDivider ? " | " : " ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}