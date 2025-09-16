using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

class MatrixMultiplier
{
    // ---------- многопоточное умножение (ограничение 32 потока) ----------
    public static int[,] MultiplyParallel(int[,] matrixA, int[,] matrixB)
    {
        int rowsA = matrixA.GetLength(0);
        int colsA = matrixA.GetLength(1);
        int colsB = matrixB.GetLength(1);

        if (colsA != matrixB.GetLength(0))
            throw new ArgumentException("Matrix dimensions are not compatible for multiplication.");

        int[,] result = new int[rowsA, colsB];

        int threadCount = Math.Min(32, rowsA);
        Thread[] threads = new Thread[threadCount];
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
            thread.Join();

        return result;
    }

    // ---------- парсер матрицы из LaTeX ----------
    public static int[,] ParseMatrix(string latex)
    {
        // разбиваем строки по \\
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

    // ---------- преобразование матрицы в LaTeX ----------
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
                if (j < cols - 1) latex += " & ";
            }
            if (i < rows - 1) latex += " \\\\\n";
        }

        latex += "\n\\end{matrix}";
        return latex;
    }

    static void Main(string[] args)
    {
        Console.Write("Введите путь к .tex файлу: ");
        string path = Console.ReadLine();

        if (!File.Exists(path))
        {
            Console.WriteLine("Файл не найден!");
            return;
        }

        string content = File.ReadAllText(path);

        // извлекаем два блока \begin{matrix}...\end{matrix}
        var matches = Regex.Matches(content, @"\\begin{matrix}([\s\S]*?)\\end{matrix}");
        if (matches.Count < 2)
        {
            Console.WriteLine("Не удалось найти две матрицы в файле.");
            return;
        }

        int[,] matrixA = ParseMatrix(matches[0].Groups[1].Value);
        int[,] matrixB = ParseMatrix(matches[1].Groups[1].Value);

        int[,] result = MultiplyParallel(matrixA, matrixB);

        string latexResult = "\n=\n" + MatrixToLatex(result);

        // дописываем результат в конец файла
        File.AppendAllText(path, latexResult);

        Console.WriteLine("Результат умножения добавлен в файл.");
    }
}
