// <copyright file="LazyMultiThreadTests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Lazy.Tests;

/// <summary>
/// Contains tests verifying thread-safety of the multi-threaded lazy implementation.
/// </summary>
[TestFixture]
public class LazyMultiThreadTests
{
    /// <summary>
    /// Verifies that under high concurrent access, the supplier is called exactly once.
    /// </summary>
    [Test]
    public void Get_HighConcurrency_SupplierCalledExactlyOnce()
    {
        int callCount = 0;
        var lazy = new LazyMultiThread<int>(() =>
        {
            Interlocked.Increment(ref callCount);
            Thread.Sleep(10);
            return 42;
        });

        const int threadCount = 50;

        Parallel.For(0, threadCount, _ => lazy.Get());

        Assert.Multiple(() =>
        {
            Assert.That(callCount, Is.EqualTo(1), "Supplier must be invoked exactly once even under concurrency.");
            int result = lazy.Get();
            Assert.That(result, Is.EqualTo(42));
        });
    }

    /// <summary>
    /// Verifies that after computation, subsequent calls are lock-free and very fast.
    /// </summary>
    [Test]
    public void Get_AfterComputation_NoSynchronizationOverhead()
    {
        var lazy = new LazyMultiThread<string>(() => "computed");

        _ = lazy.Get();

        const int iterations = 1_000_000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            _ = lazy.Get();
        }

        stopwatch.Stop();

        // Assert - this is not strict, but should be very fast (<100ms on typical hardware)
        Console.WriteLine($"Time for {iterations} cached calls: {stopwatch.ElapsedMilliseconds} ms");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(200), "Cached access should be extremely fast with minimal overhead.");
    }

    /// <summary>
    /// Verifies that even if supplier throws, the exception is propagated and supplier may be called again on next access.
    /// </summary>
    [Test]
    public void Get_SupplierThrows_ExceptionPropagatedAndNotCached()
    {
        int attempt = 0;
        var lazy = new LazyMultiThread<object>(() =>
        {
            attempt++;
            if (attempt == 1)
            {
                throw new InvalidOperationException("First failure");
            }

            return "success";
        });

        Assert.Throws<InvalidOperationException>(() => lazy.Get());

        string result = (string)lazy.Get();
        Assert.That(result, Is.EqualTo("success"));
        Assert.That(attempt, Is.EqualTo(2));
    }
}