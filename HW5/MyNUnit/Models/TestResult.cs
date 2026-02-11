// <copyright file="TestResult.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;

namespace MyNUnit.Models;

/// <summary>
/// Represents the result of a single test.
/// </summary>
public class TestResult
{
    /// <summary>
    /// Gets or sets the name of the test method.
    /// </summary>
    public required string TestName { get; set; }

    /// <summary>
    /// Gets or sets the status of the test.
    /// </summary>
    public TestStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the execution time of the test.
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the exception if the test failed.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the ignore reason if the test was ignored.
    /// </summary>
    public string? IgnoreReason { get; set; }
}
