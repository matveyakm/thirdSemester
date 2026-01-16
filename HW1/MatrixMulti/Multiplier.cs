// <copyright file="Multiplier.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MatrixMultiplier;

using System;
using System.IO;
using System.Threading;

/// <summary>
/// Provides methods for multiplying matrices.
/// </summary>
public static class Multiplier
{
    /// <summary>
    /// Multiplies two matrices in parallel.
    /// </summary>
    /// <param name="matrixA">The first matrix to multiply.</param>
    /// <param name="matrixB">The second matrix to multiply.</param>
    /// <returns>The resulting matrix after multiplication.</returns>
    public static int[,] MultiplyParallel(int[,] matrixA, int[,] matrixB)
    {
        int rowsA = matrixA.GetLength(0);
        int colsA = matrixA.GetLength(1);
        int colsB = matrixB.GetLength(1);

        if (colsA != matrixB.GetLength(0))
        {
            throw new ArgumentException("Matrix dimensions are not compatible for multiplication.");
        }

        int[,] result = new int[rowsA, colsB];

        int threadCount = Environment.ProcessorCount;
        threadCount = Math.Min(threadCount, rowsA);
        threadCount = Math.Min(threadCount, 32);
        threadCount = Math.Max(threadCount, 1);

        var threads = new Thread[threadCount];
        int rowsPerThread = (int)Math.Ceiling((double)rowsA / threadCount);

        for (int t = 0; t < threadCount; t++)
        {
            int threadIndex = t;
            threads[t] = new Thread(() =>
            {
                int startRow = threadIndex * rowsPerThread;
                int endRow = Math.Min(startRow + rowsPerThread, rowsA);

                for (int i = startRow; i < endRow; i++)
                {
                    for (int j = 0; j < colsB; j++)
                    {
                        int sum = 0;
                        for (int k = 0; k < colsA; k++)
                        {
                            sum += matrixA[i, k] * matrixB[k, j];
                        }

                        result[i, j] = sum;
                    }
                }
            });
            threads[t].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return result;
    }

    /// <summary>
    /// Multiplies two matrices using 1 thread.
    /// </summary>
    /// <param name="matrixA">The first matrix to multiply.</param>
    /// <param name="matrixB">The second matrix to multiply.</param>
    /// <returns>The resulting matrix after multiplication.</returns>
    public static int[,] MultiplySingleThread(int[,] matrixA, int[,] matrixB)
    {
        int rowsA = matrixA.GetLength(0);
        int colsA = matrixA.GetLength(1);
        int colsB = matrixB.GetLength(1);

        if (colsA != matrixB.GetLength(0))
        {
            throw new ArgumentException("Matrix dimensions are not compatible for multiplication.");
        }

        int[,] result = new int[rowsA, colsB];

        for (int i = 0; i < rowsA; i++)
        {
            for (int j = 0; j < colsB; j++)
            {
                int sum = 0;
                for (int k = 0; k < colsA; k++)
                {
                    sum += matrixA[i, k] * matrixB[k, j];
                }

                result[i, j] = sum;
            }
        }

        return result;
    }
}
