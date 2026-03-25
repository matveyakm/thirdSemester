// <copyright file="TestRunsController.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using MyNUnit.Web.Services;

/// <summary>
/// Controller for managing test runs.
/// </summary>
[ApiController]
[Route("api/runs")]
public class TestRunsController : ControllerBase
{
    private readonly IFileStorage fileStorage;
    private readonly ITestRunService testRunService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestRunsController"/> class.
    /// </summary>
    /// <param name="fileStorage">The file storage service.</param>
    /// <param name="testRunService">The test run service.</param>
    public TestRunsController(IFileStorage fileStorage, ITestRunService testRunService)
    {
        this.fileStorage = fileStorage;
        this.testRunService = testRunService;
    }

    /// <summary>
    /// Upload .dll files.
    /// </summary>
    /// <param name="files">The files to upload.</param>
    /// <returns>The run ID.</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
        {
            return this.BadRequest("Не переданы файлы");
        }

        var runId = await this.fileStorage.SaveAssembliesAsync(files);
        return this.Ok(new { runId });
    }

    /// <summary>
    /// Execute tests by run ID.
    /// </summary>
    /// <param name="runId">The run ID.</param>
    /// <returns>The test results.</returns>
    [HttpPost("{runId}/execute")]
    public async Task<IActionResult> Execute(string runId)
    {
        try
        {
            var result = await this.testRunService.ExecuteTestsAsync(runId);
            return this.Ok(result);
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get list of all runs (summary).
    /// </summary>
    /// <returns>List of run summaries.</returns>
    [HttpGet]
    public IActionResult GetHistory()
    {
        return this.Ok(this.testRunService.GetHistory());
    }

    /// <summary>
    /// Get details of a specific run.
    /// </summary>
    /// <param name="runId">The run ID.</param>
    /// <returns>The run details.</returns>
    [HttpGet("{runId}")]
    public IActionResult GetRun(string runId)
    {
        var result = this.testRunService.GetRunDetails(runId);
        if (result == null)
        {
            return this.NotFound();
        }

        return this.Ok(result);
    }
}
