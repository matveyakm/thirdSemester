// <copyright file="Program.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;

namespace MyNUnit;

/// <summary>
/// The entry point of the application.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main method to run the test runner.
    /// </summary>
    /// <param name="args">Command-line arguments. First argument is the directory path.</param>
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: MyNUnit <directory_path>");
            return;
        }

        var directoryPath = args[0];

        var testRunner = new TestRunner();
        var results = testRunner.RunTests(directoryPath);

        var reportPrinter = new ReportPrinter();
        reportPrinter.PrintReport(results);
    }
}
