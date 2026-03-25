// <copyright file="ClassWithTwoDependencies.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector.Tests;

/// <summary>
/// Test class with two dependencies.
/// </summary>
public class ClassWithTwoDependencies
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassWithTwoDependencies"/> class.
    /// </summary>
    /// <param name="serviceA">The service A dependency.</param>
    /// <param name="serviceB">The service B dependency.</param>
    public ClassWithTwoDependencies(IServiceA serviceA, IServiceB serviceB)
    {
        this.ServiceA = serviceA;
        this.ServiceB = serviceB;
    }

    /// <summary>
    /// Gets the service A dependency.
    /// </summary>
    public IServiceA ServiceA { get; }

    /// <summary>
    /// Gets the service B dependency.
    /// </summary>
    public IServiceB ServiceB { get; }
}
