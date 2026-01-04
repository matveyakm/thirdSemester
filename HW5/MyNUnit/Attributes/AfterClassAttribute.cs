// <copyright file="AfterClassAttribute.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;

namespace MyNUnit.Attributes;

/// <summary>
/// Attribute to mark static methods that should run after all tests in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterClassAttribute : Attribute
{
}
