using System;

namespace FFSNet;

public class Enum<T> where T : struct, IConvertible
{
	public static int BitsRequired
	{
		get
		{
			if (!typeof(T).IsEnum)
			{
				throw new ArgumentException("T must be an enumerated type");
			}
			return Utility.GetBitsRequired(Enum.GetNames(typeof(T)).Length);
		}
	}
}
