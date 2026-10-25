using System;
using System.Collections.Generic;
using FFSNet;
using UdpKit;

public sealed class IdListToken : StatePropertyToken
{
	private int idsCount;

	public string[] IDs { get; set; }

	public IdListToken(List<string> ids)
	{
		if (ids.IsNullOrEmpty())
		{
			IDs = Array.Empty<string>();
			return;
		}
		IDs = new string[ids.Count];
		idsCount = ids.Count;
		for (int i = 0; i < IDs.Length; i++)
		{
			IDs[i] = ids[i];
		}
	}

	public IdListToken()
	{
		idsCount = 0;
		IDs = new string[idsCount];
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(idsCount);
		for (int i = 0; i < idsCount; i++)
		{
			packet.WriteString(IDs[i]);
		}
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		idsCount = packet.ReadInt();
		IDs = new string[idsCount];
		for (int i = 0; i < idsCount; i++)
		{
			IDs[i] = packet.ReadString();
		}
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + "IDs: " + IDs.ToStringPretty());
	}
}
