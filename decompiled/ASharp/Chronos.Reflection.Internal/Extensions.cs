using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Chronos.Reflection.Internal;

public static class Extensions
{
	private static MethodInfo[] extensionMethodsCache;

	public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> groups)
	{
		HashSet<T> hashSet = null;
		foreach (IEnumerable<T> group in groups)
		{
			if (hashSet == null)
			{
				hashSet = new HashSet<T>(group);
			}
			else
			{
				hashSet.IntersectWith(group);
			}
		}
		if (hashSet != null)
		{
			return hashSet.AsEnumerable();
		}
		return Enumerable.Empty<T>();
	}

	public static bool HasFlag(this Enum value, Enum flag)
	{
		long num = Convert.ToInt64(value);
		long num2 = Convert.ToInt64(flag);
		return (num & num2) != 0;
	}

	public static IEnumerable<MethodInfo> GetExtensionMethods(this Type type)
	{
		if (extensionMethodsCache == null)
		{
			extensionMethodsCache = (from method in (from potentialType in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly assembly) => assembly.GetTypes())
					where potentialType.IsSealed && !potentialType.IsGenericType && !potentialType.IsNested
					select potentialType).SelectMany((Type extensionType) => extensionType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				where method.IsExtension()
				select method).ToArray();
		}
		return extensionMethodsCache.Where((MethodInfo method) => method.GetParameters()[0].ParameterType.IsAssignableFrom(type));
	}

	public static bool IsExtension(this MethodInfo methodInfo)
	{
		return methodInfo.IsDefined(typeof(ExtensionAttribute), inherit: false);
	}
}
