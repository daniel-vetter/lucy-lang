using System.Collections;
using System.Reflection;
using Lucy.Core.Model;

public class Dumper
{
    public static void Dump(object obj)
    {
        WritePropertyList(obj, 0);
    }

    public static void WriteTypeInfo(object value)
    {
        if (value is string)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\"" + value + "\"");
            Console.ResetColor();
        }
        else if (value is NodeId)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\"" + value.ToString() + "\"");
            Console.ResetColor();
        }
        else if (value is IEnumerable enumerable)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("<list of " + enumerable.Cast<object>().Count() + " items>");
            Console.ResetColor();
        }
        else if (value is null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("null");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(value.GetType().Name);
            Console.ResetColor();
        }
        
    }

    private static void WritePropertyList(object obj, int depth)
    {
        var padding = new String(' ', depth);
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(padding + "" + prop.Name + ": ");
            Console.ResetColor();

            var value = prop.GetValue(obj);
            WriteTypeInfo(value);
            Console.WriteLine();

            if (value is not null and not string and not IEnumerable and not NodeId)
            {
                WritePropertyList(value, depth + 2);
            }
            else if (value is IEnumerable enumerable and not string)
            {
                int index = 0;
                foreach(var entry in enumerable)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(padding + "  [" + index + "] ");
                    Console.ResetColor();
                    WriteTypeInfo(entry);
                    Console.WriteLine();
                    WritePropertyList(entry, depth + 4);
                    
                    index++;
                }
            }
        }
    }
}
