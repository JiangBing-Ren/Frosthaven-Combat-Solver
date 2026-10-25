using System;
using BestHTTP.ServerSentEvents;
using BestHTTP.SignalR.Messages;

namespace BestHTTP.SignalR.Transports;

public sealed class ServerSentEventsTransport : PostSendTransportBase
{
	private EventSource EventSource;

	public override bool SupportsKeepAlive => true;

	public override TransportTypes Type => TransportTypes.ServerSentEvents;

	public ServerSentEventsTransport(Connection con)
		: base("serverSentEvents", con)
	{
	}

	public override void Connect()
	{
		if (EventSource != null)
		{
			HTTPManager.Logger.Warning("ServerSentEventsTransport", "Start - EventSource already created!");
			return;
		}
		if (State != TransportStates.Reconnecting)
		{
			State = TransportStates.Connecting;
		}
		RequestTypes type = ((State != TransportStates.Reconnecting) ? RequestTypes.Connect : RequestTypes.Reconnect);
		Uri uri = Connection.BuildUri(type, this);
		EventSource = new EventSource(uri);
		EventSource.OnOpen += OnEventSourceOpen;
		EventSource.OnMessage += OnEventSourceMessage;
		EventSource.OnError += OnEventSourceError;
		EventSource.OnClosed += OnEventSourceClosed;
		EventSource.OnRetry += (EventSource es) => false;
		EventSource.Open();
	}

	public override void Stop()
	{
		EventSource.OnOpen -= OnEventSourceOpen;
		EventSource.OnMessage -= OnEventSourceMessage;
		EventSource.OnError -= OnEventSourceError;
		EventSource.OnClosed -= OnEventSourceClosed;
		EventSource.Close();
		EventSource = null;
	}

	protected override void Started()
	{
	}

	public override void Abort()
	{
		base.Abort();
		EventSource.Close();
	}

	protected override void Aborted()
	{
		if (State == TransportStates.Closing)
		{
			State = TransportStates.Closed;
		}
	}

	private void OnEventSourceOpen(EventSource eventSource)
	{
		HTTPManager.Logger.Information("Transport - " + Name, "OnEventSourceOpen");
	}

	private void OnEventSourceMessage(EventSource eventSource, Message message)
	{
		if (message.Data.Equals("initialized"))
		{
			OnConnected();
			return;
		}
		IServerMessage serverMessage = TransportBase.Parse(Connection.JsonEncoder, message.Data);
		if (serverMessage != null)
		{
			Connection.OnMessage(serverMessage);
		}
	}

	private void OnEventSourceError(EventSource eventSource, string error)
	{
		HTTPManager.Logger.Information("Transport - " + Name, "OnEventSourceError");
		if (State == TransportStates.Reconnecting)
		{
			Connect();
		}
		else if (State != TransportStates.Closed)
		{
			if (State == TransportStates.Closing)
			{
				State = TransportStates.Closed;
			}
			else
			{
				Connection.Error(error);
			}
		}
	}

	private void OnEventSourceClosed(EventSource eventSource)
	{
		HTTPManager.Logger.Information("Transport - " + Name, "OnEventSourceClosed");
		OnEventSourceError(eventSource, "EventSource Closed!");
	}
}
