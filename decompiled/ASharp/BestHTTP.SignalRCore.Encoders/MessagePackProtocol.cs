using System;
using System.Collections.Generic;
using BestHTTP.SignalRCore.Messages;

namespace BestHTTP.SignalRCore.Encoders;

public sealed class MessagePackProtocol : IProtocol
{
	public TransferModes Type => TransferModes.Binary;

	public IEncoder Encoder { get; private set; }

	public HubConnection Connection { get; set; }

	public MessagePackProtocol()
	{
		Encoder = new MessagePackEncoder();
	}

	public object ConvertTo(Type toType, object obj)
	{
		throw new NotImplementedException();
	}

	public byte[] EncodeMessage(Message message)
	{
		throw new NotImplementedException();
	}

	public object[] GetRealArguments(Type[] argTypes, object[] arguments)
	{
		throw new NotImplementedException();
	}

	public void ParseMessages(string data, ref List<Message> messages)
	{
		throw new NotImplementedException();
	}

	public void ParseMessages(byte[] data, ref List<Message> messages)
	{
		throw new NotImplementedException();
	}
}
