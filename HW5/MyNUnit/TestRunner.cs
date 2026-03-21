// <copyright file="TestRunner.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyNUnit.Models;

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

            var classResult = new TestClassResult(type.FullName!);

            var executor = new TestExecutor();

            bool beforeClassOk = executor.ExecuteStatic(methods.BeforeClass);

            if (!beforeClassOk)
            {
                foreach (var testMethod in methods.Tests)
                {
                    var erroredResult = new TestResult
                    {
                        TestName = testMethod.Name,
                        Status = TestStatus.Errored,
                        Exception = new Exception($"BeforeClass method failed for class {type.FullName}"),
                        ExecutionTime = TimeSpan.Zero,
                    };
                    classResult.Add(erroredResult);
                }

                lock (classResults)
                {
                    classResults.Add(classResult);
                }

                return;
            }

            foreach (var testMethod in methods.Tests)
            {
                var testResult = executor.ExecuteTest(
                    testMethod,
                    type,
                    methods.Before,
                    methods.After);

                classResult.Add(testResult);
            }

            bool afterClassOk = executor.ExecuteStatic(methods.AfterClass);

            if (!afterClassOk)
            {
                // Можно добавить общее предупреждение к классу, но тесты уже выполнены..
                // Пускай будет без этого...
            }

            lock (classResults)
            {
                classResults.Add(classResult);
            }
        });

        return classResults;
    }
}
