using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Lucy.DebugUI.Shared
{
    public partial class TreeView
    {
        private IEnumerable<ITreeViewNode> _rootNodes = ImmutableArray<ITreeViewNode>.Empty;
        private ImmutableArray<RowViewModel> _rowViewModels = ImmutableArray<RowViewModel>.Empty;

        [Parameter]
        public IEnumerable<ITreeViewNode> RootNodes
        {
            get => _rootNodes;
            set
            {
                _rootNodes = value;
                MapNodesToRows();
            }
        }

        public void Update()
        {
            MapNodesToRows();
        }

        private void MapNodesToRows()
        {
            void Map(ITreeViewNode node, int depth, List<RowViewModel> rowViewModelList)
            {
                var row = new RowViewModel();

                row.RowElements.Add(new RowElementViewModel
                {
                    Style = new Style().Set("width", $"{depth}em")
                });

                var hasChildNodes = node.ChildNodes.Any();
                var item = new RowElementViewModel();
                item.Text = hasChildNodes ? "\ueab6" : "";
                item.Style = new Style()
                    .Set("font-family", "codicon;")
                    .Set("width", "1em")
                    .Set("height", "1em")
                    .Set("padding", "4px")
                    .Set("margin", "0px 1px")
                    .SetIf(node.Expanded, "transform", "rotate(90deg)");

                if (hasChildNodes)
                {
                    item.Classes = new Classes()
                        .Set("clickable");
                    item.OnClick = () =>
                    {
                        node.Expanded = !node.Expanded;
                        MapNodesToRows();
                        return Task.CompletedTask;
                    };
                }
                row.RowElements.Add(item);

                foreach (var element in node.Elements)
                {
                    var vm = new RowElementViewModel();
                    vm.Text = element.Text;
                    if (element.OnClick != null)
                    {
                        vm.OnClick = element.OnClick;
                        vm.Classes.Set("clickable");
                    }
                    if (element.Color != null)
                        vm.Style.Set("color", element.Color);
                    if (element.Opacity.HasValue)
                        vm.Style.Set("opacity", element.Opacity.Value.ToString(CultureInfo.InvariantCulture));
                    if (element.FontStyle != null)
                        vm.Style.Set("font-style", element.FontStyle);
                    vm.Style.Set("margin", "0px 1px").Set("padding", "4px");

                    row.RowElements.Add(vm);
                }
                rowViewModelList.Add(row);

                if (node.Expanded)
                    foreach (var child in node.ChildNodes)
                        Map(child, depth + 1, rowViewModelList);
            }

            var list = new List<RowViewModel>();
            foreach (var node in _rootNodes)
            {
                Map(node, 0, list);
            }

            _rowViewModels = list.ToImmutableArray();
        }


        private class RowViewModel
        {
            public List<RowElementViewModel> RowElements { get; set; } = new List<RowElementViewModel>();
        }

        private class RowElementViewModel
        {
            public string Text { get; set; } = "";
            public Style Style { get; set; } = new Style();
            public Classes Classes { get; set; } = new Classes();
            public Func<Task> OnClick { get; set; } = () => Task.CompletedTask;
        }
    }

    public interface ITreeViewNode
    {
        IEnumerable<TreeViewNodeElement> Elements { get; }
        IEnumerable<ITreeViewNode> ChildNodes { get; }
        bool Expanded { get; set; }
    }

    public class TreeViewNode : ITreeViewNode
    {
        public TreeViewNode()
        {
        }

        public TreeViewNode(string text)
        {
            Elements = new List<TreeViewNodeElement>() { new TreeViewNodeElement(text) };
        }

        public List<TreeViewNodeElement> Elements { get; } = new List<TreeViewNodeElement>();
        public List<TreeViewNode> ChildNodes { get; } = new List<TreeViewNode>();

        IEnumerable<TreeViewNodeElement> ITreeViewNode.Elements => Elements;
        IEnumerable<ITreeViewNode> ITreeViewNode.ChildNodes => ChildNodes;

        public bool Expanded { get; set; }
    }

    public class TreeViewNodeElement
{
    public TreeViewNodeElement(string text, string? color = null, double? opacity = null, string? fontStyle = null)
    {
        Text = text;
        Color = color;
        Opacity = opacity;
        FontStyle = fontStyle;
    }

    public string Text { get; set; }
    internal Func<Task>? OnClick { get; set; }
    public string? Color { get; set; }
    public double? Opacity { get; set; }
    public string? FontStyle { get; set; }
}

public class Style
{
    private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

    public Style()
    {
    }

    public Style(string key, string value)
    {
        _values[key] = value;
    }

    public Style Set(string key, string value)
    {
        _values[key] = value;
        return this;
    }

    public Style SetIf(bool condition, string key, string value)
    {
        if (!condition)
            return this;

        _values[key] = value;
        return this;
    }

    public static implicit operator string(Style s) => s.ToString();
    public override string ToString() => string.Join("; ", _values.Select(x => x.Key + ": " + x.Value));
}

public class Classes
{
    private readonly HashSet<string> _values = new HashSet<string>();

    public Classes()
    {
    }

    public Classes Set(string name)
    {
        _values.Add(name);
        return this;
    }

    public Classes SetIf(bool condition, string name)
    {
        if (!condition)
            return this;

        _values.Add(name);
        return this;
    }

    public static implicit operator string(Classes c) => c.ToString();
    public override string ToString() => string.Join("; ", _values.Select(x => x));
}
}
