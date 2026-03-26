// <copyright file="MyThreadPoolTests.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ThreadPool.Test;

using System.Collections.Concurrent;

/// <summary>
/// Custom unit tests for MyThreadPool covering basic submission, exceptions, continuations,
/// concurrency, shutdown scenarios and edge cases.
/// </summary>
public class MyThreadPoolTests
{
    /// <summary>
    /// Verifies that the constructor throws ArgumentOutOfRangeException
    /// when zero worker threads are requested.
    /// </summary>
    [Test]
    public void MyThreadPool_Constructor_ZeroThreads_ThrowsArgumentOutOfRangeException()
     => Assert.Throws<ArgumentOutOfRangeException>(() => new MyThreadPool(0));

    /// <summary>
    /// Tests that a task throwing an exception propagates AggregateException,
    /// but the pool continues to accept and execute new tasks successfully.
    /// </summary>
    [Test]
    public void MyThreadPool_Submit_TaskWithException_ThrowsAggregateExceptionButPoolContinues()
    {
        var pool = new MyThreadPool(2);

        var failingTask = () =>
        {
            int[] numbers = [];
            if (numbers.Length == 0)
            {
                throw new InvalidOperationException("Test exception");
            }

            return numbers.Sum(x => x * x);
        };

        var task = pool.Submit(failingTask);

        Assert.Throws<AggregateException>(() => _ = task.Result);

        try
        {
            _ = task.Result;
        }
        catch (AggregateException ex)
        {
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ex.InnerException!.Message, Is.EqualTo("Test exception"));
        }

        var anotherTask = pool.Submit(() => 52);
        Assert.Multiple(() =>
        {
            Assert.That(anotherTask.Result, Is.EqualTo(52));
            Assert.That(anotherTask.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// Tests submitting a large number of tasks and verifies that all of them
    /// complete with correct results and IsCompleted flag is set.
    /// </summary>
    [Test]
    public void MyThreadPool_Submit_MultipleTasks_AllTasksCompleteCorrectly()
    {
        var pool = new MyThreadPool(5);
        const int taskCount = 64;

        var tasks = new IMyTask<string>[taskCount];
        for (var i = 1; i <= taskCount; i++)
        {
            var id = i;
            tasks[id - 1] = pool.Submit(() => string.Concat(Enumerable.Repeat($"{id}", id)));
        }

        for (var i = 1; i <= taskCount; i++)
        {
            var expectedResult = string.Concat(Enumerable.Repeat($"{i}", i));
            Assert.Multiple(() =>
            {
                Assert.That(tasks[i - 1].Result, Is.EqualTo(expectedResult));
                Assert.That(tasks[i - 1].IsCompleted, Is.True);
            });
        }

        pool.Shutdown();
    }

    /// <summary>
    /// Tests that an exception thrown inside a continuation is correctly wrapped
    /// in AggregateException and propagated through .Result.
    /// </summary>
    [Test]
    public void MyThreadPool_ContinueWith_ContinuationThrowsException_ThrowsAggregateException()
    {
        var pool = new MyThreadPool(5);

        var initialTask = pool.Submit(() => 5);

        var continuationTask = initialTask.ContinueWith<int>(_ => throw new InvalidOperationException());
        Exception? caughtException = null;
        try
        {
            _ = continuationTask.Result;
        }
        catch (AggregateException ex)
        {
            caughtException = ex.InnerException;
        }

        Assert.That(caughtException, Is.TypeOf<InvalidOperationException>());
    }

    /// <summary>
    /// Tests basic ContinueWith functionality: a continuation transforms the original
    /// task result and both tasks complete correctly.
    /// </summary>
    [Test]
    public void MyThreadPool_ContinueWith()
    {
        var pool = new MyThreadPool(2);
        int[] numbers = [-10, 2, 3, 5, 7, 11, 10021];
        var task = pool.Submit(() => numbers.Sum(x => x * x));
        var expectedTaskResult = numbers.Sum(x => x * x);

        var continuationTask = task.ContinueWith<string>(x => x.ToString());
        var expectedContinuationTaskResult = expectedTaskResult.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(continuationTask.Result, Is.EqualTo(expectedContinuationTaskResult));
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.Result, Is.EqualTo(expectedTaskResult));
        });

        pool.Shutdown();
    }

    /// <summary>
    /// Verifies that at least the requested number of worker threads become active
    /// when enough concurrent tasks are submitted.
    /// </summary>
    [Test]
    public void MyThreadPool_HasAtLeastNWorkingThreads()
    {
        const int n = 4;

        var pool = new MyThreadPool(n);

        var concurrentTasks = 0;
        var maxConcurrent = 0;
        var lockObj = new object();

        var startSignal = new ManualResetEvent(false);

        for (var i = 0; i < n * 2; i++)
        {
            pool.Submit(() =>
            {
                startSignal.WaitOne();

                lock (lockObj)
                {
                    concurrentTasks++;
                    if (concurrentTasks > maxConcurrent)
                    {
                        maxConcurrent = concurrentTasks;
                    }
                }

                Thread.Sleep(100);

                lock (lockObj)
                {
                    concurrentTasks--;
                }

                return 0;
            });
        }

        Thread.Sleep(100);

        startSignal.Set();

        Thread.Sleep(50);

        Assert.That(maxConcurrent, Is.GreaterThanOrEqualTo(n));

        pool.Shutdown();
    }

    /// <summary>
    /// Tests that a single task can have multiple independent continuations
    /// that all execute correctly and independently.
    /// </summary>
    [Test]
    public void MyThreadPool_OneTask_MultipleContinuations()
    {
        var pool = new MyThreadPool(2);

        var initialTask = pool.Submit(() => 10);

        var continuationPr = initialTask.ContinueWith(x => x * 2);
        var continuationSum = initialTask.ContinueWith(x => x + 5);

        Assert.Multiple(() =>
        {
            Assert.That(initialTask.Result, Is.EqualTo(10));
            Assert.That(continuationPr.Result, Is.EqualTo(20));
            Assert.That(continuationSum.Result, Is.EqualTo(15));

            Assert.That(initialTask.IsCompleted, Is.True);
            Assert.That(continuationPr.IsCompleted, Is.True);
            Assert.That(continuationSum.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// Tests that Submit with a valid task completes correctly and returns the expected result.
    /// Also verifies that IsCompleted becomes true after execution.
    /// </summary>
    [Test]
    public void MyThreadPool_Submit_ValidTask_CompleteAndReturnsResult()
    {
        var pool = new MyThreadPool(2);
        int[] numbers = [555, 1, -100, 5, 2, 3, 7];
        var expectedResult = numbers.Sum(x => x * x);

        var task = pool.Submit(() => numbers.Sum(x => x * x));

        Assert.Multiple(() =>
        {
            Assert.That(task.Result, Is.EqualTo(expectedResult));
            Assert.That(task.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// Tests a chain of continuations (task -> cont1 -> cont2 -> cont3)
    /// where each step transforms the previous result correctly.
    /// </summary>
    [Test]
    public void MyThreadPool_ChainOfContinuations()
    {
        var pool = new MyThreadPool(2);

        var task = pool.Submit(() => 5);
        var continuation1 = task.ContinueWith(x => x * 2);
        var continuation2 = continuation1.ContinueWith(x => x + 3);
        var continuation3 = continuation2.ContinueWith(x => x.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(task.Result, Is.EqualTo(5));
            Assert.That(continuation1.Result, Is.EqualTo(10));
            Assert.That(continuation2.Result, Is.EqualTo(13));
            Assert.That(continuation3.Result, Is.EqualTo("13"));

            Assert.That(task.IsCompleted, Is.True);
            Assert.That(continuation1.IsCompleted, Is.True);
            Assert.That(continuation2.IsCompleted, Is.True);
            Assert.That(continuation3.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// Tests a complex continuation tree with multiple branches and a long chain.
    /// Verifies correct results and completion status for every task in the structure.
    /// </summary>
    [Test]
    public void MyThreadPool_ComplexContinuationStructure()
    {
        var pool = new MyThreadPool(4);

        var root = pool.Submit(() => 100);

        var branch1 = root.ContinueWith(x => x / 2);
        var branch2 = root.ContinueWith(x => x * 2);

        var branch1Chain1 = branch1.ContinueWith(x => x + 10);
        var branch1Chain2 = branch1.ContinueWith(x => x - 10);

        var branch2Chain1 = branch2.ContinueWith(x => x.ToString());
        var branch2Chain2 = branch2.ContinueWith(x => x / 10);

        var longChain = branch1Chain1
            .ContinueWith(x => x * 3)
            .ContinueWith(x => x.ToString() + "!");

        Assert.Multiple(() =>
        {
            Assert.That(root.Result, Is.EqualTo(100));

            Assert.That(branch1.Result, Is.EqualTo(50));
            Assert.That(branch2.Result, Is.EqualTo(200));

            Assert.That(branch1Chain1.Result, Is.EqualTo(60));
            Assert.That(branch1Chain2.Result, Is.EqualTo(40));
            Assert.That(branch2Chain1.Result, Is.EqualTo("200"));
            Assert.That(branch2Chain2.Result, Is.EqualTo(20));

            Assert.That(longChain.Result, Is.EqualTo("180!"));

            Assert.That(root.IsCompleted, Is.True);
            Assert.That(branch1.IsCompleted, Is.True);
            Assert.That(branch2.IsCompleted, Is.True);
            Assert.That(branch1Chain1.IsCompleted, Is.True);
            Assert.That(branch1Chain2.IsCompleted, Is.True);
            Assert.That(branch2Chain1.IsCompleted, Is.True);
            Assert.That(branch2Chain2.IsCompleted, Is.True);
            Assert.That(longChain.IsCompleted, Is.True);
        });

        pool.Shutdown();
    }

    /// <summary>
    /// Tests concurrent submission of many tasks from multiple threads simultaneously.
    /// Verifies no exceptions during submission and all tasks complete correctly.
    /// </summary>
    [Test]
    public void MyThreadPool_ConcurrentSubmit_ManyTasks()
    {
        const int threadCount = 4;
        const int tasksPerThread = 250;

        var pool = new MyThreadPool(threadCount);
        var tasks = new List<IMyTask<int>>();
        var lockObj = new object();
        var exceptions = new ConcurrentBag<Exception>();

        var threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (var j = 0; j < tasksPerThread; j++)
                {
                    try
                    {
                        var task = pool.Submit(() => Thread.CurrentThread.ManagedThreadId);
                        lock (lockObj)
                        {
                            tasks.Add(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Multiple(() =>
        {
            Assert.That(exceptions, Is.Empty, "No exceptions should be thrown during concurrent Submit");
            Assert.That(tasks, Has.Count.EqualTo(threadCount * tasksPerThread));
        });

        foreach (var task in tasks)
        {
            Assert.DoesNotThrow(() => _ = task.Result);
            Assert.That(task.IsCompleted, Is.True);
        }

        pool.Shutdown();
    }

    /// <summary>
    /// Stress test for race condition: submits continuations from one thread
    /// while another thread calls Shutdown(). Verifies that root task and
    /// already-registered continuations complete successfully.
    /// </summary>
    [Test]
    public void MyThreadPool_ShutdownDuringContinueWith()
    {
        const int iterations = 50;

        for (var i = 0; i < iterations; i++)
        {
            var pool = new MyThreadPool(2);
            var rootTask = pool.Submit(() => 100);

            var continuations = new List<IMyTask<int>>();
            var continuationLock = new object();

            var continuationThread = new Thread(() =>
            {
                for (var iteration = 0; iteration < 20; iteration++)
                {
                    var currentIteration = iteration;
                    try
                    {
                        var continuation = rootTask.ContinueWith(x => x + currentIteration);
                        lock (continuationLock)
                        {
                            continuations.Add(continuation);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            });

            var shutdownThread = new Thread(() =>
            {
                Thread.Sleep(GetRandomDelay(0, 15));
                pool.Shutdown();
            });

            continuationThread.Start();
            shutdownThread.Start();

            continuationThread.Join();
            shutdownThread.Join();

            Assert.That(rootTask.Result, Is.EqualTo(100));

            foreach (var continuation in continuations)
            {
                if (continuation.IsCompleted)
                {
                    Assert.DoesNotThrow(() => _ = continuation.Result);
                }
            }
        }
    }

    private static int GetRandomDelay(int min, int max)
    {
        var random = new Random(Guid.NewGuid().GetHashCode());
        return random.Next(min, max);
    }
}