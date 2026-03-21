// <copyright file="ClassWithTestWithParameters.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit.Attributes;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// A class containing a test method with parameters to verify validation of test signatures.
/// </summary>
public class ClassWithTestWithParameters
{
    /// <summary>
    /// A test method that incorrectly includes parameters.
    /// </summary>
    /// <param name="dummy">A dummy integer parameter.</param>
    [MyNUnit.Attributes.Test]
    public void TestWithParameter(int dummy)
    {
    }
}