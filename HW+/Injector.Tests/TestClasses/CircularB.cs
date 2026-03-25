// <copyright file="CircularB.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector.Tests;

/// <summary>
/// Test class for circular dependency detection - part B.
/// </summary>
public class CircularB
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircularB"/> class.
    /// </summary>
    /// <param name="a">The circular A dependency.</param>
    public CircularB(CircularA a)
    {
        this.A = a;
    }

    /// <summary>
    /// Gets the circular A dependency.
    /// </summary>
    public CircularA A { get; }
}
