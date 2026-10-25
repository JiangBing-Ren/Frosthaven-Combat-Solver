using System.Collections.Generic;

public class AlphanumComparatorFast : IComparer<string>
{
	public int Compare(string x, string y)
	{
		if (x == null)
		{
			return 0;
		}
		if (y == null)
		{
			return 0;
		}
		int length = x.Length;
		int length2 = y.Length;
		int num = 0;
		int num2 = 0;
		while (num < length && num2 < length2)
		{
			char c = x[num];
			char c2 = y[num2];
			char[] array = new char[length];
			int num3 = 0;
			char[] array2 = new char[length2];
			int num4 = 0;
			do
			{
				array[num3++] = c;
				num++;
				if (num >= length)
				{
					break;
				}
				c = x[num];
			}
			while (char.IsDigit(c) == char.IsDigit(array[0]));
			do
			{
				array2[num4++] = c2;
				num2++;
				if (num2 >= length2)
				{
					break;
				}
				c2 = y[num2];
			}
			while (char.IsDigit(c2) == char.IsDigit(array2[0]));
			string text = new string(array);
			string text2 = new string(array2);
			int num6;
			if (char.IsDigit(array[0]) && char.IsDigit(array2[0]))
			{
				int num5 = int.Parse(text);
				int value = int.Parse(text2);
				num6 = num5.CompareTo(value);
			}
			else
			{
				num6 = text.CompareTo(text2);
			}
			if (num6 != 0)
			{
				return num6;
			}
		}
		return length - length2;
	}
}
