// <copyright file="ClassDependingOnAbstract.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector.Tests;

/// <summary>
/// Test class depending on abstract class.
/// </summary>
public class ClassDependingOnAbstract
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDependingOnAbstract"/> class.
    /// </summary>
    /// <param name="service">The abstract service dependency.</param>
    public ClassDependingOnAbstract(AbstractService service)
    {
        this.Service = service;
    }

    /// <summary>
    /// Gets the abstract service dependency.
    /// </summary>
    public AbstractService Service { get; }
}
