// <copyright file="Benchmark.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MatrixMultiplier;

/// <summary>
/// Provides methods for benchmarking matrix multiplication.
/// </summary>
internal class Benchmark
{
    /// <summary>
    /// Benchmarks matrix multiplication for given size and iterations.
    /// </summary>
    /// <param name="n">The size of the matrices.</param>
    /// <param name="iterations">The number of iterations to perform.</param>
    /// <returns>A string containing the benchmark results formatted as "n;iterations;singleThreadTime;parallelTime".</returns>
    public static string BenchmarkMultiplication(int n, int iterations)
    {
        if (n <= 0 || iterations <= 0)
        {
            throw new ArgumentException("Matrix size and iterations must be positive integers.");
        }

        var watchParallel = System.Diagnostics.Stopwatch.StartNew();
        watchParallel.Stop();
        var watchSingle = System.Diagnostics.Stopwatch.StartNew();
        watchSingle.Stop();
        for (int i = 0; i < iterations; i++)
        {
            string latexA = GenerateMatrix(n, n);
            string latexB = GenerateMatrix(n, n);

            int[,] matrixA = Parser.ParseMatrix(latexA);
            int[,] matrixB = Parser.ParseMatrix(latexB);

            watchParallel.Start();
            int[,] resultParallel = Multiplier.MultiplyParallel(matrixA, matrixB);
            watchParallel.Stop();

            watchSingle.Start();
            int[,] resultSingle = Multiplier.MultiplySingleThread(matrixA, matrixB);
            watchSingle.Stop();

            if (!MatrixUtils.AreMatricesEqual(resultParallel, resultSingle))
            {
                throw new Exception("Matrix multiplication results do not match!");
            }
        }

        return $"{n};{iterations};{watchSingle.ElapsedMilliseconds / iterations};{watchParallel.ElapsedMilliseconds / iterations}\n";
    }

    private static string GenerateMatrix(int rows, int cols)
    {
        Random rand = new Random();
        string latex = string.Empty;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                latex += rand.Next(1, 99).ToString();
                if (j < cols - 1)
                {
                    latex += " & ";
                }
            }

            if (i < rows - 1)
            {
                latex += " \\\\\n";
            }
        }

        return latex;
    }
}