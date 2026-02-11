// <copyright file="TestExecutorTest.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using System.Reflection;
using MyNUnit.Models;
using MyNUnit.Tests.TestClasses;
using MyNUnit.Attributes;
using NUnit.Framework;

namespace MyNUnit.Tests;

[TestFixture]
public class TestExecutorTest
{
    private TestExecutor _executor = null!;

    [SetUp]
    public void Setup()
    {
        _executor = new TestExecutor();
    }

    [Test]
    public void ExecuteTest_Passes_WhenExpectedExceptionThrown()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("ExpectedExceptionTest")!;

        var result = _executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
    }

    [Test]
    public void ExecuteTest_Fails_WhenNoExceptionButExpected()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("NoExceptionButExpected")!;

        var result = _executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(result.Exception?.Message, Does.Contain("Expected exception"));
    }

    [Test]
    public void ExecuteTest_Fails_WhenWrongExceptionType()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("WrongExpectedExceptionTest")!;

        var result = _executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Failed));
    }

    [Test]
    public void ExecuteTest_ReturnsIgnored_WhenIgnoreSet()
    {
        var type = typeof(ComplexTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("IgnoredTest")!;

        var result = _executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Ignored));
        Assert.That(result.IgnoreReason, Is.EqualTo("Temporary disabled"));
    }

    [Test]
    public void ExecuteTest_Passes_WhenNoExceptionAndNoneExpected()
    {
        var type = typeof(ExceptionTestClass);
        var instance = Activator.CreateInstance(type)!;
        var method = type.GetMethod("SuccessfulTest")!;

        var result = _executor.ExecuteTest(method, instance, null, null);

        Assert.That(result.Status, Is.EqualTo(TestStatus.Passed));
    }
}
