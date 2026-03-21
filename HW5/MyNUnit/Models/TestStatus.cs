// <copyright file="TestStatus.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Models;

/// <summary>
/// Enum representing the status of a test.
/// </summary>
public enum TestStatus
{
    /// <summary>
    /// The test passed.
    /// </summary>
    Passed,

    /// <summary>
    /// The test failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The test was ignored.
    /// </summary>
    Ignored,

    /// <summary>
    /// Test could not be executed properly due to exception in setup, teardown, Before/After methods,
    /// constructor, or unexpected runtime error during test execution.
    /// </summary>
    Errored,
}