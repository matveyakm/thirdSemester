// <copyright file="MatrixUtilsTests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MatrixMultiplier.Tests;

using MatrixMultiplier;
using NUnit.Framework;

/// <summary>
/// Tests for the MatrixUtils class.
/// </summary>
[TestFixture]
public class MatrixUtilsTests
{
    /// <summary>
    /// Tests that the AreMatricesEqual method returns true when both matrices are null.
    /// </summary>
    [Test]
    public void AreMatricesEqual_BothNull_ReturnsTrue()
    {
        bool result = MatrixUtils.AreMatricesEqual(null, null);
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that the AreMatricesEqual method returns false when one matrix is null and the other is not.
    /// </summary>
    [Test]
    public void AreMatricesEqual_OneNullOtherNot_ReturnsFalse()
    {
        int[,] matrix = new int[,]
        {
            { 1, 2 },
            { 3, 4 },
        };

        Assert.That(MatrixUtils.AreMatricesEqual(matrix, null), Is.False);
        Assert.That(MatrixUtils.AreMatricesEqual(null, matrix), Is.False);
    }

    /// <summary>
    /// Tests that the AreMatricesEqual method returns true when both matrices reference the same object.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method correctly identifies two references to the same matrix as equal.
    /// </remarks>
    [Test]
    public void AreMatricesEqual_SameReference_ReturnsTrue()
    {
        int[,] matrix = new int[,]
        {
            { 1, 2 },
            { 3, 4 },
        };

        bool result = MatrixUtils.AreMatricesEqual(matrix, matrix);

        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that the AreMatricesEqual method returns false when matrices have different dimensions.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method correctly identifies matrices of different sizes as not equal.
    /// </remarks>
    [Test]
    public void AreMatricesEqual_DifferentDimensions_ReturnsFalse()
    {
        int[,] matrix1 = new int[,]
        {
            { 1, 2 },
            { 3, 4 },
        };

        int[,] matrix2 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };
        int[,] matrix3 = new int[,]
        {
            { 1 },
            { 2 },
            { 3 },
        };

        Assert.That(MatrixUtils.AreMatricesEqual(matrix1, matrix2), Is.False);
        Assert.That(MatrixUtils.AreMatricesEqual(matrix1, matrix3), Is.False);
    }

    /// <summary>
    /// Tests that the AreMatricesEqual method returns true when both matrices have the same dimensions and values.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method correctly identifies two matrices with the same dimensions and values as equal.
    /// </remarks>
    /// <param name="matrix1">The first matrix to compare.</param>
    /// <param name="matrix2">The second matrix to compare.</param>
    [Test]
    public void AreMatricesEqual_SameDimensionsSameValues_ReturnsTrue()
    {
        int[,] matrix1 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };

        int[,] matrix2 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };

        bool result = MatrixUtils.AreMatricesEqual(matrix1, matrix2);

        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that the AreMatricesEqual method returns false when both matrices have the same dimensions but different values.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method correctly identifies two matrices with the same dimensions but different values as not equal.
    /// </remarks>
    /// <param name="matrix1">The first matrix to compare.</param>
    /// <param name="matrix2">The second matrix to compare.</param>
    [Test]
    public void AreMatricesEqual_SameDimensionsDifferentValues_ReturnsFalse()
    {
        int[,] matrix1 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };

        int[,] matrix2 = new int[,]
        {
            { 1, 2, 3 },
            { 4, 9, 6 },
        };

        bool result = MatrixUtils.AreMatricesEqual(matrix1, matrix2);

        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests that the AreMatricesEqual method returns true when both matrices are empty.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method correctly identifies two empty matrices as equal.
    /// </remarks>
    [Test]
    public void AreMatricesEqual_EmptyMatrices_ReturnsTrue()
    {
        int[,] empty1 = new int[0, 0];
        int[,] empty2 = new int[0, 0];

        bool result = MatrixUtils.AreMatricesEqual(empty1, empty2);

        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Tests that the AreMatricesEqual method returns false when one matrix is empty and the other is not.
    /// </summary>
    /// <remarks>
    /// This test verifies that the method correctly identifies an empty matrix and a non-empty matrix as not equal.
    /// </remarks>
    [Test]
    public void AreMatricesEqual_OneEmptyOneNot_ReturnsFalse()
    {
        int[,] empty = new int[0, 0];
        int[,] nonEmpty = new int[,] { { 1 } };

        Assert.That(MatrixUtils.AreMatricesEqual(empty, nonEmpty), Is.False);
    }
}