using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Infrasturcture
{
    public enum GraphExportMode
    {
        ByType,
        ByQuery
    }

    public class GraphExport
    {
        private Dictionary<IQuery, object> _calculatedQueries = new Dictionary<IQuery, object>();
        private Dictionary<Dependency, DependencyStats> _dependencies = new();
        private Dictionary<IQuery, InputStats> _inputs = new();
        private readonly string _outputDirectory;

        private record Dependency(IQuery? From, IQuery To);

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
            if (@event is QueryAnswered queryAnsweredEvent)
            {
                var key = new Dependency(queryAnsweredEvent.ParentQuery, queryAnsweredEvent.Query);
                _dependencies.TryAdd(key, new DependencyStats());
                _dependencies[key].CallCount++;

                if (queryAnsweredEvent.ParentQuery == null)
                    Flush();
            }

            if (@event is CalculationFinished cf)
            {
                _calculatedQueries.Add(cf.Query, new object());
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

                CretaeNode(d, query);
            }

            foreach (var ((from, to), stats) in _dependencies)
            {
                var edge = d.CreateEdgeFor(from, to);
                if (stats.CallCount == 0)
                    edge.Style = "dotted";
            }

            var fileContent = d.Build();
            File.WriteAllText(Path.Combine(_outputDirectory, "_last.dot"), fileContent);
            File.WriteAllText(Path.Combine(_outputDirectory, DateTime.Now.ToString("O").Replace(":", "-") + ".dot"), fileContent);

            _calculatedQueries.Clear();
            foreach (var stats in _dependencies.Values)
                stats.CallCount = 0;
            foreach (var stats in _inputs.Values)
                stats.Changed = false;
        }

        private void CretaeNode(GraphvizDiagram d, IQuery? query)
        {
            var node = d.CreateNodeFor(query);
            node.NodeShape = "rectangle";
            if (query == null)
            {
                node.Label = "root";
                return;
            }
            
            var label = new KeyValueTable(query.GetType().Name ?? "root");
            foreach(var props in query.GetType().GetProperties())
            {
                label.Set(props.Name, props.GetValue(query)?.ToString() ?? "");
            }
            node.Label = label;

            if (_calculatedQueries.ContainsKey(query))
            {
                node.Color = "#008000";
                node.FontColor = "#008000";
                node.Style = "filled";
                node.FillColor = "#00800010";
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

        }
    }
}
