// <copyright file="TestAttribute.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;

namespace MyNUnit.Attributes;

/// <summary>
/// Attribute to mark test methods.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the type of expected exception.
    /// </summary>
    public Type? Expected { get; set; }

    /// <summary>
    /// Gets or sets the ignore reason. If set, the test is ignored.
    /// </summary>
    public string? Ignore { get; set; }
}
