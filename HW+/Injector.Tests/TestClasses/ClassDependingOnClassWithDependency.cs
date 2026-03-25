// <copyright file="ClassDependingOnClassWithDependency.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector.Tests;

/// <summary>
/// Test class depending on another class with dependency.
/// </summary>
public class ClassDependingOnClassWithDependency
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDependingOnClassWithDependency"/> class.
    /// </summary>
    /// <param name="serviceA">The service A dependency.</param>
    public ClassDependingOnClassWithDependency(IServiceA serviceA)
    {
        this.ServiceA = serviceA;
    }

    /// <summary>
    /// Gets the service A dependency.
    /// </summary>
    public IServiceA ServiceA { get; }
}
