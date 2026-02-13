// <copyright file="MyTask.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ThreadPool;

using System;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Represents a task in the thread pool.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
internal sealed class MyTask<TResult> : IMyTask<TResult>
{
    private readonly Func<TResult> func;
    private readonly ManualResetEvent completionEvent = new(false);
    private readonly List<Action> continuations = [];
    private readonly MyThreadPool pool;
    private readonly object locker = new object();
    private volatile bool isCompleted;
    private TResult? result;
    private Exception? exception;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <param name="pool">The thread pool to use.</param>
    public MyTask(Func<TResult> func, MyThreadPool pool)
    {
        this.func = func ?? throw new ArgumentNullException(nameof(func));
        this.pool = pool;
        this.isCompleted = false;
    }

    /// <summary>
    /// Gets a value indicating whether the task is completed.
    /// </summary>
    public bool IsCompleted => this.isCompleted;

    /// <summary>
    /// Gets the result of the task.
    /// </summary>
    /// <exception cref="AggregateException">Thrown if the task completed with an exception.</exception>
    public TResult Result
    {
        get
        {
            if (!this.isCompleted)
            {
                this.completionEvent.WaitOne();
            }

            lock (this.locker)
            {
                if (this.pool.PoolException != null)
                {
                    throw new AggregateException(this.pool.PoolException);
                }

                return this.exception != null ? throw new AggregateException(this.exception) : this.result!;
            }
        }
    }

    /// <summary>
    /// Continues with a new task that executes after the current task.
    /// </summary>
    /// <param name="continuation">The function to execute after the current task completes.</param>
    /// <typeparam name="TNewResult">The type of the result produced by the continuation task.</typeparam>
    /// <returns>A new task that represents the continuation.</returns>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        lock (this.locker)
        {
            var newTask = new MyTask<TNewResult>(
                () => continuation(
                    this.exception != null
                ? throw new AggregateException(this.exception) : this.result!),
                this.pool);

            if (this.IsCompleted)
            {
                if (this.pool.PoolException != null)
                {
                    throw new InvalidOperationException("Cannot continue task after pool error");
                }

                this.pool.EnqueueTask(newTask.Complete);
            }
            else
            {
                this.continuations.Add(() => this.pool.EnqueueTask(newTask.Complete));
            }

            return newTask;
        }
    }

    /// <summary>
    /// To complete task.
    /// </summary>
    public void Complete()
    {
        try
        {
            this.result = this.func();
        }
        catch (Exception ex)
        {
            this.exception = ex;
            throw;
        }
        finally
        {
            lock (this.locker)
            {
                this.isCompleted = true;
                this.completionEvent.Set();
                foreach (var continuation in this.continuations)
                {
                    if (this.pool.PoolException == null)
                    {
                         this.pool.EnqueueTask(continuation);
                    }
                }
            }
        }
    }
}