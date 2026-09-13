using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableController : ControllerBase
{
    private readonly TableService _service;
    private readonly ILogger<TableController> _logger;

    public TableController(TableService service, ILogger<TableController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetTables()
    {
        _logger.LogDebug("获取表格列表");
        return Ok(_service.GetTableNames());
    }

    [HttpGet("{name}/config")]
    public IActionResult GetConfig(string name)
    {
        var cfg = _service.GetConfig(name);
        if (cfg == null)
        {
            _logger.LogWarning("表格配置不存在: {Name}", name);
            return NotFound(new { success = false, message = $"表格 '{name}' 不存在" });
        }
        return Ok(cfg);
    }

    [HttpGet("{name}/rows")]
    public IActionResult GetRows(string name)
    {
        _logger.LogDebug("获取表格数据: {Name}", name);
        return Ok(_service.GetRows(name));
    }

    [HttpPost("{name}/rows")]
    public IActionResult AddRow(string name, [FromBody] Dictionary<string, object> data)
    {
        var row = _service.AddRow(name, data);
        if (row == null)
        {
            _logger.LogWarning("新增数据失败，表格不存在: {Name}", name);
            return BadRequest(new { success = false, message = "表格不存在" });
        }
        _logger.LogInformation("新增数据成功: {Name}, ID: {Id}", name, row.Id);
        return Ok(new { success = true, message = "新增成功", data = row });
    }

    [HttpPut("{name}/rows/{id}")]
    public IActionResult UpdateRow(string name, string id, [FromBody] Dictionary<string, object> data)
    {
        if (_service.UpdateRow(name, id, data))
        {
            _logger.LogInformation("更新数据成功: {Name}, ID: {Id}", name, id);
            return Ok(new { success = true, message = "更新成功" });
        }
        _logger.LogWarning("更新数据失败: {Name}, ID: {Id}", name, id);
        return BadRequest(new { success = false, message = "数据已锁定或不存在" });
    }

    [HttpDelete("{name}/rows/{id}")]
    public IActionResult DeleteRow(string name, string id)
    {
        if (_service.DeleteRow(name, id))
        {
            _logger.LogInformation("删除数据成功: {Name}, ID: {Id}", name, id);
            return Ok(new { success = true, message = "删除成功" });
        }
        _logger.LogWarning("删除数据失败: {Name}, ID: {Id}", name, id);
        return BadRequest(new { success = false, message = "数据已锁定或不存在" });
    }

    [HttpPost("{name}/rows/{id}/lock")]
    public IActionResult Lock(string name, string id)
    {
        if (_service.SetLock(name, id, true))
        {
            _logger.LogInformation("锁定数据成功: {Name}, ID: {Id}", name, id);
            return Ok(new { success = true, message = "锁定成功" });
        }
        return NotFound(new { success = false, message = "数据不存在" });
    }

    [HttpPost("{name}/rows/{id}/unlock")]
    public IActionResult Unlock(string name, string id)
    {
        if (_service.SetLock(name, id, false))
        {
            _logger.LogInformation("解锁数据成功: {Name}, ID: {Id}", name, id);
            return Ok(new { success = true, message = "解锁成功" });
        }
        return NotFound(new { success = false, message = "数据不存在" });
    }

    [HttpPost("{name}/save")]
    public IActionResult Save(string name)
    {
        if (_service.Save(name))
        {
            return Ok(new { success = true, message = "保存成功" });
        }
        return BadRequest(new { success = false, message = "保存失败" });
    }
}
