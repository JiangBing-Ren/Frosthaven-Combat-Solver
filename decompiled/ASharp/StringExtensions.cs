using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
	public static bool IsInt(this string value)
	{
		try
		{
			int result;
			return int.TryParse(value, out result);
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static string Take(this string value, int count, bool ellipsis = false)
	{
		int num = Math.Min(count, value.Length);
		if (!ellipsis || num >= value.Length)
		{
			return value.Substring(0, num);
		}
		return $"{value.Substring(0, num)}...";
	}

	public static string Skip(this string value, int count)
	{
		return value.Substring(Math.Min(count, value.Length) - 1);
	}

	public static string Reverse(this string input)
	{
		char[] array = input.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	public static bool IsNullOrEmpty(this string value)
	{
		return string.IsNullOrEmpty(value);
	}

	public static bool IsNOTNullOrEmpty(this string value)
	{
		return !string.IsNullOrEmpty(value);
	}

	public static string Formatted(this string format, params object[] args)
	{
		return string.Format(format, args);
	}

	public static bool Match(this string value, string pattern)
	{
		return Regex.IsMatch(value, pattern);
	}

	public static string RemoveSpaces(this string value)
	{
		return value.Replace(" ", string.Empty);
	}

	public static string ReplaceLastOccurrence(this string value, string Find, string Replace)
	{
		int num = value.LastIndexOf(Find);
		if (num == -1)
		{
			return value;
		}
		return value.Remove(num, Find.Length).Insert(num, Replace);
	}

	public static string ReplaceRNWithBr(this string value)
	{
		return value.Replace("\r\n", "<br />").Replace("\n", "<br />");
	}

	public static string ToEmptyString(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return value;
		}
		return string.Empty;
	}

	public static string ToStringPretty<T>(this IEnumerable<T> source)
	{
		if (source != null)
		{
			return source.ToStringPretty(",");
		}
		return string.Empty;
	}

	public static string ToStringPretty<T>(this IEnumerable<T> source, string delimiter)
	{
		if (source != null)
		{
			return source.ToStringPretty(string.Empty, delimiter, string.Empty);
		}
		return string.Empty;
	}

	public static string ToStringPretty<T>(this IEnumerable<T> source, string before, string delimiter, string after)
	{
		if (source == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(before);
		bool flag = true;
		foreach (T item in source)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(delimiter);
			}
			stringBuilder.Append(item.ToString());
		}
		stringBuilder.Append(after);
		return stringBuilder.ToString();
	}

	public static string InvertCase(this string s)
	{
		return new string(s.Select((char c) =>
		{
			if (!char.IsLetter(c))
			{
				return c;
			}
			return (!char.IsUpper(c)) ? char.ToUpper(c) : char.ToLower(c);
		}).ToArray());
	}

	public static bool IsNullOrEmptyAfterTrimmed(this string s)
	{
		if (!s.IsNullOrEmpty())
		{
			return s.Trim().IsNullOrEmpty();
		}
		return true;
	}

	public static List<char> ToCharList(this string s)
	{
		if (!s.IsNOTNullOrEmpty())
		{
			return null;
		}
		return s.ToCharArray().ToList();
	}

	public static string SubstringFromXToY(this string s, int start, int end)
	{
		if (s.IsNullOrEmpty())
		{
			return string.Empty;
		}
		if (start >= s.Length || start < 0 || end < 0)
		{
			return string.Empty;
		}
		if (end >= s.Length)
		{
			end = s.Length - 1;
		}
		return s.Substring(start, end - start);
	}

	public static string RemoveChar(this string s, char c)
	{
		if (!s.IsNOTNullOrEmpty())
		{
			return string.Empty;
		}
		return s.Replace(c.ToString(), string.Empty);
	}

	public static int GetWordCount(this string s)
	{
		return new Regex("\\w+").Matches(s).Count;
	}

	public static string SubsetString(this string s, string startText, string endText, bool ignoreCase)
	{
		if (s.IsNullOrEmpty())
		{
			return string.Empty;
		}
		if (startText.IsNullOrEmpty() || endText.IsNullOrEmpty())
		{
			throw new ArgumentException("Start Text and End Text cannot be empty.");
		}
		string text = (ignoreCase ? s.ToUpperInvariant() : s);
		int num = (ignoreCase ? text.IndexOf(startText.ToUpperInvariant()) : text.IndexOf(startText));
		int end = (ignoreCase ? text.IndexOf(endText.ToUpperInvariant(), num) : text.IndexOf(endText, num));
		return text.SubstringFromXToY(num, end);
	}

	public static string SplitCamelCase(this string inputCamelCaseString)
	{
		return Regex.Replace(Regex.Replace(inputCamelCaseString, "([A-Z][a-z])", " $1", RegexOptions.Compiled).Trim(), "([A-Z][A-Z])", " $1", RegexOptions.Compiled).Trim();
	}

	public static string ToTitleCase(this string s)
	{
		return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
	}
}
