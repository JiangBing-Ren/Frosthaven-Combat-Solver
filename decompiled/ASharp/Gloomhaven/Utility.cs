using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Gloomhaven;

public static class Utility
{
	private static string systemId = SystemInfo.deviceUniqueIdentifier;

	public static float Round(float value, int digits)
	{
		float num = Mathf.Pow(10f, digits);
		return Mathf.Round(value * num) / num;
	}

	public static string MD5Hash(string input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(input));
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static string RandomMD5Hash()
	{
		System.Random random = new System.Random(DateTime.Now.Millisecond);
		return MD5Hash(systemId + DateTime.Now.ToString() + (float)random.NextDouble());
	}
}
