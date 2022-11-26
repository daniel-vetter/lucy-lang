using System;
using System.Collections.Generic;
using System.IO;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public class SummaryGraphExport
{
    private int _counter;
    private readonly string _outputDirectory;

    public SummaryGraphExport(string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    internal void Export(QueryExectionLog log)
    {
        var graph = WriteGraph(log);
        var path = Path.Combine(_outputDirectory, $"{++_counter} {log.RootEntry.Query.GetType().Name} {Math.Round(log.TotalDuration.TotalMilliseconds, 2)} Summary.dot");
        File.WriteAllText(path, graph);
    }

    private class SummaryNode
    {
        public string Name { get; set; } = "";
        public int ExecutionCountWithChange { get; set; }
        public int ExecutionCountWithoutChange { get; set; }
        public TimeSpan ExclusiveDuration { get; set; } = TimeSpan.Zero;
    }

    private class DependencyDetails
    {
        public int Count { get; set; }
    }

    private string WriteGraph(QueryExectionLog log)
    {
        var d = new GraphvizDiagram
        {
            NodeFontName = "Consolas",
            EdgeFontName = "Consolas"
        };

        Dictionary<Type, SummaryNode> summaryNodes = new();
        Dictionary<(SummaryNode, SummaryNode), DependencyDetails> dependencies = new();

        foreach(var entry in log.GetAllEntries())
        {
            if (!summaryNodes.TryGetValue(entry.Query.GetType(), out var summaryNode))
            {
                summaryNode = new SummaryNode();
                summaryNode.Name = entry.Query.GetType().Name;
                summaryNodes.Add(entry.Query.GetType(), summaryNode);
            }

            if (entry.Calculation != null)
            {
                if (entry.Calculation.ResultType == ResultType.WasTheSame)
                    summaryNode.ExecutionCountWithChange++;
                else
                    summaryNode.ExecutionCountWithoutChange++;

                summaryNode.ExclusiveDuration += entry.Calculation.ExlusiveHandlerExecutionTime;
            }
        }

        foreach (var entry in log.GetAllEntries())
        {
            foreach(var dependency in entry.Dependencies)
            {
                var key = (summaryNodes[entry.Query.GetType()], summaryNodes[dependency.Query.GetType()]);
                if (!dependencies.TryGetValue(key, out var dependencyDetails))
                {
                    dependencyDetails = new DependencyDetails();
                    dependencies.Add(key, dependencyDetails);
                }

                dependencyDetails.Count++;
            }
        }

        foreach (var node in summaryNodes.Values)
            CreateNode(d, node);

        foreach (var ((from, to), _) in dependencies)
            d.CreateEdgeFor(from, to);

        return d.Build();
    }

    private void CreateNode(GraphvizDiagram d, SummaryNode summaryNode)
    {
        var node = d.CreateNodeFor(summaryNode);
        node.NodeShape = "rectangle";
        var nodeTitle = summaryNode.Name;
        if (nodeTitle.EndsWith("Input"))
            nodeTitle = nodeTitle[..^"Input".Length];

        var label = new KeyValueTable(nodeTitle);
        label.Set("Execution count with change", summaryNode.ExecutionCountWithChange.ToString());
        label.Set("Execution count without change", summaryNode.ExecutionCountWithoutChange.ToString());
        label.Set("Execution time", summaryNode.ExclusiveDuration.TotalMilliseconds + "ms");
        if (summaryNode.ExecutionCountWithChange > 0)
        {
            node.Color = "#008000";
            node.FontColor = "#008000";
            node.FillColor = "#00800010";
        }
        else if (summaryNode.ExecutionCountWithoutChange > 0)
        {
            node.Color = "#808000";
            node.FontColor = "#808000";
            node.FillColor = "#80800010";
        }
        node.Style = "filled";
        node.Label = label;
    }
}