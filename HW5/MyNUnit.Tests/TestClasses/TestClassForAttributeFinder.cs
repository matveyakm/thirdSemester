// <copyright file="TestClassForAttributeFinder.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using MyNUnit.Attributes;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// A test class containing various lifecycle attributes to verify the attribute discovery mechanism.
/// </summary>
public class TestClassForAttributeFinder
{
    /// <summary>
    /// A static setup method executed once before any tests in the class.
    /// </summary>
    [BeforeClass]
    public static void StaticSetup()
    {
    }

    /// <summary>
    /// A static teardown method executed once after all tests in the class have finished.
    /// </summary>
    [AfterClass]
    public static void StaticTeardown()
    {
    }

    /// <summary>
    /// An instance setup method executed before each individual test.
    /// </summary>
    [Before]
    public void Setup()
    {
    }

    /// <summary>
    /// A standard test method to be discovered by the runner.
    /// </summary>
    [MyTest]
    public void TestMethod1()
    {
    }

    /// <summary>
    /// A test method that is marked as ignored with a specific reason.
    /// </summary>
    [MyTest(Ignore = "Not ready")]
    public void IgnoredTest()
    {
    }

    /// <summary>
    /// An instance teardown method executed after each individual test.
    /// </summary>
    [After]
    public void Teardown()
    {
    }
}
