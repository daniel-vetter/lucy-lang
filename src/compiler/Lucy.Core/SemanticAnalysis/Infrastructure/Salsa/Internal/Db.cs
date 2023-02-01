using System;
using System.Collections.Generic;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal;

public sealed class Db
{
    public Dictionary<object, Entry> Entries { get; set; } = new();
    public int CurrentRevision { get; set; } = 0;
}

public class Entry
{
    public Entry(object query, object? result, int lastChanged, int lastChecked, Entry[]? dependencies = null)
    {
        Query = query;
        Result = result;
        LastChanged = lastChanged;
        LastChecked = lastChecked;
        Dependencies = dependencies ?? Array.Empty<Entry>();
    }

    public int LastChanged { get; set; }
    public int LastChecked { get; set; }
    public Entry[] Dependencies { get; set; }
    public object Query { get; }
    public object? Result { get; set; }
}