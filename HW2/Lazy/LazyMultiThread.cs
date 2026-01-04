// <copyright file="LazyMultiThread.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Lazy;

/// <summary>
/// Provides a thread-safe implementation of <see cref="ILazy{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the value to be computed lazily.</typeparam>
/// <remarks>
/// This implementation ensures thread safety while minimizing synchronization overhead:
/// - The first thread computes the value under a lock.
/// - Subsequent accesses return the cached value without locking.
/// - Uses double-checked locking pattern with <see cref="Volatile"/> for proper memory visibility.
/// </remarks>
public sealed class LazyMultiThread<T> : ILazy<T>
{
    private readonly object syncRoot = new();
    private Func<T>? supplier;
    private T? value;
    private volatile bool isComputed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyMultiThread{T}"/> class.
    /// </summary>
    /// <param name="supplier">The delegate that produces the value when first needed.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplier"/> is null.
    /// </exception>
    public LazyMultiThread(Func<T> supplier)
    {
        this.supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
    }

    /// <inheritdoc />
    public T Get()
    {
        if (this.isComputed)
        {
            return this.value!;
        }

        lock (this.syncRoot)
        {
            if (!this.isComputed)
            {
                this.value = this.supplier!();
                this.isComputed = true;
                this.supplier = null;
            }
        }

        return this.value!;
    }
}