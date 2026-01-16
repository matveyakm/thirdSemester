// <copyright file="Benchmark.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MatrixMultiplier;

/// <summary>
/// Provides methods for benchmarking matrix multiplication.
/// </summary>
internal static class Benchmark
{
    /// <summary>
    /// Benchmarks matrix multiplication for given size and iterations.
    /// </summary>
    /// <param name="n">The size of the matrices.</param>
    /// <param name="iterations">The number of iterations to perform.</param>
    /// <returns>A string containing the benchmark results formatted as "n;iterations;singleThreadTime;parallelTime".</returns>
    public static (int ExpectedValueSingle, int ExpectedValueParallel, double StandardDeviationSingle, double StandardDeviationParallel) BenchmarkMultiplication(int n, int iterations)
    {
        if (n <= 0 || iterations <= 0)
        {
            throw new ArgumentException("Matrix size and iterations must be positive integers.");
        }

        var timesSingle = new List<long>(iterations);
        var timesParallel = new List<long>(iterations);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        watch.Stop();
        for (int i = 0; i < iterations; i++)
        {
            string latexA = GenerateMatrix(n, n);
            string latexB = GenerateMatrix(n, n);

            int[,] matrixA = Parser.ParseMatrix(latexA);
            int[,] matrixB = Parser.ParseMatrix(latexB);

            watch.Restart();
            int[,] resultParallel = Multiplier.MultiplyParallel(matrixA, matrixB);
            watch.Stop();
            timesParallel.Add(watch.ElapsedMilliseconds);

            watch.Restart();
            int[,] resultSingle = Multiplier.MultiplySingleThread(matrixA, matrixB);
            watch.Stop();
            timesSingle.Add(watch.ElapsedMilliseconds);

            if (!MatrixUtils.AreMatricesEqual(resultParallel, resultSingle))
            {
                throw new Exception("Matrix multiplication results do not match!");
            }
        }

        var expectedValueSingle = timesSingle.Average();
        var expectedValueParallel = timesParallel.Average();
        var standardDeviationSingle = StandardDeviation(timesSingle);
        var standardDeviationParallel = StandardDeviation(timesParallel);

        return (
            (int)Math.Round(expectedValueSingle),
            (int)Math.Round(expectedValueParallel),
            Math.Round(standardDeviationSingle, 2),
            Math.Round(standardDeviationParallel, 2));
    }

    private static string GenerateMatrix(int rows, int cols)
    {
        Random rand = new();
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

    private static double StandardDeviation(IReadOnlyList<long> values)
    {
        if (values == null || values.Count <= 1)
        {
            return 0;
        }

        double mean = values.Average();
        double sumOfSquares = 0;

        foreach (var val in values)
        {
            double diff = val - mean;
            sumOfSquares += diff * diff;
        }

        return Math.Sqrt(sumOfSquares / (values.Count - 1));
    }
}