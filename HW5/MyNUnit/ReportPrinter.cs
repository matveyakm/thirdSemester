// <copyright file="ReportPrinter.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit;

using System;
using System.Collections.Generic;
using MyNUnit.Models;

/// <summary>
/// Provides functionality to print test reports to the console.
/// </summary>
internal class ReportPrinter
{
    /// <summary>
    /// Prints the test report.
    /// </summary>
    /// <param name="classResults">The list of test class results.</param>
    public void PrintReport(List<TestClassResult> classResults)
    {
        int passed = 0, failed = 0, errored = 0, ignored = 0;

        foreach (var classResult in classResults)
        {
            Console.WriteLine($"Class: {classResult.ClassName}");

            foreach (var testResult in classResult.TestResults)
            {
                switch (testResult.Status)
                {
                    case TestStatus.Passed:
                        Console.WriteLine($"  Test: {testResult.TestName} - Passed in {testResult.ExecutionTime.TotalMilliseconds} ms");
                        passed++;
                        break;
                    case TestStatus.Failed:
                        Console.WriteLine($"  Test: {testResult.TestName} - Failed in {testResult.ExecutionTime.TotalMilliseconds} ms");
                        Console.WriteLine($"    Reason: {testResult.Exception?.Message}");
                        PrintException(testResult.Exception);
                        failed++;
                        break;
                    case TestStatus.Ignored:
                        Console.WriteLine($"  Test: {testResult.TestName} - Ignored");
                        Console.WriteLine($"    Reason: {testResult.IgnoreReason}");
                        ignored++;
                        break;
                    case TestStatus.Errored:
                        Console.WriteLine($"  Test: {testResult.TestName} - Errored");
                        Console.WriteLine($"    Reason: {testResult.IgnoreReason}");
                        PrintException(testResult.Exception);
                        errored++;
                        break;
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine("══════════════════════════════════════");
        Console.WriteLine($"SUMMARY");
        Console.WriteLine($"   Passed : {passed}");
        Console.WriteLine($"   Failed : {failed}");
        Console.WriteLine($"   Errored: {errored}");
        Console.WriteLine($"   Ignored: {ignored}");
        Console.WriteLine($"   Total  : {passed + failed + errored + ignored}");
        Console.WriteLine("══════════════════════════════════════");
    }

    private static void PrintException(Exception? ex)
    {
        if (ex == null)
        {
            return;
        }

        Console.WriteLine($"      Message: {ex.Message}");

        if (!string.IsNullOrEmpty(ex.StackTrace))
        {
            Console.WriteLine($"      StackTrace:");
            Console.WriteLine(ex.StackTrace
                .Split('\n')
                .Select(line => $"         {line.TrimEnd()}")
                .Take(8));
        }
    }
}
