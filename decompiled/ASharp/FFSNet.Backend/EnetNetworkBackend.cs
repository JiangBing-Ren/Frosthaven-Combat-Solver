using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ENet;
using UnityEngine;

namespace FFSNet.Backend;

public class EnetNetworkBackend : INetworkBackend, IDisposable
{
	private UserToken _currentPlayerUserToken;

	private List<NetworkBackendPlayer> _players = new List<NetworkBackendPlayer>();

	private ConcurrentBag<Peer> _peers = new ConcurrentBag<Peer>();

	private Host _enetInstance;

	private Peer _enetPeer;

	private readonly NetworkManager _networkManager;

	private static bool _initialzied;

	public string SessionID { get; private set; }

	public bool IsRunning { get; private set; }

	public bool IsHost { get; private set; }

	public bool IsClient { get; private set; }

	public NetworkBackendPlayer CurrentPlayer { get; }

	public Action<NetworkBackendConnectRequest> ConnectRequestedHandler { get; set; }

	public ConcurrentQueue<NetworkBackendEvent> NetworkEvents { get; } = new ConcurrentQueue<NetworkBackendEvent>();

	public IEnumerable<NetworkBackendPlayer> Players => _players;

	public EnetNetworkBackend(NetworkManager nm, UserToken userToken)
	{
		if (!_initialzied)
		{
			_initialzied = Library.Initialize();
			if (!_initialzied)
			{
				throw new Exception("Failed to initialize ENet");
			}
		}
		_currentPlayerUserToken = userToken;
		CurrentPlayer = new NetworkBackendPlayer(userToken)
		{
			PlayerID = 1
		};
		_networkManager = nm;
	}

	public Task<NetworkBackendResult> HostAsync()
	{
		_enetInstance = new Host();
		Address value = new Address
		{
			Port = 30000
		};
		_enetInstance.Create(value, _networkManager.MaxPlayers);
		CurrentPlayer.PlayerID = 30000;
		SessionID = "localhost";
		IsHost = true;
		IsRunning = true;
		PlatformLayer.Instance.StartCoroutine(PollEventsCrt());
		return Task.FromResult<NetworkBackendResult>(default);
	}

	public Task<NetworkBackendResult> JoinAsync(string sessionID, UserToken user)
	{
		_enetInstance = new Host();
		Address address = default;
		address.Port = 30000;
		address.SetHost(sessionID);
		_enetInstance.Create();
		_enetPeer = _enetInstance.Connect(address);
		if (Application.isEditor)
		{
			_enetPeer.Timeout(1000u, 1800000u, 3600000u);
		}
		SessionID = sessionID;
		IsClient = true;
		IsRunning = true;
		CurrentPlayer.PlayerID = (int)_enetPeer.ID;
		PlatformLayer.Instance.StartCoroutine(PollEventsCrt());
		return Task.FromResult<NetworkBackendResult>(default);
	}

	public Task ShutdownAsync()
	{
		End();
		return Task.CompletedTask;
	}

	public void SendProtocolTokenToClient(NetworkBackendPlayer client, IProtocolToken token, NetworkDataStreamChannel channel)
	{
		if (IsHost)
		{
			Packet packet = default;
			byte[] data = token.ToByteArrayWithType();
			packet.Create(data, PacketFlags.Reliable);
			_peers.First((Peer p) => p.ID == client.PlayerID).Send((byte)channel, ref packet);
		}
		else
		{
			ClientToClientToken token2 = new ClientToClientToken
			{
				Target = client.PlayerID,
				Sender = CurrentPlayer.PlayerID,
				Token = token
			};
			SendProtolTokenToServer(token2, channel);
		}
	}

	public void SendProtocolTokenToAll(IProtocolToken token, NetworkDataStreamChannel channel, NetworkBackendPlayer excludePlayer = null)
	{
		if (IsHost)
		{
			Packet packet = default;
			byte[] data = token.ToByteArrayWithType();
			packet.Create(data, PacketFlags.Reliable);
			if (excludePlayer != null)
			{
				Peer excludedPeer = _peers.FirstOrDefault((Peer p) => p.ID == excludePlayer.PlayerID);
				if (excludedPeer.IsSet)
				{
					_enetInstance.Broadcast((byte)channel, ref packet, excludedPeer);
					return;
				}
			}
			Log.NetworkTrace($"[LAN] Sending {token} from host {CurrentPlayer.EosUserId} to all clients");
			_enetInstance.Broadcast((byte)channel, ref packet);
		}
		else
		{
			ClientToClientToken token2 = new ClientToClientToken
			{
				Target = -1,
				Sender = CurrentPlayer.PlayerID,
				Token = token
			};
			SendProtolTokenToServer(token2, channel);
		}
	}

	public void SendProtolTokenToServer(IProtocolToken token, NetworkDataStreamChannel channel)
	{
		Packet packet = default;
		byte[] data = token.ToByteArrayWithType();
		packet.Create(data, PacketFlags.Reliable);
		_enetPeer.Send((byte)channel, ref packet);
	}

	public void DisconnectClient(NetworkBackendPlayer client)
	{
		_peers.First((Peer p) => p.ID == client.PlayerID).Disconnect(0u);
		_enetInstance.Flush();
	}

	private IEnumerator PollEventsCrt()
	{
		while (IsRunning)
		{
			bool flag = false;
			ENet.Event netEvent;
			while (!flag)
			{
				if (_enetInstance.CheckEvents(out netEvent) <= 0)
				{
					if (_enetInstance.Service(0, out netEvent) <= 0)
					{
						break;
					}
					flag = true;
				}
				switch (netEvent.Type)
				{
				case ENet.EventType.Connect:
					if (Application.isEditor)
					{
						netEvent.Peer.Timeout(1000u, 300000u, 3600000u);
					}
					if (IsClient)
					{
						SendProtolTokenToServer(_currentPlayerUserToken, NetworkDataStreamChannel.Main);
						_players.Add(new NetworkBackendPlayer("host", netEvent.Peer.ID.ToString())
						{
							PlayerID = (int)netEvent.Peer.ID
						});
					}
					break;
				case ENet.EventType.Disconnect:
				case ENet.EventType.Timeout:
					if (IsHost)
					{
						NetworkBackendPlayer networkBackendPlayer = _players.FirstOrDefault((NetworkBackendPlayer p) => p.PlayerID == netEvent.Peer.ID);
						if (networkBackendPlayer != null)
						{
							_players.Remove(networkBackendPlayer);
							NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.PlayerLeft, networkBackendPlayer));
						}
					}
					else
					{
						NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.DisconnectedFromServer));
					}
					break;
				case ENet.EventType.Receive:
					DataRecieved(netEvent);
					netEvent.Packet.Dispose();
					break;
				}
			}
			_enetInstance.Flush();
			yield return null;
		}
	}

	private void DataRecieved(ENet.Event netEvent)
	{
		byte[] array = new byte[netEvent.Packet.Length];
		netEvent.Packet.CopyTo(array);
		IProtocolToken protocolToken = ProtocolTokenUtils.ReadProtoclToken(array);
		if (protocolToken is UserToken userToken)
		{
			NetworkBackendConnectRequest networkBackendConnectRequest = new NetworkBackendConnectRequest(userToken);
			ConnectRequestedHandler?.Invoke(networkBackendConnectRequest);
			if (networkBackendConnectRequest.Accepted)
			{
				NetworkBackendPlayer networkBackendPlayer = new NetworkBackendPlayer(userToken)
				{
					PlayerID = (int)netEvent.Peer.ID
				};
				NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ConnectionAccepted));
				NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.PlayerJoined, networkBackendPlayer));
				_peers.Add(netEvent.Peer);
				_players.Add(networkBackendPlayer);
			}
			else
			{
				netEvent.Peer.Disconnect((uint)networkBackendConnectRequest.ErrorToken.ErrorCode);
			}
			return;
		}
		ClientToClientToken cct = protocolToken as ClientToClientToken;
		if (cct != null)
		{
			IProtocolToken token = cct.Token;
			if (IsHost)
			{
				NetworkBackendPlayer networkBackendPlayer2 = _players.First((NetworkBackendPlayer pl) => pl.PlayerID == cct.Sender);
				NetworkBackendPlayer networkBackendPlayer3 = _players.FirstOrDefault((NetworkBackendPlayer pl) => pl.PlayerID == cct.Target);
				if (networkBackendPlayer3 == null)
				{
					SendProtocolTokenToAll(cct, NetworkDataStreamChannel.Main, networkBackendPlayer2);
					NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ProtocolTokenRecieved, networkBackendPlayer2, token));
				}
				else if (networkBackendPlayer3 == CurrentPlayer)
				{
					NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ProtocolTokenRecieved, networkBackendPlayer2, token));
				}
				else
				{
					SendProtocolTokenToClient(networkBackendPlayer3, cct, NetworkDataStreamChannel.Main);
				}
				return;
			}
			NetworkBackendPlayer networkBackendPlayer4 = _players.FirstOrDefault((NetworkBackendPlayer pl) => pl.PlayerID == cct.Sender);
			if (networkBackendPlayer4 == null)
			{
				Log.NetworkWarning($"Unable to find sender with ID {cct.Sender} in EnetNetworkBackend");
				networkBackendPlayer4 = _players.First((NetworkBackendPlayer pl) => pl.PlayerID == (int)netEvent.Peer.ID);
			}
			NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ProtocolTokenRecieved, networkBackendPlayer4, token));
		}
		else
		{
			NetworkBackendPlayer data = _players.First((NetworkBackendPlayer pl) => pl.PlayerID == (int)netEvent.Peer.ID);
			NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ProtocolTokenRecieved, data, protocolToken));
		}
	}

	private void ProcessDataToken(IProtocolToken token)
	{
	}

	public void Dispose()
	{
		End();
		Library.Deinitialize();
	}

	private void End()
	{
		if (_enetPeer.IsSet)
		{
			_enetPeer.Disconnect(0u);
		}
		foreach (Peer item in _peers.ToList())
		{
			item.Disconnect(0u);
		}
		_peers.Clear();
		_enetInstance?.Flush();
		_enetInstance?.Dispose();
		_enetInstance = null;
		_players.Clear();
		IsRunning = false;
		CurrentPlayer.PlayerID = 1;
	}
}
