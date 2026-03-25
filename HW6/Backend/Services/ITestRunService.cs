// <copyright file="ITestRunService.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Services;

using MyNUnit.Web.Models.Dtos;

/// <summary>
/// Defines methods for test run operations.
/// </summary>
public interface ITestRunService
{
    /// <summary>
    /// Executes tests for a given run ID.
    /// </summary>
    /// <param name="runId">The run ID.</param>
    /// <returns>The test run result.</returns>
    Task<TestRunResultDto> ExecuteTestsAsync(string runId);

    /// <summary>
    /// Gets the history of all test runs.
    /// </summary>
    /// <returns>List of run summaries.</returns>
    List<RunSummaryDto> GetHistory();

    /// <summary>
    /// Gets the details of a specific run.
    /// </summary>
    /// <param name="runId">The run ID.</param>
    /// <returns>The run details, or null if not found.</returns>
    TestRunResultDto? GetRunDetails(string runId);
}
