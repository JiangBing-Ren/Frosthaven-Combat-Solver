using System.Collections.Generic;
using FFSNet;
using UdpKit;

public sealed class IndexToken : IProtocolToken
{
	private int indexesCount;

	public int[] Indexes { get; set; }

	public IndexToken(List<int> indexes = null)
	{
		indexesCount = indexes?.Count ?? 0;
		Indexes = new int[indexesCount];
		for (int i = 0; i < indexesCount; i++)
		{
			Indexes[i] = indexes[i];
		}
	}

	public IndexToken()
	{
		indexesCount = 0;
		Indexes = new int[indexesCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(indexesCount);
		for (int i = 0; i < indexesCount; i++)
		{
			packet.WriteInt(Indexes[i]);
		}
	}

	public void Read(UdpPacket packet)
	{
		indexesCount = packet.ReadInt();
		Indexes = new int[indexesCount];
		for (int i = 0; i < indexesCount; i++)
		{
			Indexes[i] = packet.ReadInt();
		}
	}
}
