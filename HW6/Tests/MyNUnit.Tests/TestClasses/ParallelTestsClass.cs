// <copyright file="ParallelTestsClass.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using System.Collections.Concurrent;
using MyNUnit.Attributes;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// A test class designed to verify the parallel execution capabilities of the test runner.
/// </summary>
public class ParallelTestsClass
{
    private static readonly ConcurrentBag<int> ExecutionOrder = new();

    /// <summary>
    /// Gets the current recorded order of execution as an array.
    /// </summary>
    /// <returns>An array of integers representing the recorded execution steps.</returns>
    public static int[] GetOrder() => ExecutionOrder.ToArray();

    /// <summary>
    /// A test method that adds a unique identifier to the execution order collection.
    /// </summary>
    [MyTest]
    public void TestA()
    {
        ExecutionOrder.Add(1);
    }

    /// <summary>
    /// A test method that adds a unique identifier to the execution order collection.
    /// </summary>
    [MyTest]
    public void TestB()
    {
        ExecutionOrder.Add(2);
    }
}
