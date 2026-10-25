using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FFSNet;
using FHRL;

public class FHMultiplayerServer : IDisposable
{
	private readonly StateSerializer<MultiplayerGameState> _serializer;

	private readonly DefRepository _defRepo;

	private readonly ConcurrentQueue<MultiplayerCommand> _queuedCommands = new ConcurrentQueue<MultiplayerCommand>();

	private readonly Queue<MultiplayerCommand> _executedCommands = new Queue<MultiplayerCommand>();

	private StateSerializer<MultiplayerGameState>.SerializationState _serStateA;

	private StateSerializer<MultiplayerGameState>.SerializationState _serStateB;

	public MultiplayerGameController GameController { get; private set; }

	public bool EnterScenarioTriggered { get; private set; }

	public FHMultiplayerServer(MultiplayerGameState state, DefRepository defRepo)
	{
		_defRepo = defRepo;
		_serializer = new StateSerializer<MultiplayerGameState>(defRepo);
		_serializer.StateResolveAction = (MultiplayerGameState s) =>
		{
			s.CampaignState.ResolveDefs(defRepo);
		};
		_serStateA = _serializer.CreateSerialzationState();
		_serStateB = _serializer.CreateSerialzationState();
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
		ReplaceState(state);
	}

	public void Dispose()
	{
		_serStateA.Dispose();
		_serStateB.Dispose();
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Remove(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Remove(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
	}

	private void OnPlayerLeft(NetworkPlayer player)
	{
		FHMultiplayerUtils.SyncPlayers(GameController);
	}

	private void OnPlayerJoined(NetworkPlayer player)
	{
		FHMultiplayerUtils.SyncPlayers(GameController);
	}

	private void OnControllerChanged(NetworkControllable controllable, NetworkPlayer oldController, NetworkPlayer newController)
	{
		FHMultiplayerUtils.SyncPlayers(GameController);
	}

	public void ReplaceState(MultiplayerGameState state)
	{
		GameController = new MultiplayerGameController(_serializer.Clone(state), _defRepo);
	}

	public void EnqueueCommand(MultiplayerCommand command)
	{
		_queuedCommands.Enqueue(command);
	}

	public bool TryDequeueExecutedCommand(out MultiplayerCommand command)
	{
		return _executedCommands.TryDequeue(out command);
	}

	public void Update()
	{
		MultiplayerCommand result;
		while (_queuedCommands.TryDequeue(out result))
		{
			MultiplayerGameController.Result result2 = null;
			Exception ex = null;
			string text = null;
			try
			{
				result2 = GameController.ExecuteCommand(result);
				Log.Info($"[MP SERVER] [{result.PlayerID}] Server executed command: ({result.CommandID}) {result.Name}");
			}
			catch (Exception ex2)
			{
				ex = ex2.InnerException;
				text = ex2.InnerException?.Message ?? ex2.Message;
			}
			result.Error = text;
			if (result.Failed)
			{
				if (ex is FHGameException)
				{
					Log.Warning($"[MP SERVER] [{result.PlayerID}] Command ({result.CommandID}) {result.Name} failed: {text}");
				}
				else
				{
					Log.Error($"[MP SERVER] [{result.PlayerID}] Command ({result.CommandID}) {result.Name} failed: {text}");
				}
			}
			else
			{
				Log.Info($"[MP SERVER] [{result.PlayerID}] Broadcast command: ({result.CommandID}) {result.Name}");
			}
			if (FFSNetwork.IsOnline)
			{
				Synchronizer.SendGameAction(GameActionType.FHMultiplayerCommand, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: false, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new FHMultiplayerCommandToken(result));
				if (result2 != null && result2.CampaignResult != null && result2.CampaignResult.HasEvent<CampaignEvent.EnterScenario>())
				{
					Synchronizer.SendGameAction(GameActionType.EnterScenario);
					EnterScenarioTriggered = true;
				}
			}
			_executedCommands.Enqueue(result);
		}
	}

	public bool IsStateSameAsServer(MultiplayerGameState state, bool logOnDiff = false)
	{
		_serializer.Serialize(GameController.GameState, _serStateA);
		_serializer.Serialize(state, _serStateB);
		bool flag = _serStateA.SerializedObjectSame(_serStateB);
		if (!flag & logOnDiff)
		{
			Log.Error(_serStateA.GetStringState());
			Log.Error(_serStateB.GetStringState());
		}
		return flag;
	}
}
