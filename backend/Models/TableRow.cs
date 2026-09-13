namespace Backend.Models;

public class TableRow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public bool IsLocked { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}
