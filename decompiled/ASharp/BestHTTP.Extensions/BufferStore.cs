using System.Collections.Generic;

namespace BestHTTP.Extensions;

internal struct BufferStore(long size)
{
	public readonly long Size = size;

	public List<BufferDesc> buffers = new List<BufferDesc>();

	public BufferStore(long size, byte[] buffer)
		: this(size)
	{
		buffers.Add(new BufferDesc(buffer));
	}
}
