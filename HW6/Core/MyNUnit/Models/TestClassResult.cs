// <copyright file="TestClassResult.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Models;

using System.Collections.Generic;

/// <summary>
/// Represents the results of all tests in a class.
/// </summary>
public class TestClassResult
{
    /// <summary>
    /// The internal list of test results.
    /// </summary>
    private readonly List<TestResult> testResults = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestClassResult"/> class.
    /// </summary>
    /// <param name="className">The name of the test class.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="className"/> is null.</exception>
    public TestClassResult(string className)
    {
        this.ClassName = className ?? throw new ArgumentNullException(nameof(className));
    }

    /// <summary>
    /// Gets the name of the test class.
    /// </summary>
    public string ClassName { get; init; }

    /// <summary>
    /// Gets a read-only list of individual test execution results.
    /// </summary>
    public IReadOnlyList<TestResult> TestResults => this.testResults.AsReadOnly();

    /// <summary>
    /// Adds a test result to the collection.
    /// </summary>
    /// <param name="result">The <see cref="TestResult"/> to add.</param>
    internal void Add(TestResult result)
    {
        this.testResults.Add(result);
    }
}
