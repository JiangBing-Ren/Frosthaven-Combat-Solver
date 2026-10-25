using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FFSNet.Backend;

public interface INetworkBackend : IDisposable
{
	string SessionID { get; }

	bool IsRunning { get; }

	bool IsHost { get; }

	bool IsClient { get; }

	NetworkBackendPlayer CurrentPlayer { get; }

	Action<NetworkBackendConnectRequest> ConnectRequestedHandler { get; set; }

	ConcurrentQueue<NetworkBackendEvent> NetworkEvents { get; }

	IEnumerable<NetworkBackendPlayer> Players { get; }

	Task<NetworkBackendResult> HostAsync();

	Task<NetworkBackendResult> JoinAsync(string sessionID, UserToken user);

	Task ShutdownAsync();

	void SendProtocolTokenToClient(NetworkBackendPlayer client, IProtocolToken token, NetworkDataStreamChannel channel);

	void SendProtocolTokenToAll(IProtocolToken token, NetworkDataStreamChannel channel, NetworkBackendPlayer excludePlayer = null);

	void SendProtolTokenToServer(IProtocolToken token, NetworkDataStreamChannel channel);

	void DisconnectClient(NetworkBackendPlayer client);
}
