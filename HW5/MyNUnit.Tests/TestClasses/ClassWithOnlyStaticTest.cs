// <copyright file="ClassWithOnlyStaticTest.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit.Attributes;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// A class containing a static test method to verify how the runner handles non-instance tests.
/// </summary>
public class ClassWithOnlyStaticTest
{
    /// <summary>
    /// A static test method.
    /// </summary>
    [MyNUnit.Attributes.Test]
    public static void StaticTestMethod()
    {
    }
}