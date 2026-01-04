// <copyright file="ReportPrinter.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using MyNUnit.Models;

namespace MyNUnit;

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
        foreach (var classResult in classResults)
        {
            Console.WriteLine($"Class: {classResult.ClassName}");

            foreach (var testResult in classResult.TestResults)
            {
                switch (testResult.Status)
                {
                    case TestStatus.Passed:
                        Console.WriteLine($"  Test: {testResult.TestName} - Passed in {testResult.ExecutionTime.TotalMilliseconds} ms");
                        break;
                    case TestStatus.Failed:
                        Console.WriteLine($"  Test: {testResult.TestName} - Failed in {testResult.ExecutionTime.TotalMilliseconds} ms");
                        Console.WriteLine($"    Reason: {testResult.Exception?.Message}");
                        if (testResult.Exception?.StackTrace != null)
                        {
                            Console.WriteLine($"    StackTrace: {testResult.Exception.StackTrace}");
                        }

                        break;
                    case TestStatus.Ignored:
                        Console.WriteLine($"  Test: {testResult.TestName} - Ignored");
                        Console.WriteLine($"    Reason: {testResult.IgnoreReason}");
                        break;
                }
            }

            Console.WriteLine();
        }
    }
}
