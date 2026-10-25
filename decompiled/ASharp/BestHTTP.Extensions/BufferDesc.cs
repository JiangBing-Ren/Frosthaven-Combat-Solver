using System;

namespace BestHTTP.Extensions;

internal struct BufferDesc(byte[] buff)
{
	public static readonly BufferDesc Empty = new BufferDesc(null);

	public byte[] buffer = buff;

	public DateTime released = DateTime.UtcNow;
}
