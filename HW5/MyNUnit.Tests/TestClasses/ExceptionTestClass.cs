// <copyright file="ExeptionTestClass.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using MyTest = MyNUnit.Attributes.TestAttribute;
using MyNUnit.Attributes; 

namespace MyNUnit.Tests.TestClasses;

public class ExceptionTestClass
{
    [MyTest]
    public void SuccessfulTest() { }

    [MyTest]
    public void FailingTest()
    {
        throw new InvalidOperationException("Boom!");
    }

    [MyTest(Expected = typeof(InvalidOperationException))]
    public void ExpectedExceptionTest()
    {
        throw new InvalidOperationException("Expected");
    }

    [MyTest(Expected = typeof(ArgumentException))]
    public void WrongExpectedExceptionTest()
    {
        throw new InvalidOperationException("Wrong type");
    }

    [MyTest(Expected = typeof(InvalidOperationException))]
    public void NoExceptionButExpected()
    {
    }
}
