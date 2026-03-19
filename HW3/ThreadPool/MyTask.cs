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
    private readonly Lock locker = new();
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
                if (this.exception != null)
                {
                    throw new AggregateException(this.exception);
                }

                if (this.result == null)
                {
                    throw new InvalidOperationException("Task completed with unexpected null result.");
                }

                return this.result!;
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
            var newTask = new MyTask<TNewResult>(ContinuationFunc, this.pool);

            if (this.IsCompleted)
            {
                this.pool.EnqueueTask(newTask.Complete);
            }
            else
            {
                this.continuations.Add(newTask.Complete);
            }

            return newTask;

            TNewResult ContinuationFunc()
            {
                var sourceResult = this.Result;

                return continuation(sourceResult);
            }
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
            List<Action> continuationsToExecute;

            lock (this.locker)
            {
                this.isCompleted = true;
                this.completionEvent.Set();

                continuationsToExecute = new List<Action>(this.continuations);
                this.continuations.Clear();

                foreach (var continuation in continuationsToExecute)
                {
                    if (this.exception == null)
                    {
                        this.pool.EnqueueTask(continuation);
                    }
                }
            }
        }
    }
}