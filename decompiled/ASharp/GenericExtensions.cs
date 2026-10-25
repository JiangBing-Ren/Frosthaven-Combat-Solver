using System.Collections.Generic;
using System.Linq;

public static class GenericExtensions
{
	public static bool In<T>(this T obj, params T[] args)
	{
		return args.Contains(obj);
	}

	public static bool IsIn<T>(this T source, params T[] list) where T : class
	{
		if (source != null && !list.IsNullOrEmpty())
		{
			return list.Contains(source);
		}
		return false;
	}

	public static bool IsIn<T>(this T source, params T?[] list) where T : struct
	{
		if (!list.IsNullOrEmpty())
		{
			return list.Contains(source);
		}
		return false;
	}

	public static bool IsIn(this int source, params int[] list)
	{
		if (!list.IsNullOrEmpty())
		{
			return list.Contains(source);
		}
		return false;
	}

	public static bool IsNotIn<T>(this T source, params T[] list) where T : class
	{
		if (source == null)
		{
			return false;
		}
		if (list.IsNullOrEmpty())
		{
			return true;
		}
		return !list.Contains(source);
	}

	public static bool IsNotIn<T>(this T source, params T?[] list) where T : struct
	{
		if (list.IsNullOrEmpty())
		{
			return true;
		}
		return !list.Contains(source);
	}

	public static bool IsNotIn(this int source, params int[] list)
	{
		if (list.IsNullOrEmpty())
		{
			return true;
		}
		return !list.Contains(source);
	}

	public static List<T> AsList<T>(this T tobject)
	{
		return new List<T> { tobject };
	}

	public static bool IsTNull<T>(this T tObj)
	{
		return EqualityComparer<T>.Default.Equals(tObj, default);
	}

	public static bool IsDefault<T>(this T value) where T : struct
	{
		return value.Equals(default(T));
	}
}
