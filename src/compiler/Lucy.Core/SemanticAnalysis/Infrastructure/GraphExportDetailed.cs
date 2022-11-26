using Lucy.Core.Model;
using System;
using System.IO;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public class DetailedGraphExport
{
    private int _counter;
    private readonly string _outputDirectory;
        
    public DetailedGraphExport(string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    internal void Export(QueryExectionLog log)
    {
        var graph = WriteGraph(log);
        var path = Path.Combine(_outputDirectory, $"{++_counter} {log.RootEntry.Query.GetType().Name} {Math.Round(log.TotalDuration.TotalMilliseconds, 2)} Detailed.dot");
        File.WriteAllText(path, graph);
    }

    private string WriteGraph(QueryExectionLog log)
    {
        var d = new GraphvizDiagram();
        d.NodeFontName = "Consolas";
        d.EdgeFontName = "Consolas";

        log.Calculations.ToDictionary(x => x.Query, x => x);

        foreach (var entry in log.GetAllEntries())
            CreateNode(d, entry);

        foreach (var entry in log.GetAllEntries())
        foreach(var dep in entry.Dependencies)
            d.CreateEdgeFor(entry, dep);
            
        return d.Build();
    }

    private void CreateNode(GraphvizDiagram d, RecordedEntry entry)
    {
        var node = d.CreateNodeFor(entry);
        node.NodeShape = "rectangle";
        var nodeTitle = entry.Query.GetType().Name;
        if (nodeTitle.EndsWith("Input"))
            nodeTitle = nodeTitle[..^"Input".Length];

        var label = new KeyValueTable(nodeTitle);
        if (entry.Calculation != null)
        {
            label.Set("Execution", "#" + (entry.Calculation.Index + 1).ToString() + ", Calc: " + entry.Calculation.ExlusiveHandlerExecutionTime.TotalMilliseconds + "ms, " + entry.Calculation.InclusiveHandlerExecutionTime.TotalMilliseconds + "ms, Equal: " + entry.Calculation.OverheadExecutionTime.TotalMilliseconds + "ms");
            if (entry.Calculation.ResultType is ResultType.InitialCalculation or ResultType.HasChanged)
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
            
        if (entry.IsInput)
        {
            node.Color = "#A0A0ff";
            node.FontColor = "#A0A0ff";
            node.Style = "filled";
            node.FillColor = "#8080FF10";
        }

        foreach (var props in entry.Query.GetType().GetProperties())
        {
            var value = props.GetValue(entry.Query);
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