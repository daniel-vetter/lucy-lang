using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Lucy.Core.SemanticAnalysis.Infrastructure
{
    internal class QueryMetricsExporter
    {
        private readonly string _path;
        private int _index = 0;

        public QueryMetricsExporter(string path)
        {
            _path = path;
            PrepareDirectory();
        }

        private void PrepareDirectory()
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            foreach(var file in Directory.GetFiles(_path, "QME*.txt"))
                File.Delete(file);
        }

        public void Export(QueryMetrics queryMetrics)
        {
            var summary = queryMetrics.Calculations
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

            var rows = new List<ImmutableArray<string>> { ImmutableArray.Create("Query", "Count", "I", "C", "S", "Total Time", "Avg. Time") };
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
            
            File.WriteAllText(Path.Combine(_path, $"QME{(++_index):000}_{queryMetrics.RootQuery.GetType().Name}.txt"), ConvertToTable(rows.ToImmutableArray()));
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
