using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Sessions;
using FH.Platforms;
using PlayEveryWare.EpicOnlineServices;
using SETUtil.Extend;
using Steamworks;

namespace FFSNet.Backend;

public class EOSNetworkBackend : INetworkBackend, IDisposable
{
	private class EosAsyncMethodResult
	{
		public bool Done;

		public Epic.OnlineServices.Result Result;
	}

	private const string SteamNetworkIdentity = "epiconlineservices";

	private const string DevAuthToolEndpoint = "localhost:60000";

	private readonly NetworkManager _networkManager;

	private SessionsInterface _sessionsInterface;

	private P2PInterface _p2pInterface;

	private ConnectInterface _connectInterface;

	private ulong _notifyConnectionRequestedHandle;

	private ulong _notifyConnectionEstablishedHandle;

	private ulong _notifyConnectionClosedHandle;

	private ulong _notifyConnectionInterruptedHandle;

	private ulong _notifyAuthExpirationHandle;

	private ulong _notifyIncomingPacketQueueFullHandle;

	private List<NetworkBackendPlayer> _players = new List<NetworkBackendPlayer>();

	private UserToken _userToken;

	private LoginCredentialType credentialType;

	private ExternalCredentialType externalCredentialType;

	private string id = string.Empty;

	private string token = string.Empty;

	private int _lastUsedPlayerId = 1;

	private EOSManager.EOSSingleton EosManager => EOSManager.Instance;

	public string SessionID { get; private set; }

	public bool IsRunning { get; private set; }

	public bool IsHost { get; private set; }

	public bool IsClient { get; private set; }

	public NetworkBackendPlayer CurrentPlayer { get; }

	public Action<NetworkBackendConnectRequest> ConnectRequestedHandler { get; set; }

	public ConcurrentQueue<NetworkBackendEvent> NetworkEvents { get; } = new ConcurrentQueue<NetworkBackendEvent>();

	public IEnumerable<NetworkBackendPlayer> Players => _players;

	public EOSNetworkBackend(NetworkManager nm, UserToken userToken)
	{
		_networkManager = nm;
		_userToken = userToken;
		CurrentPlayer = new NetworkBackendPlayer(userToken)
		{
			PlayerID = 1
		};
	}

	private string PrepareSessionID(string inviationCode)
	{
		StringBuilder stringBuilder = new StringBuilder(inviationCode);
		while (stringBuilder.Length < 16)
		{
			stringBuilder.Append("X");
		}
		return stringBuilder.ToString();
	}

	private static NetworkBackendResult EosResultToNetworkResult(Epic.OnlineServices.Result result, string methodName)
	{
		NetworkBackendResult result2 = new NetworkBackendResult
		{
			ErrorCode = (int)result,
			ErrorMessage = ((result == Epic.OnlineServices.Result.Success) ? "" : $"{methodName}: {result}")
		};
		if (!result2.Ok)
		{
			Log.NetworkError(result.ToString(), $"[EOS] {methodName} failed: '{result}'");
		}
		return result2;
	}

	private static async Task<NetworkBackendResult> EosAsyncMethod(string methodName, Action<EosAsyncMethodResult> method)
	{
		EosAsyncMethodResult eosAsyncMethodResult = new EosAsyncMethodResult();
		method(eosAsyncMethodResult);
		while (!eosAsyncMethodResult.Done)
		{
			await Task.Yield();
		}
		return EosResultToNetworkResult(eosAsyncMethodResult.Result, methodName);
	}

	private async Task<NetworkBackendResult> Authenticate(bool isRefresh = false)
	{
		if (!isRefresh)
		{
			if (PlatformLayer.Instance.NewPlatformLayer.CurrentPlatform is NewPlatformEGS)
			{
				credentialType = LoginCredentialType.Password;
			}
			else if (PlatformLayer.Instance.NewPlatformLayer.CurrentPlatform is NewPlatformSteam)
			{
				credentialType = LoginCredentialType.ExternalAuth;
				externalCredentialType = ExternalCredentialType.SteamSessionTicket;
				id = SteamClient.AppId.ToString();
				AuthTicket authTicket = await SteamUser.GetAuthTicketForWebApiAsync("epiconlineservices");
				if (authTicket == null || authTicket.Handle == 0 || authTicket.Data == null || authTicket.Data.Length == 0)
				{
					return new NetworkBackendResult
					{
						ErrorCode = -1,
						ErrorMessage = "[EOS] Failed to get Steam AuthTicketForWebApi."
					};
				}
				token = BitConverter.ToString(authTicket.Data).Replace("-", string.Empty);
			}
			else
			{
				credentialType = LoginCredentialType.Developer;
				id = "localhost:60000";
				token = _userToken.Username;
			}
		}
		EpicAccountId accountId = null;
		NetworkBackendResult result = default;
		ContinuanceToken continuanceToken = null;
		if (credentialType == LoginCredentialType.Developer)
		{
			Log.NetworkInfo($"[EOS] Developer login with {externalCredentialType} as '{token}' at '{id}'");
			result = await EosAsyncMethod("StartLoginWithLoginTypeAndToken", (EosAsyncMethodResult eosAsyncMethodResult) =>
			{
				EosManager.StartLoginWithLoginTypeAndToken(credentialType, externalCredentialType, id, token, (Epic.OnlineServices.Auth.LoginCallbackInfo data) =>
				{
					accountId = data.SelectedAccountId;
					eosAsyncMethodResult.Result = data.ResultCode;
					eosAsyncMethodResult.Done = true;
				});
			});
			if (!result.Ok)
			{
				return result;
			}
			result = await EosAsyncMethod("StartConnectLoginWithEpicAccount", (EosAsyncMethodResult eosAsyncMethodResult) =>
			{
				EosManager.StartConnectLoginWithEpicAccount(accountId, (Epic.OnlineServices.Connect.LoginCallbackInfo data) =>
				{
					eosAsyncMethodResult.Result = data.ResultCode;
					eosAsyncMethodResult.Done = true;
				});
			});
		}
		else if (credentialType == LoginCredentialType.ExternalAuth)
		{
			Log.NetworkInfo($"[EOS] ExtrernalAuth login with {externalCredentialType}");
			result = await EosAsyncMethod("StartConnectLoginWithOptions", (EosAsyncMethodResult eosAsyncMethodResult) =>
			{
				EosManager.StartConnectLoginWithOptions(externalCredentialType, token, null, null, (Epic.OnlineServices.Connect.LoginCallbackInfo data) =>
				{
					continuanceToken = data.ContinuanceToken;
					eosAsyncMethodResult.Result = data.ResultCode;
					eosAsyncMethodResult.Done = true;
				});
			});
			if (!result.Ok && continuanceToken != null)
			{
				Log.NetworkInfo("[EOS] ExtrernalAuth failed, continuing with CreateConnectUserWithContinuanceToken...");
				result = await EosAsyncMethod("CreateConnectUserWithContinuanceToken", (EosAsyncMethodResult eosAsyncMethodResult) =>
				{
					EosManager.CreateConnectUserWithContinuanceToken(continuanceToken, (CreateUserCallbackInfo data) =>
					{
						eosAsyncMethodResult.Result = data.ResultCode;
						eosAsyncMethodResult.Done = true;
					});
				});
			}
		}
		else
		{
			_ = credentialType;
		}
		if (result.Ok)
		{
			if (isRefresh)
			{
				if (CurrentPlayer.EosUserId != EosManager.GetProductUserId())
				{
					Log.NetworkError("ProductUserIDMismatch", $"[EOS] Login refreshed caused ProductUserID mismach!: CurrentPlayer = '{CurrentPlayer.EosUserId}', EosManager = '{EosManager.GetProductUserId()}'");
					return new NetworkBackendResult
					{
						ErrorCode = -1,
						ErrorMessage = "[EOS] Login refreshed caused ProductUserID mismach!"
					};
				}
			}
			else
			{
				CurrentPlayer.EosUserId = EosManager.GetProductUserId();
				CurrentPlayer.PlayerID = 1;
			}
			Log.NetworkInfo($"[EOS] Logged as ProductUserID: '{CurrentPlayer.EosUserId}'");
		}
		return result;
	}

	private NetworkBackendResult InitEOS()
	{
		if (PlatformLayer.Instance.gameObject.GetOrAdd<EOSManager>() == null)
		{
			return new NetworkBackendResult
			{
				ErrorCode = -1,
				ErrorMessage = "[EOS] Failed to initialize EOS manager!"
			};
		}
		_sessionsInterface = EosManager.GetEOSPlatformInterface().GetSessionsInterface();
		_p2pInterface = EosManager.GetEOSP2PInterface();
		_connectInterface = EosManager.GetEOSConnectInterface();
		if (_sessionsInterface == null)
		{
			return new NetworkBackendResult
			{
				ErrorCode = -1,
				ErrorMessage = "EOS Session interface not found!"
			};
		}
		if (_p2pInterface == null)
		{
			return new NetworkBackendResult
			{
				ErrorCode = -1,
				ErrorMessage = "EOS Peer to Peer interface not found!"
			};
		}
		if (_connectInterface == null)
		{
			return new NetworkBackendResult
			{
				ErrorCode = -1,
				ErrorMessage = "EOS Connect interface not found!"
			};
		}
		return default;
	}

	private void OnIncommingConnectionRequest(ref OnIncomingConnectionRequestInfo data)
	{
		Log.NetworkInfo($"[EOS] Incomming connection from user '{data.RemoteUserId}'");
		ProductUserId peerId = data.RemoteUserId;
		NetworkBackendResult networkBackendResult = default;
		if (!IsHost)
		{
			return;
		}
		if (_players.Any((NetworkBackendPlayer p) => p.EosUserId == peerId))
		{
			DisconnectPeer(peerId);
			Log.NetworkInfo($"[EOS] Connection with user '{peerId}' refused: user already connected");
			return;
		}
		if (_players.Count > _networkManager.MaxPlayers)
		{
			DisconnectPeer(peerId);
			Log.NetworkInfo($"[EOS] Connection with user '{peerId}' refused: max players reached");
			return;
		}
		AcceptConnectionOptions options = new AcceptConnectionOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			RemoteUserId = peerId,
			SocketId = data.SocketId
		};
		if (EosResultToNetworkResult(_p2pInterface.AcceptConnection(ref options), "AcceptConnection").Ok)
		{
			Log.NetworkInfo($"[EOS] Connection with user '{peerId}' accepted");
		}
	}

	private void OnConnectedToPeer(ref OnPeerConnectionEstablishedInfo data)
	{
		Log.NetworkInfo($"[EOS] Connected to peer {data.RemoteUserId} as connection {data.ConnectionType} at network type {data.NetworkType}");
		if (IsClient)
		{
			NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ConnectedToServer));
			_players.Add(new NetworkBackendPlayer("HOST", data.RemoteUserId.ToString()));
			_players[0].PlayerID = 1;
			_players[0].EosUserId = new ProductUserId(data.RemoteUserId.InnerHandle);
			NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.PlayerJoined, _players[0]));
			PlatformLayer.Instance.StartCoroutine(PollPackagesCrt());
			SendProtolTokenToServer(_userToken, NetworkDataStreamChannel.Main);
		}
	}

	private void OnRemoteConnectionClosed(ref OnRemoteConnectionClosedInfo data)
	{
		Log.NetworkInfo($"[EOS] Connection to peer {data.RemoteUserId} closed. Reason: {data.Reason}");
		ProductUserId remoteUserId = data.RemoteUserId;
		if (IsHost)
		{
			NetworkBackendPlayer networkBackendPlayer = _players.FirstOrDefault((NetworkBackendPlayer p) => p.EosUserId == remoteUserId);
			if (networkBackendPlayer != null)
			{
				networkBackendPlayer.Disconnected = true;
				_players.Remove(networkBackendPlayer);
				NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.PlayerLeft, networkBackendPlayer));
				UnregisterPlayer(remoteUserId);
			}
		}
		else
		{
			NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.DisconnectedFromServer));
		}
	}

	private void OnConnectionInterrupted(ref OnPeerConnectionInterruptedInfo data)
	{
		Log.NetworkInfo($"[EOS] Connection to peer {data.RemoteUserId} interuped.");
		ProductUserId remoteUserId = data.RemoteUserId;
		if (IsHost)
		{
			NetworkBackendPlayer networkBackendPlayer = _players.FirstOrDefault((NetworkBackendPlayer p) => p.EosUserId == remoteUserId);
			if (networkBackendPlayer != null)
			{
				networkBackendPlayer.Disconnected = true;
				_players.Remove(networkBackendPlayer);
				NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.PlayerLeft, networkBackendPlayer));
				UnregisterPlayer(remoteUserId);
			}
		}
		else
		{
			NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.DisconnectedFromServer));
		}
	}

	private void OnAuthExpiration(ref AuthExpirationCallbackInfo data)
	{
		Log.NetworkInfo($"[EOS] Auth expiring for user {data.LocalUserId}");
		PlatformLayer.Instance.StartCoroutine(RefreshAuthentication());
	}

	private void OnIncomingPacketQueueFull(ref OnIncomingPacketQueueFullInfo data)
	{
		Log.NetworkError("PacketQueueOverflow", $"[EOS] Packet queue verflow on channel {data.OverflowPacketChannel}! LocalUserID: {data.OverflowPacketLocalUserId}, {data.PacketQueueCurrentSizeBytes}/{data.PacketQueueMaxSizeBytes} queue size with {data.OverflowPacketSizeBytes} overflow");
		new DisconnectionErrorToken(DisconnectionErrorCode.NetworkBackendError);
		NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ShutdownCommand));
	}

	private IEnumerator RefreshAuthentication()
	{
		Task<NetworkBackendResult> task = Authenticate(isRefresh: true);
		yield return new WaitForTask(task);
		if (!task.IsCompletedSuccessfully || !task.Result.Ok)
		{
			new DisconnectionErrorToken(DisconnectionErrorCode.NetworkBackendError);
			NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ShutdownCommand));
		}
	}

	private void WireEosCallbacks()
	{
		AddNotifyAuthExpirationOptions options = default;
		_notifyAuthExpirationHandle = _connectInterface.AddNotifyAuthExpiration(ref options, null, OnAuthExpiration);
		AddNotifyPeerConnectionRequestOptions options2 = new AddNotifyPeerConnectionRequestOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			SocketId = new SocketId
			{
				SocketName = SessionID
			}
		};
		_notifyConnectionRequestedHandle = _p2pInterface.AddNotifyPeerConnectionRequest(ref options2, null, OnIncommingConnectionRequest);
		AddNotifyPeerConnectionEstablishedOptions options3 = new AddNotifyPeerConnectionEstablishedOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			SocketId = new SocketId
			{
				SocketName = SessionID
			}
		};
		_notifyConnectionEstablishedHandle = _p2pInterface.AddNotifyPeerConnectionEstablished(ref options3, null, OnConnectedToPeer);
		AddNotifyPeerConnectionClosedOptions options4 = new AddNotifyPeerConnectionClosedOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			SocketId = new SocketId
			{
				SocketName = SessionID
			}
		};
		_notifyConnectionClosedHandle = _p2pInterface.AddNotifyPeerConnectionClosed(ref options4, null, OnRemoteConnectionClosed);
		AddNotifyPeerConnectionInterruptedOptions options5 = new AddNotifyPeerConnectionInterruptedOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			SocketId = new SocketId
			{
				SocketName = SessionID
			}
		};
		_notifyConnectionInterruptedHandle = _p2pInterface.AddNotifyPeerConnectionInterrupted(ref options5, null, OnConnectionInterrupted);
		AddNotifyIncomingPacketQueueFullOptions options6 = default;
		_notifyIncomingPacketQueueFullHandle = _p2pInterface.AddNotifyIncomingPacketQueueFull(ref options6, null, OnIncomingPacketQueueFull);
	}

	private void UnwireEosCallbacks()
	{
		if (_notifyConnectionEstablishedHandle != 0)
		{
			_p2pInterface.RemoveNotifyPeerConnectionEstablished(_notifyConnectionEstablishedHandle);
			_notifyConnectionEstablishedHandle = 0uL;
		}
		if (_notifyConnectionClosedHandle != 0)
		{
			_p2pInterface.RemoveNotifyPeerConnectionClosed(_notifyConnectionClosedHandle);
			_notifyConnectionClosedHandle = 0uL;
		}
		if (_notifyConnectionRequestedHandle != 0)
		{
			_p2pInterface.RemoveNotifyPeerConnectionRequest(_notifyConnectionRequestedHandle);
			_notifyConnectionRequestedHandle = 0uL;
		}
		if (_notifyConnectionInterruptedHandle != 0)
		{
			_p2pInterface.RemoveNotifyPeerConnectionInterrupted(_notifyConnectionInterruptedHandle);
			_notifyConnectionInterruptedHandle = 0uL;
		}
		if (_notifyIncomingPacketQueueFullHandle != 0)
		{
			_p2pInterface.RemoveNotifyIncomingPacketQueueFull(_notifyIncomingPacketQueueFullHandle);
			_notifyIncomingPacketQueueFullHandle = 0uL;
		}
		if (_notifyAuthExpirationHandle != 0)
		{
			_connectInterface.RemoveNotifyAuthExpiration(_notifyAuthExpirationHandle);
			_notifyAuthExpirationHandle = 0uL;
		}
	}

	private int NewPlayerID()
	{
		return Interlocked.Increment(ref _lastUsedPlayerId);
	}

	public async Task<NetworkBackendResult> HostAsync()
	{
		NetworkBackendResult result = InitEOS();
		if (!result.Ok)
		{
			return result;
		}
		result = await Authenticate();
		if (!result.Ok)
		{
			return result;
		}
		_lastUsedPlayerId = 1;
		string sessionID = Utility.GetRandomCode(_networkManager.inviteCodeLength);
		CreateSessionModificationOptions options = new CreateSessionModificationOptions
		{
			BucketId = "KRONOS",
			MaxPlayers = (uint)_networkManager.MaxPlayers,
			SessionName = sessionID,
			SanctionsEnabled = false,
			LocalUserId = EosManager.GetProductUserId(),
			PresenceEnabled = false,
			SessionId = PrepareSessionID(sessionID)
		};
		result = EosResultToNetworkResult(_sessionsInterface.CreateSessionModification(ref options, out var outSessionModificationHandle), "CreateSessionModification");
		if (!result.Ok)
		{
			return result;
		}
		SessionModificationSetPermissionLevelOptions options2 = new SessionModificationSetPermissionLevelOptions
		{
			PermissionLevel = OnlineSessionPermissionLevel.PublicAdvertised
		};
		result = EosResultToNetworkResult(outSessionModificationHandle.SetPermissionLevel(ref options2), "SetPermissionLevel");
		if (!result.Ok)
		{
			return result;
		}
		SessionModificationSetInvitesAllowedOptions options3 = new SessionModificationSetInvitesAllowedOptions
		{
			InvitesAllowed = false
		};
		result = EosResultToNetworkResult(outSessionModificationHandle.SetInvitesAllowed(ref options3), "SetInvitesAllowed");
		if (!result.Ok)
		{
			return result;
		}
		SessionModificationSetJoinInProgressAllowedOptions options4 = new SessionModificationSetJoinInProgressAllowedOptions
		{
			AllowJoinInProgress = false
		};
		result = EosResultToNetworkResult(outSessionModificationHandle.SetJoinInProgressAllowed(ref options4), "SetJoinInProgressAllowed");
		if (!result.Ok)
		{
			return result;
		}
		SessionModificationAddAttributeOptions options5 = new SessionModificationAddAttributeOptions
		{
			AdvertisementType = SessionAttributeAdvertisementType.Advertise,
			SessionAttribute = new AttributeData
			{
				Key = "HOSTID",
				Value = EosManager.GetProductUserId().ToString()
			}
		};
		result = EosResultToNetworkResult(outSessionModificationHandle.AddAttribute(ref options5), "AddAttribute");
		if (!result.Ok)
		{
			return result;
		}
		UpdateSessionOptions updateOptions = new UpdateSessionOptions
		{
			SessionModificationHandle = outSessionModificationHandle
		};
		result = await EosAsyncMethod("UpdateSession", (EosAsyncMethodResult eosAsyncMethodResult) =>
		{
			_sessionsInterface.UpdateSession(ref updateOptions, null, delegate(ref UpdateSessionCallbackInfo data)
			{
				eosAsyncMethodResult.Result = data.ResultCode;
				eosAsyncMethodResult.Done = true;
			});
		});
		if (!result.Ok)
		{
			return result;
		}
		Log.NetworkInfo("[EOS] New session with sessionID: '" + sessionID + "'");
		SessionID = sessionID;
		IsHost = true;
		IsClient = false;
		IsRunning = true;
		result = await RegisterNewPlayer(CurrentPlayer.EosUserId, _userToken);
		if (!result.Ok)
		{
			return result;
		}
		WireEosCallbacks();
		PlatformLayer.Instance.StartCoroutine(PollPackagesCrt());
		return result;
	}

	private async Task<NetworkBackendResult> UnregisterPlayer(ProductUserId userId)
	{
		UnregisterPlayersOptions options2 = new UnregisterPlayersOptions
		{
			SessionName = SessionID,
			PlayersToUnregister = new ProductUserId[1] { userId }
		};
		return await EosAsyncMethod("UnregisterPlayers", (EosAsyncMethodResult result) =>
		{
			_sessionsInterface.UnregisterPlayers(ref options2, null, delegate(ref UnregisterPlayersCallbackInfo data)
			{
				result.Result = data.ResultCode;
				result.Done = true;
			});
		});
	}

	private async Task<NetworkBackendResult> RegisterNewPlayer(ProductUserId userId, UserToken userToken)
	{
		NetworkBackendResult result = default;
		if (IsHost)
		{
			RegisterPlayersOptions registerPlayersOptions = new RegisterPlayersOptions
			{
				SessionName = SessionID,
				PlayersToRegister = new ProductUserId[1] { userId }
			};
			bool succes = false;
			result = await EosAsyncMethod("RegisterPlayers", (EosAsyncMethodResult eosAsyncMethodResult) =>
			{
				_sessionsInterface.RegisterPlayers(ref registerPlayersOptions, null, delegate(ref RegisterPlayersCallbackInfo data)
				{
					eosAsyncMethodResult.Result = data.ResultCode;
					eosAsyncMethodResult.Done = true;
					succes = data.RegisteredPlayers.Contains(userId);
				});
			});
			if (!result.Ok || !succes)
			{
				if (userId != CurrentPlayer.EosUserId)
				{
					DisconnectPeer(userId);
				}
				if (!succes && result.Ok)
				{
					return new NetworkBackendResult
					{
						ErrorCode = -1,
						ErrorMessage = "Failed to register player to session"
					};
				}
				return result;
			}
		}
		if (userToken == _userToken)
		{
			_players.Add(CurrentPlayer);
		}
		else
		{
			_players.Add(new NetworkBackendPlayer(userToken)
			{
				PlayerID = NewPlayerID(),
				EosUserId = new ProductUserId(userId.InnerHandle)
			});
		}
		NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.PlayerJoined, _players.Last()));
		return result;
	}

	private NetworkBackendResult DisconnectPeer(ProductUserId userId)
	{
		CloseConnectionOptions options = new CloseConnectionOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			RemoteUserId = userId,
			SocketId = new SocketId
			{
				SocketName = SessionID
			}
		};
		return EosResultToNetworkResult(_p2pInterface.CloseConnection(ref options), "CloseConnection");
	}

	public async Task<NetworkBackendResult> JoinAsync(string sessionID, UserToken user)
	{
		NetworkBackendResult result = InitEOS();
		if (!result.Ok)
		{
			return result;
		}
		result = await Authenticate();
		if (!result.Ok)
		{
			return result;
		}
		CreateSessionSearchOptions options = new CreateSessionSearchOptions
		{
			MaxSearchResults = 1u
		};
		result = EosResultToNetworkResult(_sessionsInterface.CreateSessionSearch(ref options, out var sessionSearch), "CreateSessionSearch");
		if (!result.Ok)
		{
			return result;
		}
		SessionSearchSetSessionIdOptions options2 = new SessionSearchSetSessionIdOptions
		{
			SessionId = PrepareSessionID(sessionID)
		};
		result = EosResultToNetworkResult(sessionSearch.SetSessionId(ref options2), "SetSessionId");
		if (!result.Ok)
		{
			return result;
		}
		NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ChangeConnectionState, ConnectionState.SearchingForSession));
		SessionSearchFindOptions sessionSearchFindOptions = new SessionSearchFindOptions
		{
			LocalUserId = EosManager.GetProductUserId()
		};
		result = await EosAsyncMethod("Session.Find", (EosAsyncMethodResult eosAsyncMethodResult) =>
		{
			sessionSearch.Find(ref sessionSearchFindOptions, null, delegate(ref SessionSearchFindCallbackInfo data)
			{
				eosAsyncMethodResult.Result = data.ResultCode;
				eosAsyncMethodResult.Done = true;
			});
		});
		if (!result.Ok)
		{
			return result;
		}
		SessionSearchGetSearchResultCountOptions options3 = default;
		if (sessionSearch.GetSearchResultCount(ref options3) == 0)
		{
			return new NetworkBackendResult
			{
				ErrorCode = -1,
				ErrorMessage = "No session found"
			};
		}
		SessionSearchCopySearchResultByIndexOptions options4 = new SessionSearchCopySearchResultByIndexOptions
		{
			SessionIndex = 0u
		};
		result = EosResultToNetworkResult(sessionSearch.CopySearchResultByIndex(ref options4, out var outSessionHandle), "CopySearchResultByIndex");
		if (!result.Ok)
		{
			return result;
		}
		SessionDetailsCopySessionAttributeByKeyOptions options5 = new SessionDetailsCopySessionAttributeByKeyOptions
		{
			AttrKey = "HOSTID"
		};
		result = EosResultToNetworkResult(outSessionHandle.CopySessionAttributeByKey(ref options5, out var outSessionAttribute), "CopySessionAttribute");
		if (!result.Ok)
		{
			return result;
		}
		JoinSessionOptions joinSessionOptions = new JoinSessionOptions
		{
			LocalUserId = EosManager.GetProductUserId(),
			SessionName = sessionID,
			SessionHandle = outSessionHandle
		};
		SessionID = sessionID;
		IsClient = true;
		IsRunning = true;
		WireEosCallbacks();
		string text = outSessionAttribute.Value.Data.Value.Value.AsUtf8.ToString();
		AcceptConnectionOptions options6 = new AcceptConnectionOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			SocketId = new SocketId
			{
				SocketName = sessionID
			},
			RemoteUserId = ProductUserId.FromString(text)
		};
		result = EosResultToNetworkResult(_p2pInterface.AcceptConnection(ref options6), "AcceptConnection");
		if (!result.Ok)
		{
			return result;
		}
		return default;
	}

	private IEnumerator NewPlayerConnected(ProductUserId senderId, UserToken ut)
	{
		ProductUserId userId = new ProductUserId(senderId.InnerHandle);
		NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ConnectionAccepted));
		Task<NetworkBackendResult> task = RegisterNewPlayer(userId, ut);
		yield return new WaitForTask(task);
	}

	private IEnumerator PollPackagesCrt()
	{
		ArraySegment<byte> data = new byte[2048];
		ReceivePacketOptions options = new ReceivePacketOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			RequestedChannel = null,
			MaxDataSizeBytes = (uint)data.Array.Length
		};
		ProductUserId senderId = null;
		SocketId socketId = default;
		while (IsRunning)
		{
			Epic.OnlineServices.Result result = _p2pInterface.ReceivePacket(ref options, ref senderId, ref socketId, out var outChannel, data, out var _);
			switch (result)
			{
			case Epic.OnlineServices.Result.Success:
			{
				IProtocolToken protocolToken = ProtocolTokenUtils.ReadProtoclToken(data.Array);
				if (protocolToken is UserToken userToken)
				{
					NetworkBackendConnectRequest networkBackendConnectRequest = new NetworkBackendConnectRequest(userToken);
					ConnectRequestedHandler?.Invoke(networkBackendConnectRequest);
					if (networkBackendConnectRequest.Accepted)
					{
						PlatformLayer.Instance.StartCoroutine(NewPlayerConnected(senderId, userToken));
					}
					else
					{
						DisconnectPeer(senderId);
					}
					break;
				}
				IProtocolToken protocolToken2 = protocolToken;
				NetworkBackendPlayer networkBackendPlayer = _players.FirstOrDefault((NetworkBackendPlayer pl) => pl.EosUserId == senderId);
				bool flag = false;
				ClientToClientToken cct = protocolToken as ClientToClientToken;
				if (cct != null)
				{
					protocolToken2 = cct.Token;
					if (IsHost)
					{
						networkBackendPlayer = _players.First((NetworkBackendPlayer pl) => pl.PlayerID == cct.Sender);
						NetworkBackendPlayer networkBackendPlayer2 = _players.FirstOrDefault((NetworkBackendPlayer pl) => pl.PlayerID == cct.Target);
						if (networkBackendPlayer2 == null)
						{
							SendProtocolTokenToAll(cct, (NetworkDataStreamChannel)outChannel, networkBackendPlayer);
							NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ProtocolTokenRecieved, networkBackendPlayer, protocolToken2));
						}
						else if (networkBackendPlayer2 == CurrentPlayer)
						{
							NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ProtocolTokenRecieved, networkBackendPlayer, protocolToken2));
						}
						else
						{
							SendProtocolTokenToClient(networkBackendPlayer2, cct, (NetworkDataStreamChannel)outChannel);
						}
						flag = true;
					}
					else
					{
						networkBackendPlayer = _players.FirstOrDefault((NetworkBackendPlayer pl) => pl.PlayerID == cct.Sender);
					}
				}
				if (flag)
				{
					break;
				}
				if (networkBackendPlayer == null)
				{
					Log.NetworkWarning($"[EOS] Unable to find sender with ID '{senderId}' in EOSNetworkBackend");
					break;
				}
				if (protocolToken2 is GameToken)
				{
					RegisterNewPlayer(CurrentPlayer.EosUserId, _userToken);
				}
				PlayerToken t = protocolToken2 as PlayerToken;
				if (t != null)
				{
					NetworkBackendPlayer networkBackendPlayer3 = _players.FirstOrDefault((NetworkBackendPlayer pl) => pl.EosUserId.ToString() == t.EosUserID);
					if (networkBackendPlayer3 != null)
					{
						networkBackendPlayer3.PlayerID = t.PlayerID;
					}
					else
					{
						_players.Add(new NetworkBackendPlayer(t.Username, t.EosUserID)
						{
							PlayerID = t.PlayerID,
							EosUserId = ProductUserId.FromString(t.EosUserID)
						});
					}
				}
				PlayerLeftToken pt = protocolToken2 as PlayerLeftToken;
				if (pt != null)
				{
					_players.RemoveAll((NetworkBackendPlayer pl) => pl.PlayerID == pt.PlayerID);
				}
				NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ProtocolTokenRecieved, networkBackendPlayer, protocolToken2));
				break;
			}
			default:
				Log.NetworkError(result.ToString(), $"[EOS] RecievePacked failed: '{result}'");
				break;
			case Epic.OnlineServices.Result.NotFound:
				break;
			}
			yield return null;
		}
	}

	public async Task ShutdownAsync()
	{
		if (!IsRunning)
		{
			return;
		}
		Log.NetworkInfo("[EOS] Shutting down...");
		if (IsHost)
		{
			foreach (NetworkBackendPlayer player in _players)
			{
				DisconnectClient(player);
			}
			_players.Clear();
			IsHost = false;
			IsRunning = false;
			DestroySessionOptions options = new DestroySessionOptions
			{
				SessionName = SessionID
			};
			await EosAsyncMethod("DestroySession", (EosAsyncMethodResult result) =>
			{
				_sessionsInterface.DestroySession(ref options, null, delegate(ref DestroySessionCallbackInfo data)
				{
					result.Result = data.ResultCode;
					result.Done = true;
				});
			});
			SessionID = string.Empty;
			CurrentPlayer.PlayerID = 1;
		}
		else if (IsClient)
		{
			if (IsRunning)
			{
				NetworkBackendPlayer networkBackendPlayer = _players.FirstOrDefault();
				if (networkBackendPlayer != null)
				{
					DisconnectClient(networkBackendPlayer);
				}
			}
			IsRunning = false;
			IsClient = false;
			SessionID = string.Empty;
			_players.Clear();
			CurrentPlayer.PlayerID = 1;
		}
		UnwireEosCallbacks();
		Log.NetworkInfo("[EOS] Shutdown completed.");
	}

	public void SendProtocolTokenToClient(NetworkBackendPlayer client, IProtocolToken token, NetworkDataStreamChannel channel)
	{
		if (IsHost)
		{
			SendPacketOptions options = new SendPacketOptions
			{
				LocalUserId = CurrentPlayer.EosUserId,
				Channel = (byte)channel,
				Reliability = PacketReliability.ReliableOrdered,
				Data = token.ToByteArrayWithType(),
				AllowDelayedDelivery = false,
				DisableAutoAcceptConnection = false,
				RemoteUserId = client.EosUserId,
				SocketId = new SocketId
				{
					SocketName = SessionID
				}
			};
			Log.NetworkTrace($"[EOS] Sending {token} from host {CurrentPlayer.EosUserId} to client {client.EosUserId}");
			Epic.OnlineServices.Result result = _p2pInterface.SendPacket(ref options);
			if (result != Epic.OnlineServices.Result.Success)
			{
				Log.NetworkError(result.ToString(), $"[EOS] Failed to send token {token} to client {client.EosUserId}: {result}");
			}
		}
		else
		{
			ClientToClientToken clientToClientToken = new ClientToClientToken
			{
				Target = client.PlayerID,
				Sender = CurrentPlayer.PlayerID,
				Token = token
			};
			SendProtolTokenToServer(clientToClientToken, channel);
		}
	}

	public void SendProtocolTokenToAll(IProtocolToken token, NetworkDataStreamChannel channel, NetworkBackendPlayer excludePlayer = null)
	{
		if (IsHost)
		{
			foreach (NetworkBackendPlayer player in _players)
			{
				if (excludePlayer != player && player != CurrentPlayer)
				{
					SendProtocolTokenToClient(player, token, channel);
				}
			}
			return;
		}
		ClientToClientToken clientToClientToken = new ClientToClientToken
		{
			Target = -1,
			Sender = CurrentPlayer.PlayerID,
			Token = token
		};
		SendProtolTokenToServer(clientToClientToken, channel);
	}

	public void SendProtolTokenToServer(IProtocolToken token, NetworkDataStreamChannel channel)
	{
		SendPacketOptions options = new SendPacketOptions
		{
			LocalUserId = CurrentPlayer.EosUserId,
			Channel = (byte)channel,
			Reliability = PacketReliability.ReliableOrdered,
			Data = token.ToByteArrayWithType(),
			AllowDelayedDelivery = false,
			DisableAutoAcceptConnection = false,
			RemoteUserId = _players[0].EosUserId,
			SocketId = new SocketId
			{
				SocketName = SessionID
			}
		};
		Epic.OnlineServices.Result result = _p2pInterface.SendPacket(ref options);
		if (result != Epic.OnlineServices.Result.Success)
		{
			Log.NetworkError(result.ToString(), $"[EOS] Failed to send token {token} to server: {result}");
		}
	}

	public void DisconnectClient(NetworkBackendPlayer client)
	{
		Log.NetworkInfo($"[EOS] Disconnect player: {client.EosUserId}");
		DisconnectPeer(client.EosUserId);
	}

	public void Dispose()
	{
	}
}
