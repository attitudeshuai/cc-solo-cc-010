namespace Backend.Models;

public class FieldConfig
{
    public string en_name { get; set; } = "";
    public string zh_name { get; set; } = "";
    public string default_value { get; set; } = "";
    public bool is_modify { get; set; }
    public bool is_show { get; set; }
}

public class TableConfig
{
    public string table_name { get; set; } = "";
    public List<FieldConfig> fields { get; set; } = new();
}
