using System.Text;

namespace Lucy.Core.SourceGenerator.Infrastructure;

public static class Helper
{
    public static string Then(this bool condition, string message)
    {
        return condition ? message : "";
    }

    public static void WriteClass(this StringBuilder sb, string name, bool isAbstract = false, string? baseClass = null, IEnumerable<string>? interfaces = null, Action<StringBuilder>? content = null)
    {
        sb.Append("public ");
        if (isAbstract)
            sb.Append("abstract ");
        sb.Append("class ");
        sb.Append(name);

        List<string> b = new();
        if (baseClass != null)
            b.Add(baseClass);
        if (interfaces != null)
            b.AddRange(interfaces);
        if (b.Count > 0)
        {
            sb.Append(" : ");
            sb.Append(string.Join(", ", b));
        }
        sb.AppendLine();
        sb.AppendLine("{");
        content?.Invoke(sb);
        sb.Append("}");
        sb.AppendLine();
    }
}