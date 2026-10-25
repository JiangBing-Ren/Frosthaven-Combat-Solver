using System;

namespace ENet;

public static class Library
{
	public const uint maxChannelCount = 255u;

	public const uint maxPeers = 4095u;

	public const uint maxPacketSize = 33554432u;

	public const uint throttleThreshold = 40u;

	public const uint throttleScale = 32u;

	public const uint throttleAcceleration = 2u;

	public const uint throttleDeceleration = 2u;

	public const uint throttleInterval = 5000u;

	public const uint timeoutLimit = 32u;

	public const uint timeoutMinimum = 5000u;

	public const uint timeoutMaximum = 30000u;

	public const uint version = 132104u;

	public static uint Time => Native.enet_time_get();

	public static bool Initialize()
	{
		if (Native.enet_linked_version() != 132104)
		{
			throw new InvalidOperationException("Incompatatible version");
		}
		return Native.enet_initialize() == 0;
	}

	public static bool Initialize(Callbacks callbacks)
	{
		if (callbacks == null)
		{
			throw new ArgumentNullException("callbacks");
		}
		if (Native.enet_linked_version() != 132104)
		{
			throw new InvalidOperationException("Incompatatible version");
		}
		ENetCallbacks inits = callbacks.NativeData;
		return Native.enet_initialize_with_callbacks(132104u, ref inits) == 0;
	}

	public static void Deinitialize()
	{
		Native.enet_deinitialize();
	}

	public static ulong CRC64(IntPtr buffers, int bufferCount)
	{
		return Native.enet_crc64(buffers, bufferCount);
	}
}
