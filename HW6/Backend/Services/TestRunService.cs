using System.Text.Json;
using MyNUnit;
using MyNUnit.Models;

namespace MyNUnit.Web.Services;

public class TestRunService
{
    private readonly string _basePath;
    private readonly string _uploadsDir;
    private readonly string _historyDir;

    public TestRunService(IWebHostEnvironment env)
    {
        _basePath = env.ContentRootPath;
        _uploadsDir = Path.Combine(_basePath, "Uploads");
        _historyDir = Path.Combine(_basePath, "History");

        Directory.CreateDirectory(_uploadsDir);
        Directory.CreateDirectory(_historyDir);
    }

    public async Task<string> SaveAssembliesAsync(IFormFileCollection files)
    {
        var runId = Guid.NewGuid().ToString("N");
        var runDir = Path.Combine(_uploadsDir, runId);
        Directory.CreateDirectory(runDir);

        foreach (var file in files)
        {
            if (string.IsNullOrEmpty(file.FileName) || !file.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                continue;

            var targetPath = Path.Combine(runDir, file.FileName);
            await using var stream = new FileStream(targetPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        return runId;
    }

    public async Task<TestRunResultDto> ExecuteTestsAsync(string runId)
    {
        var runDir = Path.Combine(_uploadsDir, runId);
        if (!Directory.Exists(runDir))
            throw new DirectoryNotFoundException($"Директория прогона не найдена: {runId}");

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
            Ignored = classResults.Sum(cr => cr.TestResults.Count(t => t.Status == TestStatus.Ignored))
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
                IgnoreReason = tr.IgnoreReason
            }).ToList()
        }).ToList();

        var fullResult = new TestRunResultDto
        {
            Summary = summary,
            ClassResults = dtoClassResults
        };

        // Сохраняем в файл
        var jsonPath = Path.Combine(_historyDir, $"{runId}.json");
        var json = JsonSerializer.Serialize(fullResult, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, json);

        return fullResult;
    }

    public List<RunSummaryDto> GetHistory()
    {
        var files = Directory.GetFiles(_historyDir, "*.json");
        var results = new List<RunSummaryDto>();

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var full = JsonSerializer.Deserialize<TestRunResultDto>(json);
                if (full?.Summary != null)
                    results.Add(full.Summary);
            }
            catch { }
        }

        return results.OrderByDescending(r => r.Timestamp).ToList();
    }

    public TestRunResultDto? GetRunDetails(string runId)
    {
        var path = Path.Combine(_historyDir, $"{runId}.json");
        if (!File.Exists(path)) return null;

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TestRunResultDto>(json);
    }
}

// ====================== DTO ======================

public record TestRunResultDto
{
    public required RunSummaryDto Summary { get; init; }
    public required List<TestClassResultDto> ClassResults { get; init; }
}

public record RunSummaryDto
{
    public required string RunId { get; init; }
    public required DateTime Timestamp { get; init; }
    public required int AssemblyCount { get; init; }
    public required int TotalTests { get; init; }
    public required int Passed { get; init; }
    public required int Failed { get; init; }
    public required int Errored { get; init; }
    public required int Ignored { get; init; }
}

public record TestClassResultDto
{
    public required string ClassName { get; init; }
    public required List<TestResultDto> TestResults { get; init; }
}

public record TestResultDto
{
    public required string TestName { get; init; }
    public required string Status { get; init; }
    public required double ExecutionTimeMs { get; init; }
    public string? Message { get; init; }
    public string? StackTrace { get; init; }
    public string? IgnoreReason { get; init; }
}