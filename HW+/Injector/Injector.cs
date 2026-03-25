// <copyright file="Injector.cs" company="matveyakm">
// Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace Injector;

/// <summary>
/// Static class that provides dependency injection functionality.
/// </summary>
public static class Injector
{
    private static Dictionary<Type, object> instances = new();
    private static HashSet<Type> creating = new();

    /// <summary>
    /// Initializes the dependency injection container and creates an instance of the specified root class.
    /// </summary>
    /// <param name="rootClassName">The fully qualified name of the root class to instantiate.</param>
    /// <param name="availableClasses">The collection of available classes that can be used as dependencies.</param>
    /// <returns>An instance of the root class with all dependencies resolved.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the root class is not found, multiple implementations exist for a dependency, no implementation is found for a dependency, or a circular dependency is detected.</exception>
    public static object? Initialize(string rootClassName, IEnumerable<Type> availableClasses)
    {
        instances = new Dictionary<Type, object>();
        creating = new HashSet<Type>();

        var classes = availableClasses.ToList();
        var allTypesDict = classes.ToDictionary(t => t.FullName!, t => t);

        if (!allTypesDict.TryGetValue(rootClassName, out var rootType))
        {
            throw new InvalidOperationException($"Class {rootClassName} not found");
        }

        var dependencyMap = BuildDependencyMap(classes);

        foreach (var dep in dependencyMap)
        {
            if (dep.Value.Count > 1)
            {
                throw new InvalidOperationException($"Multiple implementations found for {dep.Key.FullName}");
            }

            if (dep.Value.Count == 0)
            {
                throw new InvalidOperationException($"No implementation found for {dep.Key.FullName}");
            }
        }

        return CreateInstance(rootType, dependencyMap);
    }

    /// <summary>
    /// Builds a mapping of dependency types to their available implementations.
    /// </summary>
    /// <param name="classes">The list of available classes.</param>
    /// <returns>A dictionary mapping dependency types to lists of implementing types.</returns>
    private static Dictionary<Type, List<Type>> BuildDependencyMap(List<Type> classes)
    {
        var dependencyMap = new Dictionary<Type, List<Type>>();

        foreach (var cls in classes)
        {
            var ctor = cls.GetConstructors().First();
            var paramTypes = ctor.GetParameters().Select(p => p.ParameterType).ToList();

            foreach (var paramType in paramTypes)
            {
                if (!dependencyMap.ContainsKey(paramType))
                {
                    dependencyMap[paramType] = new List<Type>();
                }

                var implementations = classes.Where(c => IsAssignable(paramType, c)).ToList();
                dependencyMap[paramType] = implementations;
            }
        }

        return dependencyMap;
    }

    /// <summary>
    /// Checks if the implementation type can be assigned to the target type.
    /// </summary>
    /// <param name="target">The target type (usually an interface or abstract class).</param>
    /// <param name="implementation">The implementation type to check.</param>
    /// <returns>True if the implementation can be assigned to the target; otherwise, false.</returns>
    private static bool IsAssignable(Type target, Type implementation)
    {
        return target.IsAssignableFrom(implementation);
    }

    /// <summary>
    /// Creates an instance of the specified type with all dependencies resolved.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="dependencyMap">The mapping of dependency types to their implementations.</param>
    /// <returns>An instance of the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected.</exception>
    private static object CreateInstance(Type type, Dictionary<Type, List<Type>> dependencyMap)
    {
        if (instances.ContainsKey(type))
        {
            return instances[type];
        }

        if (creating.Contains(type))
        {
            throw new InvalidOperationException($"Circular dependency detected for {type.FullName}");
        }

        creating.Add(type);

        try
        {
            var ctor = type.GetConstructors().First();
            var paramTypes = ctor.GetParameters();

            var args = new object[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                var paramType = paramTypes[i].ParameterType;

                if (!dependencyMap.TryGetValue(paramType, out var implementations) || implementations.Count == 0)
                {
                    throw new InvalidOperationException($"No implementation found for {paramType.FullName}");
                }

                if (implementations.Count > 1)
                {
                    throw new InvalidOperationException($"Multiple implementations found for {paramType.FullName}");
                }

                var implType = implementations[0];
                args[i] = CreateInstance(implType, dependencyMap);
            }

            var instance = ctor.Invoke(args);
            instances[type] = instance;
            return instance;
        }
        finally
        {
            creating.Remove(type);
        }
    }
}
