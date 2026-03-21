// <copyright file="TestExecutorTest.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using System;
using System.Reflection;
using MyNUnit.Models;
using MyNUnit.Tests.TestClasses;
using NUnit.Framework;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// Provides unit tests for the <see cref="TestExecutor"/> class, ensuring correct execution of individual test methods.
/// </summary>
[TestFixture]
public class TestExecutorTest
{
    private TestExecutor executor = null!;

    /// <summary>
    /// Initializes the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        this.executor = new TestExecutor();
    }

    /// <summary>
    /// Initializes the test environment before each test.
    /// </summary>
    [Test]
    public void ExecuteTest_Passes_WhenExpectedExceptionThrown()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("ExpectedExceptionTest")!;

        var result = this.executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
    }

    /// <summary>
    /// Verifies that the test status is <see cref="TestStatus.Passed"/> when the expected exception is thrown.
    /// </summary>
    [Test]
    public void ExecuteTest_Fails_WhenNoExceptionButExpected()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("NoExceptionButExpected")!;

        var result = this.executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(result.Exception?.Message, Does.Contain("Expected exception"));
    }

    /// <summary>
    /// Verifies that the test status is <see cref="TestStatus.Failed"/> when no exception is thrown but one was expected.
    /// </summary>
    [Test]
    public void ExecuteTest_Fails_WhenWrongExceptionType()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("WrongExpectedExceptionTest")!;

        var result = this.executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
    }

    /// <summary>
    /// Verifies that the test status is <see cref="TestStatus.Failed"/> when a different exception type is thrown than expected.
    /// </summary>
    [Test]
    public void ExecuteTest_ReturnsIgnored_WhenIgnoreSet()
    {
        var type = typeof(ComplexTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("IgnoredTest")!;

        var result = this.executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Ignored));
        Assert.That(result.IgnoreReason, Is.EqualTo("Temporary disabled"));
    }

    /// <summary>
    /// Verifies that the test status is <see cref="TestStatus.Passed"/> when no exception occurs and none is expected.
    /// </summary>
    [Test]
    public void ExecuteTest_Passes_WhenNoExceptionAndNoneExpected()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("SuccessfulTest")!;

        var result = this.executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
    }
}
