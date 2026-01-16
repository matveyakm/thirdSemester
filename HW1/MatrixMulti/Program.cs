// <copyright file="Program.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using MatrixMultiplier;

using ScottPlot;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            PrintHelp();
            return;
        }

        switch (args[0])
        {
            case "calc":
                if (args.Length < 2)
                {
                    Console.WriteLine("Ошибка: укажите путь к файлу");
                    Console.WriteLine("Пример: dotnet run calc matrix.tex");
                    return;
                }

                string path = args[1];
                if (!File.Exists(path))
                {
                    Console.WriteLine($"Файл не найден: {path}");
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

            case "bench":
                int maxSize = 450;
                int step = 50;
                int iterations = 10;

                if (args.Length >= 2 && int.TryParse(args[1], out int parsedMax) && parsedMax >= 60)
                {
                    maxSize = parsedMax;
                }

                if (args.Length >= 3 && int.TryParse(args[2], out int parsedStep) && parsedStep > 0)
                {
                    step = parsedStep;
                }

                if (args.Length >= 4 && int.TryParse(args[3], out int parsedIter) && parsedIter > 0)
                {
                    iterations = parsedIter;
                }

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

                var sizes = new List<double>(); // n
                var timesSingle = new List<double>();
                var timesParallel = new List<double>();

                int n = 10;
                while (n <= maxSize)
                {
                    var (expectedValueSingle, expectedValueParallel,
                         standardDeviationSingle, standardDeviationParallel) =
                        Benchmark.BenchmarkMultiplication(n, iterations);

                    sizes.Add(n);
                    timesSingle.Add(expectedValueSingle);
                    timesParallel.Add(expectedValueParallel);

                    Console.WriteLine($"{n,4} × {n,-4} | single: {expectedValueSingle,6} ms ± {standardDeviationSingle,5:F1} | parallel: {expectedValueParallel,6} ms ± {standardDeviationParallel,5:F1}");
                    n = n + step < maxSize || n == maxSize ? n + step : maxSize;
                }

                var plot = new ScottPlot.Plot();
                var singlePlot = plot.Add.Scatter(sizes.ToArray(), timesSingle.ToArray());
                singlePlot.MarkerSize = 7;
                singlePlot.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
                singlePlot.LineWidth = 2.2f;
                singlePlot.Color = ScottPlot.Colors.Blue;
                singlePlot.LegendText = "Однопоточная версия";

                var parallelPlot = plot.Add.Scatter(sizes.ToArray(), timesParallel.ToArray());
                parallelPlot.MarkerSize = 7;
                parallelPlot.MarkerShape = ScottPlot.MarkerShape.FilledSquare;
                parallelPlot.LineWidth = 2.2f;
                parallelPlot.Color = ScottPlot.Colors.OrangeRed;
                parallelPlot.LegendText = "Параллельная версия";

                plot.Title("Время умножения матриц в зависимости от размера");
                plot.XLabel("Размер матрицы n (n × n)");
                plot.YLabel("Среднее время, мс");
                plot.Axes.AutoScale();
                plot.ShowLegend();

                plot.SavePng("matrix_multiplication_benchmark.png", 1200, 750);
                break;

            default:
                Console.WriteLine("Некорректный ввод. Завершение программы.");
                break;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Использование:");
        Console.WriteLine("  dotnet run calc <путь_к_файлу.tex>          → умножить матрицы из файла");
        Console.WriteLine("  dotnet run bench [maxSize] [step] [iterations]  → запустить бенчмарк");
        Console.WriteLine();
        Console.WriteLine("Примеры:");
        Console.WriteLine("  dotnet run calc matrix.tex");
        Console.WriteLine("  dotnet run bench                     # дефолт: до 400, шаг 50, 10 итераций");
        Console.WriteLine("  dotnet run bench 2000 100 5");
        Console.WriteLine();
        Console.WriteLine("Для справки: dotnet run --help");
    }

    private static string GetProcessorName()
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
}