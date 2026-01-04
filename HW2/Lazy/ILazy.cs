// <copyright file="ILazy.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// Represents a lazy computation that evaluates a value on first access
/// and caches the result for subsequent calls.
/// </summary>
/// <typeparam name="T">The type of the value to be computed lazily.</typeparam>
public interface ILazy<out T>
{
    /// <summary>
    /// Gets the lazily computed value.
    /// The supplier is invoked only on the first call.
    /// Subsequent calls return the cached result.
    /// </summary>
    /// <returns>The computed value, which may be null.</returns>
    T Get();
}