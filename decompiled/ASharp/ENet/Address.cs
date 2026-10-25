using System;
using System.Text;

namespace ENet;

public struct Address
{
	private ENetAddress nativeAddress;

	internal ENetAddress NativeData
	{
		get
		{
			return nativeAddress;
		}
		set
		{
			nativeAddress = value;
		}
	}

	public ushort Port
	{
		get
		{
			return nativeAddress.port;
		}
		set
		{
			nativeAddress.port = value;
		}
	}

	internal Address(ENetAddress address)
	{
		nativeAddress = address;
	}

	public string GetIP()
	{
		StringBuilder stringBuilder = new StringBuilder(1025);
		if (Native.enet_address_get_ip(ref nativeAddress, stringBuilder, (IntPtr)stringBuilder.Capacity) != 0)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	public bool SetIP(string ip)
	{
		if (ip == null)
		{
			throw new ArgumentNullException("ip");
		}
		return Native.enet_address_set_ip(ref nativeAddress, ip) == 0;
	}

	public string GetHost()
	{
		StringBuilder stringBuilder = new StringBuilder(1025);
		if (Native.enet_address_get_hostname(ref nativeAddress, stringBuilder, (IntPtr)stringBuilder.Capacity) != 0)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	public bool SetHost(string hostName)
	{
		if (hostName == null)
		{
			throw new ArgumentNullException("hostName");
		}
		return Native.enet_address_set_hostname(ref nativeAddress, hostName) == 0;
	}
}
