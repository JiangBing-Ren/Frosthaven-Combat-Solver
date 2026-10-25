using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHRL;
using Libs.Threading;
using SharedLibrary.Achievments;

public class FHMultiplayerClient : IDisposable
{
	public class Promise
	{
		private Action _completed;

		private Action _failed;

		private Func<Promise> _next;

		public void OnFailed(Action action)
		{
			_failed = action;
		}

		public void OnComplete(Action action)
		{
			_completed = action;
		}

		internal Promise Next(Func<Promise> func)
		{
			_next = func;
			return this;
		}

		internal void Completed()
		{
			if (_next == null)
			{
				_completed?.Invoke();
				return;
			}
			Promise promise = _next();
			if (promise != null)
			{
				promise.OnComplete(() =>
				{
					_completed?.Invoke();
				});
			}
			else
			{
				_completed?.Invoke();
			}
		}

		internal void Failed()
		{
			_failed?.Invoke();
		}
	}

	private class PendingCommand
	{
		public MultiplayerCommand Command;

		public MultiplayerGameState PrevState;

		public Promise Promise = new Promise();

		public bool Sent;
	}

	private StateSerializer<MultiplayerGameState> _serializer;

	private StateSerializer<CampaignState> _serializer2;

	private readonly DefRepository _defRepo;

	private List<PendingCommand> _pendingCommands = new List<PendingCommand>();

	private readonly ConcurrentQueue<MultiplayerCommand> _queuedCommands = new ConcurrentQueue<MultiplayerCommand>();

	private readonly Queue<MultiplayerCommand> _failedCommands = new Queue<MultiplayerCommand>();

	private Queue<MultiplayerGameController.Result> _commandUnprocessedResults = new Queue<MultiplayerGameController.Result>();

	public bool IsPartyLeader => GameController.IsPartyLeader(PlayerID);

	public MultiplayerGameController GameController { get; private set; }

	public MultiplayerGameState GameState => GameController.GameState;

	public PlayerID PlayerID { get; private set; }

	public IEnumerable<FHControllableCharacter> Controllables => ControllableRegistry.AllControllables.Select((NetworkControllable s) => s.ControllableObject).OfType<FHControllableCharacter>();

	public bool StateReplaced { get; private set; }

	public bool HasUnprocessedResults => _commandUnprocessedResults.Any();

	public FHMultiplayerClient(DefRepository defRepo, PlayerID playerId = default(PlayerID))
	{
		PlayerID = playerId;
		_defRepo = defRepo;
		_serializer = new StateSerializer<MultiplayerGameState>(defRepo);
		_serializer.StateResolveAction = (MultiplayerGameState s) =>
		{
			s.CampaignState.ResolveDefs(defRepo);
		};
		_serializer2 = new StateSerializer<CampaignState>(defRepo);
		_serializer2.StateResolveAction = (CampaignState s) =>
		{
			s.ResolveDefs(defRepo);
		};
		PlayerRegistry.OnPlayerJoined = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerJoined, new PlayersChangedEvent(OnPlayerJoined));
		PlayerRegistry.OnPlayerLeft = (PlayersChangedEvent)Delegate.Combine(PlayerRegistry.OnPlayerLeft, new PlayersChangedEvent(OnPlayerLeft));
		ControllableRegistry.OnControllerChanged = (ControllerChangedEvent)Delegate.Combine(ControllableRegistry.OnControllerChanged, new ControllerChangedEvent(OnControllerChanged));
	}

	public void Dispose()
	{
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

	public CampaignState CloneCurrentGameState()
	{
		return _serializer2.Clone(GameState.CampaignState);
	}

	public void ReplaceState(CampaignState state, List<CharacterState> slogAssignment)
	{
		if (!ThreadManagerComponent.Manager.IsMainThread)
		{
			throw new InvalidOperationException("Multiplayer Client's state can be modified only on main thread!");
		}
		CampaignState campaignState = _serializer2.Clone(state);
		GameController = new MultiplayerGameController(new MultiplayerGameState(campaignState, slogAssignment), _defRepo);
		NewStateControllables();
		SyncParty();
		NewStateCleanup();
	}

	public void ReplaceState(MultiplayerGameState state)
	{
		if (!ThreadManagerComponent.Manager.IsMainThread)
		{
			throw new InvalidOperationException("Multiplayer Client's state can be modified only on main thread!");
		}
		GameController = new MultiplayerGameController(_serializer.Clone(state), _defRepo);
		NewStateControllables();
		SyncParty();
		NewStateCleanup();
	}

	public FHControllableCharacter GetControllableCharacter(int id)
	{
		return Controllables.FirstOrDefault((FHControllableCharacter c) => c.ID == id);
	}

	public void Init()
	{
		StateReplaced = true;
		PlayerID = PlayerRegistry.MyPlayer.PlayerID;
		FHMultiplayerUtils.SyncPlayers(GameController);
		SyncControllables();
	}

	private void NewStateCleanup()
	{
		StateReplaced = true;
		_commandUnprocessedResults.Clear();
	}

	public CharacterState QuickAddCharacterToParty(uint slot, string classID)
	{
		CharacterClassDef def = GameController.DefRepository.CharacterClasses[new CharacterClassID(classID)];
		CharacterID characterID = CharacterID.NewID();
		GameController.StartCreateNewCharacter(new PartySlotID(slot), def, characterID, def.Name);
		GameController.CampaignController.CharacterController.SelectPersonalQuest(characterID, GameController.CampaignController.ValidPersonalQuests().FirstOrDefault(), isRetiring: false);
		GameController.CampaignController.CharacterController.FinishCreateCharacters(characterID);
		return GameState.CampaignState.PartyCharacters.SingleOrDefault((CharacterState x) => x.Name == def.Name);
	}

	public void CreateDefaultParty(int partySize = 2)
	{
		GameState.CampaignState.AllCharacters.Clear();
		GameState.CharacterControls.Clear();
		List<string> list = new List<string>();
		QuickAddCharacterToParty(1u, "BannerSpearID");
		list.Add("BannerSpearID");
		QuickAddCharacterToParty(2u, "BoneshaperID");
		list.Add("BoneshaperID");
		if (partySize > 2)
		{
			QuickAddCharacterToParty(3u, "DrifterID");
			list.Add("DrifterID");
		}
		if (partySize > 3)
		{
			QuickAddCharacterToParty(4u, "DeathwalkerID");
			list.Add("DeathwalkerID");
		}
		if (SceneController.Instance != null)
		{
			IEnumerator enumerator = SceneController.Instance.LoadAbilityCards(list);
			while (enumerator.MoveNext())
			{
			}
		}
		SyncParty();
		StateReplaced = true;
		Singleton<FHMapChoreographer>.InstanceFast.RecalcAttackModifierDecks();
	}

	private void NewStateControllables()
	{
		foreach (FHControllableCharacter controllable in Controllables)
		{
			CharacterState characterState = GameState.CampaignState.AllCharacters.FirstOrDefault((CharacterState c) => c.ClassDef == controllable.Character.ClassDef);
			if (characterState != null)
			{
				controllable.UpdateState(characterState);
			}
		}
	}

	private void SyncParty()
	{
		SyncControllables();
	}

	public void SyncCharacters()
	{
		if (SaveData.Instance == null)
		{
			return;
		}
		foreach (FHControllableCharacter controllable in Controllables)
		{
			controllable.RecreateGHCharacter(GameController.CampaignController.State, GameController.CampaignController);
		}
		FHGlue.UpdateSave();
		SaveData.Instance.Global.CurrentAdventureData.FrosthavenCampaign = GameController.CampaignController.State;
		SaveData.Instance.Global.CurrentAdventureData.FrosthavenSaveData.ScenarioStatsRepo = ScenarioStatRepository.Instance;
		List<CharacterID> list = new List<CharacterID>();
		SaveData.Instance.Global.CurrentAdventureData.FrosthavenSaveData.SlotAssignments = list;
		int i = 0;
		while (i < GameState.CampaignState.Settings.MaxPartySize)
		{
			MultiplayerGameState.Character character = GameState.CharacterControls.FirstOrDefault((MultiplayerGameState.Character t) => t.SlotID.Index == i);
			if (character == null)
			{
				list.Add(null);
			}
			else
			{
				list.Add(character.CharacterID);
			}
			int num = i + 1;
			i = num;
		}
	}

	private void SyncControllables()
	{
		foreach (FHControllableCharacter c in Controllables.ToList())
		{
			NetworkControllable controllable = ControllableRegistry.GetControllable(c.ID);
			if (GameState.CharacterControls.FirstOrDefault((MultiplayerGameState.Character s) => s.CharacterID == c.Character.ID && s.SlotID == controllable.Slot) == null)
			{
				ControllableRegistry.DestroyControllable(controllable);
			}
		}
		foreach (MultiplayerGameState.Character cc in GameState.CharacterControls)
		{
			CharacterState character = GameState.CampaignState.AllCharacters.First((CharacterState characterState) => characterState.ID == cc.CharacterID);
			FHControllableCharacter fHControllableCharacter = Controllables.FirstOrDefault((FHControllableCharacter fHControllableCharacter2) => fHControllableCharacter2.Character == character);
			if (fHControllableCharacter == null)
			{
				fHControllableCharacter = new FHControllableCharacter(character, GameController.CampaignController.State, GameController.CampaignController);
				ControllableRegistry.CreateControllable(fHControllableCharacter.ID, fHControllableCharacter, cc.SlotID);
			}
		}
		SyncCharacters();
		Singleton<FHMapChoreographer>.InstanceFast?.View?.RefreshSelectedCharacter();
	}

	public (MultiplayerCommand, Promise) ExecuteCommand(string name, params object[] parameters)
	{
		if (!ThreadManagerComponent.Manager.IsMainThread)
		{
			throw new InvalidOperationException("MultiplayerClient ExecuteCommand can be executed only on main thread!");
		}
		PendingCommand pendingCommand = new PendingCommand
		{
			Command = MultiplayerCommand.New(PlayerID, name, GameController.DefRepository, parameters),
			PrevState = _serializer.Clone(GameController.GameState)
		};
		MultiplayerGameController.Result result = GameController.ExecuteCommand(pendingCommand.Command);
		if (result != null)
		{
			ProcessResult(result);
		}
		Log.Info($"[MP CLIENT] ({PlayerID}) Local executed command: ({pendingCommand.Command.CommandID}) {pendingCommand.Command.Name}");
		if (FFSNetwork.IsOnline)
		{
			Log.Info($"[MP CLIENT] ({PlayerID}) Pending command: ({pendingCommand.Command.CommandID}) {pendingCommand.Command.Name}");
			_pendingCommands.Add(pendingCommand);
			if (FFSNetwork.IsClient)
			{
				Synchronizer.SendGameAction(GameActionType.FHMultiplayerCommand, ActionPhaseType.NONE, validateOnServerBeforeExecuting: false, disableAutoReplication: true, 0, 0, 0, 0, supplementaryDataBoolean: false, null, new FHMultiplayerCommandToken(pendingCommand.Command));
			}
		}
		return (pendingCommand.Command, pendingCommand.Promise);
	}

	public bool TryGetUnprocessedResult(out MultiplayerGameController.Result r)
	{
		return _commandUnprocessedResults.TryDequeue(out r);
	}

	public bool TryGetFailedCommand(out MultiplayerCommand r)
	{
		return _failedCommands.TryDequeue(out r);
	}

	private void ProcessResult(MultiplayerGameController.Result cmdResult)
	{
		if (cmdResult == null)
		{
			return;
		}
		if (cmdResult.CampaignResult != null)
		{
			if (cmdResult.CampaignResult.HasEvent<CampaignEvent.CharacterAddedToParty>() || cmdResult.CampaignResult.HasEvent<CampaignEvent.CharacterRemovedFromParty>() || cmdResult.CampaignResult.HasEvent<CampaignEvent.CharacterCreationStarted>() || cmdResult.CampaignResult.HasEvent<CampaignEvent.CharacterCreationCancelled>() || cmdResult.CampaignResult.HasEvent<CampaignEvent.CharacterRetired>() || cmdResult.CampaignResult.HasEvent<CampaignEvent.CharacterDeleted>())
			{
				SyncParty();
			}
		}
		else if (cmdResult.Action == MultiplayerGameController.Action.PartySlotChanged)
		{
			SyncParty();
		}
		_commandUnprocessedResults.Enqueue(cmdResult);
	}

	public void EnqueueCommand(MultiplayerCommand command)
	{
		_queuedCommands.Enqueue(command);
		Log.Info($"[MP CLIENT] ({PlayerID}) Enqueued Command: ({command.CommandID}) {command.Name}");
	}

	private void EnqueueFailedCommand(MultiplayerCommand command)
	{
		_failedCommands.Enqueue(command);
		while (_failedCommands.Count() > 100)
		{
			_failedCommands.Dequeue();
		}
	}

	public string DetectNetworkDesync()
	{
		if (Controllables.Count() != ControllableRegistry.AllControllables.Count)
		{
			return "Network contollers are not same amount as controllables";
		}
		if (Controllables.Count() != GameState.CharacterControls.Count)
		{
			return "Game state character controls are not same amount as controllables";
		}
		foreach (FHControllableCharacter controllable in Controllables)
		{
			if (ControllableRegistry.GetControllable(controllable.ID) == null)
			{
				return $"Missing network controller for controllable {controllable.ID}";
			}
			if (GameState.CharacterControls.FirstOrDefault((MultiplayerGameState.Character c) => c.CharacterID == controllable.Character.ID) == null)
			{
				return $"Missing game state character control for controllable {controllable.ID}";
			}
		}
		foreach (NetworkControllable controllable2 in ControllableRegistry.AllControllables)
		{
			if (!Controllables.Any((FHControllableCharacter c) => c.ID == controllable2.ID))
			{
				return $"Missing controllable for network controller {controllable2.ID}";
			}
		}
		foreach (MultiplayerGameState.Player player in GameState.Players)
		{
			if (PlayerRegistry.AllPlayers.FirstOrDefault((NetworkPlayer p) => p.PlayerID == player.ID) == null)
			{
				return $"Missing network player {player.ID}";
			}
		}
		if (PlayerID != PlayerRegistry.MyPlayer.PlayerID)
		{
			return $"Network player id {PlayerRegistry.MyPlayer.PlayerID} is different from state player id {PlayerID}";
		}
		if (GameState.PartyLeaderID != PlayerRegistry.HostPlayer.PlayerID)
		{
			return $"Network party leader id {PlayerRegistry.HostPlayer.PlayerID} is different from state party leader id {GameState.PartyLeaderID?.ID ?? 0}";
		}
		return null;
	}

	public void Update()
	{
		if (!ThreadManagerComponent.Manager.IsMainThread)
		{
			throw new InvalidOperationException("MultiplayerClient Update must be executed only on main thread!");
		}
		while (true)
		{
			if (!_queuedCommands.TryDequeue(out var cmd))
			{
				break;
			}
			if (_pendingCommands.Any())
			{
				PendingCommand pendingCommand = _pendingCommands.FirstOrDefault((PendingCommand c) => c.Command.IsSameAs(cmd));
				if (pendingCommand != null)
				{
					_pendingCommands.Remove(pendingCommand);
					if (cmd.Failed)
					{
						Log.Error($"[MP CLIENT] ({PlayerID}) Command ({cmd.CommandID}) {cmd.Name} failed: {cmd.Error}");
						EnqueueFailedCommand(cmd);
						MultiplayerGameState prevState = pendingCommand.PrevState;
						ReplaceState(prevState);
						foreach (PendingCommand pendingCommand2 in _pendingCommands)
						{
							pendingCommand2.PrevState = _serializer.Clone(GameController.GameState);
							MultiplayerGameController.Result cmdResult = GameController.ExecuteCommand(pendingCommand2.Command);
							ProcessResult(cmdResult);
						}
						pendingCommand.Promise.Failed();
					}
					else
					{
						Log.Info($"[MP CLIENT] ({PlayerID}) Pending command confirmed: ({cmd.CommandID}) {cmd.Name}");
						pendingCommand.Promise.Completed();
					}
				}
				else
				{
					if (cmd.Failed)
					{
						continue;
					}
					Log.Info($"[MP CLIENT] ({PlayerID}) Replaying state for server command: ({cmd.CommandID}) {cmd.Name}");
					MultiplayerGameState prevState2 = _pendingCommands.First().PrevState;
					ReplaceState(prevState2);
					MultiplayerGameController.Result cmdResult2 = GameController.ExecuteCommand(cmd);
					ProcessResult(cmdResult2);
					foreach (PendingCommand pendingCommand3 in _pendingCommands)
					{
						pendingCommand3.PrevState = _serializer.Clone(GameController.GameState);
						try
						{
							cmdResult2 = GameController.ExecuteCommand(pendingCommand3.Command);
							ProcessResult(cmdResult2);
						}
						catch (Exception ex)
						{
							pendingCommand3.Command.Error = ex.InnerException?.Message ?? ex.Message;
							if (ex.InnerException is FHGameException)
							{
								Log.Warning($"[MP CLIENT] ({PlayerID}) Command ({cmd.CommandID}) {pendingCommand3.Command.Name} failed: {pendingCommand3.Command.Error}");
							}
							else
							{
								Log.Error($"[MP CLIENT] ({PlayerID}) Command ({cmd.CommandID}) {pendingCommand3.Command.Name} failed: {pendingCommand3.Command.Error}");
							}
							int index = _pendingCommands.IndexOf(pendingCommand3);
							_pendingCommands.RemoveAll((PendingCommand p) => _pendingCommands.IndexOf(p) >= index);
							pendingCommand3.Promise.Failed();
							break;
						}
					}
				}
			}
			else if (!cmd.Failed)
			{
				Log.Info($"[MP CLIENT] ({PlayerID}) Server command: ({cmd.CommandID}) {cmd.Name}");
				MultiplayerGameController.Result cmdResult3 = GameController.ExecuteCommand(cmd);
				ProcessResult(cmdResult3);
			}
		}
	}

	public void UpdateCleanup()
	{
		StateReplaced = false;
	}
}
