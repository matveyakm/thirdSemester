// <copyright file="BeforeClassAttribute.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;

namespace MyNUnit.Attributes;

/// <summary>
/// Attribute to mark static methods that should run before all tests in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeClassAttribute : Attribute
{
}
