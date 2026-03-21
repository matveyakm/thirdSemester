// <copyright file="ComplexTestClass.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests.TestClasses;

using System;
using MyNUnit.Attributes;
using MyTest = MyNUnit.Attributes.TestAttribute;

/// <summary>
/// Represents a complex test class used to verify the lifecycle of the MyNUnit framework.
/// </summary>
public class ComplexTestClass
{
    /// <summary>
    /// A value indicating whether the <see cref="BeforeClass"/> method has been executed.
    /// </summary>
    private static bool beforeClassRan;

    /// <summary>
    /// A value indicating whether the <see cref="AfterClass"/> method has been executed.
    /// </summary>
    private static bool afterClassRan;

    /// <summary>
    /// A value indicating whether the <see cref="Before"/> method has been executed for the current test.
    /// </summary>
    private bool beforeRan;

    /// <summary>
    /// A value indicating whether the <see cref="After"/> method has been executed for the current test.
    /// </summary>
    private bool afterRan;

    /// <summary>
    /// Gets or sets a value indicating whether the class-level setup method has been executed.
    /// </summary>
    public static bool BeforeClassRan { get => beforeClassRan; set => beforeClassRan = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the class-level teardown method has been executed.
    /// </summary>
    public static bool AfterClassRan { get => afterClassRan; set => afterClassRan = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the per-test setup method has been executed.
    /// </summary>
    public bool BeforeRan { get => this.beforeRan; set => this.beforeRan = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the per-test teardown method has been executed.
    /// </summary>
    public bool AfterRan { get => this.afterRan; set => this.afterRan = value; }

    /// <summary>
    /// Sets up the class-level state before any tests in this class are run.
    /// </summary>
    [BeforeClass]
    public static void SetupClass() => BeforeClassRan = true;

    /// <summary>
    /// Cleans up the class-level state after all tests in this class have run.
    /// </summary>
    [AfterClass]
    public static void TeardownClass() => afterClassRan = true;

    /// <summary>
    /// Sets up the environment before each test method.
    /// </summary>
    [Before]
    public void Setup() => this.beforeRan = true;

    /// <summary>
    /// Cleans up the environment after each test method.
    /// </summary>
    [After]
    public void Teardown() => this.AfterRan = true;

    /// <summary>
    /// Validates that the setup method was executed before the test.
    /// </summary>
    /// <exception cref="Exception">Thrown when the setup method has not run.</exception>
    [MyTest]
    public void Test1()
    {
        if (!this.beforeRan)
        {
            throw new Exception("Before not ran");
        }
    }

    /// <summary>
    /// A test method that is currently ignored.
    /// </summary>
    [MyTest(Ignore = "Temporary disabled")]
    public void IgnoredTest()
    {
    }

    /// <summary>
    /// Validates that both setup and teardown mechanisms are functioning correctly.
    /// </summary>
    /// <exception cref="Exception">Thrown when either setup or teardown has failed to execute correctly.</exception>
    [MyTest]
    public void Test2()
    {
        if (!this.beforeRan || !this.AfterRan)
        {
            throw new Exception("Setup/teardown failed");
        }
    }
}
