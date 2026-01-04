// <copyright file="AfterAttribute.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;

namespace MyNUnit.Attributes;

/// <summary>
/// Attribute to mark methods that should run after each test in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterAttribute : Attribute
{
}
