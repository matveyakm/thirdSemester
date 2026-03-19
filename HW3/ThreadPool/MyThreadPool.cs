// <copyright file="MyThreadPool.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ThreadPool;

using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A simple thread pool with a fixed number of worker threads.
/// </summary>
public sealed class MyThreadPool : IDisposable
{
    private readonly Thread[] threads;
    private readonly Queue<Action> taskQueue = new();
    private readonly Lock queueLock = new();
    private readonly ManualResetEvent workAvailable = new(false);
    private readonly CancellationTokenSource cts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    /// <param name="threadCount">Number of worker threads. Must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="threadCount"/> is zero or negative.</exception>
    public MyThreadPool(int threadCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threadCount);
        this.threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            this.threads[i] = new Thread(this.WorkerLoop);
            this.threads[i].Start();
        }
    }

    /// <summary>
    /// Submits a task to the thread pool for execution.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
    /// <param name="func">The function to execute. Cannot be <c>null</c>.</param>
    /// <returns>An <see cref="IMyTask{TResult}"/> representing the submitted task.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the pool is shutting down.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is <c>null</c>.</exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        if (func is null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        if (this.cts.IsCancellationRequested)
        {
            throw new InvalidOperationException("Cannot submit tasks after shutdown");
        }

        var task = new MyTask<TResult>(func, this);
        this.EnqueueTask(task.Complete);
        return task;
    }

    /// <summary>
    /// Initiates shutdown and waits for all tasks to complete.
    /// </summary>
    public void Shutdown()
    {
       lock (this.queueLock)
        {
            if (!this.cts.Token.IsCancellationRequested)
            {
                this.cts.Cancel();
                this.workAvailable.Set();
            }
        }

       foreach (var thread in this.threads)
        {
            thread.Join();
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="MyThreadPool"/>.
    /// </summary>
    public void Dispose()
    {
        this.Shutdown();
        this.cts.Dispose();
        this.workAvailable.Dispose();
    }

    /// <summary>
    /// Enqueues a task for execution.
    /// </summary>
    /// <param name="action">The action to execute. Cannot be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the pool is shutting down.</exception>
    internal void EnqueueTask(Action action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        lock (this.queueLock)
        {
            if (this.cts.IsCancellationRequested)
            {
                throw new InvalidOperationException("Cannot enqueue tasks after shutdown");
            }

            this.taskQueue.Enqueue(action);
            this.workAvailable.Set();
        }
    }

    private void WorkerLoop()
    {
        while (true)
        {
            Action? action = null;

            lock (this.queueLock)
            {
                if (this.taskQueue.Count > 0)
                {
                    action = this.taskQueue.Dequeue();
                }
                else if (this.cts.Token.IsCancellationRequested)
                {
                    break;
                }
            }

            if (action is not null)
            {
                try
                {
                    action();
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                this.workAvailable.WaitOne();

                if (this.cts.Token.IsCancellationRequested)
                {
                    lock (this.queueLock)
                    {
                        if (this.taskQueue.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}