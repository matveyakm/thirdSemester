// <copyright file="ModelsTest.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using MyNUnit.Models;
using NUnit.Framework;

/// <summary>
/// Contains unit tests for the data models used in the MyNUnit framework, such as <see cref="TestResult"/> and <see cref="TestClassResult"/>.
/// </summary>
[TestFixture]
public class ModelsTest
{
    /// <summary>
    /// Verifies that the <see cref="TestResult"/> properties are correctly initialized and hold their assigned values.
    /// </summary>
    [Test]
    public void TestResult_Properties_WorkCorrectly()
    {
        var result = new TestResult
        {
            TestName = "Test",
            Status = TestStatus.Passed,
            ExecutionTime = TimeSpan.FromMilliseconds(100),
            IgnoreReason = "reason",
        };

        Assert.That(result.TestName, Is.EqualTo("Test"));
        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(result.ExecutionTime.TotalMilliseconds, Is.EqualTo(100));
        Assert.That(result.IgnoreReason, Is.EqualTo("reason"));
    }

    /// <summary>
    /// Verifies that <see cref="TestClassResult"/> correctly stores the class name and maintains a collection of individual test results.
    /// </summary>
    [Test]
    public void TestClassResult_ContainsTestResults()
    {
        var classResult = new TestClassResult("MyClass");
        classResult.Add(new TestResult { TestName = "T1" });

        Assert.That(classResult.ClassName, Is.EqualTo("MyClass"));
        Assert.That(classResult.TestResults, Has.Count.EqualTo(1));
    }
}
