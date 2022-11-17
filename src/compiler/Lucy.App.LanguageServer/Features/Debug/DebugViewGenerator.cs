using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Features.Debug
{
    [Service(Lifetime.Singleton)]
    public class DebugViewGenerator
    {
        private readonly CurrentWorkspace _currentWorkspace;

        public DebugViewGenerator(CurrentWorkspace currentWorkspace)
        {
            _currentWorkspace = currentWorkspace;
        }

        internal async Task<string> Generate()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".DebugView.html");
            if (stream == null)
                throw new Exception("Could not find DebugView.html");

            var strReader = new StreamReader(stream);
            var html = await strReader.ReadToEndAsync();

            var sb = new StringBuilder();

            void Process(object? obj)
            {
                if (obj is SyntaxElement)
                    return;

                if (obj is SyntaxTreeNode node)
                {
                    var map = GetObjectPropertyNames(node);
                    var children = node.GetChildNodes();

                    var props = Rearrange(node.GetType().GetProperties());

                    sb.Append("<ul>");
                    foreach (var prop in props)
                    {
                        sb.Append("<li>");
                        sb.Append($"<span class='property'>{prop.Name}</span>: ");

                        var value = prop.GetValue(node);
                        WriteValueHeader(sb, value);
                        sb.Append("</li>");
                        Process(value);
                    }
                    sb.Append("</ul>");
                }

                if (obj is IEnumerable<SyntaxTreeNode> subList)
                {
                    sb.Append("<ul>");
                    int index = 0;
                    foreach (var item in subList)
                    {
                        sb.Append("<li>");
                        sb.Append($"<span class='property'>[{index}]</span>: ");
                        WriteValueHeader(sb, item);
                        index++;
                        sb.Append("</li>");
                        Process(item);
                    }
                    sb.Append("</ul>");
                }
            }

            sb.Append("<ul class='tree'>");
            foreach (var doc in _currentWorkspace.Analysis.GetDocumentList()) //TODO: Use CurrentWorkspace directly without using the analysis
            {
                var syntaxTree = _currentWorkspace.Analysis.GetSyntaxTree(doc);
                sb.Append($"<li>{doc}</li>");
                Process(syntaxTree);

            }
            sb.Append("</ul>");

            html = sb.ToString() + html;
            return html;
        }

        private PropertyInfo[] Rearrange(PropertyInfo[] propertyInfos)
        {
            var list = propertyInfos.ToList();
            var matching = list.Where(x => x.Name == "NodeId").ToArray();
            foreach (var toRemove in matching)
                list.Remove(toRemove);
            foreach (var toInsert in matching)
                list.Insert(0, toInsert);
            return list.ToArray();
        }

        private static void WriteValueHeader(StringBuilder sb, object? value)
        {
            if (value == null)
            {
                sb.Append("<span style='opacity: 0.5'>&lt;null&gt;</span>");
            }
            else if (value is SyntaxElement se)
            {
                sb.Append($"{value.GetType().Name} <span class=\"string\">\"{se.Token.Text}\"</span>");
            }
            else if (value is string str)
            {
                sb.Append($"{value.GetType().Name} <span class=\"string\">\"{str}\"</span>");
            }
            else if (value is NodeId nodeId)
            {
                sb.Append($"{value.GetType().Name} <span class=\"nodeId\">\"{nodeId.ToString()}\"</span>");
            }
            else if (value is IEnumerable<object> list)
            {
                sb.Append("<span style='opacity: 0.5'>&lt;list of " + list.Count() + " elements&gt;</span>");
            }
            else
            {
                sb.Append(value.GetType().Name);
            }
        }

        private Dictionary<object, string> GetObjectPropertyNames(SyntaxTreeNode node)
        {
            var dict = new Dictionary<object, string>(new ObjectReferenceEqualityComparer<object>());

            var props = node.GetType().GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(node);
                if (value == null)
                    continue;

                if (value is IEnumerable<SyntaxTreeNode> subList)
                {
                    var index = 1;
                    foreach (var element in subList)
                    {
                        dict.Add(element, $"{prop.Name} {index}");
                        index++;
                    }
                }
                if (value is SyntaxTreeNode parserTreeNode)
                    dict.Add(value, prop.Name);
            }
            return dict;
        }

        public class ObjectReferenceEqualityComparer<T> : EqualityComparer<T> where T : class
        {
            public override bool Equals(T? x, T? y) => ReferenceEquals(x, y);
            public override int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
