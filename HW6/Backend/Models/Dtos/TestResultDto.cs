// <copyright file="TestResultDto.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Models.Dtos;

/// <summary>
/// Represents the result of a single test.
/// </summary>
public record TestResultDto
{
    /// <summary>
    /// Gets the name of the test.
    /// </summary>
    public required string TestName { get; init; }

    /// <summary>
    /// Gets the status of the test.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gets the execution time in milliseconds.
    /// </summary>
    public required double ExecutionTimeMs { get; init; }

    /// <summary>
    /// Gets the error message, if any.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the stack trace, if any.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Gets the reason for ignoring the test, if any.
    /// </summary>
    public string? IgnoreReason { get; init; }
}
