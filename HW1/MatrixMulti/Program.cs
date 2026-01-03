// <copyright file="Program.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using MatrixMultiplier;

static string GetProcessorName()
{
    try
    {
        if (OperatingSystem.IsWindows())
        {
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Неизвестно";
        }

        if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = OperatingSystem.IsMacOS() ? "sysctl" : "cat",
                Arguments = OperatingSystem.IsMacOS() ? "-n machdep.cpu.brand_string" : "/proc/cpuinfo",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });

            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (OperatingSystem.IsLinux())
                {
                    var line = output.Split('\n').FirstOrDefault(l => l.Contains("model name"));
                    if (line != null)
                    {
                        return line.Split(':')[1].Trim();
                    }
                }
                else
                {
                    return output;
                }
            }
        }
    }
    catch
    {
        // Ignore exceptions and fall through to return unknown
    }

    return "Не удалось определить";
}

Console.Write("Введите соответствующую цифру для:\n\t 0 -- вычисление,  1 -- бенчмарк\n");
switch (Console.ReadLine())
{
    case "0":
        Console.Write("Введите путь к .tex файлу: ");
        string? path = Console.ReadLine();
        if (path == null)
        {
            Console.WriteLine("Путь не может быть пустым. Завершение программы.");
            return;
        }

        (string strMatrixA, string strMatrixB) = Parser.ReadLatexFromFile(path);

        int[,] matrixA = Parser.ParseMatrix(strMatrixA);
        int[,] matrixB = Parser.ParseMatrix(strMatrixB);

        int[,] result = Multiplier.MultiplyParallel(matrixA, matrixB);

        string latexResult = "\n=\n" + Parser.MatrixToLatex(result);

        File.AppendAllText(path, latexResult);
        Console.WriteLine("Результат умножения добавлен в файл.");
        break;

    case "1":
        Console.WriteLine("Введите максимальный размер матрицы для бенчмарка (не менее 60, по умолчанию 1000): ");
        string? maxSizeInput = Console.ReadLine();
        int maxSize = int.TryParse(maxSizeInput, out int parsedSize) && parsedSize >= 60 ? parsedSize : 1000;

        int step = 50;
        int iterations = 2;
        Console.WriteLine(
            $@"Запуск бенчмарка:
                с размера 10x10
                до размера {maxSize}x{maxSize}
                с шагом {step}
                по {iterations} итерации на размер");

        Console.WriteLine(
            $@"Информация о процессоре:
                Логических ядер (потоков):      {Environment.ProcessorCount}
                Название процессора:            {GetProcessorName()}
                Архитектура:                    {RuntimeInformation.ProcessArchitecture}
                Is 64-bit process:              {Environment.Is64BitProcess}
                Is 64-bit OS:                   {Environment.Is64BitOperatingSystem}
        ");

        Console.WriteLine("size;iterations;single_thread_ms;parallel_ms");
        for (int n = 10; n <= maxSize; n += step)
        {
            string resultLine = Benchmark.BenchmarkMultiplication(n, iterations);
            File.AppendAllText("benchmark_results.csv", resultLine);
            Console.Write(resultLine);
        }

        break;

    default:
        Console.WriteLine("Некорректный ввод. Завершение программы.");
        break;
}