using System;

namespace ENet;

[Flags]
public enum PacketFlags
{
	None = 0,
	Reliable = 1,
	Unsequenced = 2,
	NoAllocate = 4,
	UnreliableFragmented = 8,
	Instant = 0x10,
	Unthrottled = 0x20,
	Sent = 0x100
}
