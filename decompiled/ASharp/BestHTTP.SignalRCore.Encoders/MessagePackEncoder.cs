using System;

namespace BestHTTP.SignalRCore.Encoders;

public sealed class MessagePackEncoder : IEncoder
{
	public string Name => "messagepack";

	public object ConvertTo(Type toType, object obj)
	{
		throw new NotImplementedException();
	}

	public T DecodeAs<T>(string text)
	{
		throw new NotImplementedException();
	}

	public T DecodeAs<T>(byte[] data)
	{
		throw new NotImplementedException();
	}

	public byte[] EncodeAsBinary<T>(T value)
	{
		throw new NotImplementedException();
	}

	public string EncodeAsText<T>(T value)
	{
		throw new NotImplementedException();
	}
}
