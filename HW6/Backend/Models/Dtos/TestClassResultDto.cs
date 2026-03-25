// <copyright file="TestClassResultDto.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Web.Models.Dtos;

/// <summary>
/// Represents the results of tests in a single class.
/// </summary>
public record TestClassResultDto
{
    /// <summary>
    /// Gets the name of the test class.
    /// </summary>
    public required string ClassName { get; init; }

    /// <summary>
    /// Gets the list of test results.
    /// </summary>
    public required List<TestResultDto> TestResults { get; init; }
}
