// <copyright file="AttributeFinderTest.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using MyNUnit.Tests.TestClasses;
using NUnit.Framework;

namespace MyNUnit.Tests;

[TestFixture]
public class AttributeFinderTest
{
    private Dictionary<Type, AttributeFinder.TestClassMethods> FindInCurrentAssembly()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        var testClasses = new Dictionary<Type, AttributeFinder.TestClassMethods>();

        foreach (var type in currentAssembly.GetTypes())
        {
            var testMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<MyNUnit.Attributes.TestAttribute>() != null)
                .ToList();

            if (testMethods.Any())
            {
                var methods = new AttributeFinder.TestClassMethods
                {
                    BeforeClass = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(m => m.GetCustomAttribute<MyNUnit.Attributes.BeforeClassAttribute>() != null),

                    AfterClass = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(m => m.GetCustomAttribute<MyNUnit.Attributes.AfterClassAttribute>() != null),

                    Before = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(m => m.GetCustomAttribute<MyNUnit.Attributes.BeforeAttribute>() != null),

                    After = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .FirstOrDefault(m => m.GetCustomAttribute<MyNUnit.Attributes.AfterAttribute>() != null),

                    Tests = testMethods
                };

                testClasses[type] = methods;
            }
        }

        return testClasses;
    }

    [Test]
    public void FindTestMethods_ReturnsCorrectStructure_ForTestClass()
    {
        var testClasses = FindInCurrentAssembly();

        var testClassType = typeof(TestClassForAttributeFinder);

        Assert.That(testClasses.Keys, Does.Contain(testClassType));

        var methods = testClasses[testClassType];

        Assert.That(methods.BeforeClass, Is.Not.Null);
        Assert.That(methods.AfterClass, Is.Not.Null);
        Assert.That(methods.Before, Is.Not.Null);
        Assert.That(methods.After, Is.Not.Null);
        Assert.That(methods.Tests, Has.Count.EqualTo(2));

        var ignoredTest = methods.Tests
            .FirstOrDefault(m => m.GetCustomAttribute<MyNUnit.Attributes.TestAttribute>()?.Ignore != null);

        Assert.That(ignoredTest, Is.Not.Null);
        Assert.That(ignoredTest!.GetCustomAttribute<MyNUnit.Attributes.TestAttribute>()!.Ignore,
            Is.EqualTo("Not ready"));
    }

    [Test]
    public void FindTestMethods_DoesNotIncludeClassesWithoutTestMethods()
    {
        var testClasses = FindInCurrentAssembly();

        Assert.That(testClasses.Keys, Has.None.EqualTo(typeof(EmptyClass)));
    }
}
