namespace MyNUnit.Web.Models;

public record TestResultDto
{
    public string TestName { get; init; } = string.Empty;
    public string Status { get; init; } = "Unknown";
    public double ExecutionTimeMs { get; init; }
    public string? Message { get; init; }
    public string? StackTrace { get; init; }
    public string? IgnoreReason { get; init; }
}

public record TestClassResultDto
{
    public string ClassName { get; init; } = string.Empty;
    public List<TestResultDto> TestResults { get; init; } = new();
}