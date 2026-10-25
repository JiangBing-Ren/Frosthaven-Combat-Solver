using BestHTTP.SignalR.Hubs;

namespace BestHTTP.SignalR.Messages;

public struct ClientMessage(Hub hub, string method, object[] args, ulong callIdx, OnMethodResultDelegate resultCallback, OnMethodFailedDelegate resultErrorCallback, OnMethodProgressDelegate progressCallback)
{
	public readonly Hub Hub = hub;

	public readonly string Method = method;

	public readonly object[] Args = args;

	public readonly ulong CallIdx = callIdx;

	public readonly OnMethodResultDelegate ResultCallback = resultCallback;

	public readonly OnMethodFailedDelegate ResultErrorCallback = resultErrorCallback;

	public readonly OnMethodProgressDelegate ProgressCallback = progressCallback;
}
