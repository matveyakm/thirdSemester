// <copyright file="ExceptionTestClass.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using System;
using MyNUnit.Attributes;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// Provides a set of tests to verify how the framework handles successful execution and various exception scenarios.
/// </summary>
public class ExceptionTestClass
{
    /// <summary>
    /// A test that completes successfully without throwing any exceptions.
    /// </summary>
    [MyTest]
    public void SuccessfulTest()
    {
    }

    /// <summary>
    /// A test that intentionally fails by throwing an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always thrown to simulate a failing test.</exception>
    [MyTest]
    public void FailingTest()
    {
        throw new InvalidOperationException("Boom!");
    }

    /// <summary>
    /// A test that throws an exception which is expected by the test attribute.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown as expected by the test runner.</exception>
    [MyTest(Expected = typeof(InvalidOperationException))]
    public void ExpectedExceptionTest()
    {
        throw new InvalidOperationException("Expected");
    }

    /// <summary>
    /// A test that throws an exception of a different type than what was expected.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown to verify mismatch with expected <see cref="ArgumentException"/>.</exception>
    [MyTest(Expected = typeof(ArgumentException))]
    public void WrongExpectedExceptionTest()
    {
        throw new InvalidOperationException("Wrong type");
    }

    /// <summary>
    /// A test that does not throw any exception even though one was specified as expected.
    /// </summary>
    [MyTest(Expected = typeof(InvalidOperationException))]
    public void NoExceptionButExpected()
    {
    }
}
