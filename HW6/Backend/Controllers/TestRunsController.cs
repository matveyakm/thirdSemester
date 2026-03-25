using Microsoft.AspNetCore.Mvc;
using MyNUnit.Web.Services;

namespace MyNUnit.Web.Controllers;

[ApiController]
[Route("api/runs")]
public class TestRunsController : ControllerBase
{
    private readonly TestRunService _service;

    public TestRunsController(TestRunService service)
    {
        _service = service;
    }

    /// <summary>
    /// Загрузка .dll файлов
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("Не переданы файлы");

        var runId = await _service.SaveAssembliesAsync(files);
        return Ok(new { runId });
    }

    /// <summary>
    /// Запуск тестов по runId
    /// </summary>
    [HttpPost("{runId}/execute")]
    public async Task<IActionResult> Execute(string runId)
    {
        try
        {
            var result = await _service.ExecuteTestsAsync(runId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Список всех прогонов (summary)
    /// </summary>
    [HttpGet]
    public IActionResult GetHistory()
    {
        return Ok(_service.GetHistory());
    }

    /// <summary>
    /// Детали конкретного прогона
    /// </summary>
    [HttpGet("{runId}")]
    public IActionResult GetRun(string runId)
    {
        var result = _service.GetRunDetails(runId);
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}