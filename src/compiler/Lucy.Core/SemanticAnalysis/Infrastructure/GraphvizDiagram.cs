using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public class GraphvizDiagram
{
    public string? NodeShape { get; set; }
    public string? NodeFontName { get; set; }
    public int? NodeFontSize { get; set; }

    public string? EdgeFontName { get; set; }
    public int? EdgeFontSize { get; set; }

    private int _lastId;
    private readonly Dictionary<NullableKey, Node> _nodesByKey = new();
    private readonly HashSet<EdgeByNodeKeys> _edgesByNodeKeys = new();
    private readonly List<Edge> _edges = new();
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record NullableKey(object? Value);
        

    private class EdgeByNodeKeys : IEquatable<EdgeByNodeKeys?>
    {
        public EdgeByNodeKeys(object? from, object? to)
        {
            _from = from;
            _to = to;
            _hash = HashCode.Combine(from, to);
        }

        private readonly object? _from;
        private readonly object? _to;
        private readonly int _hash;

        public override bool Equals(object? obj)
        {
            return Equals(obj as EdgeByNodeKeys);
        }

        public bool Equals(EdgeByNodeKeys? other)
        {
            return other is not null &&
                   EqualityComparer<object?>.Default.Equals(_from, other._from) &&
                   EqualityComparer<object?>.Default.Equals(_to, other._to);
        }

        public override int GetHashCode()
        {
            return _hash;
        }
    }

    string BuildProps(Dictionary<string, object> values)
    {
        var converted = new Dictionary<string, string>();
        foreach (var (key, value) in values)
        {
            if (value is string)
                converted.Add(key, $"\"{value}\"");
            else if (value is int i)
                converted.Add(key, i.ToString());
            else if (value is float f)
                converted.Add(key, f.ToString(CultureInfo.InvariantCulture));
            else if (value is float d)
                converted.Add(key, d.ToString(CultureInfo.InvariantCulture));
            else
                converted.Add(key, value.ToString() ?? "");
        }
        return "[" + string.Join(", ", converted.Select(x => $"{x.Key}={x.Value}").ToArray()) + "]";
    }


    public Node CreateNodeFor(object? key)
    {
        var nullableKey = new NullableKey(key);
        if (_nodesByKey.ContainsKey(nullableKey))
            throw new Exception("A node for this key already exists.");

        var node = new Node("n" + ++_lastId);
        _nodesByKey.Add(nullableKey, node);
        return node;
    }

    public bool HasNodeFor(object? key)
    {
        return _nodesByKey.ContainsKey(new NullableKey(key));
    }

    public bool HasEdgeBetween(object? from, object? to)
    {
        return _edgesByNodeKeys.Contains(new EdgeByNodeKeys(from, to));
    }

    public string Build()
    {
        var sb = new StringBuilder();
        sb.AppendLine("digraph D {");
        WriteHeader(sb);
        WriteNodes(sb);
        WriteEdges(sb);

        sb.AppendLine();
        sb.AppendLine("}");

        return sb.ToString();
    }

    private void WriteEdges(StringBuilder sb)
    {
        foreach (var edge in _edges)
        {
            sb.Append($"    {edge.From.Id} -> {edge.To.Id} ");
            var props = new Dictionary<string, object>();
            if (edge.Label != null)
                props["label"] = edge.Label;
            if (edge.Color != null)
                props["color"] = edge.Color;
            if (edge.Style != null)
                props["style"] = edge.Style;
            if (edge.PenWidth.HasValue)
                props["penwidth"] = edge.PenWidth.Value;
            sb.AppendLine(BuildProps(props));
        }
    }

    private void WriteHeader(StringBuilder sb)
    {
        sb.AppendLine();
        var dict = new Dictionary<string, object>();
        if (NodeShape != null) dict["shape"] = NodeShape;
        if (NodeFontName != null) dict["fontname"] = NodeFontName;
        if (NodeFontSize.HasValue) dict["fontsize"] = NodeFontSize.Value;
        if (dict.Count > 0)
            sb.AppendLine("    node " + BuildProps(dict));

        dict.Clear();
        if (EdgeFontName != null) dict["fontname"] = EdgeFontName;
        if (EdgeFontSize.HasValue) dict["fontsize"] = EdgeFontSize;
        if (dict.Count > 0)
            sb.AppendLine("    edge " + BuildProps(dict));
    }

    private void WriteNodes(StringBuilder sb)
    {
        sb.AppendLine();
        foreach (var node in _nodesByKey.Values.OrderBy(x => x.Id))
        {
            sb.Append($"    {node.Id} ");
            var props = new Dictionary<string, object>();
            props["label"] = node.Label;
            if (node.Color != null) props["color"] = node.Color;
            if (node.FontColor != null) props["fontcolor"] = node.FontColor;
            if (node.NodeShape != null) props["shape"] = node.NodeShape;
            if (node.Style != null) props["style"] = node.Style;
            if (node.FillColor != null) props["fillcolor"] = node.FillColor;
            sb.AppendLine(BuildProps(props));
        }
    }

    internal Edge CreateEdgeFor(object? from, object? to)
    {
        if (!_nodesByKey.TryGetValue(new NullableKey(from), out var fromNode))
            throw new Exception("'from' key is unknown.");
        if (!_nodesByKey.TryGetValue(new NullableKey(to), out var toNode))
            throw new Exception("'to' key is unknown.");

        var edge = new Edge(fromNode, toNode);
        _edges.Add(edge);
        _edgesByNodeKeys.Add(new EdgeByNodeKeys(from, to));
        return edge;
    }
}

public class Node
{
    public string Id { get; set; }
    public object Label { get; set; } = "";
    public string? NodeShape { get; set; }
    public string? NodeFontName { get; set; }
    public int? NodeFontSize { get; set; }
    public string? Color { get; set; }
    public string? FontColor { get; set; }
    public string? FillColor { get; internal set; }
    public string? Style { get; internal set; }

    public Node(string id)
    {
        Id = id;
    }
}

public class Edge
{
    public Node From { get; }
    public Node To { get; }
    public string? Label { get; set; }
    public string? Color { get; set; }
    public string? Style { get; set; }
    public int? PenWidth { get; set; }

    public Edge(Node from, Node to)
    {
        From = from;
        To = to;
    }
}

public class KeyValueTable
{
    private readonly string _title;
    private readonly Dictionary<string, string> _props = new();

    public KeyValueTable(string title)
    {
        _title = title;
    }

    public void Set(string key, string value)
    {
        _props[key] = value;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("<<table border=\"0\">");
        sb.Append("<tr>");
        sb.Append($"<td align=\"left\" colspan=\"2\"><b><font point-size=\"24\">{_title}</font></b></td>");
        sb.Append("</tr>");
        foreach (var (key, value) in _props)
        {
            foreach (var (valuePart, isFirst) in Split(value))
            {
                sb.Append("<tr>");
                if (isFirst)
                    sb.Append($"<td align=\"left\"><b>{key}:</b></td>");
                else
                    sb.Append($"<td></td>");
                sb.Append($"<td align=\"left\">{valuePart}</td>");
                sb.Append("</tr>");
            }
        }
        sb.Append("</table>>");
        return sb.ToString();
    }

    private IEnumerable<(string, bool)> Split(string value)
    {
        bool isFirst = true;
        var max = 40;
        while (value.Length > max)
        {
            var part = value[..max];
            value = value[max..];
            yield return (part, isFirst);
            isFirst = false;
        }
        if (value.Length > 0)
            yield return (value, isFirst);
    }
}