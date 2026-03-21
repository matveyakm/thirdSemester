// <copyright file="ParallelTestsClass2.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using System.Collections.Concurrent;
using MyNUnit.Attributes;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// An additional test class to verify cross-class parallel execution behavior.
/// </summary>
public class ParallelTestsClass2
{
    /// <summary>
    /// A simple test method to confirm the class is being processed by the runner.
    /// </summary>
    [MyTest]
    public void SingleTest()
    {
    }
}