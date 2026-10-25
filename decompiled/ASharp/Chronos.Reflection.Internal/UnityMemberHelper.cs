using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Chronos.Reflection.Internal;

internal static class UnityMemberHelper
{
	internal static bool TryReflectMethod(out MethodInfo methodInfo, out UnityReflectionException exception, UnityEngine.Object reflectionTarget, string name, Type[] parameterTypes)
	{
		methodInfo = null;
		Type type = reflectionTarget.GetType();
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		if (parameterTypes != null)
		{
			methodInfo = type.GetMethod(name, bindingAttr, null, parameterTypes, null);
			if (methodInfo == null)
			{
				methodInfo = (from extension in type.GetExtensionMethods()
					where extension.Name == name
					where (from paramInfo in extension.GetParameters()
						select paramInfo.ParameterType).SequenceEqual(parameterTypes)
					select extension).FirstOrDefault();
			}
			if (methodInfo == null)
			{
				exception = new UnityReflectionException(string.Format("No matching method found: '{0}.{1} ({2})'", type.Name, name, string.Join(", ", parameterTypes.Select((Type t) => t.Name).ToArray())));
				return false;
			}
		}
		else
		{
			List<MethodInfo> collection = type.GetMember(name, MemberTypes.Method, bindingAttr).OfType<MethodInfo>().ToList();
			List<MethodInfo> collection2 = (from extension in type.GetExtensionMethods()
				where extension.Name == name
				select extension).ToList();
			List<MethodInfo> list = new List<MethodInfo>();
			list.AddRange(collection);
			list.AddRange(collection2);
			if (list.Count == 0)
			{
				exception = new UnityReflectionException($"No matching method found: '{type.Name}.{name}'");
				return false;
			}
			if (list.Count > 1)
			{
				exception = new UnityReflectionException($"Multiple method signatures found for '{type.FullName}.{name}'\nSpecify the parameter types explicitly.");
				return false;
			}
			methodInfo = list[0];
		}
		exception = null;
		return true;
	}

	internal static MethodInfo ReflectMethod(UnityEngine.Object reflectionTarget, string name, Type[] parameterTypes)
	{
		if (!TryReflectMethod(out var methodInfo, out var exception, reflectionTarget, name, parameterTypes))
		{
			throw exception;
		}
		return methodInfo;
	}

	internal static object InvokeMethod(UnityEngine.Object reflectionTarget, MethodInfo methodInfo, bool isExtension, params object[] parameters)
	{
		if (isExtension)
		{
			object[] array = new object[parameters.Length + 1];
			array[0] = reflectionTarget;
			Array.Copy(parameters, 0, array, 1, parameters.Length);
			parameters = array;
		}
		return methodInfo.Invoke(reflectionTarget, parameters);
	}

	internal static bool TryReflectVariable(out MemberInfo variableInfo, out UnityReflectionException exception, UnityEngine.Object reflectionTarget, string name)
	{
		variableInfo = null;
		Type type = reflectionTarget.GetType();
		MemberTypes type2 = MemberTypes.Field | MemberTypes.Property;
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		MemberInfo[] member = type.GetMember(name, type2, bindingAttr);
		if (member.Length == 0)
		{
			exception = new UnityReflectionException($"No matching field or property found: '{type.Name}.{name}'");
			return false;
		}
		variableInfo = member[0];
		exception = null;
		return true;
	}

	internal static MemberInfo ReflectVariable(UnityEngine.Object reflectionTarget, string name)
	{
		if (!TryReflectVariable(out var variableInfo, out var exception, reflectionTarget, name))
		{
			throw exception;
		}
		return variableInfo;
	}
}
