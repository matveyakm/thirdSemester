// <copyright file="TestRunResultDto.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Models.Dtos;

/// <summary>
/// Represents the complete result of a test run.
/// </summary>
public record TestRunResultDto
{
    /// <summary>
    /// Gets the summary of the test run.
    /// </summary>
    public required RunSummaryDto Summary { get; init; }

    /// <summary>
    /// Gets the results for each test class.
    /// </summary>
    public required List<TestClassResultDto> ClassResults { get; init; }
}
