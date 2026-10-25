using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using SharedLibrary;

namespace ScenarioRuleLibrary;

public static class Extensions
{
	[Serializable]
	[StateDump(UseToString = true, NoMembers = true)]
	public class RandomState : ISerializable
	{
		public readonly byte[] State;

		public RandomState(byte[] state)
		{
			State = state;
		}

		public override string ToString()
		{
			SharedLibrary.Random random = this.Restore();
			int num = random.Next();
			int num2 = random.Next();
			int num3 = random.Next();
			return $"RandomState({num}, {num2}, {num3})";
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("State", State);
		}

		private RandomState(SerializationInfo info, StreamingContext context)
		{
			try
			{
				State = (byte[])info.GetValue("State", typeof(byte[]));
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize RandomState\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public static string ToStringWithInfinity(this int value, int threshold = 90)
	{
		if (value >= threshold)
		{
			return "∞";
		}
		return value.ToString();
	}

	public static void Shuffle<T>(this IList<T> list, SharedLibrary.Random rng = null)
	{
		rng = ((rng != null) ? rng : ScenarioManager.CurrentScenarioState.ScenarioRNG);
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = rng.Next(num);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static RandomState Save(this SharedLibrary.Random random)
	{
		if (random != null)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, random);
			return new RandomState(memoryStream.ToArray());
		}
		return null;
	}

	public static SharedLibrary.Random Restore(this RandomState state)
	{
		if (state != null)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter
			{
				Binder = new RandomSerializationBinding()
			};
			using MemoryStream serializationStream = new MemoryStream(state.State);
			return (SharedLibrary.Random)binaryFormatter.Deserialize(serializationStream);
		}
		return null;
	}

	public static int AddOrdered<T>(this List<T> list, T addItem, Comparison<T> comparer)
	{
		if (list.Count == 0)
		{
			list.Add(addItem);
			return 0;
		}
		if (comparer == null)
		{
			list.Add(addItem);
			return list.Count - 1;
		}
		for (int i = 0; i < list.Count; i++)
		{
			T y = list[i];
			if (comparer(addItem, y) < 0)
			{
				list.Insert(i, addItem);
				return i;
			}
		}
		list.Add(addItem);
		return list.Count - 1;
	}

	public static int AddOrdered<T>(this List<T> list, T addItem, List<T> orderList)
	{
		if (list.Count == 0)
		{
			list.Add(addItem);
			return 0;
		}
		if (orderList == null)
		{
			list.Add(addItem);
			return list.Count - 1;
		}
		int num = orderList.IndexOf(addItem);
		if (num < 0)
		{
			list.Add(addItem);
			return list.Count - 1;
		}
		for (int i = 0; i < list.Count; i++)
		{
			T item = list[i];
			int num2 = orderList.IndexOf(item);
			if (num2 < 0)
			{
				num2 = int.MaxValue;
			}
			if (num < num2)
			{
				list.Insert(i, addItem);
				return i;
			}
		}
		list.Add(addItem);
		return list.Count - 1;
	}

	public static IEnumerable<T> AsIterable<T>(this T t)
	{
		return Iterate<T>(t);
	}

	public static IEnumerable<T> Iterate<T>(params T[] p)
	{
		return p;
	}
}
