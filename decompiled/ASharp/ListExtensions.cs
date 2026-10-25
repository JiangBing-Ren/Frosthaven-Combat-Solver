using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
	public static bool IsNullOrEmpty<T>(this T[] data)
	{
		if (data != null)
		{
			return data.Length == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>(this List<T> data)
	{
		if (data != null)
		{
			return data.Count == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T>(this HashSet<T> data)
	{
		if (data != null)
		{
			return data.Count == 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1, T2> data)
	{
		if (data != null)
		{
			return data.Count == 0;
		}
		return true;
	}

	public static IEnumerable<T> RemoveDuplicates<T>(this ICollection<T> list, Func<T, int> Predicate)
	{
		Dictionary<int, T> dictionary = new Dictionary<int, T>();
		foreach (T item in list)
		{
			if (!dictionary.ContainsKey(Predicate(item)))
			{
				dictionary.Add(Predicate(item), item);
			}
		}
		return dictionary.Values.AsEnumerable();
	}

	public static T DequeueOrNull<T>(this Queue<T> q)
	{
		try
		{
			return (q.Count > 0) ? q.Dequeue() : default(T);
		}
		catch (Exception)
		{
			return default;
		}
	}

	public static int IndexOf<T>(this T[] data, T element)
	{
		if (data == null)
		{
			return -1;
		}
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i] != null)
			{
				if (data[i].Equals(element))
				{
					return i;
				}
			}
			else if (element == null)
			{
				return i;
			}
		}
		return -1;
	}

	public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		foreach (T item in items)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static T MaxBy<T, TU>(this IEnumerable<T> data, Func<T, TU> selector) where TU : IComparable<TU>
	{
		if (data == null)
		{
			return default;
		}
		T result = default;
		TU other = default;
		bool flag = true;
		foreach (T datum in data)
		{
			TU val = selector(datum);
			if (flag || val.CompareTo(other) > 0)
			{
				flag = false;
				other = val;
				result = datum;
			}
		}
		return result;
	}

	public static List<T> MaxManyBy<T, TU>(this IEnumerable<T> data, Func<T, TU> selector) where TU : IComparable<TU>
	{
		if (data == null)
		{
			return new List<T>();
		}
		List<T> list = new List<T>();
		TU other = default;
		bool flag = true;
		foreach (T datum in data)
		{
			TU val = selector(datum);
			if (flag)
			{
				other = val;
				list.Add(datum);
				flag = false;
			}
			else if (val.CompareTo(other) > 0)
			{
				other = val;
				list.Clear();
				list.Add(datum);
			}
			else if (val.CompareTo(other) == 0)
			{
				list.Add(datum);
			}
		}
		return list;
	}

	public static Tuple<TU, List<T>> MaxManyByAndValue<T, TU>(this IEnumerable<T> data, Func<T, TU> selector) where TU : IComparable<TU>
	{
		if (data == null)
		{
			return null;
		}
		List<T> list = new List<T>();
		TU val = default;
		bool flag = true;
		foreach (T datum in data)
		{
			TU val2 = selector(datum);
			if (flag)
			{
				val = val2;
				list.Add(datum);
				flag = false;
			}
			else if (val2.CompareTo(val) > 0)
			{
				val = val2;
				list.Clear();
				list.Add(datum);
			}
			else if (val2.CompareTo(val) == 0)
			{
				list.Add(datum);
			}
		}
		return new Tuple<TU, List<T>>(val, list);
	}
}
