// <copyright file="InjectorTests.cs" company="matveyakm">
//  Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector.Tests;

/// <summary>
/// Unit tests for the Injector class.
/// </summary>
[TestFixture]
public class InjectorTests
{
    /// <summary>
    /// Tests that a class without dependencies is created successfully.
    /// </summary>
    [Test]
    public void Initialize_ClassWithoutDependencies_CreatesInstance()
    {
        var classes = new List<Type> { typeof(ClassWithoutDependencies) };
        var result = Injector.Initialize("Injector.Tests.ClassWithoutDependencies", classes);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ClassWithoutDependencies>());
    }

    /// <summary>
    /// Tests that a class with one dependency is created with resolved dependency.
    /// </summary>
    [Test]
    public void Initialize_ClassWithOneDependency_CreatesInstanceWithResolvedDependency()
    {
        var classes = new List<Type> { typeof(ClassWithOneDependency), typeof(ServiceA) };
        var result = Injector.Initialize("Injector.Tests.ClassWithOneDependency", classes);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ClassWithOneDependency>());

        var instance = (ClassWithOneDependency)result;
        Assert.That(instance.Dependency, Is.Not.Null);
        Assert.That(instance.Dependency, Is.InstanceOf<ServiceA>());
    }

    /// <summary>
    /// Tests that a class with two dependencies is created with both resolved.
    /// </summary>
    [Test]
    public void Initialize_ClassWithTwoDependencies_CreatesInstanceWithBothResolved()
    {
        var classes = new List<Type> { typeof(ClassWithTwoDependencies), typeof(ServiceA), typeof(ServiceB) };
        var result = Injector.Initialize("Injector.Tests.ClassWithTwoDependencies", classes);

        Assert.That(result, Is.Not.Null);
        var instance = (ClassWithTwoDependencies)result;
        Assert.That(instance.ServiceA, Is.Not.Null);
        Assert.That(instance.ServiceB, Is.Not.Null);
    }

    /// <summary>
    /// Tests that an exception is thrown when multiple implementations exist for one dependency.
    /// </summary>
    [Test]
    public void Initialize_MultipleImplementationsForOneDependency_ThrowsException()
    {
        var classes = new List<Type> { typeof(ClassWithOneDependency), typeof(ServiceA), typeof(ServiceAImpl2) };

        Assert.Throws<InvalidOperationException>(() =>
            Injector.Initialize("Injector.Tests.ClassWithOneDependency", classes));
    }

    /// <summary>
    /// Tests that an exception is thrown when no implementation is found for a dependency.
    /// </summary>
    [Test]
    public void Initialize_NoImplementationForDependency_ThrowsException()
    {
        var classes = new List<Type> { typeof(ClassWithOneDependency) };

        Assert.Throws<InvalidOperationException>(() =>
            Injector.Initialize("Injector.Tests.ClassWithOneDependency", classes));
    }

    /// <summary>
    /// Tests that an exception is thrown when a circular dependency is detected.
    /// </summary>
    [Test]
    public void Initialize_CircularDependency_ThrowsException()
    {
        var classes = new List<Type> { typeof(CircularA), typeof(CircularB) };

        Assert.Throws<InvalidOperationException>(() =>
            Injector.Initialize("Injector.Tests.CircularA", classes));
    }

    /// <summary>
    /// Tests that an abstract class can be used as a dependency.
    /// </summary>
    [Test]
    public void Initialize_AbstractClassAsDependency_CreatesInstance()
    {
        var classes = new List<Type> { typeof(ClassDependingOnAbstract), typeof(ConcreteService) };
        var result = Injector.Initialize("Injector.Tests.ClassDependingOnAbstract", classes);

        Assert.That(result, Is.Not.Null);
        var instance = (ClassDependingOnAbstract)result;
        Assert.That(instance.Service, Is.Not.Null);
        Assert.That(instance.Service, Is.InstanceOf<ConcreteService>());
    }

    /// <summary>
    /// Tests that an exception is thrown when the root class does not exist.
    /// </summary>
    [Test]
    public void Initialize_NonExistentClass_ThrowsException()
    {
        var classes = new List<Type> { typeof(ServiceA) };

        Assert.Throws<InvalidOperationException>(() =>
            Injector.Initialize("NonExistent.Class", classes));
    }

    /// <summary>
    /// Tests that an exception is thrown when the root class is not in available classes.
    /// </summary>
    [Test]
    public void Initialize_ClassNotInAvailableClasses_ThrowsException()
    {
        var classes = new List<Type> { typeof(ServiceA) };

        Assert.Throws<InvalidOperationException>(() =>
            Injector.Initialize("Injector.Tests.ClassWithOneDependency", classes));
    }
}
