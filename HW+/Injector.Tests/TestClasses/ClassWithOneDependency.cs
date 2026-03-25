// <copyright file="ClassWithOneDependency.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector.Tests;

/// <summary>
/// Test class with one dependency.
/// </summary>
public class ClassWithOneDependency
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassWithOneDependency"/> class.
    /// </summary>
    /// <param name="serviceA">The service A dependency.</param>
    public ClassWithOneDependency(IServiceA serviceA)
    {
        this.Dependency = serviceA;
    }

    /// <summary>
    /// Gets the service A dependency.
    /// </summary>
    public IServiceA Dependency { get; }
}
