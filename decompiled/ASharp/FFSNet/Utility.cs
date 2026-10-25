using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SevenZip.Compression.LZMA;

namespace FFSNet;

public static class Utility
{
	public static string GetRandomCode(int codeLength = 8)
	{
		string text = "ABCDEFGHJKLMNPQRSTUVWXY3456789";
		if (codeLength < 3)
		{
			codeLength = 3;
		}
		char[] array = new char[codeLength];
		Random random = new Random();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = text[random.Next(text.Length)];
		}
		return new string(array);
	}

	public static int GetBitsRequired(int count)
	{
		if (count <= 0)
		{
			return 1;
		}
		return (int)Math.Log(count, 2.0) + 1;
	}

	public static byte[] CompressData(byte[] data)
	{
		if (data == null)
		{
			data = Array.Empty<byte>();
		}
		if (data.Length == 0)
		{
			return data;
		}
		return SevenZipHelper.Compress(data);
	}

	public static byte[] DecompressData(byte[] data)
	{
		if (data.Length == 0)
		{
			return data;
		}
		return SevenZipHelper.Decompress(data);
	}

	public static byte[] ObjectToByteArray(object obj)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		using MemoryStream memoryStream = new MemoryStream();
		binaryFormatter.Serialize(memoryStream, obj);
		return memoryStream.ToArray();
	}

	public static object ByteArrayToObject(byte[] byteArray)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		memoryStream.Write(byteArray, 0, byteArray.Length);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return binaryFormatter.Deserialize(memoryStream);
	}
}
