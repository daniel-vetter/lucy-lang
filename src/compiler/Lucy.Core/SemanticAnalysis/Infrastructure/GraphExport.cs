using Lucy.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Lucy.Core.SemanticAnalysis.Infrastructure
{
    public class GraphExport
    {
        private Dictionary<IQuery, CalculationStats> _calculatedQueries = new();
        private Dictionary<IQuery, InputStats> _inputs = new();
        private int _counter;
        private readonly string _outputDirectory;
        private Stopwatch _lastQueryStopwatch = new Stopwatch();
        private IQuery? _currentRootQuery;

        private record Dependency(IQuery? From, IQuery To);

        public GraphExport(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            _outputDirectory = outputDirectory;
        }

        private class CalculationStats
        {
            public int Index { get; set; } = -1;
            public ResultType ResultType { get; set; } = ResultType.WasTheSame;
            public TimeSpan InclusiveHandlerExecutionTime { get; set; } = TimeSpan.Zero;
            public TimeSpan ExclusiveHandlerExecutionTime { get; set; } = TimeSpan.Zero;
            public TimeSpan OverheadExecutionTime { get; set; } = TimeSpan.Zero;
        }

        private class InputStats
        {
            public bool Changed { get; set; }
            public bool Removed { get; set; }
        }

        public void ProcessDbEvent(Db db, IDbEvent @event)
        {
            if (@event is QueryReceived { ParentQuery: null } qr)
            {
                _currentRootQuery = qr.Query;
                _lastQueryStopwatch.Restart();
            }

            if (@event is QueryAnswered { ParentQuery: null } qa)
            {
                _lastQueryStopwatch.Stop();
                if (_currentRootQuery != null)
                    WriteGraph(db.GetEntryDetails(_currentRootQuery));
                Reset();
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

        private void WriteGraph(EntryDetails entry)
        {
            var d = new GraphvizDiagram();
            d.NodeFontName = "Consolas";
            d.EdgeFontName = "Consolas";

            void Add(EntryDetails ed)
            {
                foreach(var target in ed.Dependencies)
                {
                    if (!d.HasNodeFor(target.Query))
                        CreateNode(d, target.Query);
                    if (!d.HasEdgeBetween(ed.Query, target.Query))
                        d.CreateEdgeFor(ed.Query, target.Query);

                    Add(target);
                }
            }

            CreateNode(d, entry.Query);
            Add(entry);

            var fileContent = d.Build();
            File.WriteAllText(Path.Combine(_outputDirectory, ++_counter + " " + entry.Query.GetType().Name + Math.Round(_lastQueryStopwatch.Elapsed.TotalMilliseconds,2) + ".dot"), fileContent);
        }

        private void Reset()
        {
            _currentRootQuery = null;
            _calculatedQueries.Clear();
            foreach (var stats in _inputs.Values)
            {
                stats.Changed = false;
                stats.Removed = false;
            }
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

            var nodeTitle = query.GetType().Name;
            if (nodeTitle.EndsWith("Input"))
                nodeTitle = nodeTitle[..^"Input".Length];

            var label = new KeyValueTable(nodeTitle ?? "root");
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
                var value = props.GetValue(query);
                var displayString = value switch
                {
                    null => "",
                    SyntaxTreeNode syntaxTreeNode => syntaxTreeNode.NodeId.ToString(),
                    _ => value.ToString() ?? ""
                };
                
                label.Set(props.Name, displayString);
            }
            node.Label = label;
        }
    }
}
