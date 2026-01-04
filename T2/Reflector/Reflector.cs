// <copyright file="Reflector.cs" company="matveyakm">
//     Copyright (c) matveyakm. All rights reserved.
// </copyright>

namespace ReflectionTools;

using System.Reflection;
using System.Text;

/// <summary>
/// Provides reflection utilities for analyzing and generating class structures.
/// </summary>
public class Reflector
{
    /// <summary>
    /// Compares two types and prints differences in fields and methods to the console.
    /// Differences include missing members, differing types, modifiers, parameters, etc.
    /// </summary>
    /// <param name="a">The first type to compare.</param>
    /// <param name="b">The second type to compare.</param>
    public void DiffClasses(Type a, Type b)
    {
        Console.WriteLine($"Comparing {a.Name} and {b.Name}:");

        this.CompareFields(a, b);
        this.CompareFields(b, a, true);

        this.CompareMethods(a, b);
        this.CompareMethods(b, a, true);
    }

    /// <summary>
    /// Generates a .cs file representing the structure of the given type, including fields, properties, methods, constructors, and nested types.
    /// The generated code is designed to be as close as possible to the original and compilable.
    /// Inherited members from System.Object are excluded.
    /// </summary>
    /// <param name="someClass">The type to reflect and generate code for.</param>
    public void PrintStructure(Type someClass)
    {
        if (someClass == null)
        {
            throw new ArgumentNullException(nameof(someClass));
        }

        string cleanName = this.GetCleanTypeName(someClass);
        string fileName = $"{cleanName}.g.cs";
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(someClass.Namespace))
        {
            sb.AppendLine($"namespace {someClass.Namespace}");
            sb.AppendLine("{");
        }

        string modifiers = this.GetTypeModifiers(someClass);
        string kind = someClass.IsInterface ? "interface" :
                    someClass.IsValueType ? "struct" :
                    someClass.IsEnum ? "enum" : "class";

        string genericParams = this.GetGenericParametersDeclaration(someClass);
        string baseTypes = this.GetBaseTypesAndInterfaces(someClass);

        sb.AppendLine($"    {modifiers}{kind} {this.GetCleanTypeName(someClass)}{genericParams}{baseTypes}");
        sb.AppendLine("    {");

        var nestedTypes = someClass.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var nested in nestedTypes.OrderBy(t => t.Name))
        {
            this.AppendNestedType(sb, nested, "        ");
        }

        var fields = someClass.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                            .Where(f => !f.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
                                        && !f.Name.Contains("k__BackingField"))
                            .OrderBy(f => f.Name);

        foreach (var field in fields)
        {
            string fieldMods = this.GetMemberModifiers(field);
            string fieldType = this.GetDisplayTypeName(field.FieldType);
            sb.AppendLine($"        {fieldMods}{fieldType} {field.Name};");
        }

        sb.AppendLine();

        var properties = someClass.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                                .Where(p => !p.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
                                .OrderBy(p => p.Name);

        foreach (var prop in properties)
        {
            string propMods = this.GetPropertyModifiers(prop);
            string propType = this.GetDisplayTypeName(prop.PropertyType);

            var getMethod = prop.GetGetMethod(true);
            var setMethod = prop.GetSetMethod(true);

            string getter = getMethod != null ? (getMethod.IsPublic ? "get;" : "private get;") : string.Empty;
            string setter = setMethod != null ? (setMethod.IsPublic ? "set;" : "private set;") : string.Empty;

            if (getMethod?.IsAbstract == true || setMethod?.IsAbstract == true)
            {
                getter = getter.Replace(";", " => throw null;");
                setter = setter.Replace(";", " => throw null;");
            }

            sb.AppendLine($"        {propMods}{propType} {prop.Name} {{ {getter} {setter} }}");
        }

        sb.AppendLine();

        var ctors = someClass.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .OrderBy(c => c.GetParameters().Length);

        foreach (var ctor in ctors)
        {
            string ctorMods = this.GetMemberModifiers(ctor);
            string paramsList = this.GetParametersDeclaration(ctor.GetParameters());
            sb.AppendLine($"        {ctorMods} {cleanName}{genericParams}({paramsList}) {{ }}");
        }

        sb.AppendLine();

        var methods = someClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                            .Where(m => !m.IsSpecialName
                                        && !this.IsObjectMethod(m)
                                        && !m.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
                            .OrderBy(m => m.Name);

        foreach (var method in methods)
        {
            string methodMods = this.GetMemberModifiers(method);
            string returnType = this.GetDisplayTypeName(method.ReturnType);
            string methodName = method.Name;
            string methodGenerics = this.GetGenericParametersDeclaration(method);
            string paramsList = this.GetParametersDeclaration(method.GetParameters());

            sb.AppendLine($"        {methodMods}{returnType} {methodName}{methodGenerics}({paramsList})");
            sb.AppendLine("        {");

            if (method.IsAbstract)
            {
                sb.AppendLine("            throw new NotImplementedException();");
            }
            else if (method.ReturnType != typeof(void))
            {
                sb.AppendLine($"            return default({returnType});");
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("    }");

        if (!string.IsNullOrEmpty(someClass.Namespace))
        {
            sb.AppendLine("}");
        }

        File.WriteAllText(fileName, sb.ToString());
        Console.WriteLine($"Файл {fileName} успешно создан.");
    }

    private bool IsObjectMethod(MethodInfo method)
    {
        if (method.DeclaringType != typeof(object))
        {
            return false;
        }

        return method.Name is "ToString" or "Equals" or "GetHashCode" or "Finalize" or "GetType" or "MemberwiseClone";
    }

    private void AppendNestedType(StringBuilder sb, Type nested, string indent)
    {
        string mods = this.GetTypeModifiers(nested);
        string kind = nested.IsInterface ? "interface" : nested.IsValueType ? "struct" : "class";
        string name = this.GetCleanTypeName(nested);
        string generics = this.GetGenericParametersDeclaration(nested);

        sb.AppendLine($"{indent}{mods}{kind} {name}{generics} {{ }}");
        sb.AppendLine();
    }

    private string GetCleanTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string name = type.Name.Split('`')[0];
        return name;
    }

    private string GetDisplayTypeName(Type type)
    {
        if (type == typeof(void))
        {
            return "void";
        }

        if (type == typeof(int))
        {
            return "int";
        }

        if (type == typeof(string))
        {
            return "string";
        }

        if (type == typeof(bool))
        {
            return "bool";
        }

        if (type == typeof(double))
        {
            return "double";
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string baseName = type.Name.Split('`')[0];
        var args = type.GetGenericArguments().Select(this.GetDisplayTypeName);
        return $"{baseName}<{string.Join(", ", args)}>";
    }

    private string GetGenericParametersDeclaration(Type type)
    {
        if (!type.IsGenericTypeDefinition && !type.ContainsGenericParameters)
        {
            return string.Empty;
        }

        var args = type.GetGenericArguments().Select(t => t.Name);
        return args.Any() ? $"<{string.Join(", ", args)}>" : string.Empty;
    }

    private string GetGenericParametersDeclaration(MethodInfo method)
    {
        if (!method.IsGenericMethod)
        {
            return string.Empty;
        }

        var args = method.GetGenericArguments().Select(t => t.Name);
        return $"<{string.Join(", ", args)}>";
    }

    private string GetBaseTypesAndInterfaces(Type type)
    {
        if (type.BaseType == null || type.BaseType == typeof(object) || type.BaseType == typeof(ValueType))
        {
            return string.Empty;
        }

        var parts = new List<string>();
        if (type.BaseType != typeof(object))
        {
            parts.Add(this.GetDisplayTypeName(type.BaseType));
        }

        var interfaces = type.GetInterfaces()
                            .Where(i => !type.BaseType?.GetInterfaces().Contains(i) ?? true)
                            .Select(this.GetDisplayTypeName);

        parts.AddRange(interfaces);
        return parts.Any() ? " : " + string.Join(", ", parts) : string.Empty;
    }

    private string GetPropertyModifiers(PropertyInfo prop)
    {
        var accessors = new[] { prop.GetGetMethod(true), prop.GetSetMethod(true) }
                        .Where(m => m != null)
                        .OrderByDescending(static m => m.IsPublic ? 1 : m.IsFamily ? 0 : -1);

        var highest = accessors.FirstOrDefault();
        return highest != null ? this.GetMemberModifiers(highest).TrimEnd() : "public ";
    }

    private string GetParametersDeclaration(ParameterInfo[] parameters)
    {
        return string.Join(", ", parameters.Select(p => $"{this.GetDisplayTypeName(p.ParameterType)} {p.Name}"));
    }

    private void CompareFields(Type source, Type target, bool reverse = false)
    {
        var sourceFields = source.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToDictionary(f => f.Name);
        var targetFields = target.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).ToDictionary(f => f.Name);

        Console.WriteLine("\nFields:");

        foreach (var field in sourceFields)
        {
            if (!targetFields.TryGetValue(field.Key, out FieldInfo? targetField))
            {
                string direction = reverse ? $"in {target.Name} but not in {source.Name}" : $"in {source.Name} but not in {target.Name}";
                Console.WriteLine($" - Field {field.Value} {direction}");
                continue;
            }

            List<string> diffs = new List<string>();
            if (field.Value.FieldType != targetField.FieldType)
            {
                diffs.Add($"Type: {field.Value.FieldType} vs {targetField.FieldType}");
            }

            if (field.Value.Attributes != targetField.Attributes)
            {
                diffs.Add($"Attributes: {field.Value.Attributes} vs {targetField.Attributes}");
            }

            if (field.Value.IsPublic != targetField.IsPublic)
            {
                diffs.Add("Visibility");
            }

            if (field.Value.IsStatic != targetField.IsStatic)
            {
                diffs.Add("Static modifier");
            }

            if (diffs.Any())
            {
                Console.WriteLine($" - Field {field.Key} differs: {string.Join(", ", diffs)}");
            }
        }
    }

    private void CompareMethods(Type source, Type target, bool reverse = false)
    {
        var sourceMethods = source.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .GroupBy(m => m.Name)
            .ToDictionary(g => g.Key, g => g.ToList());

        var targetMethods = target.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .GroupBy(m => m.Name)
            .ToDictionary(g => g.Key, g => g.ToList());

        Console.WriteLine("\nMethods:");

        foreach (var methodGroup in sourceMethods)
        {
            if (!targetMethods.TryGetValue(methodGroup.Key, out List<MethodInfo>? targetGroup))
            {
                string direction = reverse ? $"in {target.Name} but not in {source.Name}" : $"in {source.Name} but not in {target.Name}";
                Console.WriteLine($" - Method {methodGroup.Key} {direction}");
                continue;
            }

            foreach (var sourceMethod in methodGroup.Value)
            {
                var matchingTarget = targetGroup.FirstOrDefault(t => this.MethodSignaturesMatch(sourceMethod, t));
                if (matchingTarget == null)
                {
                    string direction = reverse ? $"in {target.Name} but not in {source.Name}" : $"in {source.Name} but not in {target.Name}";
                    Console.WriteLine($" - Method {this.GetMethodSignature(sourceMethod)} {direction}");
                    continue;
                }

                List<string> diffs = new List<string>();
                if (sourceMethod.ReturnType != matchingTarget.ReturnType)
                {
                    diffs.Add($"Return type: {sourceMethod.ReturnType} vs {matchingTarget.ReturnType}");
                }

                if (sourceMethod.Attributes != matchingTarget.Attributes)
                {
                    diffs.Add($"Attributes: {sourceMethod.Attributes} vs {matchingTarget.Attributes}");
                }

                if (sourceMethod.IsPublic != matchingTarget.IsPublic)
                {
                    diffs.Add("Visibility");
                }

                if (sourceMethod.IsStatic != matchingTarget.IsStatic)
                {
                    diffs.Add("Static modifier");
                }

                if (sourceMethod.IsGenericMethod != matchingTarget.IsGenericMethod)
                {
                    diffs.Add("Generics");
                }

                if (diffs.Any())
                {
                    Console.WriteLine($" - Method {this.GetMethodSignature(sourceMethod)} differs: {string.Join(", ", diffs)}");
                }

                targetGroup.Remove(matchingTarget);
            }
        }
    }

    private bool MethodSignaturesMatch(MethodInfo a, MethodInfo b)
    {
        if (a.Name != b.Name)
        {
            return false;
        }

        var aParams = a.GetParameters();
        var bParams = b.GetParameters();
        if (aParams.Length != bParams.Length)
        {
            return false;
        }

        for (int i = 0; i < aParams.Length; i++)
        {
            if (aParams[i].ParameterType != bParams[i].ParameterType)
            {
                return false;
            }
        }

        if (a.IsGenericMethod != b.IsGenericMethod)
        {
            return false;
        }

        if (a.IsGenericMethod)
        {
            var aGen = a.GetGenericArguments();
            var bGen = b.GetGenericArguments();
            if (aGen.Length != bGen.Length)
            {
                return false;
            }
        }

        return true;
    }

    private string GetMethodSignature(MethodInfo method)
    {
        string paramsList = string.Join(", ", method.GetParameters().Select(p => $"{this.GetTypeName(p.ParameterType)} {p.Name}"));
        string generics = this.GetGenericParameters(method);
        return $"{method.Name}{generics}({paramsList})";
    }

    private string GetTypeModifiers(Type type)
    {
        string mods = string.Empty;
        if (type.IsPublic)
        {
            mods += "public ";
        }
        else if (type.IsNotPublic)
        {
            mods += "internal ";
        }

        if (type.IsAbstract && !type.IsInterface)
        {
            mods += "abstract ";
        }

        if (type.IsSealed)
        {
            mods += "sealed ";
        }

        return mods;
    }

    private string GetMemberModifiers(MemberInfo member)
    {
        string mods = string.Empty;
        if (member is MethodBase methodBase)
        {
            if (methodBase.IsPublic)
            {
                mods += "public ";
            }
            else if (methodBase.IsFamily)
            {
                mods += "protected ";
            }
            else if (methodBase.IsPrivate)
            {
                mods += "private ";
            }

            if (methodBase.IsStatic)
            {
                mods += "static ";
            }

            if (methodBase.IsAbstract)
            {
                mods += "abstract ";
            }

            if (methodBase.IsVirtual)
            {
                mods += "virtual ";
            }
        }
        else if (member is FieldInfo field)
        {
            if (field.IsPublic)
            {
                mods += "public ";
            }
            else if (field.IsFamily)
            {
                mods += "protected ";
            }
            else if (field.IsPrivate)
            {
                mods += "private ";
            }

            if (field.IsStatic)
            {
                mods += "static ";
            }

            if (field.IsInitOnly)
            {
                mods += "readonly ";
            }
        }
        else if (member is PropertyInfo prop)
        {
            var getMethod = prop.GetGetMethod(true);
            if (getMethod != null)
            {
                if (getMethod.IsPublic)
                {
                    mods += "public ";
                }
                else if (getMethod.IsFamily)
                {
                    mods += "protected ";
                }
                else if (getMethod.IsPrivate)
                {
                    mods += "private ";
                }

                if (getMethod.IsStatic)
                {
                    mods += "static ";
                }

                if (getMethod.IsAbstract)
                {
                    mods += "abstract ";
                }

                if (getMethod.IsVirtual)
                {
                    mods += "virtual ";
                }
            }
        }

        return mods;
    }

    private string GetGenericParameters(MethodInfo method)
    {
        if (!method.IsGenericMethod)
        {
            return string.Empty;
        }

        string[] genArgs = method.GetGenericArguments().Select(arg => arg.Name).ToArray();
        return $"<{string.Join(", ", genArgs)}>";
    }

    private string GetParametersList(ParameterInfo[] parameters)
    {
        return string.Join(", ", parameters.Select(p => $"{this.GetTypeName(p.ParameterType)} {p.Name}"));
    }

    private string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string baseName = type.Name.Substring(0, type.Name.IndexOf('`'));
            string[] genArgs = type.GetGenericArguments().Select(this.GetTypeName).ToArray();
            return $"{baseName}<{string.Join(", ", genArgs)}>";
        }

        return type.Name;
    }
}