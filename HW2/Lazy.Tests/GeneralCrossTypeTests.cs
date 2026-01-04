// <copyright file="GeneralCrossTypeTests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Lazy.Tests;

using Lazy;

/// <summary>
/// Contains common tests that are not tied to the generic parameter of the fixture (for string and nullable types).
/// </summary>
[TestFixture]
public class GeneralCrossTypeTests
{
    /// <summary>
    /// Verifies that subsequent calls return the same cached value and supplier is called only once.
    /// </summary>
    /// <param name="lazyType">
    /// The type of the lazy implementation to test: either <see cref="LazySingleThread{string}"/> or <see cref="LazyMultiThread{string}"/>.
    /// </param>
    [TestCase(typeof(LazySingleThread<string>))]
    [TestCase(typeof(LazyMultiThread<string>))]
    public void Get_MultipleCalls_ReturnsSameValue(Type lazyType)
    {
        int callCount = 0;
        ILazy<string> lazy = CreateLazy(
            () =>
            {
            callCount++;
            return "test" + callCount;
        },
            lazyType);

        string first = lazy.Get();
        string second = lazy.Get();
        string third = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo("test1"));
            Assert.That(second, Is.EqualTo("test1"));
            Assert.That(third, Is.EqualTo("test1"));
            Assert.That(callCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Verifies that supplier returning null is correctly handled and cached.
    /// </summary>
    /// <param name="lazyType">
    /// The type of the lazy implementation to test: either <see cref="LazySingleThread{string?}"/> or <see cref="LazyMultiThread{string?}"/>.
    /// </param>
    [TestCase(typeof(LazySingleThread<string?>))]
    [TestCase(typeof(LazyMultiThread<string?>))]
    public void Get_SupplierReturnsNull_ReturnsAndCachesNull(Type lazyType)
    {
        ILazy<string?> lazy = CreateLazy<string?>(() => null, lazyType);

        string? first = lazy.Get();
        string? second = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Null);
            Assert.That(second, Is.Null);
        });
    }

    private static ILazy<T> CreateLazy<T>(Func<T> supplier, Type lazyType)
    {
        return Activator.CreateInstance(lazyType, supplier) as ILazy<T>
               ?? throw new InvalidOperationException($"Failed to create instance of {lazyType.Name}");
    }
}