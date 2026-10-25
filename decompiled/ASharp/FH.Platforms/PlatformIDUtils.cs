using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FH.Platforms;

public static class PlatformIDUtils
{
	public static string[] GetPlatformIDs()
	{
		List<PlatformIDAttribute> platformIDAttributes = GetPlatformIDAttributes();
		string[] array = new string[platformIDAttributes.Count];
		for (int i = 0; i < platformIDAttributes.Count; i++)
		{
			array[i] = platformIDAttributes[i].ID;
		}
		return array;
	}

	public static List<PlatformIDAttribute> GetPlatformIDAttributes()
	{
		List<PlatformIDAttribute> list = new List<PlatformIDAttribute>();
		foreach (Type item2 in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly x) => x.GetTypes()))
		{
			if (item2.IsAbstract)
			{
				continue;
			}
			object[] customAttributes = item2.GetCustomAttributes(typeof(PlatformIDAttribute), inherit: true);
			if (customAttributes.Length != 0)
			{
				PlatformIDAttribute item = customAttributes[0] as PlatformIDAttribute;
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
