// <copyright file="ModelsTest.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using MyNUnit.Models;
using NUnit.Framework;

namespace MyNUnit.Tests;

[TestFixture]
public class ModelsTest
{
    [Test]
    public void TestResult_Properties_WorkCorrectly()
    {
        var result = new TestResult
        {
            TestName = "Test",
            Status = TestStatus.Passed,
            ExecutionTime = TimeSpan.FromMilliseconds(100),
            IgnoreReason = "reason"
        };

        Assert.That(result.TestName, Is.EqualTo("Test"));
        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(result.ExecutionTime.TotalMilliseconds, Is.EqualTo(100));
        Assert.That(result.IgnoreReason, Is.EqualTo("reason"));
    }

    [Test]
    public void TestClassResult_ContainsTestResults()
    {
        var classResult = new TestClassResult { ClassName = "MyClass" };
        classResult.TestResults.Add(new TestResult { TestName = "T1" });

        Assert.That(classResult.ClassName, Is.EqualTo("MyClass"));
        Assert.That(classResult.TestResults, Has.Count.EqualTo(1));
    }
}
