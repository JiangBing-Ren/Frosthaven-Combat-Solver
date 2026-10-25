using System;
using System.Collections.Generic;
using UdpKit;

namespace FFSNet;

public static class ProtocolTokenUtils
{
	private static Dictionary<string, Type> _protocolTokenTypeCache = new Dictionary<string, Type>();

	public static byte[] ToRawByteArray(this IProtocolToken token)
	{
		using UdpPacket udpPacket = new UdpPacket();
		token.Write(udpPacket);
		return udpPacket.GetData();
	}

	public static void FromRawByteArray(this IProtocolToken token, byte[] data)
	{
		if (data == null)
		{
			return;
		}
		using UdpPacket packet = new UdpPacket(data);
		token.Read(packet);
	}

	public static IProtocolToken ReadProtoclToken(byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			return null;
		}
		using UdpPacket packet = new UdpPacket(data);
		return packet.ReadProtocolToken();
	}

	public static IProtocolToken ReadProtocolToken(this UdpPacket packet)
	{
		string text = packet.ReadString();
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (!_protocolTokenTypeCache.TryGetValue(text, out var value))
		{
			value = Type.GetType(text);
			if (value == null || !typeof(IProtocolToken).IsAssignableFrom(value))
			{
				throw new Exception("Invalid IProtocolToken type: '" + text + "'");
			}
			_protocolTokenTypeCache[text] = value;
		}
		IProtocolToken obj = Activator.CreateInstance(value) as IProtocolToken;
		obj.Read(packet);
		return obj;
	}

	public static void WriteProtocolToken(this UdpPacket packet, IProtocolToken token)
	{
		if (token == null)
		{
			packet.WriteString(string.Empty);
			return;
		}
		packet.WriteString(token?.GetType().AssemblyQualifiedName);
		token.Write(packet);
	}

	public static byte[] ToByteArrayWithType(this IProtocolToken token)
	{
		using UdpPacket udpPacket = new UdpPacket();
		udpPacket.WriteString(token?.GetType().AssemblyQualifiedName);
		token.Write(udpPacket);
		return udpPacket.GetData();
	}
}
