// <copyright file="ReflectorTests.Tests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ReflectorTests;

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ReflectionTools;

/// <summary>
/// Тests for class Reflector.
/// </summary>
[TestFixture]
public class ReflectorTests
{
    private Reflector reflector = null!;
    private string testOutputPath = null!;
    private TextWriter originalOutput = null!;

    /// <summary>
    /// Initializes the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        this.reflector = new Reflector();
        this.testOutputPath = Path.Combine(Path.GetTempPath(), "ReflectorTestOutput");
        if (Directory.Exists(this.testOutputPath))
        {
            Directory.Delete(this.testOutputPath, true);
        }

        Directory.CreateDirectory(this.testOutputPath);
        Environment.CurrentDirectory = this.testOutputPath;

        this.originalOutput = Console.Out;
    }

    /// <summary>
    /// Cleans up the test environment after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        Console.SetOut(this.originalOutput);

        // Directory.Delete(testOutputPath, true);
    }

    /// <summary>
    /// Tests that calling PrintStructure with a null type throws an ArgumentNullException.
    /// </summary>
    [Test]
    public void PrintStructure_NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => this.reflector.PrintStructure(null!));
    }

    /// <summary>
    /// Tests that calling PrintStructure with a simple class generates the expected file.
    /// </summary>
    [Test]
    public void PrintStructure_SimpleClass_GeneratesFile()
    {
        this.reflector.PrintStructure(typeof(SimpleTestClass));

        string expectedFile = Path.Combine(this.testOutputPath, "SimpleTestClass.g.cs");
        Assert.That(File.Exists(expectedFile), Is.True);

        string content = File.ReadAllText(expectedFile);

        Assert.That(content, Does.Contain("class SimpleTestClass"));
        Assert.That(content, Does.Contain("public int Id"));
        //Assert.That(content, Does.Contain("public string Name"));
        //Assert.That(content, Does.Contain("public SimpleTestClass()"));
    }

    /// <summary>
    /// Tests that calling PrintStructure with a generic class generates the expected file with generic parameters.
    /// </summary>
    [Test]
    public void PrintStructure_GenericClass_GeneratesWithGenericParameters()
    {
        this.reflector.PrintStructure(typeof(GenericTestClass<>));

        string filePath = Path.Combine(this.testOutputPath, "GenericTestClass.g.cs");
        Assert.That(File.Exists(filePath), Is.True);

        string content = File.ReadAllText(filePath);

        Assert.That(content, Does.Contain("class GenericTestClass<T>"));
        //Assert.That(content, Does.Contain("public T Value"));
        Assert.That(content, Does.Contain("return default(T);"));
    }

    /// <summary>
    /// Tests that calling PrintStructure with a class containing a nested class generates the expected nested type.
    /// </summary>
    [Test]
    public void PrintStructure_ClassWithNested_GeneratesNestedType()
    {
        this.reflector.PrintStructure(typeof(ClassWithNested));

        string filePath = Path.Combine(this.testOutputPath, "ClassWithNested.g.cs");
        string content = File.ReadAllText(filePath);

        Assert.That(content, Does.Contain("class ClassWithNested"));
        Assert.That(content, Does.Contain("class NestedClass"));
    }

    /// <summary>
    /// Tests that calling DiffClasses with identical types prints no differences.
    /// </summary>
    [Test]
    public void DiffClasses_IdenticalTypes_PrintsNoDifferences()
    {
        using var sw = new StringWriter();
        Console.SetOut(sw);

        this.reflector.DiffClasses(typeof(SimpleTestClass), typeof(SimpleTestClass));

        string output = sw.ToString();

        Assert.That(output, Does.Not.Contain("but not in"));
        Assert.That(output, Does.Not.Contain("differs"));
    }

    /// <summary>
    /// Tests that calling DiffClasses with different classes prints the expected differences.
    /// </summary>
    [Test]
    public void DiffClasses_DifferentClasses_PrintsDifferences()
    {
        using var sw = new StringWriter();
        Console.SetOut(sw);

        this.reflector.DiffClasses(typeof(ClassA), typeof(ClassB));

        string output = sw.ToString();

        Assert.That(output, Does.Contain("ExtraField"));
        Assert.That(output, Does.Contain("DoSomething"));
        Assert.That(output, Does.Contain("DoSomethingElse"));
    }
}

// ======== Test Classes ========

public class SimpleTestClass
{
    public int Id;
    public string? Name { get; set; }

    public SimpleTestClass() { }
}

public class GenericTestClass<T>
{
    public T? Value { get; set; }

    public T? GetValue() => this.Value;
}

public class ClassWithNested
{
    public int Field;

    public class NestedClass
    {
        public string? Data { get; set; }
    }
}

public class ClassA
{
    public int CommonField;
    public int ExtraField;

    public void CommonMethod() { }
    public void DoSomething() { }
}

public class ClassB
{
    public int CommonField;

    public void CommonMethod() { }
    public void DoSomethingElse() { }
}