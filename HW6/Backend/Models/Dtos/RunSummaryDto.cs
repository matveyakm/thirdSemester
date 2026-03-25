// <copyright file="RunSummaryDto.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Models.Dtos;

/// <summary>
/// Represents a summary of a test run.
/// </summary>
public record RunSummaryDto
{
    /// <summary>
    /// Gets the unique identifier of the run.
    /// </summary>
    public required string RunId { get; init; }

    /// <summary>
    /// Gets the timestamp when the run was executed.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the number of assemblies in the run.
    /// </summary>
    public required int AssemblyCount { get; init; }

    /// <summary>
    /// Gets the total number of tests.
    /// </summary>
    public required int TotalTests { get; init; }

    /// <summary>
    /// Gets the number of passed tests.
    /// </summary>
    public required int Passed { get; init; }

    /// <summary>
    /// Gets the number of failed tests.
    /// </summary>
    public required int Failed { get; init; }

    /// <summary>
    /// Gets the number of errored tests.
    /// </summary>
    public required int Errored { get; init; }

    /// <summary>
    /// Gets the number of ignored tests.
    /// </summary>
    public required int Ignored { get; init; }
}
