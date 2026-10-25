using System;
using System.Globalization;
using UnityEngine;

public static class CommonColorBuffer
{
	public static Color value = Color.white;

	public static string ColorToString(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
	}

	public static Color32 StringToColor(string colorString)
	{
		int num = int.Parse(colorString, NumberStyles.HexNumber);
		Color32 result;
		if (colorString.Length == 8)
		{
			result = new Color32((byte)((num >> 24) & 0xFF), (byte)((num >> 16) & 0xFF), (byte)((num >> 8) & 0xFF), (byte)(num & 0xFF));
		}
		else if (colorString.Length == 6)
		{
			result = new Color32((byte)((num >> 16) & 0xFF), (byte)((num >> 8) & 0xFF), (byte)(num & 0xFF), byte.MaxValue);
		}
		else if (colorString.Length == 4)
		{
			result = new Color32((byte)(((num >> 12) & 0xF) * 17), (byte)(((num >> 8) & 0xF) * 17), (byte)(((num >> 4) & 0xF) * 17), (byte)((num & 0xF) * 17));
		}
		else
		{
			if (colorString.Length != 3)
			{
				throw new FormatException("Support only RRGGBBAA, RRGGBB, RGBA, RGB formats");
			}
			result = new Color32((byte)(((num >> 8) & 0xF) * 17), (byte)(((num >> 4) & 0xF) * 17), (byte)((num & 0xF) * 17), byte.MaxValue);
		}
		return result;
	}
}
