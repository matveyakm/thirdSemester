// <copyright file="ParallelTestClass.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using MyTest = MyNUnit.Attributes.TestAttribute;
using MyNUnit.Attributes; 

namespace MyNUnit.Tests.TestClasses;

public class ParallelTestsClass
{
    private static readonly ConcurrentBag<int> ExecutionOrder = new();

    [MyTest]
    public void TestA()
    {
        ExecutionOrder.Add(1);
    }

    [MyTest]
    public void TestB()
    {
        ExecutionOrder.Add(2);
    }

    public static int[] GetOrder() => ExecutionOrder.ToArray();
}

public class ParallelTestsClass2
{
    [MyTest]
    public void SingleTest() { }
}
