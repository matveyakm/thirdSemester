// <copyright file="CircularA.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector.Tests;

/// <summary>
/// Test class for circular dependency detection - part A.
/// </summary>
public class CircularA
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CircularA"/> class.
    /// </summary>
    /// <param name="b">The circular B dependency.</param>
    public CircularA(CircularB b)
    {
        this.B = b;
    }

    /// <summary>
    /// Gets the circular B dependency.
    /// </summary>
    public CircularB B { get; }
}
