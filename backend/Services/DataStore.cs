using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Services;

public class DataStore
{
    public ConcurrentDictionary<string, TableConfig> Configs { get; } = new();
    public ConcurrentDictionary<string, List<TableRow>> Tables { get; } = new();
}
