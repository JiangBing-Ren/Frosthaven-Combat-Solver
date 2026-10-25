using System;
using System.Runtime.InteropServices;

namespace ENet;

public class Host : IDisposable
{
	private IntPtr nativeHost;

	internal IntPtr NativeData
	{
		get
		{
			return nativeHost;
		}
		set
		{
			nativeHost = value;
		}
	}

	public bool IsSet => nativeHost != IntPtr.Zero;

	public uint PeersCount
	{
		get
		{
			ThrowIfNotCreated();
			return Native.enet_host_get_peers_count(nativeHost);
		}
	}

	public uint PacketsSent
	{
		get
		{
			ThrowIfNotCreated();
			return Native.enet_host_get_packets_sent(nativeHost);
		}
	}

	public uint PacketsReceived
	{
		get
		{
			ThrowIfNotCreated();
			return Native.enet_host_get_packets_received(nativeHost);
		}
	}

	public uint BytesSent
	{
		get
		{
			ThrowIfNotCreated();
			return Native.enet_host_get_bytes_sent(nativeHost);
		}
	}

	public uint BytesReceived
	{
		get
		{
			ThrowIfNotCreated();
			return Native.enet_host_get_bytes_received(nativeHost);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (nativeHost != IntPtr.Zero)
		{
			Native.enet_host_destroy(nativeHost);
			nativeHost = IntPtr.Zero;
		}
	}

	~Host()
	{
		Dispose(disposing: false);
	}

	internal void ThrowIfNotCreated()
	{
		if (nativeHost == IntPtr.Zero)
		{
			throw new InvalidOperationException("Host not created");
		}
	}

	private static void ThrowIfChannelsExceeded(int channelLimit)
	{
		if (channelLimit < 0 || (long)channelLimit > 255L)
		{
			throw new ArgumentOutOfRangeException("channelLimit");
		}
	}

	public void Create()
	{
		Create(null, 1, 0);
	}

	public void Create(int bufferSize)
	{
		Create(null, 1, 0, 0u, 0u, bufferSize);
	}

	public void Create(Address? address, int peerLimit)
	{
		Create(address, peerLimit, 0);
	}

	public void Create(Address? address, int peerLimit, int channelLimit)
	{
		Create(address, peerLimit, channelLimit, 0u, 0u, 0);
	}

	public void Create(int peerLimit, int channelLimit)
	{
		Create(null, peerLimit, channelLimit, 0u, 0u, 0);
	}

	public void Create(int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
	{
		Create(null, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth, 0);
	}

	public void Create(Address? address, int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
	{
		Create(address, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth, 0);
	}

	public void Create(Address? address, int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth, int bufferSize)
	{
		if (nativeHost != IntPtr.Zero)
		{
			throw new InvalidOperationException("Host already created");
		}
		if (peerLimit < 0 || (long)peerLimit > 4095L)
		{
			throw new ArgumentOutOfRangeException("peerLimit");
		}
		ThrowIfChannelsExceeded(channelLimit);
		if (address.HasValue)
		{
			ENetAddress address2 = address.Value.NativeData;
			nativeHost = Native.enet_host_create(ref address2, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth, bufferSize);
		}
		else
		{
			nativeHost = Native.enet_host_create(IntPtr.Zero, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth, bufferSize);
		}
		if (nativeHost == IntPtr.Zero)
		{
			throw new InvalidOperationException("Host creation call failed");
		}
	}

	public void PreventConnections(bool state)
	{
		ThrowIfNotCreated();
		Native.enet_host_prevent_connections(nativeHost, (byte)(state ? 1u : 0u));
	}

	public void Broadcast(byte channelID, ref Packet packet)
	{
		ThrowIfNotCreated();
		packet.ThrowIfNotCreated();
		Native.enet_host_broadcast(nativeHost, channelID, packet.NativeData);
		packet.NativeData = IntPtr.Zero;
	}

	public void Broadcast(byte channelID, ref Packet packet, Peer excludedPeer)
	{
		ThrowIfNotCreated();
		packet.ThrowIfNotCreated();
		Native.enet_host_broadcast_exclude(nativeHost, channelID, packet.NativeData, excludedPeer.NativeData);
		packet.NativeData = IntPtr.Zero;
	}

	public void Broadcast(byte channelID, ref Packet packet, Peer[] peers)
	{
		if (peers == null)
		{
			throw new ArgumentNullException("peers");
		}
		ThrowIfNotCreated();
		packet.ThrowIfNotCreated();
		if (peers.Length != 0)
		{
			IntPtr[] pointerBuffer = ArrayPool.GetPointerBuffer();
			int num = 0;
			for (int i = 0; i < peers.Length; i++)
			{
				if (peers[i].NativeData != IntPtr.Zero)
				{
					pointerBuffer[num] = peers[i].NativeData;
					num++;
				}
			}
			Native.enet_host_broadcast_selective(nativeHost, channelID, packet.NativeData, pointerBuffer, (IntPtr)num);
			packet.NativeData = IntPtr.Zero;
			return;
		}
		packet.Dispose();
		throw new ArgumentOutOfRangeException("Peers array can't be empty");
	}

	public int CheckEvents(out Event @event)
	{
		ThrowIfNotCreated();
		int num = Native.enet_host_check_events(nativeHost, out var event2);
		if (num <= 0)
		{
			@event = default;
			return num;
		}
		@event = new Event(event2);
		return num;
	}

	public Peer Connect(Address address)
	{
		return Connect(address, 0, 0u);
	}

	public Peer Connect(Address address, int channelLimit)
	{
		return Connect(address, channelLimit, 0u);
	}

	public Peer Connect(Address address, int channelLimit, uint data)
	{
		ThrowIfNotCreated();
		ThrowIfChannelsExceeded(channelLimit);
		ENetAddress address2 = address.NativeData;
		Peer result = new Peer(Native.enet_host_connect(nativeHost, ref address2, (IntPtr)channelLimit, data));
		if (result.NativeData == IntPtr.Zero)
		{
			throw new InvalidOperationException("Host connect call failed");
		}
		return result;
	}

	public int Service(int timeout, out Event @event)
	{
		if (timeout < 0)
		{
			throw new ArgumentOutOfRangeException("timeout");
		}
		ThrowIfNotCreated();
		int num = Native.enet_host_service(nativeHost, out var event2, (uint)timeout);
		if (num <= 0)
		{
			@event = default;
			return num;
		}
		@event = new Event(event2);
		return num;
	}

	public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth)
	{
		ThrowIfNotCreated();
		Native.enet_host_bandwidth_limit(nativeHost, incomingBandwidth, outgoingBandwidth);
	}

	public void SetChannelLimit(int channelLimit)
	{
		ThrowIfNotCreated();
		ThrowIfChannelsExceeded(channelLimit);
		Native.enet_host_channel_limit(nativeHost, (IntPtr)channelLimit);
	}

	public void SetMaxDuplicatePeers(ushort number)
	{
		ThrowIfNotCreated();
		Native.enet_host_set_max_duplicate_peers(nativeHost, number);
	}

	public void SetInterceptCallback(IntPtr callback)
	{
		ThrowIfNotCreated();
		Native.enet_host_set_intercept_callback(nativeHost, callback);
	}

	public void SetInterceptCallback(InterceptCallback callback)
	{
		ThrowIfNotCreated();
		Native.enet_host_set_intercept_callback(nativeHost, Marshal.GetFunctionPointerForDelegate(callback));
	}

	public void SetChecksumCallback(IntPtr callback)
	{
		ThrowIfNotCreated();
		Native.enet_host_set_checksum_callback(nativeHost, callback);
	}

	public void SetChecksumCallback(ChecksumCallback callback)
	{
		ThrowIfNotCreated();
		Native.enet_host_set_checksum_callback(nativeHost, Marshal.GetFunctionPointerForDelegate(callback));
	}

	public void Flush()
	{
		ThrowIfNotCreated();
		Native.enet_host_flush(nativeHost);
	}
}
