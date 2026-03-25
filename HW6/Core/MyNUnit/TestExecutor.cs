// <copyright file="TestExecutor.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using MyNUnit.Attributes;
using MyNUnit.Models;

/// <summary>
/// Executes individual tests and setup/teardown methods.
/// </summary>
internal class TestExecutor
{
    /// <summary>
    /// Executes a single test method.
    /// </summary>
    /// <param name="testMethod">The test method info.</param>
    /// <param name="testClassType">The type of the test class.</param>
    /// <param name="before">The before method.</param>
    /// <param name="after">The after method.</param>
    /// <returns>The test result.</returns>
    public TestResult ExecuteTest(MethodInfo testMethod, Type testClassType, MethodInfo? before, MethodInfo? after)
    {
        var testAttr = testMethod.GetCustomAttribute<TestAttribute>() ?? throw new InvalidOperationException("Method must have [Test] attribute");
        var result = new TestResult
        {
            TestName = testMethod.Name,
            Status = TestStatus.Passed,
            ExecutionTime = TimeSpan.Zero,
        };

        if (testAttr.Ignore != null)
        {
            result.Status = TestStatus.Ignored;
            result.IgnoreReason = testAttr.Ignore;
            return result;
        }

        var stopwatch = Stopwatch.StartNew();

        Exception? caught = null;
        var instance = Activator.CreateInstance(testClassType)!;

        try
        {
            before?.Invoke(instance, null);

            var isAsync = testMethod.ReturnType == typeof(Task);
            if (isAsync)
            {
                var task = (Task?)testMethod.Invoke(instance, null);
                if (task != null)
                {
                     task.Wait();
                }
            }
            else
            {
                testMethod.Invoke(instance, null);
            }
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            caught = tie.InnerException;
        }
        catch (Exception ex)
        {
            caught = ex;
        }

        if (caught != null)
        {
            if (testAttr.Expected != null && caught.GetType() == testAttr.Expected)
            {
                result.Status = TestStatus.Passed;
            }
            else
            {
                result.Status = TestStatus.Errored;
                result.Exception = caught;
            }
        }
        else if (testAttr.Expected != null)
        {
            result.Status = TestStatus.Failed;
            result.Exception = new Exception($"Expected exception of type {testAttr.Expected.FullName} but none was thrown.");
        }

        if (after != null)
        {
            try
            {
                after.Invoke(instance, null);
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                if (result.Status is TestStatus.Passed or TestStatus.Failed)
                {
                    result.Status = TestStatus.Errored;
                    result.Exception ??= tie.InnerException;
                }
            }
            catch (Exception ex)
            {
                if (result.Status is TestStatus.Passed or TestStatus.Failed)
                {
                    result.Status = TestStatus.Errored;
                    result.Exception ??= ex;
                }
            }
        }

        stopwatch.Stop();
        result.ExecutionTime = stopwatch.Elapsed;

        return result;
    }

    /// <summary>
    /// Executes a static method (BeforeClass / AfterClass).
    /// Returns true if executed successfully, false if exception occurred.
    /// </summary>
    /// <param name="method">The method to execute.</param>
    /// <returns>True if executed successfully, false if exception occurred.</returns>
    public bool ExecuteStatic(MethodInfo? method)
    {
        if (method == null)
        {
            return true;
        }

        try
        {
            method.Invoke(null, null);
            return true;
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            Console.Error.WriteLine(
                $"Error in {method.DeclaringType?.Name}.{method.Name}: {tie.InnerException.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Error in {method.DeclaringType?.Name}.{method.Name}: {ex.Message}");
            return false;
        }
    }
}
