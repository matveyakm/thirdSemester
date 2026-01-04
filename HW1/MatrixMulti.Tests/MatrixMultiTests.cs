// <copyright file="MatrixMultiTests.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MatrixMulti.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MatrixMultiplier;
using NUnit.Framework;

/// <summary>
/// Contains tests for matrix multiplication methods.
/// </summary>
[TestFixture]
public class MatrixMultiTests
{
    private static readonly string TestFilesPath =
        Path.Combine(AppContext.BaseDirectory, "TestFiles");

    private static readonly string[] SuccessfulTestNames = new[]
    {
        "[2x2][2x2]",
        "[10x13][13x10]",
        "[20x7][7x9]",
    };

    private static readonly string[] FailingTestNames = new[]
    {
        "[2x5][3x4].fail",
    };

    /// <summary>
    /// Tests the multiplication of matrices using a single-threaded method with the provided test and answer files.
    /// </summary>
    /// <param name="testFilePath">The path to the test file containing the first matrix.</param>
    /// <param name="answerFilePath">The path to the answer file containing the expected result matrix.</param>
    [Test]
    [TestCaseSource(nameof(SuccessfulTestCases))]
    public void MultiplySingleThread_WithTestFile_MatchesExpectedAnswer(string testFilePath, string answerFilePath)
    {
        MultiplyAndAssert(testFilePath, answerFilePath, Multiplier.MultiplySingleThread);
    }

    /// <summary>
    /// Tests the multiplication of matrices using a parallel method with the provided test and answer files.
    /// </summary>
    /// <param name="testFilePath">The path to the test file containing the first matrix.</param>
    /// <param name="answerFilePath">The path to the answer file containing the expected result matrix.</param>
    [Test]
    [TestCaseSource(nameof(SuccessfulTestCases))]
    public void MultiplyParallel_WithTestFile_MatchesExpectedAnswer(string testFilePath, string answerFilePath)
    {
        MultiplyAndAssert(testFilePath, answerFilePath, Multiplier.MultiplyParallel);
    }

    /// <summary>
    /// Tests that multiplying incompatible matrices throws an ArgumentException.
    /// </summary>
    /// <param name="failFilePath">The path to the test file containing the incompatible matrices.</param>
    [Test]
    [TestCaseSource(nameof(FailingTestCases))]
    public void Multiply_WithIncompatibleMatrices_ThrowsArgumentException(string failFilePath)
    {
        var (latexA, latexB) = Parser.ReadLatexFromFile(failFilePath);
        int[,] matrixA = Parser.ParseMatrix(latexA);
        int[,] matrixB = Parser.ParseMatrix(latexB);

        Assert.Throws<ArgumentException>(() => Multiplier.MultiplySingleThread(matrixA, matrixB));
        Assert.Throws<ArgumentException>(() => Multiplier.MultiplyParallel(matrixA, matrixB));
    }

    private static void MultiplyAndAssert(
        string testFilePath,
        string answerFilePath,
        Func<int[,], int[,], int[,]> multiplyMethod)
    {
        var (latexA, latexB) = Parser.ReadLatexFromFile(testFilePath);
        int[,] matrixA = Parser.ParseMatrix(latexA);
        int[,] matrixB = Parser.ParseMatrix(latexB);

        int[,] result = multiplyMethod(matrixA, matrixB);

        string expectedLatex = ReadSingleLatexMatrixFromFile(answerFilePath);
        int[,] expectedMatrix = Parser.ParseMatrix(expectedLatex);

        bool equal = MatrixUtils.AreMatricesEqual(result, expectedMatrix);

        Assert.That(equal, Is.True, $"Результат не совпадает для {Path.GetFileName(testFilePath)}");
    }

    private static IEnumerable<TestCaseData> SuccessfulTestCases()
    {
        foreach (var name in SuccessfulTestNames)
        {
            string testFile = Path.Combine(TestFilesPath, name + ".test.tex");
            string answerFile = Path.Combine(TestFilesPath, name + ".answer.tex");

            yield return new TestCaseData(testFile, answerFile)
                .SetName($"{{m}}({name}.test.tex)");
        }
    }

    private static IEnumerable<TestCaseData> FailingTestCases()
    {
        foreach (var name in FailingTestNames)
        {
            string failFile = Path.Combine(TestFilesPath, name + ".tex");

            yield return new TestCaseData(failFile)
                .SetName($"{{m}}({name}.tex - ожидается исключение)");
        }
    }

    private static string ReadSingleLatexMatrixFromFile(string path)
    {
        string content = File.ReadAllText(path);
        var matches = Regex.Matches(content, @"\\begin{matrix}([\s\S]*?)\\end{matrix}");
        if (matches.Count == 0)
        {
            throw new ArgumentException("No matrix found in answer file");
        }

        return matches[0].Groups[1].Value.Trim();
    }
}