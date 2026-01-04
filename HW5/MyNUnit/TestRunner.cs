// <copyright file="TestRunner.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyNUnit.Models;

namespace MyNUnit;

/// <summary>
/// Runs the tests across multiple classes in parallel.
/// </summary>
internal class TestRunner
{
    /// <summary>
    /// Runs all tests found in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The directory path containing assemblies.</param>
    /// <returns>The list of test class results.</returns>
    public List<TestClassResult> RunTests(string directoryPath)
    {
        var testClasses = AttributeFinder.FindTestMethods(directoryPath);
        var classResults = new List<TestClassResult>();

        Parallel.ForEach(testClasses, testClass =>
        {
            var type = testClass.Key;
            var methods = testClass.Value;

            var classResult = new TestClassResult { ClassName = type.FullName! };

            var executor = new TestExecutor();

            executor.ExecuteStatic(methods.BeforeClass);

            foreach (var testMethod in methods.Tests)
            {
                var instance = Activator.CreateInstance(type)!;

                var testResult = executor.ExecuteTest(testMethod, instance, methods.Before, methods.After);
                classResult.TestResults.Add(testResult);
            }

            executor.ExecuteStatic(methods.AfterClass);

            lock (classResults)
            {
                classResults.Add(classResult);
            }
        });

        return classResults;
    }
}
