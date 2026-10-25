using System;
using BestHTTP.Extensions;

namespace BestHTTP.WebSocket.Frames;

public struct RawFrameData(byte[] data, int length) : IDisposable
{
	public byte[] Data = data;

	public int Length = length;

	public void Dispose()
	{
		VariableSizedBufferPool.Release(Data);
		Data = null;
	}
}
