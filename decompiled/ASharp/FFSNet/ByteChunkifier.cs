using System;
using UnityEngine;

namespace FFSNet;

public class ByteChunkifier
{
	private int byteIndex;

	private int bytesLeft;

	public byte[] Data { get; private set; }

	public int ChunkSize { get; private set; }

	public int DataSize => Data.Length;

	public ByteChunkifier(byte[] data, int chunkSize)
	{
		Data = data;
		ChunkSize = chunkSize;
		byteIndex = 0;
		bytesLeft = DataSize;
	}

	public ByteChunkifier(int totalSize, int chunkSize)
	{
		Data = new byte[totalSize];
		ChunkSize = chunkSize;
	}

	public bool ReadChunk(out byte[] buffer)
	{
		buffer = null;
		if (!HasBytesLeft())
		{
			return false;
		}
		int num = Mathf.Min(bytesLeft, ChunkSize);
		buffer = new byte[num];
		Array.Copy(Data, byteIndex, buffer, 0, num);
		byteIndex += num;
		bytesLeft -= num;
		return true;
	}

	public bool HasBytesLeft()
	{
		return bytesLeft > 0;
	}

	public void WriteChunk(byte[] buffer, int index)
	{
		int destinationIndex = index * ChunkSize;
		Array.Copy(buffer, 0, Data, destinationIndex, buffer.Length);
	}
}
