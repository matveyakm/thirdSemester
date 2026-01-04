// <copyright file="TestExecutor.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Reflection;
using MyNUnit.Attributes;
using MyNUnit.Models;

namespace MyNUnit;

/// <summary>
/// Executes individual tests and setup/teardown methods.
/// </summary>
internal class TestExecutor
{
    /// <summary>
    /// Executes a single test method.
    /// </summary>
    /// <param name="testMethod">The test method info.</param>
    /// <param name="instance">The instance of the test class.</param>
    /// <param name="before">The before method.</param>
    /// <param name="after">The after method.</param>
    /// <returns>The test result.</returns>
    public TestResult ExecuteTest(MethodInfo testMethod, object instance, MethodInfo? before, MethodInfo? after)
    {
        var testAttr = testMethod.GetCustomAttribute<TestAttribute>()!;
        var result = new TestResult
        {
            TestName = testMethod.Name,
            Status = TestStatus.Passed
        };

        if (testAttr.Ignore != null)
        {
            result.Status = TestStatus.Ignored;
            result.IgnoreReason = testAttr.Ignore;
            return result;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            before?.Invoke(instance, null);
            testMethod.Invoke(instance, null);
            after?.Invoke(instance, null);

            if (testAttr.Expected != null)
            {
                result.Status = TestStatus.Failed;
                result.Exception = new Exception($"Expected exception of type {testAttr.Expected} but none was thrown.");
            }
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            var inner = ex.InnerException;
            if (testAttr.Expected != null && inner.GetType() == testAttr.Expected)
            {
                after?.Invoke(instance, null);
            }
            else
            {
                result.Status = TestStatus.Failed;
                result.Exception = inner;
            }
        }
        catch (Exception ex)
        {
            result.Status = TestStatus.Failed;
            result.Exception = ex;
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }

    /// <summary>
    /// Executes a static setup or teardown method.
    /// </summary>
    /// <param name="method">The method to execute.</param>
    public void ExecuteStatic(MethodInfo? method)
    {
        if (method != null)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in {method.Name}: {ex.Message}");
            }
        }
    }
}
