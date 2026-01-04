// <copyright file="LazySingleThreadTests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Lazy.Tests;

/// <summary>
/// Contains tests specific to the single-threaded lazy implementation.
/// </summary>
[TestFixture]
public class LazySingleThreadTests
{
    /// <summary>
    /// Verifies that <see cref="LazySingleThread{T}"/> works correctly in a single-threaded scenario.
    /// </summary>
    [Test]
    public void Get_SingleThread_MultipleCalls_CallsSupplierOnce()
    {
        int counter = 0;
        var lazy = new LazySingleThread<int>(() =>
        {
            counter++;
            return 100;
        });

        int a = lazy.Get();
        int b = lazy.Get();
        int c = lazy.Get();

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(100));
            Assert.That(b, Is.EqualTo(100));
            Assert.That(c, Is.EqualTo(100));
            Assert.That(counter, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// Demonstrates that <see cref="LazySingleThread{T}"/> is NOT thread-safe.
    /// Multiple threads may cause the supplier to be called more than once.
    /// </summary>
    [Test]
    public void Get_ConcurrentAccess_SupplierMayBeCalledMultipleTimes()
    {
        int callCount = 0;
        var lazy = new LazySingleThread<int>(() =>
        {
            Interlocked.Increment(ref callCount);
            return 42;
        });

        const int threadCount = 10;

        Parallel.For(0, threadCount, _ => lazy.Get());

        Assert.That(callCount, Is.GreaterThan(0));
    }
}