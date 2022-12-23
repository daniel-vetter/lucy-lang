using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Model;

namespace Lucy.App.LanguageServer.Features.Debug;

[Service(Lifetime.Singleton)]
public class DebugViewGenerator
{
    internal async Task<string> Generate(object value)
    {
        var stream = GetType().Assembly.GetManifestResourceStream(GetType().Namespace + ".DebugView.html");
        if (stream == null)
            throw new Exception("Could not find DebugView.html");

        var strReader = new StreamReader(stream);
        var html = await strReader.ReadToEndAsync();

        var sb = new StringBuilder();

        void Process(object? obj)
        {
            if (obj is string or NodeId) return;

            if (obj is IEnumerable subList and not string)
            {
                sb.Append("<ul>");
                var index = 0;
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
            else if (obj != null)
            {
                var props = Rearrange(obj.GetType().GetProperties());

                sb.Append("<ul>");
                foreach (var prop in props)
                {
                    sb.Append("<li>");
                    sb.Append($"<span class='property'>{prop.Name}</span>: ");

                    var propValue = prop.GetValue(obj);
                    WriteValueHeader(sb, propValue);
                    sb.Append("</li>");

                    Process(propValue);
                }
                sb.Append("</ul>");
            }
        }

        sb.Append("<ul class='tree'>");
        sb.Append("<li>");
        WriteValueHeader(sb, value);
        sb.Append("</li>");
        Process(value);
        sb.Append("</ul>");

        html = sb + html;
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
        else if (value is TokenNode se)
        {
            sb.Append($"{value.GetType().Name} <span class=\"string\">\"{se.Text}\"</span>");
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
        else if (value is Enum)
        {
            sb.Append(value);
        }
        else
        {
            sb.Append(value.GetType().Name);
        }
    }
}