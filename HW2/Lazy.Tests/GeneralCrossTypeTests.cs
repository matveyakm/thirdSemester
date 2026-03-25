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
    /// Verifies that multiple Get() calls return the same value.
    /// and the supplier is invoked only once (single-threaded version).
    /// </summary>
    [Test]
    public void MultipleCalls_ReturnsSameValue_SingleThread()
    {
        var callCount = 0;
        var lazy = new LazySingleThread<string>(() =>
        {
            callCount++;
            return "test-value";
        });

        var first = lazy.Get();
        var second = lazy.Get();
        var third = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo("test-value"));
            Assert.That(second, Is.EqualTo("test-value"));
            Assert.That(third, Is.EqualTo("test-value"));
            Assert.That(callCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Verifies that multiple Get() calls return the same value.
    /// and the supplier is invoked only once (multithreaded version).
    /// </summary>
    [Test]
    public void MultipleCalls_ReturnsSameValue_MultiThread()
    {
        var callCount = 0;
        var lazy = new LazyMultiThread<string>(() =>
        {
            callCount++;
            return "test-value";
        });

        var first = lazy.Get();
        var second = lazy.Get();
        var third = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo("test-value"));
            Assert.That(second, Is.EqualTo("test-value"));
            Assert.That(third, Is.EqualTo("test-value"));
            Assert.That(callCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Checks that null returned by the supplier is correctly cached.
    /// and returned on subsequent calls (single-threaded).
    /// </summary>
    [Test]
    public void SupplierReturnsNull_CachesNull_SingleThread()
    {
        var lazy = new LazySingleThread<string?>(() => null);

        var first = lazy.Get();
        var second = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Null);
            Assert.That(second, Is.Null);
        });
    }
}