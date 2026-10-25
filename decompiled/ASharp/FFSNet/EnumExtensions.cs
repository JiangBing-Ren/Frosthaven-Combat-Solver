using System;
using System.Reflection;

namespace FFSNet;

public static class EnumExtensions
{
	public static T GetAttribute<T>(this Enum value) where T : Attribute
	{
		Type type = value.GetType();
		string name = Enum.GetName(type, value);
		return type.GetField(name).GetCustomAttribute<T>();
	}
}
