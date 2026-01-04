// <copyright file="AttributeFinder.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MyNUnit.Attributes;

namespace MyNUnit;

/// <summary>
/// Provides functionality to find and collect test-related methods from assemblies.
/// </summary>
internal static class AttributeFinder
{
    /// <summary>
    /// Finds all test classes and their associated methods from the assemblies in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing the assemblies.</param>
    /// <returns>A dictionary where keys are test classes and values are collections of test methods and setup/teardown methods.</returns>
    public static Dictionary<Type, TestClassMethods> FindTestMethods(string directoryPath)
    {
        var testClasses = new Dictionary<Type, TestClassMethods>();

        IEnumerable<string> assemblyFiles;
        try
        {
            assemblyFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly);
        }
        catch
        {
            assemblyFiles = Enumerable.Empty<string>();
        }

        foreach (var file in assemblyFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);

                foreach (var type in assembly.GetTypes())
                {
                    var testMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m => m.GetCustomAttribute<TestAttribute>() != null)
                        .ToList();

                    if (testMethods.Count > 0)
                    {
                        var methods = new TestClassMethods
                        {
                            BeforeClass = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                .FirstOrDefault(m => m.GetCustomAttribute<BeforeClassAttribute>() != null),

                            AfterClass = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                .FirstOrDefault(m => m.GetCustomAttribute<AfterClassAttribute>() != null),

                            Before = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .FirstOrDefault(m => m.GetCustomAttribute<BeforeAttribute>() != null),

                            After = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .FirstOrDefault(m => m.GetCustomAttribute<AfterAttribute>() != null),

                            Tests = testMethods
                        };

                        testClasses[type] = methods;
                    }
                }
            }
            catch (BadImageFormatException)
            {
                // Не .NET сборка — пропускаем
            }
            catch (ReflectionTypeLoadException)
            {
                // Проблемы с загрузкой типов — пропускаем сборку
            }
            catch
            {
                // Другие ошибки загрузки — молча пропускаем
            }
        }

        return testClasses;
    }

    /// <summary>
    /// Represents the methods associated with a test class.
    /// </summary>
    public class TestClassMethods
    {
        /// <summary>
        /// Gets or sets the BeforeClass method.
        /// </summary>
        public MethodInfo? BeforeClass { get; set; }

        /// <summary>
        /// Gets or sets the AfterClass method.
        /// </summary>
        public MethodInfo? AfterClass { get; set; }

        /// <summary>
        /// Gets or sets the Before method.
        /// </summary>
        public MethodInfo? Before { get; set; }

        /// <summary>
        /// Gets or sets the After method.
        /// </summary>
        public MethodInfo? After { get; set; }

        /// <summary>
        /// Gets or sets the list of test methods.
        /// </summary>
        public List<MethodInfo> Tests { get; set; } = new();
    }
}