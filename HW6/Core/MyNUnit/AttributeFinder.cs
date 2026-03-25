// <copyright file="AttributeFinder.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace MyNUnit;

using System.Reflection;
using MyNUnit.Attributes;

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
        IEnumerable<string> assemblyFiles;
        try
        {
            assemblyFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly);
        }
        catch (ArgumentException)
        {
            assemblyFiles = Enumerable.Empty<string>();
        }
        catch (DirectoryNotFoundException)
        {
            assemblyFiles = Enumerable.Empty<string>();
        }
        catch (UnauthorizedAccessException)
        {
            assemblyFiles = Enumerable.Empty<string>();
        }
        catch (IOException)
        {
            assemblyFiles = Enumerable.Empty<string>();
        }

        var assemblies = new List<Assembly>();

        foreach (var file in assemblyFiles)
        {
            try
            {
                assemblies.Add(Assembly.LoadFrom(file));
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

        return FindTestMethodsInAssemblies(assemblies);
    }

    /// <summary>
    /// Finds test classes and methods in the given assemblies (useful for unit testing).
    /// </summary>
    /// // <param name="assemblies">The collection of assemblies to scan for test classes and methods.</param>
    /// <returns>A dictionary where keys are test classes and values are collections of test methods and setup/teardown methods.</returns>
    internal static Dictionary<Type, TestClassMethods> FindTestMethodsInAssemblies(IEnumerable<Assembly> assemblies)
    {
        var testClasses = new Dictionary<Type, TestClassMethods>();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                var testMethods = FindValidTestMethods(type);

                if (testMethods.Count > 0)
                {
                    var methods = new TestClassMethods
                    {
                        BeforeClass = FindBeforeClassMethod(type),
                        AfterClass = FindAfterClassMethod(type),
                        Before = FindBeforeMethod(type),
                        After = FindAfterMethod(type),
                        Tests = testMethods,
                    };

                    testClasses[type] = methods;
                }
            }
        }

        return testClasses;
    }

    private static List<MethodInfo> FindValidTestMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m =>
                m.GetCustomAttribute<TestAttribute>() != null &&
                !m.IsStatic &&
                (m.ReturnType == typeof(void) || m.ReturnType == typeof(Task)) &&
                m.GetParameters().Length == 0)
            .ToList();
    }

    private static MethodInfo? FindBeforeClassMethod(Type type)
    {
        return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m =>
                m.GetCustomAttribute<BeforeClassAttribute>() != null &&
                m.ReturnType == typeof(void) &&
                m.GetParameters().Length == 0);
    }

    private static MethodInfo? FindAfterClassMethod(Type type)
    {
        return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m =>
                m.GetCustomAttribute<AfterClassAttribute>() != null &&
                m.ReturnType == typeof(void) &&
                m.GetParameters().Length == 0);
    }

    private static MethodInfo? FindBeforeMethod(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m =>
                m.GetCustomAttribute<BeforeAttribute>() != null &&
                m.ReturnType == typeof(void) &&
                m.GetParameters().Length == 0);
    }

    private static MethodInfo? FindAfterMethod(Type type)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m =>
                m.GetCustomAttribute<AfterAttribute>() != null &&
                m.ReturnType == typeof(void) &&
                m.GetParameters().Length == 0);
    }

    /// <summary>
    /// Represents the methods associated with a test class.
    /// </summary>
    public record struct TestClassMethods
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestClassMethods"/> struct.
        /// </summary>
        public TestClassMethods()
        {
        }

        /// <summary>
        /// Gets the method decorated with the BeforeClass attribute, if any.
        /// </summary>
        public MethodInfo? BeforeClass { get; init; }

        /// <summary>
        /// Gets the method decorated with the AfterClass attribute, if any.
        /// </summary>
        public MethodInfo? AfterClass { get; init; }

        /// <summary>
        /// Gets the method decorated with the Before attribute, if any.
        /// </summary>
        public MethodInfo? Before { get; init; }

        /// <summary>
        /// Gets the method decorated with the After attribute, if any.
        /// </summary>
        public MethodInfo? After { get; init; }

        /// <summary>
        /// Gets the list of methods decorated with the Test attribute.
        /// </summary>
        public List<MethodInfo> Tests { get; init; } = new();
    }
}