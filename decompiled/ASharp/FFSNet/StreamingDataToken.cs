using System;
using System.Collections.Generic;
using UdpKit;

namespace FFSNet;

public class StreamingDataToken : IProtocolToken
{
	public int Offset { get; private set; }

	public int Total { get; private set; }

	public byte[] Array { get; private set; }

	public bool IsStart => Offset == 0;

	public bool IsEnd => Offset + Array.Length == Total;

	public StreamingDataToken()
	{
	}

	public StreamingDataToken(byte[] data)
	{
		Array = data;
		Offset = 0;
		Total = Array.Length;
	}

	public void Read(UdpPacket packet)
	{
		Offset = packet.ReadInt();
		Total = packet.ReadInt();
		Array = packet.ReadByteArrayWithPrefix();
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(Offset);
		packet.WriteInt(Total);
		packet.WriteByteArrayWithPrefix(Array);
	}

	public static IEnumerable<StreamingDataToken> SplitByteArrayIntoTokens(byte[] data)
	{
		int size = 900;
		if (data.Length == 0)
		{
			yield return new StreamingDataToken
			{
				Array = new byte[0]
			};
			yield break;
		}
		for (int i = 0; i < data.Length; i += size)
		{
			StreamingDataToken streamingDataToken = new StreamingDataToken
			{
				Offset = i,
				Total = data.Length
			};
			int num = Math.Min(size, data.Length - i);
			streamingDataToken.Array = new byte[num];
			Buffer.BlockCopy(data, streamingDataToken.Offset, streamingDataToken.Array, 0, streamingDataToken.Array.Length);
			yield return streamingDataToken;
		}
	}
}
