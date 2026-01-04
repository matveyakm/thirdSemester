// <copyright file="Parser.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MatrixMultiplier;

using System.Text.RegularExpressions;

/// <summary>
/// Provides methods for parsing LaTeX formatted matrices and converting them back to LaTeX.
/// </summary>
public class Parser
{
    /// <summary>
    /// Reads LaTeX formatted matrices from a file.
    /// </summary>
    /// <param name="path">The path to the file containing the matrices.</param>
    /// <returns>
    /// A tuple containing two strings:
    /// <list type="bullet">
    /// <item><description>MatrixA: The first matrix in LaTeX format.</description></item>
    /// <item><description>MatrixB: The second matrix in LaTeX format.</description></item>
    /// </list>
    /// </returns>
    public static (string MatrixA, string MatrixB) ReadLatexFromFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new ArgumentException("File not found!");
        }

        string content = File.ReadAllText(path);

        var matches = Regex.Matches(content, @"\\begin{matrix}([\s\S]*?)\\end{matrix}");
        if (matches.Count < 2)
        {
            throw new ArgumentException("The file must contain at least two matrices in LaTeX format.");
        }

        var matrixA = matches[0].Groups[1].Value;
        var matrixB = matches[1].Groups[1].Value;

        return (matrixA, matrixB);
    }

    /// <summary>
    /// Parses a LaTeX formatted matrix string into a 2D integer array.
    /// </summary>
    /// <param name="latex">The LaTeX string representing the matrix.</param>
    /// <returns>A 2D array of integers representing the parsed matrix.</returns>
    public static int[,] ParseMatrix(string latex)
    {
        string[] rows = latex.Trim().Split(new string[] { "\\\\" }, StringSplitOptions.RemoveEmptyEntries);
        int rowCount = rows.Length;
        int colCount = rows[0].Split('&').Length;
        int[,] matrix = new int[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            string[] cols = rows[i].Split('&');
            for (int j = 0; j < colCount; j++)
            {
                matrix[i, j] = int.Parse(cols[j].Trim());
            }
        }

        return matrix;
    }

    /// <summary>
    /// Parses a 2D integer array into a LaTeX formatted matrix string.
    /// </summary>
    /// <param name="matrix">A 2D array of integers representing the matrix to convert to LaTeX format.</param>
    /// <returns>A LaTeX formatted string representing the matrix.</returns>
    public static string MatrixToLatex(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        string latex = "\\begin{matrix}\n";

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                latex += matrix[i, j].ToString();
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

        latex += "\n\\end{matrix}";
        return latex;
    }
}