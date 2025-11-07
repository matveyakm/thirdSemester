// <copyright file="IMyTask.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ThreadPool;

/// <summary>
/// Represents a task in my thread pool.
/// </summary>
/// <typeparam name="TResult">The type of the result produced upon task completion.</typeparam>
public interface IMyTask<out TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task has completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the result of the task. Blocks if not yet completed.
    /// Throws <see cref="AggregateException"/> if the task failed.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Creates a continuation task that runs after this task completes.
    /// </summary>
    /// <typeparam name="TNewResult">The type of the result of the continuation.</typeparam>
    /// <param name="continuation">Function to run on the result of this task.</param>
    /// <returns>A new task representing the continuation.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> continuation);
}