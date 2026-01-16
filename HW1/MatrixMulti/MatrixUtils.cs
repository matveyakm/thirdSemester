// <copyright file="MatrixUtils.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MatrixMultiplier;

using System.Linq;

/// <summary>
/// Provides utility methods for matrix operations.
/// </summary>
public static class MatrixUtils
{
    /// <summary>
    /// Compares two matrices for equality.
    /// </summary>
    /// <param name="matrixA">The first matrix to compare.</param>
    /// <param name="matrixB">The second matrix to compare.</param>
    /// <returns>True if the matrices are equal; otherwise, false.</returns>
    public static bool AreMatricesEqual(int[,]? matrixA, int[,]? matrixB)
    {
        if (matrixA is null || matrixB is null)
        {
            return matrixA is null && matrixB is null;
        }

        if (matrixA.GetLength(0) != matrixB.GetLength(0) ||
            matrixA.GetLength(1) != matrixB.GetLength(1))
        {
            return false;
        }

        return matrixA.Cast<int>().SequenceEqual(matrixB.Cast<int>());
    }
}