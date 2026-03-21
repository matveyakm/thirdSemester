// <copyright file="AttributeFinderTest.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit.Tests;

using System;
using System.Linq;
using System.Reflection;
using MyNUnit;
using MyNUnit.Tests.TestClasses;
using NUnit.Framework;

/// <summary>
/// Contains unit tests for the <see cref="AttributeFinder"/> class to ensure correct discovery of test methods and attributes.
/// </summary>
[TestFixture]
public class AttributeFinderTest
{
    private static readonly Assembly ThisAssembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Verifies that <see cref="AttributeFinder.FindTestMethodsInAssemblies"/> returns the correct structure for a valid test class.
    /// </summary>
    [Test]
    public void FindTestMethods_ReturnsCorrectStructure_ForTestClass()
    {
        var testClasses = AttributeFinder.FindTestMethodsInAssemblies(new[] { ThisAssembly });

        var testClassType = typeof(TestClassForAttributeFinder);

        Assert.That(testClasses, Contains.Key(testClassType));

        var methods = testClasses[testClassType];

        Assert.Multiple(() =>
        {
            Assert.That(methods.BeforeClass, Is.Not.Null, "BeforeClass method not found");
            Assert.That(methods.AfterClass,  Is.Not.Null, "AfterClass method not found");
            Assert.That(methods.Before,      Is.Not.Null, "Before method not found");
            Assert.That(methods.After,       Is.Not.Null, "After method not found");

            Assert.That(methods.Tests, Has.Count.EqualTo(2), "Expected exactly 2 test methods");
        });

        var ignoredTest = methods.Tests
            .FirstOrDefault(m => m.GetCustomAttribute<MyNUnit.Attributes.TestAttribute>()?.Ignore is { Length: > 0 });

        Assert.That(ignoredTest, Is.Not.Null, "Test with Ignore attribute not found");

        var testAttr = ignoredTest!.GetCustomAttribute<MyNUnit.Attributes.TestAttribute>()!;

        Assert.That(testAttr.Ignore, Is.EqualTo("Not ready"), "Incorrect Ignore message");
    }

    /// <summary>
    /// Verifies that classes without valid test methods or with invalid signatures are excluded from the results.
    /// </summary>
    [Test]
    public void FindTestMethods_DoesNotIncludeClassesWithoutAnyValidTestMethods()
    {
        var testClasses = AttributeFinder.FindTestMethodsInAssemblies(new[] { ThisAssembly });

        Assert.That(testClasses.Keys, Has.None.EqualTo(typeof(EmptyClass)), "Class without test methods should not be included");

        Assert.That(testClasses.Keys, Has.None.EqualTo(typeof(ClassWithOnlyStaticTest)), "Class with only static [Test] methods should not be included");

        Assert.That(testClasses.Keys, Has.None.EqualTo(typeof(ClassWithTestWithParameters)), "Class with [Test] method having parameters should not be included");
    }

    /// <summary>
    /// Verifies that the discovery mechanism respects and validates method signatures (e.g., non-static, void return type, no parameters).
    /// </summary>
    [Test]
    public void FindTestMethods_RespectsMethodSignatureValidation()
    {
        var testClasses = AttributeFinder.FindTestMethodsInAssemblies(new[] { ThisAssembly });

        var testClassType = typeof(TestClassForAttributeFinder);

        Assert.That(testClasses, Contains.Key(testClassType));

        var methods = testClasses[testClassType];

        var invalidTestsCount = methods.Tests.Count(m =>
            m.IsStatic ||
            m.ReturnType != typeof(void) ||
            m.GetParameters().Length > 0);

        Assert.That(invalidTestsCount, Is.Zero, "Methods with invalid signature (static / non-void / with parameters) should be filtered out");
    }
}