using System;
using System.Reflection;

namespace Chronos.Reflection.Internal;

public static class TypeSerializer
{
	public static string Serialize(Type type)
	{
		return type.FullName;
	}

	public static Type Deserialize(string fullName)
	{
		Type type = Type.GetType(fullName, throwOnError: false, ignoreCase: false);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(fullName);
			if (type != null)
			{
				return type;
			}
		}
		throw new Exception("Failed to deserialize type: " + fullName);
	}
}
