// <copyright file="MyThreadPoolTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ThreadPool.Tests;

using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

/// <summary>
/// Unit tests for <see cref="MyThreadPool"/> and <see cref="IMyTask{TResult}"/>.
/// </summary>
[TestFixture]
public class MyThreadPoolTests
{
    private MyThreadPool? pool;

    /// <summary>
    /// Sets up a new thread pool with 3 worker threads before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        this.pool = new MyThreadPool(3);
    }

    /// <summary>
    /// Cleans up the thread pool after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        this.pool?.Dispose();
    }

    /// <summary>
    /// Verifies that <see cref="MyThreadPool.Submit{TResult}(Func{TResult})"/> returns the correct result.
    /// </summary>
    [Test]
    public void Submit_ReturnsCorrectResult()
    {
        var task = this.pool!.Submit(() => 42);
        Assert.That(task.Result, Is.EqualTo(42));
    }

    /// <summary>
    /// Verifies that <see cref="IMyTask{TResult}.ContinueWith{TNewResult}(Func{TResult, TNewResult})"/>
    /// executes after the original task completes.
    /// </summary>
    [Test]
    public void ContinueWith_ExecutesAfterCompletion()
    {
        var task1 = this.pool!.Submit(() => 10);
        var task2 = task1.ContinueWith(x => x * 2);
        var task3 = task2.ContinueWith(x => x.ToString());

        Assert.That(task3.Result, Is.EqualTo("20"));
    }

    /// <summary>
    /// Verifies that multiple continuations from the same task execute independently.
    /// </summary>
    [Test]
    public void MultipleContinuations_RunIndependently()
    {
        var task = this.pool!.Submit(() => 5);
        var t1 = task.ContinueWith(x => x + 1);
        var t2 = task.ContinueWith(x => x * 10);

        Assert.Multiple(() =>
        {
            Assert.That(t1.Result, Is.EqualTo(6));
            Assert.That(t2.Result, Is.EqualTo(50));
        });
    }

    /// <summary>
    /// Verifies that an exception in the task function is wrapped in <see cref="AggregateException"/>.
    /// </summary>
    [Test]
    public void Exception_InTask_PropagatesViaAggregateException()
    {
        var task = this.pool!.Submit<int>(() =>
        {
            throw new InvalidOperationException("boom");
        });

        var ex = Assert.Throws<AggregateException>(() =>
        {
            var unused = task.Result;
        });
        Assert.That(ex!.InnerException!.Message, Is.EqualTo("boom"));
    }

    /// <summary>
    /// Verifies that an exception in a continuation task is propagated correctly.
    /// </summary>
    [Test]
    public void Exception_InContinuation_Propagates()
    {
        var task1 = this.pool!.Submit(() => 1);
        var task2 = task1.ContinueWith<int>(_ => throw new ArgumentException("fail"));

        var ex = Assert.Throws<AggregateException>(() =>
        {
            var unused = task2.Result;
        });
        Assert.That(ex!.InnerException!.Message, Is.EqualTo("fail"));
    }

    /// <summary>
    /// Verifies that <see cref="MyThreadPool.Shutdown"/> waits for all tasks to complete.
    /// </summary>
    [Test]
    public void Shutdown_WaitsForAllTasksToComplete()
    {
        var completed = 0;
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => this.pool!.Submit(() =>
            {
                Thread.Sleep(50);
                Interlocked.Increment(ref completed);
                return completed;
            }))
            .ToArray();

        this.pool!.Shutdown();

        Assert.That(completed, Is.EqualTo(10));
        foreach (var t in tasks)
        {
            Assert.That(t.IsCompleted, Is.True);
        }
    }

    /// <summary>
    /// Verifies that submitting a task after shutdown throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [Test]
    public void Submit_AfterShutdown_ThrowsInvalidOperationException()
    {
        this.pool!.Shutdown();
        Assert.Throws<InvalidOperationException>(() => this.pool.Submit(() => 1));
    }

    /// <summary>
    /// Verifies that at least N worker threads are active when enough tasks are submitted.
    /// </summary>
    [Test]
    public void AtLeastNThreads_AreRunning_WhenTasksAreSubmitted()
    {
        const int threadCount = 4;
        using var pool = new MyThreadPool(threadCount);
        var started = new ManualResetEventSlim(false);
        var activeCount = 0;
        var maxActive = 0;

        for (int i = 0; i < threadCount * 2; i++)
        {
            pool.Submit<object?>(() =>
            {
                var current = Interlocked.Increment(ref activeCount);
                maxActive = Math.Max(maxActive, current);
                if (current >= threadCount)
                {
                    started.Set();
                }

                Thread.Sleep(100);
                Interlocked.Decrement(ref activeCount);
                return null;
            });
        }

        Assert.That(started.Wait(5000), Is.True, "Not all threads started.");
        Assert.That(maxActive, Is.AtLeast(threadCount), $"Only {maxActive} threads active, expected {threadCount}.");
    }

    /// <summary>
    /// Verifies that a continuation added before shutdown still completes.
    /// </summary>
    [Test]
    public void ContinueWith_AfterShutdown_StillCompletes()
    {
        var task = this.pool!.Submit(() => 100);
        this.pool!.Shutdown();

        var cont = task.ContinueWith(x => x + 50);
        Assert.That(cont.Result, Is.EqualTo(150));
    }

    /// <summary>
    /// Verifies that submitting a <c>null</c> function throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Test]
    public void NullFunc_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            this.pool!.Submit<int>(null!);
        });
    }

    /// <summary>
    /// Verifies that <see cref="IMyTask{TResult}.ContinueWith{TNewResult}"/> with <c>null</c> throws.
    /// </summary>
    [Test]
    public void ContinueWith_NullContinuation_ThrowsArgumentNullException()
    {
        var task = this.pool!.Submit(() => 1);
        Assert.Throws<ArgumentNullException>(() =>
        {
            task.ContinueWith<int>(null!);
        });
    }

    /// <summary>
    /// Verifies that <see cref="IMyTask{TResult}.IsCompleted"/> becomes <c>true</c> after task finishes.
    /// </summary>
    [Test]
    public void IsCompleted_IsTrue_AfterTaskFinishes()
    {
        var task = this.pool!.Submit<object?>(() =>
        {
            Thread.Sleep(50);
            return null;
        });

        var unused = task.Result;
        Assert.That(task.IsCompleted, Is.True);
    }
}