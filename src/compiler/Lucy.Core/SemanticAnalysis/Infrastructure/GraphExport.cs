using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Infrastructure
{
    public enum GraphExportMode
    {
        ByType,
        ByQuery
    }

    public class GraphExport
    {
        private Dictionary<IQuery, CalculationStats> _calculatedQueries = new();
        private Dictionary<Dependency, DependencyStats> _dependencies = new();
        private Dictionary<IQuery, InputStats> _inputs = new();
        private int _counter;
        private readonly string _outputDirectory;
        private Stopwatch _lastQueryStopwatch = new Stopwatch();

        private record Dependency(IQuery? From, IQuery To);

        private class CalculationStats
        {
            public int Index { get; set; } = -1;
            public ResultType ResultType { get; set; } = ResultType.WasTheSame;
            public TimeSpan InclusiveHandlerExecutionTime { get; set; } = TimeSpan.Zero;
            public TimeSpan ExclusiveHandlerExecutionTime { get; set; } = TimeSpan.Zero;
            public TimeSpan OverheadExecutionTime { get; set; } = TimeSpan.Zero;
        }

        private class DependencyStats
        {
            public int CallCount { get; set; } = 0;
        }

        private class InputStats
        {
            public bool Changed { get; set; }
            public bool Removed { get; set; }
        }

        public GraphExport(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            _outputDirectory = outputDirectory;
        }

        public void ProcessDbEvent(IDbEvent @event)
        {
            if (@event is QueryReceived queryReceived)
            {
                if (queryReceived.ParentQuery == null)
                    _lastQueryStopwatch.Restart();
            }

            if (@event is QueryAnswered queryAnsweredEvent)
            {
                var key = new Dependency(queryAnsweredEvent.ParentQuery, queryAnsweredEvent.Query);
                _dependencies.TryAdd(key, new DependencyStats());
                _dependencies[key].CallCount++;

                if (queryAnsweredEvent.ParentQuery == null)
                {
                    _lastQueryStopwatch.Stop();
                    Flush();
                }
            }

            if (@event is CalculationStarted cs)
            {
                _calculatedQueries.Add(cs.Query, new CalculationStats
                {
                    Index = _calculatedQueries.Count,
                });
            }

            if (@event is CalculationFinished cf)
            {
                var entry = _calculatedQueries[cf.Query];
                entry.ResultType = cf.ResultType;
                entry.ExclusiveHandlerExecutionTime = cf.ExlusiveHandlerExecutionTime;
                entry.InclusiveHandlerExecutionTime = cf.InclusiveHandlerExecutionTime;
                entry.OverheadExecutionTime = cf.OverheadExecutionTime;

            }

            if (@event is InputWasChanged ic)
            {
                _inputs.TryAdd(ic.Query, new InputStats());
                _inputs[ic.Query].Changed = true;
                _inputs[ic.Query].Removed = false;
            }

            if (@event is InputWasRemoved ir)
            {
                _inputs.TryAdd(ir.Query, new InputStats());
                _inputs[ir.Query].Changed = false;
                _inputs[ir.Query].Removed = true;
            }
        }

        private void Flush()
        {
            var d = new GraphvizDiagram();
            d.NodeFontName = "Consolas";
            d.EdgeFontName = "Consolas";

            foreach (var query in _dependencies.Keys.SelectMany(x => new[] { x.From, x.To }))
            {
                if (d.HasNodeFor(query))
                    continue;

                CreateNode(d, query);
            }

            foreach (var ((from, to), stats) in _dependencies)
            {
                var edge = d.CreateEdgeFor(from, to);
                if (stats.CallCount == 0)
                    edge.Style = "dotted";
            }

            var fileContent = d.Build();
            File.WriteAllText(Path.Combine(_outputDirectory, "_last.dot"), fileContent);
            File.WriteAllText(Path.Combine(_outputDirectory, ++_counter + ".dot"), fileContent);

            _calculatedQueries.Clear();
            foreach (var stats in _dependencies.Values)
                stats.CallCount = 0;
            foreach (var stats in _inputs.Values)
                stats.Changed = false;
        }

        private void CreateNode(GraphvizDiagram d, IQuery? query)
        {
            var node = d.CreateNodeFor(query);
            node.NodeShape = "rectangle";
            if (query == null)
            {
                var rootData = new KeyValueTable("root");
                rootData.Set("Total duration", _lastQueryStopwatch.Elapsed.TotalMilliseconds + "ms");
                node.Label = rootData;
                return;
            }

            var label = new KeyValueTable(query.GetType().Name ?? "root");
            if (_calculatedQueries.TryGetValue(query, out var calculationStats))
            {
                label.Set("Execution", "#" + (calculationStats.Index + 1).ToString() + ", Calc: " + calculationStats.ExclusiveHandlerExecutionTime.TotalMilliseconds + "ms, " + calculationStats.InclusiveHandlerExecutionTime.TotalMilliseconds + "ms, Equal: " + calculationStats.OverheadExecutionTime.TotalMilliseconds + "ms");
                if (calculationStats.ResultType is ResultType.InitialCalculation or ResultType.HasChanged)
                {
                    node.Color = "#008000";
                    node.FontColor = "#008000";
                    node.FillColor = "#00800010";
                }
                else
                {
                    node.Color = "#808000";
                    node.FontColor = "#808000";
                    node.FillColor = "#80800010";
                }
                node.Style = "filled";
            }
            else if (_inputs.TryGetValue(query, out var inputStats))
            {
                node.Color = "#A0A0ff";
                node.FontColor = "#A0A0ff";
                node.Style = "filled";
                node.FillColor = "#8080FF10";

                if (inputStats.Removed)
                {
                    node.Color = "#FF0000";
                    node.FontColor = "#FF0000";
                }
                else if (inputStats.Changed)
                {
                    node.Color = "#0000FF";
                    node.FontColor = "#0000FF";
                }
            }

            foreach (var props in query.GetType().GetProperties())
            {
                label.Set(props.Name, props.GetValue(query)?.ToString() ?? "");
            }
            node.Label = label;
        }
    }
}
