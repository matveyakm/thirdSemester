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
    Ignored
}