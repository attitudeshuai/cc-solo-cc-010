using System.Text.Json;
using Backend.Models;

namespace Backend.Services;

public class TableService
{
    private readonly DataStore _store;
    private readonly string _configPath;
    private readonly ILogger<TableService> _logger;

    public TableService(DataStore store, IWebHostEnvironment env, ILogger<TableService> logger)
    {
        _store = store;
        _logger = logger;
        _configPath = Path.Combine(env.ContentRootPath, "Config");
        LoadAll();
    }

    private void LoadAll()
    {
        var configFile = Path.Combine(_configPath, "config.json");
        
        if (!File.Exists(configFile))
        {
            _logger.LogWarning("配置文件不存在: {ConfigFile}", configFile);
            return;
        }

        try
        {
            var content = File.ReadAllText(configFile);
            var configs = JsonSerializer.Deserialize<List<TableConfig>>(content);
            
            if (configs == null || configs.Count == 0)
            {
                _logger.LogWarning("配置文件为空或格式无效: {ConfigFile}", configFile);
                return;
            }

            foreach (var cfg in configs)
            {
                if (string.IsNullOrWhiteSpace(cfg.table_name))
                {
                    _logger.LogWarning("跳过无效配置项: table_name 为空");
                    continue;
                }
                _store.Configs[cfg.table_name] = cfg;
                LoadTableData(cfg.table_name);
            }
            
            _logger.LogInformation("成功加载 {Count} 个表格配置", _store.Configs.Count);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "配置文件JSON格式错误: {ConfigFile}", configFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置文件失败: {ConfigFile}", configFile);
        }
    }

    private void LoadTableData(string tableName)
    {
        var file = Path.Combine(_configPath, $"{tableName}.json");
        var rows = new List<TableRow>();

        if (!File.Exists(file))
        {
            _logger.LogInformation("表格数据文件不存在，创建空表: {TableName}", tableName);
            _store.Tables[tableName] = rows;
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(file));
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                var data = new Dictionary<string, object>();
                foreach (var prop in element.EnumerateObject())
                {
                    data[prop.Name] = prop.Value.GetString() ?? "";
                }
                rows.Add(new TableRow { Data = data });
            }
            _logger.LogInformation("加载表格 {TableName} 成功，共 {Count} 条数据", tableName, rows.Count);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "表格数据JSON格式错误: {File}", file);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载表格数据失败: {File}", file);
        }
        
        _store.Tables[tableName] = rows;
    }

    public List<string> GetTableNames() => _store.Configs.Keys.ToList();

    public TableConfig? GetConfig(string name) => 
        _store.Configs.TryGetValue(name, out var cfg) ? cfg : null;

    public List<TableRow> GetRows(string name) =>
        _store.Tables.TryGetValue(name, out var rows) ? rows : new();

    public TableRow? AddRow(string tableName, Dictionary<string, object> data)
    {
        if (!_store.Tables.ContainsKey(tableName)) return null;
        var cleanData = new Dictionary<string, object>();
        foreach (var kvp in data)
        {
            cleanData[kvp.Key] = kvp.Value is JsonElement je ? je.GetString() ?? "" : kvp.Value?.ToString() ?? "";
        }
        var row = new TableRow { Data = cleanData };
        _store.Tables[tableName].Add(row);
        return row;
    }

    public bool UpdateRow(string tableName, string id, Dictionary<string, object> data)
    {
        var row = GetRows(tableName).FirstOrDefault(r => r.Id == id);
        if (row == null || row.IsLocked) return false;
        
        var config = GetConfig(tableName);
        if (config == null) return false;

        foreach (var field in config.fields)
        {
            if (field.is_modify && data.ContainsKey(field.en_name))
            {
                var val = data[field.en_name];
                row.Data[field.en_name] = val is JsonElement je ? je.GetString() ?? "" : val?.ToString() ?? "";
            }
        }
        return true;
    }

    public bool DeleteRow(string tableName, string id)
    {
        var rows = GetRows(tableName);
        var row = rows.FirstOrDefault(r => r.Id == id);
        if (row == null || row.IsLocked) return false;
        return rows.Remove(row);
    }

    public bool SetLock(string tableName, string id, bool locked)
    {
        var row = GetRows(tableName).FirstOrDefault(r => r.Id == id);
        if (row == null) return false;
        row.IsLocked = locked;
        return true;
    }

    public bool Save(string tableName)
    {
        if (!_store.Tables.ContainsKey(tableName))
        {
            _logger.LogWarning("保存失败，表格不存在: {TableName}", tableName);
            return false;
        }
        
        try
        {
            var file = Path.Combine(_configPath, $"{tableName}.json");
            var data = _store.Tables[tableName].Select(r => r.Data).ToList();
            File.WriteAllText(file, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            _logger.LogInformation("保存表格 {TableName} 成功，共 {Count} 条数据", tableName, data.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存表格失败: {TableName}", tableName);
            return false;
        }
    }
}
