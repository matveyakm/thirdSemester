// <copyright file="GeneralTests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Lazy.Tests;

using Lazy;

/// <summary>
/// Contains tests that verify common behavior for both implementations using int as the lazy type.
/// </summary>
/// <typeparam name="TLazy">
/// The concrete lazy implementation type being tested.
/// It must implement <see cref="ILazy{int}"/> and have a public constructor accepting <see cref="Func{int}"/>.
/// </typeparam>
[TestFixture(typeof(LazySingleThread<int>))]
[TestFixture(typeof(LazyMultiThread<int>))]
public class GeneralTests<TLazy>
    where TLazy : class, ILazy<int>, new()
{
    /// <summary>
    /// Verifies that <see cref="ILazy{T}.Get"/> returns the value produced by the supplier on first call.
    /// </summary>
    [Test]
    public void Get_FirstCall_ReturnsSupplierResult()
    {
        const int expected = 42;
        ILazy<int> lazy = CreateLazy(() => expected);

        int result = lazy.Get();

        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Verifies that passing a null supplier throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Test]
    public void Constructor_NullSupplier_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => CreateLazy(null!));
    }

    private static ILazy<int> CreateLazy(Func<int> supplier)
    {
        return Activator.CreateInstance(typeof(TLazy), supplier) as ILazy<int>
               ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TLazy).Name}");
    }
}