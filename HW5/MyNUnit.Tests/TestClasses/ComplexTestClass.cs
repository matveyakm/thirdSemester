// <copyright file="ComplexTestClass.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using MyTest = MyNUnit.Attributes.TestAttribute;
using MyNUnit.Attributes; 

namespace MyNUnit.Tests.TestClasses;

public class ComplexTestClass
{
    public static bool BeforeClassRan;
    public static bool AfterClassRan;
    public bool BeforeRan;
    public bool AfterRan;

    [BeforeClass]
    public static void SetupClass()
    {
        BeforeClassRan = true;
    }

    [AfterClass]
    public static void TeardownClass()
    {
        AfterClassRan = true;
    }

    [Before]
    public void Setup()
    {
        BeforeRan = true;
    }

    [After]
    public void Teardown()
    {
        AfterRan = true;
    }

    [MyTest]
    public void Test1()
    {
        if (!BeforeRan) throw new Exception("Before not ran");
    }

    [MyTest(Ignore = "Temporary disabled")]
    public void IgnoredTest() { }

    [MyTest]
    public void Test2()
    {
        if (!BeforeRan || !AfterRan) throw new Exception("Setup/teardown failed");
    }
}
