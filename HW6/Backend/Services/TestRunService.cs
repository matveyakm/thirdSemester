// <copyright file="TestRunService.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Services;

using System.Text.Json;
using MyNUnit;
using MyNUnit.Models;
using MyNUnit.Web.Models.Dtos;

/// <summary>
/// Provides test run execution and history operations.
/// </summary>
public class TestRunService : ITestRunService
{
    private readonly string historyDir;
    private readonly IFileStorage fileStorage;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestRunService"/> class.
    /// </summary>
    /// <param name="fileStorage">The file storage service.</param>
    /// <param name="env">The web host environment.</param>
    public TestRunService(IFileStorage fileStorage, IWebHostEnvironment env)
    {
        this.fileStorage = fileStorage;
        this.historyDir = Path.Combine(env.ContentRootPath, "History");
        Directory.CreateDirectory(this.historyDir);
    }

    /// <inheritdoc />
    public Task<TestRunResultDto> ExecuteTestsAsync(string runId)
    {
        var runDir = this.fileStorage.GetUploadsPath(runId);
        if (runDir == null)
        {
            throw new DirectoryNotFoundException($"Директория прогона не найдена: {runId}");
        }

        var runner = new TestRunner();
        var classResults = runner.RunTests(runDir);

        var summary = new RunSummaryDto
        {
            RunId = runId,
            Timestamp = DateTime.UtcNow,
            AssemblyCount = Directory.GetFiles(runDir, "*.dll").Length,
            TotalTests = classResults.Sum(cr => cr.TestResults.Count),
            Passed = classResults.Sum(cr => cr.TestResults.Count(t => t.Status == TestStatus.Passed)),
            Failed = classResults.Sum(cr => cr.TestResults.Count(t => t.Status == TestStatus.Failed)),
            Errored = classResults.Sum(cr => cr.TestResults.Count(t => t.Status == TestStatus.Errored)),
            Ignored = classResults.Sum(cr => cr.TestResults.Count(t => t.Status == TestStatus.Ignored)),
        };

        var dtoClassResults = classResults.Select(cr => new TestClassResultDto
        {
            ClassName = cr.ClassName ?? "UnknownClass",
            TestResults = cr.TestResults.Select(tr => new TestResultDto
            {
                TestName = tr.TestName ?? "UnknownTest",
                Status = tr.Status.ToString(),
                ExecutionTimeMs = tr.ExecutionTime.TotalMilliseconds,
                Message = tr.Exception?.Message,
                StackTrace = tr.Exception?.StackTrace,
                IgnoreReason = tr.IgnoreReason,
            }).ToList(),
        }).ToList();

        var fullResult = new TestRunResultDto
        {
            Summary = summary,
            ClassResults = dtoClassResults,
        };

        var jsonPath = Path.Combine(this.historyDir, $"{runId}.json");
        var json = JsonSerializer.Serialize(fullResult, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, json);

        return Task.FromResult(fullResult);
    }

    /// <inheritdoc />
    public List<RunSummaryDto> GetHistory()
    {
        var files = Directory.GetFiles(this.historyDir, "*.json");
        var results = new List<RunSummaryDto>();

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var full = JsonSerializer.Deserialize<TestRunResultDto>(json);
                if (full?.Summary != null)
                {
                    results.Add(full.Summary);
                }
            }
            catch
            {
            }
        }

        return results.OrderByDescending(r => r.Timestamp).ToList();
    }

    /// <inheritdoc />
    public TestRunResultDto? GetRunDetails(string runId)
    {
        var path = Path.Combine(this.historyDir, $"{runId}.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TestRunResultDto>(json);
    }
}
