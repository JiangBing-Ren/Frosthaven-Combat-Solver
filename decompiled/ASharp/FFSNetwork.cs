using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FFSNet;
using FFSNet.Backend;
using FH.Platforms;
using FHRL;
using GLOOM.MainMenu;
using Libs.Build;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using UnityEngine.Events;

public static class FFSNetwork
{
	private static bool _cancelingJoining;

	public static Action<Exception> OnDesyncDetected;

	private static INetworkBackend _backend;

	private static byte[] _streamingData;

	public static NetworkManager Manager { get; private set; }

	public static NetworkBehaviour Behaviour { get; private set; }

	public static bool IsOnline
	{
		get
		{
			if (IsRunning)
			{
				return !IsShuttingDown;
			}
			return false;
		}
	}

	public static bool IsHost { get; private set; }

	public static bool IsRunning => _backend?.IsRunning ?? false;

	public static bool IsClient { get; private set; }

	public static bool IsStartingUp { get; private set; }

	public static bool IsShuttingDown { get; private set; }

	public static bool HasDesynchronized { get; private set; }

	public static bool IsExecutingGameAction { get; set; }

	public static string SessionID => _backend?.SessionID ?? string.Empty;

	public static UserToken CurrentUserToken { get; private set; }

	public static NetworkBackendPlayer CurrentPlayer => _backend?.CurrentPlayer;

	public static IEnumerable<NetworkBackendPlayer> Players => _backend?.Players;

	public static ConnectionState ConnectionState { get; private set; }

	public static UnityEvent HostingStartedEvent { get; private set; } = new UnityEvent();

	public static UnityEvent HostingEndedEvent { get; private set; } = new UnityEvent();

	public static void Unintiialize()
	{
		Log.NetworkInfo("Unintiializing FFSNetwork");
		_backend?.Dispose();
		_backend = null;
	}

	public static IEnumerator InitializeCrt(NetworkManager manager, NetworkBehaviour behaviour)
	{
		Log.NetworkInfo("Initializing FFSNetwork");
		ConnectionState = ConnectionState.None;
		Manager = manager;
		Behaviour = behaviour;
		IsHost = false;
		IsClient = false;
		CurrentUserToken = null;
		GameState.s_getHostCharacter = GetHostCharacter;
		GameState.s_getHostCharacterIncludingDead = GetHostCharacterIncludingDead;
		yield return PlatformLayer.Instance.StartCoroutine(LoadCurrentUserTokenCrt((UserToken t) =>
		{
			CurrentUserToken = t;
		}));
		_backend = new EOSNetworkBackend(manager, CurrentUserToken);
		_backend.ConnectRequestedHandler = GHNetworkCallbacks.ConnectRequest;
		Log.NetworkInfo($"FFSNetwork finished initialization with backend: {_backend}");
	}

	public static IEnumerator WaitForInitializationCrt()
	{
		Log.NetworkInfo("WaitForInitializationCrt start");
		Stopwatch sw = new Stopwatch();
		sw.Start();
		while (_backend == null)
		{
			yield return null;
			if (sw.Elapsed.TotalSeconds > 30.0)
			{
				throw new TimeoutException("FFSNetwork.WaitForInitializationCrt timed out! Possible problem with platform.");
			}
		}
		sw.Stop();
		Log.NetworkInfo($"WaitForInitializationCrt finished after {sw.Elapsed.TotalSeconds} seconds");
	}

	private static void StartUp()
	{
		Log.NetworkInfo("Starting up FFSNetwork");
		_backend.NetworkEvents.Clear();
		IsStartingUp = true;
		HasDesynchronized = false;
	}

	private static void StartDone()
	{
		Log.NetworkInfo("FFSNetwork started.");
		IsStartingUp = false;
		GHNetworkCallbacks.StartDone();
	}

	public static void ToggleServer(Action onHostingStarted = null, Action onHostingEnded = null)
	{
		if (!IsOnline && !IsStartingUp)
		{
			PlatformLayer.Instance.StartCoroutine(StartServerCrt(onHostingStarted, onHostingEnded));
			return;
		}
		Shutdown(new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession));
		PlatformLayer.Instance.StartCoroutine(WaitForShutdownCrt(onHostingEnded));
	}

	private static IEnumerator StartServerCrt(Action onHostingStarted = null, Action onHostingEnded = null)
	{
		Log.NetworkInfo("About to start a server.");
		yield return WaitForInitializationCrt();
		StartUp();
		Task<NetworkBackendResult> task = _backend.HostAsync();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		if (!task.Result.Ok)
		{
			IsStartingUp = false;
			Log.NetworkError(task.Result.ErrorCode.ToString(), "Failed to start server: '" + task.Result.ErrorMessage + "'");
			yield break;
		}
		IsHost = true;
		StartDone();
		HostingStartedEvent?.Invoke();
		onHostingStarted?.Invoke();
		IProtocolToken clientDisconnectToken = null;
		bool isEnd = false;
		PlatformLayer.Instance.NewPlatformLayer.AdvertiseLobbyKey(SessionID);
		while (IsRunning && !isEnd)
		{
			NetworkBackendEvent result;
			while (_backend.NetworkEvents.TryDequeue(out result))
			{
				switch (result.EventType)
				{
				case NetworkBackendEvent.Type.PlayerJoined:
					GHHostCallbacks.Connected(result.Get<NetworkBackendPlayer>());
					break;
				case NetworkBackendEvent.Type.PlayerLeft:
					GHHostCallbacks.Disconnected(result.Get<NetworkBackendPlayer>(), null);
					break;
				case NetworkBackendEvent.Type.ProtocolTokenRecieved:
					HandleHostProtocolToken(result.Get<NetworkBackendPlayer>(), result.Get2<IProtocolToken>());
					break;
				case NetworkBackendEvent.Type.ShutdownCommand:
					clientDisconnectToken = result.Get<IProtocolToken>();
					isEnd = true;
					break;
				}
			}
			yield return null;
		}
		yield return ShutdownCrt(clientDisconnectToken);
		onHostingEnded?.Invoke();
	}

	private static void HandleHostProtocolToken(NetworkBackendPlayer player, IProtocolToken token)
	{
		Log.NetworkTrace($"Recieved token {token} from player {player.PlayerID}");
		if (token is DebugMessageToken dmt)
		{
			WriteMessage(player, dmt);
		}
		if (token is PlayerEntityRequestToken request)
		{
			GHHostCallbacks.OnEvent(request, player);
		}
		if (token is PlayerEntityInitializedEventToken request2)
		{
			GHHostCallbacks.OnEvent(request2, player);
		}
		if (token is ControllableAssignmentRequestToken request3)
		{
			GHHostCallbacks.OnEvent(request3, player);
		}
		if (token is GameDataRequestToken request4)
		{
			GHHostCallbacks.OnEvent(request4, player);
		}
		if (token is GameDataEventToken evnt)
		{
			GHHostCallbacks.OnEvent(evnt, player);
		}
		if (token is NetworkActionEventToken evnt2)
		{
			GHNetworkCallbacks.OnEvent(evnt2, player);
		}
		if (token is GameActionEventClassIDToken evnt3)
		{
			GHNetworkCallbacks.OnEvent(evnt3, player);
		}
		if (token is GameActionEventToken evnt4)
		{
			GHNetworkCallbacks.OnEvent(evnt4, player);
		}
		if (token is PlayerAvatarStreamingDataToken evnt5)
		{
			GHNetworkCallbacks.OnEvent(evnt5, player);
		}
		if (token is StreamingDataToken token2)
		{
			GHHostCallbacks.OnStreamingDataFromClient(token2, player);
		}
	}

	private static void WriteMessage(NetworkBackendPlayer player, DebugMessageToken dmt)
	{
		GameConsole.Engine.Print("[" + player.Username + "] " + dmt.Message);
		Log.NetworkTrace(string.Format("[{0}] Player {1} says: {2}", DateTime.Now.ToString("HH:mm:ss.fff"), player.PlayerID, dmt.Message));
	}

	public static void JoinSession(string sessionID = "", Action<CustomDataToken> onConnectionEstablished = null, Action<ConnectionState> onConnectionStateUpdated = null, Action<ConnectionErrorCode> onConnectionFailed = null, Action<DisconnectionErrorCode> onDisconnected = null)
	{
		PlatformLayer.Instance.StartCoroutine(JoinSessionCrt(sessionID, onConnectionEstablished, onConnectionStateUpdated, onConnectionFailed, onDisconnected));
	}

	private static IEnumerator JoinSessionCrt(string sessionID = "", Action<CustomDataToken> onConnectionEstablished = null, Action<ConnectionState> onConnectionStateUpdated = null, Action<ConnectionErrorCode> onConnectionFailed = null, Action<DisconnectionErrorCode> onDisconnected = null)
	{
		sessionID = sessionID ?? string.Empty;
		Log.NetworkInfo("About to start joining session: " + sessionID);
		yield return WaitForInitializationCrt();
		if (_backend.IsRunning)
		{
			Log.NetworkError("ERROR_MULTIPLAYER_00032", "We are already connecting to a session.");
			yield break;
		}
		if (!SceneController.Instance.InitialLoadingComplete || !PersistentData.s_Instance.IsDataLoaded)
		{
			Log.NetworkInfo("Game is still loading, deferring session join until main menu is ready.");
			while (!SceneController.Instance.InitialLoadingComplete || !PersistentData.s_Instance.IsDataLoaded)
			{
				yield return null;
			}
			while (true)
			{
				MainMenuUIManager instance = MainMenuUIManager.Instance;
				if ((object)instance != null && instance.Initialised)
				{
					break;
				}
				yield return null;
			}
		}
		if (SaveData.Instance.Global.CurrentGameState != EGameState.None)
		{
			Log.NetworkInfo("Going back to main menu before join");
			yield return UIManager.LoadMainMenu(sessionID);
			yield break;
		}
		ProcessingUI.Show(show: true);
		ConnectionState = ConnectionState.SearchingForSession;
		onConnectionStateUpdated?.Invoke(ConnectionState);
		if (sessionID.Equals(string.Empty))
		{
			Log.NetworkInfo("About to join a random session.");
		}
		Log.NetworkInfo("About to start the client.");
		StartUp();
		IsClient = true;
		ConnectionState = ConnectionState.StartingClient;
		onConnectionStateUpdated?.Invoke(ConnectionState);
		Task<NetworkBackendResult> task = _backend.JoinAsync(sessionID, CurrentUserToken);
		while (!task.IsCompleted)
		{
			if (_cancelingJoining)
			{
				yield return ShutdownCrt();
				IsStartingUp = false;
				IsClient = false;
				ProcessingUI.Show(show: false);
				yield break;
			}
			yield return null;
		}
		if (!task.Result.Ok)
		{
			IsStartingUp = false;
			IsClient = false;
			Log.NetworkError(task.Result.ErrorCode.ToString(), "Failed to start client: '" + task.Result.ErrorMessage + "'");
			if (_backend.NetworkEvents.TryDequeue(out var result) && result.EventType == NetworkBackendEvent.Type.ConnectToServerFailed)
			{
				onConnectionFailed?.Invoke(result.Get<ConnectionErrorCode>());
			}
			else
			{
				onConnectionFailed?.Invoke(ConnectionErrorCode.SessionNotFound);
			}
			ProcessingUI.Show(show: false);
			yield break;
		}
		StartDone();
		ConnectionState = ConnectionState.Connecting;
		onConnectionStateUpdated?.Invoke(ConnectionState);
		bool end = false;
		DisconnectionErrorToken disconnectToken = null;
		while (IsRunning && !end)
		{
			NetworkBackendEvent result2;
			while (_backend.NetworkEvents.TryDequeue(out result2))
			{
				switch (result2.EventType)
				{
				case NetworkBackendEvent.Type.ConnectedToServer:
					GHClientCallbacks.SessionConnected();
					break;
				case NetworkBackendEvent.Type.GameConnectionEstablished:
					onConnectionEstablished?.Invoke(result2.Get<GameToken>());
					break;
				case NetworkBackendEvent.Type.DisconnectedFromServer:
					end = true;
					DesyncDetector.OnPlayerDisconnected(PlayerRegistry.HostPlayer);
					break;
				case NetworkBackendEvent.Type.ConnectToServerFailed:
				case NetworkBackendEvent.Type.GameConnectionFailed:
					GHClientCallbacks.SessionConnectFailed(result2.Get<ConnectionErrorCode>());
					onConnectionFailed?.Invoke(result2.Get<ConnectionErrorCode>());
					end = true;
					break;
				case NetworkBackendEvent.Type.CancelConnectingToServer:
					ConnectionState = ConnectionState.CancelingConnection;
					onConnectionStateUpdated?.Invoke(ConnectionState);
					end = true;
					break;
				case NetworkBackendEvent.Type.ChangeConnectionState:
					ConnectionState = result2.Get<ConnectionState>();
					onConnectionStateUpdated?.Invoke(ConnectionState);
					break;
				case NetworkBackendEvent.Type.ProtocolTokenRecieved:
				{
					NetworkBackendPlayer player = result2.Get<NetworkBackendPlayer>();
					IProtocolToken token = result2.Get2<IProtocolToken>();
					HandleClientProtocolToken(player, token);
					break;
				}
				case NetworkBackendEvent.Type.ShutdownCommand:
					disconnectToken = result2.Get<DisconnectionErrorToken>();
					end = true;
					break;
				}
			}
			yield return null;
		}
		if (disconnectToken == null)
		{
			disconnectToken = new DisconnectionErrorToken(DisconnectionErrorCode.HostEndedSession);
		}
		GHClientCallbacks.Disconnected(disconnectToken);
		onDisconnected?.Invoke((DisconnectionErrorCode)disconnectToken.ErrorCode);
		yield return ShutdownCrt();
		ProcessingUI.Show(show: false);
		if (SaveData.Instance.Global.CurrentGameState != EGameState.None || SceneController.Instance.IsLoading || SceneController.Instance.ScenarioIsLoading || Choreographer.s_Choreographer != null)
		{
			SceneController.Instance.ReloadMainMenu = true;
			while (SceneController.Instance.IsLoading || SceneController.Instance.ScenarioIsLoading)
			{
				yield return null;
			}
			if (SceneController.Instance.ReloadMainMenu)
			{
				SceneController.Instance.ReloadMainMenu = false;
				UIManager.LoadMainMenu();
			}
		}
		else
		{
			ControllableRegistry.Reset();
			PlayerRegistry.Reset();
		}
	}

	private static void HandleClientProtocolToken(NetworkBackendPlayer player, IProtocolToken token)
	{
		Log.NetworkTrace(string.Format("[{0}] Recieved token {1} from player {2}", DateTime.Now.ToString("HH:mm:ss.fff"), token, player.PlayerID));
		if (token is DebugMessageToken dmt)
		{
			WriteMessage(player, dmt);
		}
		if (token is DisconnectionErrorToken data)
		{
			_backend.NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ShutdownCommand, data));
		}
		if (token is ControllableAssignmentEventToken evnt)
		{
			GHClientCallbacks.OnEvent(evnt, player);
		}
		if (token is ControllableReleaseEventToken evnt2)
		{
			GHClientCallbacks.OnEvent(evnt2, player);
		}
		if (token is ControllableDestructionEventToken evnt3)
		{
			GHClientCallbacks.OnEvent(evnt3, player);
		}
		if (token is GameDataRequestToken request)
		{
			GHClientCallbacks.OnEvent(request, player);
		}
		if (token is GameToken gameToken)
		{
			GHClientCallbacks.Connected(gameToken);
		}
		if (token is PlayerToken evnt4)
		{
			GHClientCallbacks.OnEvent(evnt4, player);
		}
		if (token is NetworkActionEventToken evnt5)
		{
			GHNetworkCallbacks.OnEvent(evnt5, player);
		}
		if (token is GameActionEventClassIDToken evnt6)
		{
			GHNetworkCallbacks.OnEvent(evnt6, player);
		}
		if (token is GameActionEventToken evnt7)
		{
			GHNetworkCallbacks.OnEvent(evnt7, player);
		}
		if (token is SlotStateToken evnt8)
		{
			GHClientCallbacks.OnEvent(evnt8, player);
		}
		if (token is PlayerAvatarStreamingDataToken evnt9)
		{
			GHNetworkCallbacks.OnEvent(evnt9, player);
		}
		if (token is PlayerLeftToken evnt10)
		{
			GHClientCallbacks.OnEvent(evnt10, player);
		}
		if (token is SavePointReachedEventToken evnt11)
		{
			GHClientCallbacks.OnEvent(evnt11, player);
		}
		if (!(token is StreamingDataToken streamingDataToken))
		{
			return;
		}
		if (streamingDataToken.IsStart && streamingDataToken.IsEnd)
		{
			GHClientCallbacks.StreamDataStarted();
			GHClientCallbacks.StreamDataProgress((float)streamingDataToken.Offset + (float)streamingDataToken.Array.Length / (float)streamingDataToken.Total);
			GHClientCallbacks.StreamDataReceived(streamingDataToken.Array);
			return;
		}
		if (streamingDataToken.IsStart)
		{
			GHClientCallbacks.StreamDataStarted();
			_streamingData = new byte[streamingDataToken.Total];
		}
		GHClientCallbacks.StreamDataProgress((float)streamingDataToken.Offset + (float)streamingDataToken.Array.Length / (float)streamingDataToken.Total);
		Buffer.BlockCopy(streamingDataToken.Array, 0, _streamingData, streamingDataToken.Offset, streamingDataToken.Array.Length);
		if (streamingDataToken.IsEnd)
		{
			GHClientCallbacks.StreamDataReceived(_streamingData);
			_streamingData = null;
		}
	}

	public static void CancelJoiningSession(Action onJoiningCanceled = null)
	{
		if (!_cancelingJoining)
		{
			Log.NetworkInfo("Canceling session join.");
			_cancelingJoining = true;
			QueueNetworkEvent(NetworkBackendEvent.Type.CancelConnectingToServer);
			Shutdown();
			PlatformLayer.Instance.StartCoroutine(WaitForShutdownCrt(() =>
			{
				_cancelingJoining = false;
				onJoiningCanceled?.Invoke();
			}));
		}
	}

	public static void QueueNetworkEvent(NetworkBackendEvent.Type type, object data1 = null, object data2 = null)
	{
		_backend.NetworkEvents.Enqueue(new NetworkBackendEvent(type, data1, data2));
	}

	public static void HandleDesync(Exception ex)
	{
		if (IsOnline && !HasDesynchronized)
		{
			Log.NetworkError(null, "Desynchronization occurred.\n" + ex);
			OnDesyncDetected?.Invoke(ex);
			if (Manager.AutoShutdownUponDesynchronization)
			{
				Shutdown();
			}
		}
	}

	public static void Shutdown(IProtocolToken clientDisconnectionToken = null)
	{
		if (IsStartingUp || IsOnline)
		{
			IsShuttingDown = true;
			_backend.NetworkEvents.Enqueue(new NetworkBackendEvent(NetworkBackendEvent.Type.ShutdownCommand, clientDisconnectionToken));
		}
	}

	private static IEnumerator WaitForShutdownCrt(Action onShutdownComplete = null)
	{
		while (IsShuttingDown)
		{
			yield return null;
		}
		onShutdownComplete?.Invoke();
	}

	private static IEnumerator ShutdownCrt(IProtocolToken clientDisconnectionToken = null)
	{
		Log.NetworkInfo("Shutting down FFSNetwork");
		PlatformLayer.Instance.NewPlatformLayer.AdvertiseLobbyKey(null);
		IsShuttingDown = true;
		if (IsHost)
		{
			if (HasDesynchronized)
			{
				clientDisconnectionToken = new DisconnectionErrorToken(DisconnectionErrorCode.Desynchronization);
			}
			if (clientDisconnectionToken != null)
			{
				foreach (NetworkBackendPlayer item in Players.ToList())
				{
					if (item != CurrentPlayer)
					{
						_backend.SendProtocolTokenToClient(item, clientDisconnectionToken, NetworkDataStreamChannel.Main);
						_backend.DisconnectClient(item);
					}
				}
			}
			HostingEndedEvent?.Invoke();
		}
		PlatformLayer.Instance.NewPlatformLayer.LeaveSession();
		GHNetworkCallbacks.ShutdownBegin(string.Empty);
		Task task = _backend.ShutdownAsync();
		while (!task.IsCompleted)
		{
			yield return null;
		}
		IsShuttingDown = false;
		IsHost = false;
		IsClient = false;
		if (Singleton<UIResultsManager>.InstanceFast != null && Singleton<UIResultsManager>.InstanceFast.IsShown)
		{
			Singleton<UIResultsManager>.InstanceFast.MPSessionEndedOnResults();
		}
		Log.NetworkInfo("FFSNetwork is down.");
	}

	public static void StreamBytesToClient(NetworkBackendPlayer toPlayer, byte[] data, NetworkDataStreamChannel channel = NetworkDataStreamChannel.Main)
	{
		if (data == null)
		{
			data = Array.Empty<byte>();
		}
		foreach (StreamingDataToken item in StreamingDataToken.SplitByteArrayIntoTokens(data))
		{
			_backend.SendProtocolTokenToClient(toPlayer, item, channel);
		}
	}

	public static void Disconnect(NetworkBackendPlayer player)
	{
		if (!IsHost)
		{
			Log.NetworkError(null, "Only host can disconnect clients");
		}
		else if (player == _backend.CurrentPlayer)
		{
			Log.NetworkError(null, "Cannot disconnect host");
		}
		else if (_backend.Players.Contains(player))
		{
			_backend.DisconnectClient(player);
		}
	}

	public static void SendTokenToServer(IProtocolToken token, NetworkDataStreamChannel channel = NetworkDataStreamChannel.Main)
	{
		if (token == null)
		{
			Log.NetworkError(null, "Tried to send null token to server");
			return;
		}
		if (!IsOnline)
		{
			Log.NetworkError(null, "Tried to send token to server while not connected");
			return;
		}
		if (IsHost)
		{
			Log.NetworkError(null, "Tried to send token to server while being host");
			return;
		}
		Log.NetworkTrace(string.Format("[{0}] Sending token {1} to server through channel {2}", DateTime.Now.ToString("HH:mm:ss.fff"), token, channel));
		_backend.SendProtolTokenToServer(token, channel);
	}

	public static void SendTokenToClient(IProtocolToken token, NetworkBackendPlayer client, NetworkDataStreamChannel channel = NetworkDataStreamChannel.Main)
	{
		if (token == null)
		{
			Log.NetworkError(null, "Tried to send null token to client");
			return;
		}
		if (!IsOnline)
		{
			Log.NetworkError(null, "Tried to send token to client while not connected");
			return;
		}
		if (client == null || !_backend.Players.Contains(client))
		{
			Log.NetworkError(null, "Tried to send token to invalid client");
			return;
		}
		if (client == _backend.CurrentPlayer)
		{
			Log.NetworkError(null, "Tried to send token to self");
			return;
		}
		if (IsClient)
		{
			Log.NetworkError(null, "Tried to send token to client while being client");
			return;
		}
		Log.NetworkTrace(string.Format("[{0}] Sending token {1} to client {2} through channel {3}", DateTime.Now.ToString("HH:mm:ss.fff"), token, client.PlayerID, channel));
		_backend.SendProtocolTokenToClient(client, token, channel);
	}

	public static void SendTokenToClient(IProtocolToken token, PlayerID playerID, NetworkDataStreamChannel channel = NetworkDataStreamChannel.Main)
	{
		SendTokenToClient(token, _backend.Players.FirstOrDefault((NetworkBackendPlayer p) => p.PlayerID == playerID.ID), channel);
	}

	public static void SendTokenToClients(IProtocolToken token, NetworkDataStreamChannel channel = NetworkDataStreamChannel.Main)
	{
		if (token == null)
		{
			Log.NetworkError(null, "Tried to send null token to client");
			return;
		}
		if (!IsOnline)
		{
			Log.NetworkError(null, "Tried to send token to client while not connected");
			return;
		}
		Log.NetworkTrace(string.Format("[{0}] Sending token {1} to all clients through channel {2}", DateTime.Now.ToString("HH:mm:ss.fff"), token, channel));
		_backend.SendProtocolTokenToAll(token, channel);
	}

	public static bool IsUnderMyControl(this CActor actor)
	{
		return GetControllingPlayer(actor) == PlayerRegistry.MyPlayer;
	}

	public static bool IsPlayerControlled(this CActor actor)
	{
		return GetControllingPlayer(actor) != null;
	}

	public static NetworkPlayer GetControllingPlayer(CActor actor)
	{
		if (actor == null)
		{
			return null;
		}
		CActor controllingActor = GameState.GetControllingActor(actor);
		if (controllingActor != null)
		{
			CPlayerActor cPlayerActor = controllingActor as CPlayerActor;
			if (cPlayerActor == null)
			{
				if (controllingActor is CHeroSummonActor cHeroSummonActor)
				{
					return ControllableRegistry.GetControllable(ControllableRegistry.GetControllableID(cHeroSummonActor.Summoner))?.Controller;
				}
				return PlayerRegistry.HostPlayer;
			}
			if (actor is CPlayerActor)
			{
				cPlayerActor = (CPlayerActor)actor;
			}
			return ControllableRegistry.GetControllable(ControllableRegistry.GetControllableID(cPlayerActor))?.Controller;
		}
		CAbility cAbility = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
		if (cAbility != null && cAbility.TargetingActor == actor && !(actor is CPlayerActor) && cAbility.ShouldBePlayerControlled)
		{
			return PlayerRegistry.HostPlayer;
		}
		if (actor is CPlayerActor playerActor)
		{
			return ControllableRegistry.GetControllable(ControllableRegistry.GetControllableID(playerActor))?.Controller;
		}
		return null;
	}

	public static CPlayerActor GetHostCharacter()
	{
		if (ScenarioManager.Scenario?.PlayerActors == null)
		{
			return null;
		}
		CPlayerActor cPlayerActor = null;
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			NetworkPlayer controllingPlayer = GetControllingPlayer(playerActor);
			if (controllingPlayer != null)
			{
				if (controllingPlayer == PlayerRegistry.HostPlayer)
				{
					return playerActor;
				}
				if (cPlayerActor == null)
				{
					cPlayerActor = playerActor;
				}
			}
		}
		return cPlayerActor;
	}

	public static CPlayerActor GetHostCharacterIncludingDead()
	{
		if (ScenarioManager.Scenario?.AllPlayers == null)
		{
			return null;
		}
		CPlayerActor cPlayerActor = null;
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			NetworkPlayer controllingPlayer = GetControllingPlayer(allPlayer);
			if (controllingPlayer != null)
			{
				if (controllingPlayer == PlayerRegistry.HostPlayer)
				{
					return allPlayer;
				}
				if (cPlayerActor == null)
				{
					cPlayerActor = allPlayer;
				}
			}
		}
		return cPlayerActor;
	}

	private static IEnumerator LoadCurrentUserTokenCrt(Action<UserToken> onUserTokenLoaded)
	{
		string gameVersion = string.Empty;
		string currentMod = string.Empty;
		gameVersion = RuntimeBuildInfo.BuildVersion;
		bool done = false;
		UserToken userToken = null;
		PlatformLayer.Instance.NewPlatformLayer.GetCurrentUserData((PlatformUserData platformUserData) =>
		{
			string name = platformUserData.Name;
			string platformPlayerID = platformUserData.PlatformPlayerID;
			userToken = new UserToken(0, string.Empty, name, platformPlayerID, gameVersion, currentMod);
			done = true;
		});
		while (!done)
		{
			yield return null;
		}
		onUserTokenLoaded?.Invoke(userToken);
	}

	[ConsoleCommand(Command = "net_stat")]
	public static void PrintNetworkStat(IConsole console)
	{
		console.WriteLine($"IsStartingUp: {IsStartingUp}");
		console.WriteLine($"IsShuttingDown: {IsShuttingDown}");
		console.WriteLine($"IsRunning: {IsRunning}");
		console.WriteLine($"IsOnline: {IsOnline}");
		console.WriteLine($"IsClient: {IsClient}");
		console.WriteLine($"IsHost: {IsHost}");
		console.WriteLine("SessionID: " + SessionID);
		console.WriteLine($"Connection State: {ConnectionState}");
		if (_backend != null)
		{
			console.WriteLine("Backend: " + _backend.GetType().Name);
			console.WriteLine("Players: " + string.Join(", ", _backend.Players.Select((NetworkBackendPlayer s) => s.Username)));
			console.WriteLine("Local Player: " + _backend.CurrentPlayer.Username);
		}
	}

	[ConsoleCommand(Command = "net_host")]
	public static void HostMultiplayer(IConsole console)
	{
		ToggleServer(() =>
		{
			console.WriteLine("Hosting with sessionID: " + SessionID);
		});
	}

	[ConsoleCommand(Command = "net_join")]
	public static void ConnectToLocalServer(IConsole console, string sessionID = null)
	{
		sessionID = sessionID ?? "localhost";
		JoinSession(sessionID, (CustomDataToken c) =>
		{
			console.WriteLine("Connected");
		}, (ConnectionState s) =>
		{
			console.WriteLine(s.ToString());
		}, null, (DisconnectionErrorCode s) =>
		{
			console.WriteLine($"Diconnected: {s}");
		});
	}

	[ConsoleCommand(Command = "net_stop")]
	public static void LeaveLocalServer(IConsole console, string host = null)
	{
		Shutdown();
	}

	[ConsoleCommand(Command = "net_send_message")]
	public static void SendMessageToAll(IConsole console, string message)
	{
		SendTokenToClients(new DebugMessageToken
		{
			Message = message
		});
	}
}
