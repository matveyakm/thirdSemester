// <copyright file="LazySingleThread.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// Provides a thread-unsafe implementation of <see cref="ILazy{T}"/>.
/// Suitable only for single-threaded scenarios.
/// </summary>
/// <typeparam name="T">The type of the value to be computed lazily.</typeparam>
/// <remarks>
/// This implementation does not use any synchronization mechanisms
/// and assumes that access occurs from a single thread only.
/// </remarks>
public sealed class LazySingleThread<T> : ILazy<T>
{
    private Func<T>? supplier;
    private T? value;
    private bool isComputed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazySingleThread{T}"/> class.
    /// </summary>
    /// <param name="supplier">The delegate that produces the value when first needed.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplier"/> is null.
    /// </exception>
    public LazySingleThread(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
    }

    /// <inheritdoc />
    public T Get()
    {
        if (!this.isComputed)
        {
            this.value = this.supplier!();
            this.isComputed = true;
            this.supplier = null;
        }

        return this.value!;
    }
}