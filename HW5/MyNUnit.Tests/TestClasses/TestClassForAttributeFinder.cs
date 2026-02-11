// <copyright file="TestClassForAttributeFinder.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using MyTest = MyNUnit.Attributes.TestAttribute;
using MyNUnit.Attributes; 

namespace MyNUnit.Tests.TestClasses;

public class TestClassForAttributeFinder
{
    [BeforeClass]
    public static void StaticSetup() { }

    [Before]
    public void Setup() { }

    [MyTest]
    public void TestMethod1() { }

    [MyTest(Ignore = "Not ready")]
    public void IgnoredTest() { }

    [After]
    public void Teardown() { }

    [AfterClass]
    public static void StaticTeardown() { }
}

public class EmptyClass { }
