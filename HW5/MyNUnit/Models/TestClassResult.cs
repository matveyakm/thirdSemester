// <copyright file="TestClassResult.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace MyNUnit.Models;

/// <summary>
/// Represents the results of all tests in a class.
/// </summary>
public class TestClassResult
{
    /// <summary>
    /// Gets or sets the name of the test class.
    /// </summary>
    public required string ClassName { get; set; }

    /// <summary>
    /// Gets the list of test results.
    /// </summary>
    public List<TestResult> TestResults { get; } = new List<TestResult>();
}
