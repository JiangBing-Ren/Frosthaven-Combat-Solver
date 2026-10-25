using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using FFSNet;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.EventChannels;
using FHGUI.Modals;
using FHGUI.Modules;
using FHGUI.Services;
using FHGUI.States;
using FHRL;
using GLOOM;
using I2.Loc;
using Libs.GameConsole;
using Libs.Utils.Maths;
using ScenarioRuleLibrary;
using SharedLibrary.Achievments;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace FHGUI;

public class FHMapView : MonoBehaviour
{
	private struct ShowingUnresolvedAction(CampaignUnresolvedAction action, FHMapViewLayer.LayerType layerType)
	{
		public CampaignUnresolvedAction Action = action;

		public FHMapViewLayer.LayerType LayerType = layerType;
	}

	private enum TransitionType
	{
		None,
		OnCategoryChange,
		OnEveryState
	}

	public UIBuildingTooltipActionEventChannel BuildingTooltipActionEventChannel;

	public OutpostSceneController OutpostSceneController;

	private bool m_PlayersJoining;

	public ModulesCatalogue Modules;

	public ModalsCatalogue Modals;

	private readonly Dictionary<Type, FHMapViewService> _services = new Dictionary<Type, FHMapViewService>();

	private readonly Dictionary<FHMapViewLayer.LayerType, FHMapViewLayer> _layers = new Dictionary<FHMapViewLayer.LayerType, FHMapViewLayer>
	{
		{
			FHMapViewLayer.LayerType.Base,
			new FHMapViewLayer(FHMapViewLayer.LayerType.Base)
		},
		{
			FHMapViewLayer.LayerType.Modal,
			new FHMapViewLayer(FHMapViewLayer.LayerType.Modal)
		}
	};

	private readonly Queue<Action> _afterAllUnresolvedActionsExec = new Queue<Action>();

	private readonly List<CampaignEvent.CampaignSectionActivated> _remainingSections = new List<CampaignEvent.CampaignSectionActivated>();

	private readonly Dictionary<IUIViewModule, object> _persistantModulePayloadObjects = new Dictionary<IUIViewModule, object>();

	private ShowingUnresolvedAction _showingUnresolvedAction;

	private readonly List<CampaignEvent.CharacterClassUnlocked> _remainingUnlockCharacterEvents = new List<CampaignEvent.CharacterClassUnlocked>();

	private bool _showCredits;

	private bool _hasShownAttackOptionOutcome;

	private bool _hasTravelStateChanged;

	private bool _needsRefreshStartScenario;

	private CampaignEvent.ScenarioCompleted PendingScenarioCompletionEvent;

	public Action<FHMapViewState> OnStateExit;

	public Action<FHMapViewState> OnStateEnter;

	private bool _queryRefreshView;

	[ConsoleVariable(Alias = "dbg_show_debriefing_info", Persistent = true)]
	public static bool ShowDebugDebriefingInfo;

	[Header("State Transition Settings")]
	[SerializeField]
	private CanvasGroup _transitionCanvasGroup;

	[SerializeField]
	[Range(0f, 1f)]
	private float _transitionSeconds = 0.3f;

	[SerializeField]
	private TransitionType _transitionType;

	private Tween _transitionTween;

	private bool _isOnline;

	public FHMapChoreographer Choreographer => Singleton<FHMapChoreographer>.InstanceFast;

	public CampaignController GameController => Choreographer.GameController;

	public MultiplayerGameController MultiplayerGameController => Choreographer.GameClient.GameController;

	public CampaignState GameState => GameController.State;

	public MultiplayerGameState MultiplayerGameState => MultiplayerGameController.GameState;

	public bool IsPartyLeader => Choreographer.GameClient.IsPartyLeader;

	public bool IsMultiplayerEnabled => Choreographer.IsMultiplayerEnabled;

	public PlayerID PlayerID => Choreographer.GameClient.PlayerID;

	public MultiplayerGameState.Player Player => MultiplayerGameState.Players.FirstOrDefault((MultiplayerGameState.Player p) => p.ID == PlayerID);

	public IEnumerable<MultiplayerGameController.PartySlotDesc> PlayerSlots => MultiplayerGameController.GetOwnedSlots(PlayerID);

	public IEnumerable<CharacterState> PlayerCharacters => from psd in PlayerSlots
		where psd.Character != null
		select psd.Character;

	public bool IsOnline => FFSNetwork.IsOnline;

	public bool IsSoloGame
	{
		get
		{
			if (IsOnline)
			{
				return MultiplayerGameState.Players.Count < 2;
			}
			return true;
		}
	}

	public bool IsReadyCheckRunning
	{
		get
		{
			if (IsOnline)
			{
				return !MultiplayerGameState.ReadyCheckID.IsNullOrEmpty();
			}
			return false;
		}
	}

	private bool PlayersJoining
	{
		get
		{
			return m_PlayersJoining;
		}
		set
		{
			if (m_PlayersJoining != value)
			{
				m_PlayersJoining = value;
				RefreshView();
			}
		}
	}

	public CharacterState SelectedCharacter { get; private set; }

	public int SelectedPartySlotId { get; private set; }

	public ScenarioDef SelectedScenarioDef { get; private set; }

	public List<IUIViewModule> AllModules { get; private set; }

	public int StateStackLength => _layers[FHMapViewLayer.LayerType.Base].GetStates().Count();

	public FHMapViewState CurrentState => _layers[FHMapViewLayer.LayerType.Base].State;

	public FHMapViewState CurrentModalState => _layers[FHMapViewLayer.LayerType.Modal].State;

	public bool HasInit { get; private set; }

	public bool InTransition
	{
		get
		{
			if (_transitionTween != null)
			{
				return _transitionTween.active;
			}
			return false;
		}
	}

	private void Awake()
	{
		AllModules = GetComponentsInChildren<IUIViewModule>(includeInactive: true).ToList();
		AllModules.ForEach((IUIViewModule m) =>
		{
			m.GameObject.SetActive(value: false);
		});
		InitServices();
		WireEventChannels();
		enabled = false;
	}

	private void OnDestroy()
	{
		if (Singleton<UINotificationManager>.IsInitialized)
		{
			UIMultiplayerNotifications.HideAllMultiplayerNotification(instant: true);
		}
		foreach (KeyValuePair<Type, FHMapViewService> service in _services)
		{
			service.Value.OnDestroy();
		}
	}

	public bool ShouldAllowPlayersToJoin()
	{
		CampaignState gameState = GameState;
		if (gameState == null || gameState.Outpost?.Phase != OutpostPhase.Downtime)
		{
			return false;
		}
		if (_afterAllUnresolvedActionsExec.Count > 0)
		{
			return false;
		}
		if (SceneController.Instance.IsLoading)
		{
			return false;
		}
		if (GameController.PhaseController.CanMoveToNextPhase() && IsPartyLeader && (GameState.Outpost.Phase != OutpostPhase.Downtime || GameState.CompletedDowntimeScenario))
		{
			return false;
		}
		if (GameState.PartyTravelState != CampaignState.PartyTravelStateType.None && GameState.PartyTravelState != CampaignState.PartyTravelStateType.WaitingInOutpost && GameState.PartyTravelState != CampaignState.PartyTravelStateType.WaitingInScenario)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(MultiplayerGameState.ReadyCheckID))
		{
			return false;
		}
		if (CurrentModalState != null)
		{
			return false;
		}
		if (!ShouldAllowPlayersToJoinInCurrentState())
		{
			return false;
		}
		return true;
	}

	private bool ShouldAllowPlayersToJoinInCurrentState()
	{
		FHMapViewState currentState = CurrentState;
		if (currentState != null && !(currentState is EventViewState) && !(currentState is NarrationViewState) && !(currentState is LinkingSectionViewState) && !(currentState is OutpostAttackViewState) && !(currentState is GenericListModalViewState) && !(currentState is GenericModalViewState) && !(currentState is ImportFromPartyViewState) && !(currentState is InitViewState) && !(currentState is LinkedScenarioChoiceViewState))
		{
			if (currentState is TravellingMerchantViewState || currentState is ShopViewState || currentState is AlchemistViewState || currentState is EnhancerViewState || currentState is TownPerksViewState || currentState is CraftingViewState || currentState is EditCharacterViewState || currentState is UnlockCardsViewState || currentState is CharacterPerksViewState || currentState is ManageRosterViewState || currentState is HandManagementViewState || currentState is ScenarioSelectionViewState || currentState is OutpostVillageMapState || currentState is BattleGoalSelectionViewState || currentState is ChallengeSelectionViewState || currentState is PetsSelectionViewState || currentState is FavorsSelectionViewState || currentState is ScenarioBriefingSummaryViewState || currentState is DistributionViewState || currentState is BuildingMaterialDistributionViewState || currentState is CharacterEquipmentViewState || currentState is ProloguePartyCreationViewState || currentState is RenameCharacterViewState || currentState is PersonalQuestSelectionViewState || currentState is SpendStartingGoldViewState)
			{
				return true;
			}
			Log.GUIWarning($"View State {CurrentState} is not listed. Please include desired behaviors");
			return false;
		}
		return false;
	}

	public bool IsActionQueuedAfterUnresolvedActions(Action action)
	{
		return _afterAllUnresolvedActionsExec.Contains(action);
	}

	private void SetShownUnresolvedAction(CampaignUnresolvedAction action, FHMapViewState state)
	{
		SetShownUnresolvedAction(action, state.TargetLayerType);
	}

	private void SetShownUnresolvedAction(CampaignUnresolvedAction action, FHMapViewLayer.LayerType layerType)
	{
		_showingUnresolvedAction = new ShowingUnresolvedAction(action, layerType);
	}

	private void ResetShownUnresolvedAction()
	{
		_showingUnresolvedAction = default;
	}

	public void ResetView()
	{
		foreach (FHMapViewLayer value in _layers.Values)
		{
			value.Clear();
		}
		ResetShownUnresolvedAction();
		DecideViewStateFromGameState();
	}

	public void DecideViewStateFromGameState()
	{
		if (!HasInit)
		{
			Log.Warning("Entering InitViewState when the UI is not ready");
			return;
		}
		IEnumerable<FHRL.ScenarioState> linkedScenarios = GameController.ScenarioController.GetLinkedScenarios();
		if (linkedScenarios != null && linkedScenarios.Any((FHRL.ScenarioState s) => !s.Mandatory) && GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingInScenario && !GameState.HasUnresolvedActions())
		{
			ReplaceState(new LinkedScenarioChoiceViewState());
			return;
		}
		if (GameState.ActiveCharacters.Count() < GameController.Settings.MinPartySize)
		{
			if (GameState.IsInPrologue)
			{
				SetState(new ProloguePartyCreationViewState());
				return;
			}
			if (GameState.GameMode.CanPlayWithoutCharacters)
			{
				FHRL.ScenarioState scenarioState = GameState.UnlockedScenarios.FirstOrDefault();
				if (scenarioState != null && GameState.SelectedScenario == null)
				{
					Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.StartScenario, scenarioState.Def);
					Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.EnterActiveScenario);
					return;
				}
			}
		}
		if (GameState.ActiveScenario != null && GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingInScenario)
		{
			GetService<ScenarioViewService>().SetupSelectedScenario();
			return;
		}
		if (GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingInOutpost)
		{
			SetState(new OutpostVillageMapState());
		}
		else
		{
			SetState(new WorldViewState());
			_hasTravelStateChanged = true;
		}
		TryResumeTravels();
	}

	private void EnsureCurrentStateExists()
	{
		if (!_layers[FHMapViewLayer.LayerType.Base].HasState)
		{
			DecideViewStateFromGameState();
			_queryRefreshView = true;
		}
	}

	public void RefreshView(bool immediate = false)
	{
		if (immediate)
		{
			foreach (FHMapViewLayer value in _layers.Values)
			{
				if (value.TryRefresh() && value.Type == FHMapViewLayer.LayerType.Base)
				{
					OnStateEnter?.Invoke(value.State);
				}
			}
			_queryRefreshView = false;
		}
		else
		{
			_queryRefreshView = true;
		}
		RefreshMultiplayerNotifications();
	}

	public void InitUI()
	{
		if (HasInit)
		{
			return;
		}
		Modules.WorldMap.WorldMap.SetSceneRootReference(OutpostSceneController.WorldMapScene);
		Modules.OutpostMap.OutpostOverviewModule.SetSceneRootReference(OutpostSceneController.OutpostMapScene);
		HasInit = true;
		OnStateEnter = (Action<FHMapViewState>)Delegate.Combine(OnStateEnter, new Action<FHMapViewState>(UIContextualTutorialModule.OnFHMapViewStateEnter));
		enabled = true;
		CardViewService service = GetService<CardViewService>();
		foreach (KeyValuePair<CharacterClassID, CharacterClassDef> characterClass in Choreographer.DefRepo.CharacterClasses)
		{
			service.UpdateCardEnhancements(characterClass.Value);
		}
		GetService<OutpostNotificationViewService>().Init();
		foreach (CharacterClassDef unlockedClass in GameController.CharacterController.UnlockedClasses)
		{
			FHUICodex.UnlockCodexTag(FHUICodexUtils.CodexHeroIDToUnlockTag(unlockedClass.ID));
		}
		foreach (BuildingTypeID availableBuildingToConstructID in GameController.State.Outpost.AvailableBuildingToConstructIDs)
		{
			FHUICodex.UnlockCodexTag(availableBuildingToConstructID);
		}
		foreach (BuildingState building in GameController.State.Outpost.Buildings)
		{
			FHUICodex.UnlockCodexTag(building.ID);
		}
		foreach (ItemDef allItem in GameController.State.AllItems)
		{
			FHUICodex.UnlockCodexTag(allItem.ID);
		}
	}

	public void ShowLoadingScreen()
	{
		if (Choreographer.IsStandaloneScene)
		{
			Modules.EmptyModule.gameObject.SetActive(value: true);
		}
		else
		{
			SceneController.Instance.ShowLoadingScreen();
		}
	}

	public void HideLoadingScreen()
	{
		if (Choreographer.IsStandaloneScene)
		{
			Modules.EmptyModule.gameObject.SetActive(value: false);
		}
		else
		{
			SceneController.Instance.DisableLoadingScreen();
		}
	}

	public void RefreshSelectedCharacter()
	{
		if (SelectedCharacter != null)
		{
			MultiplayerGameController.PartySlotDesc partySlotDesc = MultiplayerGameController.GetSlots().FirstOrDefault((MultiplayerGameController.PartySlotDesc s) => s.Character?.ID == SelectedCharacter.ID);
			if (partySlotDesc.IsValid())
			{
				SetSelectedCharacter(partySlotDesc.Character, (int)partySlotDesc.ID.ID);
			}
			else
			{
				SetSelectedCharacter(null, -1);
			}
		}
	}

	public void SetSelectedCharacter(CharacterState character, int slotId)
	{
		if (slotId < 0 && character != null)
		{
			MultiplayerGameController.PartySlotDesc partySlotDesc = MultiplayerGameController.GetSlots().FirstOrDefault((MultiplayerGameController.PartySlotDesc s) => s.Character == character);
			if (partySlotDesc.Character == character)
			{
				slotId = (int)partySlotDesc.ID.ID;
			}
		}
		if (SelectedCharacter != character)
		{
			SelectedCharacter = character;
			SelectedPartySlotId = slotId;
			RefreshView();
		}
	}

	public bool TryEnsureSelectedCharacter()
	{
		if (SelectedCharacter == null || !PlayerCharacters.Contains(SelectedCharacter))
		{
			CharacterState character = PlayerCharacters.FirstOrDefault();
			SetSelectedCharacter(character, -1);
		}
		return SelectedCharacter != null;
	}

	public void SetSelectedScenario(int scenarioId)
	{
		if (scenarioId < 0)
		{
			SelectedScenarioDef = null;
			return;
		}
		ScenarioID key = new ScenarioID((uint)scenarioId);
		SelectedScenarioDef = Choreographer.DefRepo.Scenarios[key];
		_queryRefreshView = true;
	}

	public void SetPersistantModulePayload(IUIViewModule module, object payload)
	{
		_persistantModulePayloadObjects[module] = payload;
	}

	public bool TryGetPersistantModulePayload(IUIViewModule module, out object value)
	{
		return _persistantModulePayloadObjects.TryGetValue(module, out value);
	}

	public bool TryGetPersistantModulePayload<T>(IUIViewModule module, out T value) where T : class
	{
		if (_persistantModulePayloadObjects.TryGetValue(module, out var value2))
		{
			value = value2 as T;
			return true;
		}
		value = null;
		return false;
	}

	public void ClearPersistantModulePayload(IUIViewModule module)
	{
		if (_persistantModulePayloadObjects.ContainsKey(module))
		{
			_persistantModulePayloadObjects.Remove(module);
		}
	}

	public void ExecAfterNoUnresolvedActions(Action action)
	{
		_afterAllUnresolvedActionsExec.Enqueue(action);
	}

	public void ProcessGameResult(MultiplayerGameController.Result result)
	{
		bool flag = false;
		if (result.CampaignResult != null)
		{
			if (result.CampaignResult.Events.OfType<CampaignEvent.WeekPassed>().FirstOrDefault() != null)
			{
				ShowWeekPassedScreen();
			}
			CampaignEvent.OutpostEventIterated outpostEventIterated = result.CampaignResult.Events.OfType<CampaignEvent.OutpostEventIterated>().FirstOrDefault();
			if (outpostEventIterated != null)
			{
				if (GameController.State.HasUnresolvedAttack())
				{
					List<CampaignUnresolvedAction> list = GameController.State.UnresolvedActionsOutsideAttack().ToList();
					if (list.Count > 0)
					{
						if (CurrentState is EventViewState)
						{
							PopState(CurrentState.TargetLayerType);
						}
						ShowResolveAction(list[0]);
					}
				}
				else
				{
					List<CampaignUnresolvedAction> unresolvedActions = GameController.State.UnresolvedActions;
					if (unresolvedActions.Count > 0)
					{
						if (CurrentState is EventViewState)
						{
							PopState(CurrentState.TargetLayerType);
						}
						ShowResolveAction(unresolvedActions[0]);
					}
				}
			}
			foreach (CampaignEvent.CharacterClassUnlocked item in result.CampaignResult.Events.OfType<CampaignEvent.CharacterClassUnlocked>())
			{
				string value = $"CODEX_Unlock_{item.Class.ID}";
				if (!GlobalStatRepository.Instance.UnlockedCodexTags.HasValue(value))
				{
					GlobalStatRepository.Instance.UnlockedCodexTags.NewValue(value);
				}
				_remainingUnlockCharacterEvents.Add(item);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.TravelStateChanged>() || _hasTravelStateChanged)
			{
				UpdateTravel();
			}
			CampaignEvent.OutpostEventResolved outpostEventResolved = result.CampaignResult.Events.OfType<CampaignEvent.OutpostEventResolved>().FirstOrDefault();
			if (outpostEventResolved != null)
			{
				ShowOutpostEventResolved(outpostEventResolved);
			}
			else if (outpostEventIterated != null && outpostEventIterated.NeedsNewOptionSelection)
			{
				flag = true;
				TryPushActiveEvent(forcePush: true);
			}
			CampaignEvent.ScenarioCompleted scenarioCompleted = result.CampaignResult.Events.OfType<CampaignEvent.ScenarioCompleted>().FirstOrDefault();
			if (scenarioCompleted != null)
			{
				ShowScenarioCompleted(scenarioCompleted);
			}
			CampaignEvent.NewBrewableRecipieDiscovered newBrewableRecipieDiscovered = result.CampaignResult.Events.OfType<CampaignEvent.NewBrewableRecipieDiscovered>().FirstOrDefault();
			if (newBrewableRecipieDiscovered != null)
			{
				ShowNewRecipieDiscovered(newBrewableRecipieDiscovered);
			}
			CampaignEvent.CharacterStartRetireProcess characterStartRetireProcess = result.CampaignResult.Events.OfType<CampaignEvent.CharacterStartRetireProcess>().FirstOrDefault((CampaignEvent.CharacterStartRetireProcess process) => result.PlayerID == PlayerID);
			if (characterStartRetireProcess != null)
			{
				PushState(new HeroRetirementView(characterStartRetireProcess.Character), FHMapViewLayer.LayerType.Modal);
			}
			CampaignEvent.CharacterRetired characterRetired = result.CampaignResult.Events.OfType<CampaignEvent.CharacterRetired>().FirstOrDefault((CampaignEvent.CharacterRetired process) => result.PlayerID == PlayerID);
			if (characterRetired != null)
			{
				if (SaveData.Instance.Global?.CurrentAdventureData != null)
				{
					AnalyticsWrapper.LogCharacterRetired(SaveData.Instance.Global.CurrentAdventureData, characterRetired.Character);
				}
				PushState(new RetirementStoryViewState(characterRetired.Character.ClassDef, characterRetired.AppliedEffects));
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.CampaignSectionQueued>())
			{
				ProcessPendingSections();
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.PropsperityLevelChanged>())
			{
				foreach (CampaignEvent.PropsperityLevelChanged item2 in result.CampaignResult.Events.OfType<CampaignEvent.PropsperityLevelChanged>())
				{
					GameObject gameObject = Singleton<FHMapChoreographer>.InstanceFast?.View?.Modules.MainHeaderModule.TownIconGO;
					UIContextualTutorialModule.OnEvent("ProsperityLevelChanged", item2.Current, item2.Prev, gameObject);
				}
			}
			foreach (CampaignEvent.CampaignSectionActivated item3 in result.CampaignResult.Events.OfType<CampaignEvent.CampaignSectionActivated>())
			{
				_remainingSections.Add(item3);
				UIContextualTutorialModule.OnEvent("ActivatedCampaignSection", item3.Event.ID.ID);
			}
			GetService<QuickAccessBarViewService>()?.ProcessCampaignEvents(result.CampaignResult.Events);
			GetService<OutpostNotificationViewService>().ProcessGameResult(result);
			if (result.CampaignResult.HasEvent<CampaignEvent.EnterScenario>())
			{
				GetService<BattleBriefingViewService>()?.ClearUITags();
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.OutpostEventResolved>() || result.CampaignResult.HasEvent<CampaignEvent.OutpostEventIterated>())
			{
				GetService<MultiplayerSyncViewService>()?.ClearSyncPoint();
				GetService<OutpostEventsService>()?.ClearUITags();
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.BuildingFinishDonation>())
			{
				GetService<BuildingViewService>()?.HideDonationModal();
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.StartScenario>())
			{
				GetService<ScenarioViewService>().ProcessStartScenarioCommand(result.CampaignResult);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.NextCampaignSectionSelected>())
			{
				PopState(_showingUnresolvedAction.LayerType);
				ResetShownUnresolvedAction();
				flag = true;
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.ResolveCampaignAction>())
			{
				CampaignEvent.ResolveCampaignAction resolveCampaignAction = result.CampaignResult.Events.OfType<CampaignEvent.ResolveCampaignAction>().FirstOrDefault();
				if (resolveCampaignAction != null && resolveCampaignAction.ActionType == _showingUnresolvedAction.Action.Type)
				{
					PopState(_showingUnresolvedAction.LayerType);
				}
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.AlternativeScenarioLevelSelected>())
			{
				_afterAllUnresolvedActionsExec.Clear();
				PopState(_showingUnresolvedAction.LayerType);
				ResetShownUnresolvedAction();
				ReplaceState(new NarrationViewState(NarrationSetting.FromAlternativeScenarioLevelChosen(GameState.SelectedScenario, GameController))
				{
					OnDone = () =>
					{
						GetService<BattleBriefingViewService>().EnterActiveScenario();
					}
				});
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.BuildingUnlocked>())
			{
				FHUICodex.UnlockCodexTag(result.CampaignResult.Events.OfType<CampaignEvent.BuildingUnlocked>().FirstOrDefault().Building);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.CharacterGainItem>())
			{
				FHUICodex.UnlockCodexTag(result.CampaignResult.Events.OfType<CampaignEvent.CharacterGainItem>().FirstOrDefault().Item.ID);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.NewBrewableRecipieDiscovered>())
			{
				FHUICodex.UnlockCodexTag(result.CampaignResult.Events.OfType<CampaignEvent.NewBrewableRecipieDiscovered>().FirstOrDefault().Item.ID);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.NewItemAvailabeForPurchase>())
			{
				FHUICodex.UnlockCodexTag(result.CampaignResult.Events.OfType<CampaignEvent.NewItemAvailabeForPurchase>().FirstOrDefault().Item.ID);
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.NewItemAvailabeForCrafting>())
			{
				FHUICodex.UnlockCodexTag(result.CampaignResult.Events.OfType<CampaignEvent.NewItemAvailabeForCrafting>().FirstOrDefault().Item.ID);
			}
			if (ReleaseStage.IsDemo())
			{
				CampaignEvent.PhaseEnd phaseEnd = result.CampaignResult.Events.LastOrDefault((CampaignEvent x) => x is CampaignEvent.PhaseEnd) as CampaignEvent.PhaseEnd;
				if (phaseEnd != null && (phaseEnd.Phase == OutpostPhase.Event || (phaseEnd.Phase == OutpostPhase.TimePassage && GameState.Outpost.SkipNextEvent)) && GetService<OutpostNotificationViewService>().IsDemoCompleted())
				{
					PushState(new DemoMarketingPopupViewState(), FHMapViewLayer.LayerType.Modal);
				}
			}
			if (result.CampaignResult.HasEvent<CampaignEvent.ShowCredits>())
			{
				_showCredits = true;
			}
		}
		MultiplayerGameController.Action action = result.Action;
		if (action == MultiplayerGameController.Action.ReadyCanceled || action == MultiplayerGameController.Action.PlayerReadyChanged || action == MultiplayerGameController.Action.AllPlayersReady)
		{
			ProcessReadyCheck(result);
		}
		_queryRefreshView = true;
		if (flag)
		{
			TickUpdate();
		}
	}

	private void ProcessReadyCheck(MultiplayerGameController.Result result)
	{
		string voteCommandId = result.ReadyCheckID;
		IReadyCheckProcessor readyCheckProcessor = _services.Values.OfType<IReadyCheckProcessor>().FirstOrDefault((IReadyCheckProcessor processor) => processor.IsApplicableReadyCheckCommand(voteCommandId));
		MultiplayerGameController.Result result2 = ((result.ReadyCheckID != null) ? result : null);
		if (result2 != null)
		{
			readyCheckProcessor?.ProcessReadyCheckResult(result2);
		}
	}

	private FHMapViewLayer GetViewLayer(FHMapViewLayer.LayerType layerType)
	{
		if (!_layers.TryGetValue(layerType, out var value))
		{
			Log.GUIError($"{typeof(FHMapViewLayer)} of type {layerType} not found!");
		}
		return value;
	}

	public void PushState(FHMapViewState state, FHMapViewLayer.LayerType layerType)
	{
		state.TargetLayerType = layerType;
		PushState(state);
	}

	public void PushState(FHMapViewState state, bool avoidDuplicates = false)
	{
		GetViewLayer(state.TargetLayerType).PushState(state, avoidDuplicates);
	}

	public void PopState(FHMapViewLayer.LayerType layerType)
	{
		GetViewLayer(layerType).PopState();
	}

	public bool TryPopState(FHMapViewState state)
	{
		FHMapViewLayer viewLayer = GetViewLayer(state.TargetLayerType);
		if (viewLayer?.State != state)
		{
			return false;
		}
		viewLayer.PopState();
		return true;
	}

	public void SetState(FHMapViewState state)
	{
		Log.GUITrace($"Layer [{state.TargetLayerType}]: Set State {state} at Frame {Time.frameCount}");
		GetViewLayer(state.TargetLayerType).SetState(state);
	}

	public void ReplaceState(FHMapViewState state)
	{
		GetViewLayer(state.TargetLayerType).ReplaceState(state);
	}

	public void ReplaceState(FHMapViewState state, FHMapViewLayer.LayerType layer)
	{
		state.TargetLayerType = layer;
		ReplaceState(state);
	}

	public void ShowResolveChooseAlternativeLevel(CampaignUnresolvedAction action, Action onResolved)
	{
		if (_showingUnresolvedAction.Action.Type != action.Type && action.Type == CampaignInteractionType.ChooseAlternativeScenarioLevel)
		{
			string text = action.GetString(0);
			string questionKey = "Scenario_" + text + "_AlternativeLevelChoiceQuestion";
			string answer1Key = "Scenario_" + text + "_AlternativeLevelOption1";
			string answer2Key = "Scenario_" + text + "_AlternativeLevelOption2";
			string illustrationKey = GameController.DefRepository.Scenarios[new ScenarioID(action.GetUInt(0))].LevelID + "_Choice";
			NarrationViewState state = new NarrationViewState(NarrationSetting.FromResolveChooseAlternativeLevel(questionKey, answer1Key, answer2Key, illustrationKey, Choreographer));
			SetShownUnresolvedAction(action, state);
			ReplaceState(state);
		}
	}

	public void ShowResolveChooseUnlockMandatoryScenario(CampaignUnresolvedAction action)
	{
		if (_showingUnresolvedAction.Action.Type != action.Type && action.Type == CampaignInteractionType.ChooseUnlockMandatoryScenario)
		{
			string text = action.GetString(0);
			int scenario = action.GetInt(1);
			int scenario2 = action.GetInt(2);
			string answer1Key = text + "_Option1";
			string answer2Key = text + "_Option2";
			string illustrationKey = GameController.DefRepository.Scenarios[PendingScenarioCompletionEvent.ScenarioID].LevelID + "_Choice";
			NarrationViewState state = new NarrationViewState(NarrationSetting.FromResolveChooseUnlockMandatoryScenario(text, answer1Key, answer2Key, illustrationKey, scenario, scenario2, Choreographer));
			SetShownUnresolvedAction(action, state);
			ReplaceState(state);
		}
	}

	public void ShowResolveAction(CampaignUnresolvedAction action)
	{
		if (GameController.State.BlockShowUnresolvedActions)
		{
			return;
		}
		if (action.Type == CampaignInteractionType.AttackEvent)
		{
			List<CampaignUnresolvedAction> list = GameState.UnresolvedActions.Where((CampaignUnresolvedAction x) => x.ShowDuringOutposAttack).ToList();
			if (list.Count > 0)
			{
				ShowResolveAction(list[0]);
				return;
			}
		}
		if (_showingUnresolvedAction.Action.Equals(action) || action.Type == CampaignInteractionType.ChooseAlternativeScenarioLevel)
		{
			return;
		}
		FHMapViewLayer.LayerType targetLayer = FHMapViewLayer.LayerType.Base;
		SetTargetLayer(targetLayer);
		if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.AttackEvent)
		{
			if (!_hasShownAttackOptionOutcome && _showingUnresolvedAction.Action.Parameter1 != "SkipOutcomeText")
			{
				List<CampaignUnresolvedAction> list2 = GameState.UnresolvedActionsOnAttackStart().ToList();
				if (list2.Count > 0)
				{
					list2.ForEach((CampaignUnresolvedAction x) =>
					{
						x.OnAttackStart = false;
					});
				}
				EventViewState state = new EventViewState(new CampaignEvent.OutpostEventResolved
				{
					Event = GameState.Outpost.ActiveEvent,
					Option = new OutpostEventDef.Option
					{
						ID = GameState.Outpost.ActiveEvent.OptionSelectedID
					}
				})
				{
					OnDone = () =>
					{
						_hasShownAttackOptionOutcome = true;
						if (IsPartyLeader)
						{
							Choreographer.ExecuteCommand(FHRPC.Campaign.Attack.StartAttack);
						}
						ResetShownUnresolvedAction();
						PopState(targetLayer);
					}
				};
				if (_layers[targetLayer].State is EventViewState)
				{
					ReplaceState(state, targetLayer);
				}
				else
				{
					PushState(state, targetLayer);
				}
			}
			else if (!(CurrentState is OutpostAttackViewState))
			{
				PushState(new OutpostAttackViewState
				{
					OnAttackResolved = () =>
					{
						PopState(targetLayer);
					}
				}, targetLayer);
			}
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.LoseGold)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetGoldDistribution", GameController.OutpostController.GetGoldMustLose(), DistributionChangeType.Lose);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Gold = GameController.OutpostController.GetGoldMustLose(),
				ChangeType = DistributionChangeType.Lose
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.GainGold)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetGoldDistribution", GameController.OutpostController.GetGoldToGain(), DistributionChangeType.Gain);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Gold = GameController.OutpostController.GetGoldToGain(),
				ChangeType = DistributionChangeType.Gain
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.LoseMaterials)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetMaterialDistribution", GameController.OutpostController.GetMaterialsMustLose(), DistributionChangeType.Lose);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Resources = GameController.OutpostController.GetMaterialsMustLose(),
				ChangeType = DistributionChangeType.Lose
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.LoseAnyHerbs)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetHerbDistribution", GameController.OutpostController.GetHerbsMustLose(), DistributionChangeType.Lose);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Resources = GameController.OutpostController.GetHerbsMustLose(),
				ChangeType = DistributionChangeType.Lose
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.Event_LoseMaterialsEach)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetMaterialDistribution", GameController.OutpostEventController.GetMaterialFor_LoseMaterialsEach(), DistributionChangeType.LoseEach);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Resources = GameController.OutpostEventController.GetMaterialFor_LoseMaterialsEach(),
				ChangeType = DistributionChangeType.LoseEach
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.GainMaterials)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetMaterialDistribution", GameController.OutpostController.GetMaterialsMustGain(), DistributionChangeType.Gain);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Resources = GameController.OutpostController.GetMaterialsMustGain(),
				ChangeType = DistributionChangeType.Gain
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.GainAnyHerbs)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetHerbDistribution", GameController.OutpostController.GetHerbsMustGain(), DistributionChangeType.Gain);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Resources = GameController.OutpostController.GetHerbsMustGain(),
				ChangeType = DistributionChangeType.Gain
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.GainItem)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetItemDistribution", GameController.OutpostController.GetItemsMustGain(), DistributionChangeType.Gain, ItemFilterFlags.None);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Items = GameController.OutpostController.GetItemsMustGain().ToList(),
				ChangeType = DistributionChangeType.Gain
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.Event_GainOneItemForParty)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetItemDistribution", GameController.OutpostController.GetItemsMustGain(), DistributionChangeType.GainOnePerParty, ItemFilterFlags.None);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Items = GameController.OutpostController.GetItemsMustGain().ToList(),
				ChangeType = DistributionChangeType.GainOnePerParty
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.Event_BuyItemsDiscountOptional)
		{
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetItemDistribution", GameController.TradingController.GetItemsAvailableForBuying(), DistributionChangeType.BuyAtDiscountOptional, ItemFilterFlags.None);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				Items = GameController.TradingController.GetItemsAvailableForBuying().ToList(),
				ChangeType = DistributionChangeType.BuyAtDiscountOptional
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.LoseItem)
		{
			ItemFilterFlags itemFilterFlags = ItemFilterFlags.None;
			if (_showingUnresolvedAction.Action.Parameter1 != null)
			{
				itemFilterFlags = (ItemFilterFlags)_showingUnresolvedAction.Action.GetInt(0);
			}
			if (IsPartyLeader)
			{
				Choreographer.ExecuteCommand("ResetItemDistribution", new List<ItemDef> { null }, DistributionChangeType.Lose, itemFilterFlags);
			}
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new DistributionViewState(new DistributionChangeDTO
			{
				LoseItem = 1,
				ChangeType = DistributionChangeType.Lose,
				ItemFilter = itemFilterFlags
			})
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.SelectClassUnlock)
		{
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			List<GenericListModalViewState.ListItem> items = (from c in GameController.CharacterController.GetClassesToChooseFrom()
				select new GenericListModalViewState.ListItem
				{
					Enabled = IsPartyLeader,
					Item = c,
					Text = GLOOM.LocalizationManager.GetTranslation(c.ID.ID.Replace("ID", "") + "_FullName")
				}).ToList();
			GenericListModalViewState state2 = new GenericListModalViewState("Choose class to unlock", items, (GenericListModalViewState.ListItem item) =>
			{
				CharacterClassDef characterClassDef = (CharacterClassDef)item.Item;
				Choreographer.ExecuteCommand(FHRPC.Campaign.Character.ResolveChoseClass, characterClassDef);
			});
			PushState(state2, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.ChooseBuilding)
		{
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			OutpostEventsService service = GetService<OutpostEventsService>();
			List<GenericListModalViewState.ListItem> items2 = (from id in GameController.State.BuildingsToSelectIDs
				select GameController.DefRepository.Buildings[id] into buildingDef
				where !GameController.State.UnresolvedActionChosenBuildings.Contains(buildingDef.ID)
				select new GenericListModalViewState.ListItem
				{
					Enabled = IsPartyLeader,
					Item = buildingDef,
					Text = (service.GetBuildingNameLocalized(buildingDef.ID.ID) ?? "")
				}).ToList();
			string text = _showingUnresolvedAction.Action.Parameter1;
			if (!string.IsNullOrEmpty(text))
			{
				text = GLOOM.LocalizationManager.GetTranslation(text);
			}
			GenericListModalViewState state3 = new GenericListModalViewState(text, items2, (GenericListModalViewState.ListItem item) =>
			{
				BuildingTypeDef buildingTypeDef = (BuildingTypeDef)item.Item;
				Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseBuilding, buildingTypeDef);
			});
			PushState(state3, targetLayer);
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.ChooseCharacters)
		{
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			List<GenericListModalViewState.ListItem> items3 = (from c in GameController.State.PartyCharacters
				where !GameController.State.UnresolvedActionChosenCharacters.Contains(c.ClassDef)
				select new GenericListModalViewState.ListItem
				{
					Enabled = IsPartyLeader,
					Item = c.ClassDef,
					Text = c.Name + " " + CrossReference.ClassIdToCharacterSprite(c.ClassDef.ID.ID)
				}).ToList();
			int numberOfCharactersToChooseForEvent = GameController.OutpostEventController.NumberOfCharactersToChooseForEvent;
			string text2 = _showingUnresolvedAction.Action.Parameter3;
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = GLOOM.LocalizationManager.GetTranslation(text2);
			}
			if (numberOfCharactersToChooseForEvent == 1)
			{
				if (string.IsNullOrEmpty(text2))
				{
					text2 = GLOOM.LocalizationManager.GetTranslation("GUI_CHOOSE_CHARACTER");
				}
				GenericListModalViewState state4 = new GenericListModalViewState(text2, items3, (GenericListModalViewState.ListItem item) =>
				{
					CharacterClassDef characterClassDef = (CharacterClassDef)item.Item;
					Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseSingleCharacter, characterClassDef);
				});
				PushState(state4, targetLayer);
			}
			else
			{
				if (string.IsNullOrEmpty(text2))
				{
					text2 = string.Format(GLOOM.LocalizationManager.GetTranslation("GUI_CHOOSE_CHARACTERS"), numberOfCharactersToChooseForEvent);
				}
				GenericListModalViewState state5 = new GenericListModalViewState(text2, items3, (GenericListModalViewState.ListItem item) =>
				{
					CharacterClassDef characterClassDef = (CharacterClassDef)item.Item;
					Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ResolveChooseSingleCharacter, characterClassDef);
				});
				PushState(state5, targetLayer);
			}
		}
		else if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.ChooseNextCampaignSection)
		{
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			LinkingSectionViewState state6 = new LinkingSectionViewState(_showingUnresolvedAction.Action);
			PushState(state6, targetLayer);
			TickUpdate();
		}
		else
		{
			ResetUnresolvedActionResourceDistribution();
			SetTargetLayer(FHMapViewLayer.LayerType.Modal);
			PushState(new EventDistributionViewState(_showingUnresolvedAction.Action)
			{
				OnResolved = () =>
				{
					PopState(targetLayer);
				}
			}, targetLayer);
		}
		void SetTargetLayer(FHMapViewLayer.LayerType newTargetLayer)
		{
			targetLayer = newTargetLayer;
			SetShownUnresolvedAction(action, targetLayer);
		}
	}

	private void ResetUnresolvedActionResourceDistribution()
	{
		switch (_showingUnresolvedAction.Action.Type)
		{
		case CampaignInteractionType.Event_LosePet:
		{
			List<PetID> list5 = new List<PetID>();
			Choreographer.ExecuteCommand("UpdateFrosthavenPetDistribution", list5);
			break;
		}
		case CampaignInteractionType.Event_SelectBuildingsToProtect:
		{
			List<BuildingTypeID> list6 = new List<BuildingTypeID>();
			Choreographer.ExecuteCommand("UpdateFrosthaveBuildingDistribution", list6);
			break;
		}
		case CampaignInteractionType.Event_LoseAnySnowthistle:
		{
			List<ResourceDistribution> list2 = new List<ResourceDistribution>
			{
				new ResourceDistribution()
			};
			Choreographer.ExecuteCommand("SetResourceDistribution", list2);
			break;
		}
		case CampaignInteractionType.Event_GainAnyMaterialsEach:
		{
			List<ResourceDistribution> list3 = GameState.GenerateDefaultGainAnyMaterialsEachDistribution(new OutpostResourceChange
			{
				AnyMaterial = _showingUnresolvedAction.Action.GetInt(0)
			});
			Choreographer.ExecuteCommand("SetResourceDistribution", list3);
			break;
		}
		case CampaignInteractionType.Event_TradeLumberForMetal:
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToMaterials:
		{
			List<ResourceDistribution> list4 = GameState.PartyCharacters.Select((CharacterState p) => new ResourceDistribution
			{
				CharacterID = p.ID
			}).ToList();
			Choreographer.ExecuteCommand("SetResourceDistribution", list4);
			break;
		}
		case CampaignInteractionType.Event_EverybodyChooseLoseGold:
		case CampaignInteractionType.Event_EverybodyChooseLoseUpToGold:
		{
			List<GoldDistribution> list = GameState.PartyCharacters.Select((CharacterState p) => new GoldDistribution
			{
				CharacterID = p.ID
			}).ToList();
			Choreographer.ExecuteCommand("SetGoldDistribution", list);
			break;
		}
		case CampaignInteractionType.Event_WR24OptionA:
			Choreographer.ExecuteCommand("ResetItemDistribution", new List<ItemDef> { null }, DistributionChangeType.Event_WR24OptionA_Lose);
			break;
		default:
			throw new Exception($"Invalid action to handle distribution for:{_showingUnresolvedAction.Action.Type}");
		}
	}

	private void ShowWeekPassedScreen()
	{
		ResetView();
		if (ShowDebugDebriefingInfo)
		{
			uint week = Choreographer.GameState.Outpost.Week;
			week = ((week != 0) ? ((week - 1) % 10 + 1) : 0u);
			GenericModalViewState state = new GenericModalViewState($"A new week has come! \n Week {week}/{10} \n {Choreographer.GameState.Outpost.Calendar.Season.ToString()}", null, canCancel: false);
			PushState(state);
		}
	}

	private void ShowOutpostEventResolved(CampaignEvent.OutpostEventResolved ev)
	{
		ResetView();
		int num = ev.Event.AppliedEffectsFromOutcome.FindIndex((CampaignEffect e) => e.Type == CampaignEffectType.TriggerAttackOutpost);
		if (num == -1)
		{
			EventViewState eventViewState = new EventViewState(ev);
			eventViewState.OnDone = (Action)Delegate.Combine(eventViewState.OnDone, new Action(TryResumeTravels));
			PushState(eventViewState);
			return;
		}
		string text = NarrationSetting.EventAttackOutcomeKey(ev.Event);
		if (!GLOOM.LocalizationManager.TryGetTranslation(text, out var translation))
		{
			translation = GLOOM.LocalizationManager.GetTranslation("GUI_EVENT_OUTPOST_ATTACK_COMPLETED");
		}
		EventViewState eventViewState2 = new EventViewState(ev.Event, text, translation, ev.Event.AppliedEffectsFromOutcome.Skip(num));
		eventViewState2.OnDone = (Action)Delegate.Combine(eventViewState2.OnDone, new Action(TryResumeTravels));
		PushState(eventViewState2);
	}

	[Obsolete]
	public static void FormatEffects(IEnumerable<CampaignEffect> effects, StringBuilder changes)
	{
		foreach (CampaignEffect effect in effects)
		{
			if (effect.Type != CampaignEffectType.TriggerAttackOutpost && !OutpostEventsService.IsHiddenEffect(effect))
			{
				changes.AppendLine(effect.ToString() ?? "");
			}
		}
		if (effects.Count() == 0)
		{
			changes.AppendLine("No effect");
		}
	}

	public void TryUpdateTravel(string destination, CampaignState.PartyTravelStateType travelState)
	{
		if (_remainingSections.Count <= 0 && GameState.PendingCampaignSectionIDs.Count <= 0 && IsPartyLeader && GameController.CanTravelTo(destination, travelState))
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.UpdatePartyMarkerLocation, destination, travelState);
		}
	}

	private void UpdateTravel()
	{
		if (CurrentState is WorldTravelViewState { IsTravelling: not false })
		{
			_hasTravelStateChanged = true;
			return;
		}
		_hasTravelStateChanged = false;
		OutpostEventDTO outpostEventDTO = ((GameState.Outpost.ActiveEvent != null) ? GetService<OutpostEventsService>().GetEventDTO(GameState.Outpost.ActiveEvent) : null);
		switch (GameState.PartyTravelState)
		{
		case CampaignState.PartyTravelStateType.TravelingToScenario:
			if (CurrentState is ProloguePartyCreationViewState)
			{
				TryPopState(CurrentState);
			}
			if (CurrentState is ScenarioSelectionViewState)
			{
				ReplaceState(new WorldViewState());
			}
			PushState(new WorldTravelViewState(0f, (outpostEventDTO != null) ? 0.5f : 1f, outpostEventDTO));
			break;
		case CampaignState.PartyTravelStateType.TravelingToScenarioAfterEncounter:
			PushState(new WorldTravelViewState(0.5f, 1f, null));
			break;
		case CampaignState.PartyTravelStateType.TravelingToOutpost:
			PushState(new WorldTravelViewState(0f, 1f, null));
			break;
		case CampaignState.PartyTravelStateType.TravelingToSpecialEncounter:
			if (GameState.Outpost.ActiveEvent != null)
			{
				GameState.Outpost.ActiveEvent.ClearTravelWaitFlag();
			}
			else
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Phase.ProcessPendingSections);
			}
			PushState(new WorldTravelViewState(0f, 1f, outpostEventDTO));
			break;
		case CampaignState.PartyTravelStateType.WaitingInOutpost:
			ResetView();
			break;
		case CampaignState.PartyTravelStateType.WaitingEncounter:
			if (CurrentState is WorldTravelViewState state2)
			{
				TryPopState(state2);
			}
			break;
		case CampaignState.PartyTravelStateType.WaitingInScenario:
		{
			if (CurrentState is WorldTravelViewState state)
			{
				TryPopState(state);
			}
			ScenarioViewService service = GetService<ScenarioViewService>();
			if (GameState.ActiveScenario != null && !IsActionQueuedAfterUnresolvedActions(service.SetupSelectedScenario))
			{
				ExecAfterNoUnresolvedActions(service.SetupSelectedScenario);
			}
			break;
		}
		}
	}

	private void ShowScenarioCompleted(CampaignEvent.ScenarioCompleted ev)
	{
		PendingScenarioCompletionEvent = null;
		GameController.State.BlockShowUnresolvedActions = false;
		ScenarioDef.Conclusion conclusion = ev.Conclusion;
		if (conclusion == null || !ev.Success)
		{
			if (!ev.Restart)
			{
				TryUpdateTravel(GameState.GameMode.OutpostMarkerLocation, CampaignState.PartyTravelStateType.TravelingToOutpost);
			}
		}
		else if (ev.GetScenario(GameState).Def.NoIntroOutro)
		{
			ResetView();
		}
		else
		{
			PendingScenarioCompletionEvent = ev;
			ResetView();
			NarrationViewState state = new NarrationViewState(NarrationSetting.FromConclustion(ev.GetScenario(GameState), GameController, conclusion))
			{
				OnDone = onOutroDone
			};
			ReplaceState(state);
		}
		void onOutroDone()
		{
			ShowScenarioResults();
		}
	}

	private bool ShowScenarioResults()
	{
		if (GameState.UnresolvedActions.Any((CampaignUnresolvedAction x) => x.Type == CampaignInteractionType.ChooseUnlockMandatoryScenario))
		{
			return false;
		}
		CampaignEvent.ScenarioCompleted ev = PendingScenarioCompletionEvent;
		PendingScenarioCompletionEvent = null;
		if (ev?.Conclusion == null || !ev.Success)
		{
			return false;
		}
		PushState(new WorldViewState());
		ScenarioRewardsViewState scenarioRewardsViewState = new ScenarioRewardsViewState(ev.ScenarioID.ID, ev.AppliedEffects);
		scenarioRewardsViewState.OnResult = ShowTravel;
		PushState(scenarioRewardsViewState);
		return true;
		void HandleScenarioUnlocks(Action OnDoneCallback)
		{
			int[] array = (from effect in ev.AppliedEffects
				where effect.Type == CampaignEffectType.UnlockScenario || effect.Type == CampaignEffectType.UnlockLinkedScenario || effect.Type == CampaignEffectType.UnlockMandatoryScenario
				select effect.GetInt(0)).ToArray();
			if (array.Length == 0)
			{
				OnDoneCallback?.Invoke();
			}
			else
			{
				PushState(new ScenarioUnlockViewState(array)
				{
					OnDone = OnDoneCallback
				});
			}
		}
		void ShowTravel()
		{
			HandleScenarioUnlocks(TryResumeTravels);
		}
	}

	private void DecideNextTravelLocation()
	{
		if (GameController.WaitingForTravelSection || PendingScenarioCompletionEvent != null || CurrentState is NarrationViewState)
		{
			return;
		}
		if (!IsPartyLeader)
		{
			if (GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingInScenario && GameState.ActiveScenario != null)
			{
				GetService<ScenarioViewService>().SetupSelectedScenario();
				return;
			}
			IEnumerable<FHRL.ScenarioState> linkedScenarios = GameController.ScenarioController.GetLinkedScenarios();
			if (linkedScenarios != null && linkedScenarios.Any((FHRL.ScenarioState s) => !s.Mandatory))
			{
				ReplaceState(new LinkedScenarioChoiceViewState());
			}
		}
		else if (GameState.Outpost.ActiveEvent != null && !string.IsNullOrEmpty(GameState.Outpost.ActiveEvent.Def.TravelTo))
		{
			TryUpdateTravel(GameState.Outpost.ActiveEvent.Def.TravelTo, CampaignState.PartyTravelStateType.TravelingToSpecialEncounter);
		}
		else if (GameState.ActiveScenario != null)
		{
			if (!GameState.GameMode.CanPlayWithoutCharacters)
			{
				if (!GameState.ActiveScenario.HasStartedBriefing && GameState.PartyTravelState != CampaignState.PartyTravelStateType.WaitingInScenario)
				{
					GetService<ScenarioViewService>().ContinueTravelToScenario();
				}
				else if (GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingInScenario)
				{
					GetService<ScenarioViewService>().SetupSelectedScenario();
				}
			}
		}
		else if (!GameController.ScenarioController.HasActiveScenario() && GameController.ScenarioController.GetMandatoryScenarios().Count() == 1)
		{
			if (IsPartyLeader)
			{
				FHRL.ScenarioState scenarioState = GameController.ScenarioController.GetMandatoryScenarios().First();
				Choreographer.ExecuteCommand(FHRPC.Campaign.Scenario.StartScenario, scenarioState.Def);
			}
		}
		else
		{
			IEnumerable<FHRL.ScenarioState> linkedScenarios2 = GameController.ScenarioController.GetLinkedScenarios();
			if (linkedScenarios2 != null && linkedScenarios2.Any((FHRL.ScenarioState s) => !s.Mandatory))
			{
				ReplaceState(new LinkedScenarioChoiceViewState());
			}
			else if (GameState.GameMode.OutpostMarkerLocation != GameState.PartyMarkerLocation)
			{
				TryUpdateTravel(GameState.GameMode.OutpostMarkerLocation, CampaignState.PartyTravelStateType.TravelingToOutpost);
			}
		}
	}

	private void ShowNewRecipieDiscovered(CampaignEvent.NewBrewableRecipieDiscovered ev)
	{
		if (IsOnline && !(ev.Brewer == null) && !(ev.Recipient == null))
		{
			bool flag = PlayerCharacters.Any((CharacterState state) => state.ClassDef == ev.Brewer);
			bool flag2 = PlayerCharacters.Any((CharacterState state) => state.ClassDef == ev.Recipient);
			if (!(flag & flag2) && flag2)
			{
				ItemDataDTO itemDataDTO = new ItemDataDTO(ev.Item);
				Singleton<UINotificationManager>.Instance.ShowNotification("BUILDING_Name_35", GLOOM.LocalizationManager.GetTranslation("GUI_GIFT_ITEM") + " " + FHUILocFormatting.Color(itemDataDTO.NameLocalized, "FFC776FF"), UIInfoTools.Instance.defaultNotificationDuration, "NEW_RECIPE", "GAMEPLAY_NOTIFICATIONS");
			}
		}
	}

	private void ProcessPendingSections()
	{
		if (GameController.WaitingForTravelSection)
		{
			if (!IsActionQueuedAfterUnresolvedActions(ShowTravelToCampaignSectionLocation))
			{
				ExecAfterNoUnresolvedActions(ShowTravelToCampaignSectionLocation);
			}
		}
		else if (IsPartyLeader && GameController.State.PendingCampaignSectionIDs.Count > 0)
		{
			ExecAfterNoUnresolvedActions(() =>
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.Phase.ProcessPendingSections);
			});
		}
	}

	private void ShowTravelToCampaignSectionLocation()
	{
		CampaignSectionDef travelSectionToWaitFor = GameController.TravelSectionToWaitFor;
		TryUpdateTravel(travelSectionToWaitFor.TravelTo, CampaignState.PartyTravelStateType.TravelingToSpecialEncounter);
	}

	private void ShowCampaignSectionActivated(CampaignEvent.CampaignSectionActivated ev)
	{
		ProcessPendingSections();
		if (!ev.Event.SkipNarration)
		{
			GameState.BlockShowUnresolvedActions = false;
		}
		if (ev.Event.SkipNarration || !ev.Event.SkipCompletedNotification)
		{
			_remainingSections.Remove(ev);
			return;
		}
		if (string.IsNullOrEmpty(ev.Event.TravelTo))
		{
			ResetView();
		}
		GameController.State.BlockShowUnresolvedActions = false;
		IEnumerable<CampaignEffect> validConditionalEffects = GameController.GetValidConditionalEffects(ev.Event, forApply: false);
		if (validConditionalEffects.Any())
		{
			CampaignEffect campaignEffect = validConditionalEffects.First();
			string text = $"Event_{ev.Event.ID.ID}_{campaignEffect.Type}_Text";
			if (GLOOM.LocalizationManager.TryGetTranslation(text, out var _))
			{
				PushState(new LinkingSectionViewState(ev.Event, text)
				{
					OnDone = () =>
					{
						_remainingSections.Remove(ev);
					}
				});
				TickUpdate();
			}
			else
			{
				_remainingSections.Remove(ev);
			}
			return;
		}
		PushState(new LinkingSectionViewState(ev.Event, ev.AppliedEffects)
		{
			OnDone = () =>
			{
				_remainingSections.Remove(ev);
				if (GameState.PendingCampaignSectionIDs.Count == 0 && _remainingSections.Count == 0 && (!string.IsNullOrEmpty(ev.Event.TravelTo) || GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingEncounter))
				{
					TryResumeTravels();
				}
			}
		});
		TickUpdate();
	}

	public void TryResumeTravels()
	{
		ScenarioViewService service = GetService<ScenarioViewService>();
		if (!IsActionQueuedAfterUnresolvedActions(service.SetupSelectedScenario) && !IsActionQueuedAfterUnresolvedActions(service.ContinueTravelToScenario) && !IsActionQueuedAfterUnresolvedActions(DecideNextTravelLocation) && !(CurrentState is WorldTravelViewState))
		{
			ExecAfterNoUnresolvedActions(DecideNextTravelLocation);
		}
	}

	public IEnumerator WaitUntilScreenIsReady()
	{
		bool waiting = true;
		while (waiting)
		{
			if (HasInit && _transitionTween == null && !_queryRefreshView && GetViewLayer(FHMapViewLayer.LayerType.Base).HasState)
			{
				waiting = false;
			}
			if (waiting)
			{
				yield return null;
			}
		}
		yield return null;
	}

	private void Update()
	{
		TickUpdate();
	}

	private void TryPushActiveEvent(bool forcePush = false)
	{
		CampaignUnresolvedAction campaignUnresolvedAction = GameController.State.UnresolvedActions.Where((CampaignUnresolvedAction x) => x.Type != CampaignInteractionType.ChooseAlternativeScenarioLevel).FirstOrDefault();
		if (!campaignUnresolvedAction.IsDefault())
		{
			ShowResolveAction(campaignUnresolvedAction);
		}
		if (GameController.OutpostEventController.ShouldContinueResolveEvent && IsPartyLeader)
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.OutpostEvent.ContinueResolveCurrentEvent);
		}
		if (_showingUnresolvedAction.Action.Type == CampaignInteractionType.None && GameController.OutpostEventController.EventToResolve != null && (forcePush || !(CurrentState is EventViewState)) && !GameController.OutpostEventController.EventToResolve.WaitingToFinishTravel && (GameController.OutpostEventController.EventToResolve.DontWaitForTravel || (GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingEncounter && GameController.OutpostEventController.EventToResolve.Def.Type != OutpostEventType.Outpost) || (GameState.PartyTravelState == CampaignState.PartyTravelStateType.WaitingInOutpost && GameController.OutpostEventController.EventToResolve.Def.Type == OutpostEventType.Outpost)))
		{
			PushState(new EventViewState(GameController.OutpostEventController.EventToResolve));
			_hasShownAttackOptionOutcome = false;
		}
	}

	public void TickUpdate()
	{
		if (!HasInit || _transitionTween != null)
		{
			return;
		}
		bool flag = false;
		foreach (FHMapViewLayer item in _layers.Values.Where((FHMapViewLayer layer) => layer.Type != FHMapViewLayer.LayerType.Base))
		{
			flag |= item.TryUpdate(out var _);
		}
		if (_layers[FHMapViewLayer.LayerType.Base].TryUpdate(out var oldState2))
		{
			DoStateTransition(oldState2);
		}
		else if (flag)
		{
			UpdateModules();
		}
		EnsureCurrentStateExists();
		if (_queryRefreshView)
		{
			RefreshView(immediate: true);
		}
		Action result;
		while (!GameState.HasUnresolvedActions() && (!GameController.OutpostEventController.HasEventToResolve || GameController.OutpostEventController.WaitingForTravelToEvent) && !_layers.Values.Any((FHMapViewLayer l) => l.GetStates().Any((FHMapViewState s) => s is EventViewState)) && _afterAllUnresolvedActionsExec.TryDequeue(out result))
		{
			result();
		}
		RefreshMultiplayerNotifications();
		bool flag2 = _layers.Values.Any((FHMapViewLayer l) =>
		{
			FHMapViewState state = l.State;
			return state is GenericModalViewState || state is GenericListModalViewState || state is EventViewState || state is NarrationViewState || state is LinkingSectionViewState || state is RetirementStoryViewState || state is WorldTravelViewState || state is ScenarioUnlockViewState || state is ScenarioRewardsViewState;
		});
		CampaignUnresolvedAction campaignUnresolvedAction = GameController.State.UnresolvedActions.Where((CampaignUnresolvedAction x) => x.Type == CampaignInteractionType.ChooseAlternativeScenarioLevel).FirstOrDefault();
		bool flag3 = !campaignUnresolvedAction.IsDefault();
		if ((GameController.State.Outpost.ActiveEvent == null) & flag3)
		{
			ShowResolveChooseAlternativeLevel(campaignUnresolvedAction, null);
		}
		else
		{
			if (flag2)
			{
				return;
			}
			if (PendingScenarioCompletionEvent != null)
			{
				if (ShowScenarioResults())
				{
					return;
				}
				CampaignUnresolvedAction action = GameState.UnresolvedActions.FirstOrDefault((CampaignUnresolvedAction x) => x.Type == CampaignInteractionType.ChooseUnlockMandatoryScenario);
				if (action.Type != CampaignInteractionType.None)
				{
					ShowResolveChooseUnlockMandatoryScenario(action);
					return;
				}
			}
			if (!(CurrentModalState is LinkingSectionViewState) && _remainingSections.Any())
			{
				ShowCampaignSectionActivated(_remainingSections.First());
				return;
			}
			TryPushActiveEvent();
			if (!GameController.State.HasUnresolvedActions() && _showingUnresolvedAction.Action.Type != CampaignInteractionType.None)
			{
				if (CurrentState is WorldViewState)
				{
					ResetShownUnresolvedAction();
				}
				else
				{
					ResetView();
				}
			}
			if (CurrentState is OutpostVillageMapState && !Choreographer.GameClient.HasUnprocessedResults && GameController.PhaseController.CanMoveToNextPhase() && IsPartyLeader && (GameState.Outpost.Phase != OutpostPhase.Downtime || GameState.CompletedDowntimeScenario))
			{
				Choreographer.ExecuteCommand(FHRPC.Campaign.NextPhase);
			}
			if (!Choreographer.GameClient.HasUnprocessedResults && GameState.Outpost.Phase == OutpostPhase.Downtime && _remainingUnlockCharacterEvents.Any() && _afterAllUnresolvedActionsExec.Count == 0 && !_remainingSections.Any() && !GameState.CompletedDowntimeScenario && CurrentState is OutpostVillageMapState && !(CurrentModalState is UnlockCharacterViewState))
			{
				CampaignEvent.CharacterClassUnlocked characterClassUnlocked = _remainingUnlockCharacterEvents.First();
				PushState(new UnlockCharacterViewState(characterClassUnlocked.Class));
				GetService<CharacterDetailsViewService>()?.ShowCharacterIntroVideo(characterClassUnlocked.Class.ID, fullscreen: true, closeOnComplete: true);
				_remainingUnlockCharacterEvents.Remove(characterClassUnlocked);
			}
			else if (!Choreographer.GameClient.HasUnprocessedResults && _showCredits && GameState.Outpost.Phase == OutpostPhase.Downtime && _afterAllUnresolvedActionsExec.Count == 0 && !GameState.CompletedDowntimeScenario && CurrentState is OutpostVillageMapState && !(CurrentModalState is ShowCreditsPopupViewState))
			{
				PushState(new ShowCreditsPopupViewState());
				_showCredits = false;
			}
			else
			{
				ProcessInputEvents();
			}
		}
	}

	private void ProcessInputEvents()
	{
		if (InputFHManager.Controls.UI.Cancel.WasPressedThisFrame())
		{
			FHMapViewState currentState = CurrentState;
			if (currentState != null && currentState.Category == FHMapViewState.StateCategory.Outpost && BuildingTooltipActionEventChannel.SelectedBuilding.HasValue)
			{
				if (CurrentModalState is BuildingDonationDistributionViewState)
				{
					PopState(CurrentModalState.TargetLayerType);
				}
				else
				{
					BuildingTooltipActionEventChannel.DeselectBuilding();
				}
			}
		}
		else if (InputFHManager.Controls.UI.ShowInfo.WasPressedThisFrame())
		{
			Modules.GeneralInfo.TryShowGeneralInfoUnderCursor();
		}
		else if (InputFHManager.GetMouse(MouseButton.Right))
		{
			FHMapViewState currentState2 = CurrentState;
			if (currentState2 != null && currentState2.Category == FHMapViewState.StateCategory.WorldMap && CurrentState is ScenarioSelectionViewState { IsScenarioSelected: not false } scenarioSelectionViewState && !IsReadyCheckRunning)
			{
				Modules.WorldMap.WorldMap.DeselectScenario();
				Modules.WorldMap.ScenarioLauncher.gameObject.SetActive(value: false);
				scenarioSelectionViewState.OnScenarioSelected(null);
				RefreshView();
			}
		}
	}

	public void TryStartScenario()
	{
		_needsRefreshStartScenario = true;
	}

	private void LateUpdate()
	{
		if (_hasTravelStateChanged)
		{
			UpdateTravel();
		}
		if (_needsRefreshStartScenario)
		{
			_needsRefreshStartScenario = false;
			GetService<ScenarioViewService>().ProcessStartScenarioCommand(null);
		}
		if (_queryRefreshView)
		{
			RefreshView(immediate: true);
		}
	}

	private void UpdateModules()
	{
		if (_layers.Values.Any((FHMapViewLayer l) => l.HasState))
		{
			List<IUIViewModule> nextModules = _layers.Values.SelectMany((FHMapViewLayer l) => l.GetStateModules()).ToList();
			AllModules.ForEach((IUIViewModule m) =>
			{
				m.GameObject.SetActive(nextModules.Contains(m));
			});
			RefreshView(immediate: true);
		}
		else
		{
			AllModules.ForEach((IUIViewModule m) =>
			{
				m.GameObject.SetActive(value: false);
			});
		}
	}

	private bool DontFade()
	{
		if (_remainingSections.Count > 0 && _remainingSections[0].Event.DontFadeIn)
		{
			return true;
		}
		if (GameState.HasUnresolvedActions() && GameState.UnresolvedActions.First().Type == CampaignInteractionType.ChooseNextCampaignSection)
		{
			return true;
		}
		return false;
	}

	private void DoStateTransition(FHMapViewState oldState)
	{
		if (_transitionTween != null)
		{
			cleanupOldState();
			return;
		}
		if (!shouldUseTransition())
		{
			setUpNewState();
			return;
		}
		float halfTrans = _transitionSeconds * 0.5f;
		_transitionCanvasGroup.blocksRaycasts = true;
		_transitionTween = _transitionCanvasGroup.DOFade(1f, halfTrans).SetEase(Ease.InOutCubic).OnComplete(transitionBack);
		void cleanupOldState()
		{
			if (oldState != null)
			{
				oldState.OnHide();
				OnStateExit?.Invoke(oldState);
			}
		}
		void setUpNewState()
		{
			cleanupOldState();
			EnsureCurrentStateExists();
			UpdateModules();
		}
		bool shouldUseTransition()
		{
			if (_transitionType == TransitionType.None)
			{
				return false;
			}
			if (Utl.LesserThanOrEqualTo(_transitionSeconds, 0.01f))
			{
				return false;
			}
			if (_transitionCanvasGroup == null)
			{
				return false;
			}
			if (_transitionType == TransitionType.OnEveryState)
			{
				return oldState?.GetType() != CurrentState?.GetType();
			}
			FHMapViewState.StateCategory stateCategory = oldState?.Category ?? FHMapViewState.StateCategory.None;
			FHMapViewState.StateCategory stateCategory2 = CurrentState?.Category ?? FHMapViewState.StateCategory.None;
			switch (stateCategory2)
			{
			case FHMapViewState.StateCategory.None:
				return false;
			case FHMapViewState.StateCategory.Outpost:
				if (stateCategory == FHMapViewState.StateCategory.None)
				{
					return false;
				}
				break;
			}
			if (DontFade())
			{
				return false;
			}
			return stateCategory != stateCategory2;
		}
		void transitionBack()
		{
			setUpNewState();
			_transitionTween = _transitionCanvasGroup.DOFade(0f, halfTrans).SetEase(Ease.InOutCubic).OnComplete(() =>
			{
				_transitionCanvasGroup.blocksRaycasts = false;
				_transitionTween = null;
			});
		}
	}

	public TService GetService<TService>() where TService : FHMapViewService
	{
		if (!_services.TryGetValue(typeof(TService), out var value))
		{
			return null;
		}
		return (TService)value;
	}

	private void InitServices()
	{
		_services.Add(typeof(UIElementTagsViewService), new UIElementTagsViewService(this));
		_services.Add(typeof(PartyViewService), new PartyViewService(this));
		_services.Add(typeof(ItemViewService), new ItemViewService(this));
		_services.Add(typeof(ScenarioViewService), new ScenarioViewService(this));
		_services.Add(typeof(BattleBriefingViewService), new BattleBriefingViewService(this, GetService<PartyViewService>(), GetService<UIElementTagsViewService>()));
		_services.Add(typeof(BuildingViewService), new BuildingViewService(this));
		_services.Add(typeof(CharacterDetailsViewService), new CharacterDetailsViewService(this));
		_services.Add(typeof(QuickAccessBarViewService), new QuickAccessBarViewService(this, GetService<BuildingViewService>(), GetService<CharacterDetailsViewService>()));
		_services.Add(typeof(OutpostNotificationViewService), new OutpostNotificationViewService(this));
		_services.Add(typeof(HeaderViewService), new HeaderViewService(this, GetService<CharacterDetailsViewService>()));
		_services.Add(typeof(ImportPartyService), new ImportPartyService(this));
		_services.Add(typeof(CardViewService), new CardViewService(this));
		_services.Add(typeof(RosterViewService), new RosterViewService(this, GetService<CharacterDetailsViewService>(), GetService<PartyViewService>()));
		_services.Add(typeof(CraftingViewService), new CraftingViewService(this));
		_services.Add(typeof(EnhancerViewService), new EnhancerViewService(this));
		_services.Add(typeof(PerksViewService), new PerksViewService(this));
		_services.Add(typeof(AlchemistViewService), new AlchemistViewService(this));
		_services.Add(typeof(OutpostEventsService), new OutpostEventsService(this, GetService<UIElementTagsViewService>()));
		_services.Add(typeof(InputActionHintsService), new InputActionHintsService(this));
		_services.Add(typeof(MultiplayerSyncViewService), new MultiplayerSyncViewService(this));
		_services.Add(typeof(ResourceAllocationService), new ResourceAllocationService(this, GetService<CharacterDetailsViewService>()));
		_services.Add(typeof(PuzzleBookViewService), new PuzzleBookViewService(this));
		_services.Add(typeof(OutpostCalendarViewService), new OutpostCalendarViewService(this));
	}

	private void WireEventChannels()
	{
		BuildingTooltipActionEventChannel.SetReference(this);
	}

	public UIMultiplayerUser.Data? GetPlayerData(MultiplayerGameState.Player player)
	{
		if (player == null)
		{
			return null;
		}
		bool isLeader = MultiplayerGameState.PartyLeaderID == player.ID;
		string username = PlayerRegistry.GetPlayer(player.ID)?.Username;
		return new UIMultiplayerUser.Data
		{
			Id = player.ID.ID,
			Username = username,
			Avatar = PlayerRegistry.GetPlayer(player.ID)?.Avatar,
			IsLeader = isLeader
		};
	}

	public List<UIMultiplayerUser.Data> TryGetReadyUsersList()
	{
		if (IsSoloGame)
		{
			return null;
		}
		return (from p in MultiplayerGameState.Players
			where p.Ready
			select GetPlayerData(p).Value).ToList();
	}

	public void ShowRecievingGameState(float progress)
	{
		if (Singleton<UINotificationManager>.IsInitialized)
		{
			Singleton<UINotificationManager>.Instance.HideNotification("RECIEVE_GAME_STATE", instant: true);
			Singleton<UINotificationManager>.Instance.ShowPermanentNotification("RECIEVE_GAME_STATE", "GUI_MULTIPLAYER", $"Recieving new game state: {(int)(progress * 100f)}%", "RECIEVE_GAME_STATE");
		}
	}

	public void HideRecievingGameState()
	{
		if (Singleton<UINotificationManager>.IsInitialized)
		{
			Singleton<UINotificationManager>.Instance.HideNotification("RECIEVE_GAME_STATE");
		}
	}

	public void OnReceivingGameState()
	{
		GetService<OutpostNotificationViewService>()?.Refresh();
	}

	public void RefreshMultiplayerNotifications()
	{
		if (Singleton<UINotificationManager>.IsInitialized && (_isOnline != FFSNetwork.IsOnline || _isOnline))
		{
			_isOnline = FFSNetwork.IsOnline;
			RefreshWaitingNotifications();
			ValidateCharactersAssignedToSlots();
		}
	}

	private void RefreshWaitingNotifications()
	{
		if ((!FFSNetwork.IsHost || PlayerRegistry.JoiningPlayers != null) && PlayerRegistry.AllPlayers != null && Singleton<UINotificationManager>.IsInitialized)
		{
			bool flag;
			if ((FFSNetwork.IsOnline && FFSNetwork.IsHost && (PlayerRegistry.JoiningPlayers.Count > 0 || PlayerRegistry.AllPlayers.Any((NetworkPlayer w) => w.IsClient && !w.PlayerReadyForAssignment))) || (FFSNetwork.IsOnline && FFSNetwork.IsClient && PlayerRegistry.OtherClientsAreJoining))
			{
				flag = true;
				UIMultiplayerNotifications.ShowWaitingPlayersJoining();
			}
			else
			{
				flag = false;
				UIMultiplayerNotifications.HideWaitingPlayersJoining();
			}
			if (FFSNetwork.IsOnline && !flag && PlayerRegistry.AllPlayers.Count > 1 && PlayerRegistry.AllPlayers.Exists((NetworkPlayer x) => !x.IsParticipant))
			{
				UIMultiplayerNotifications.ShowWaitingPlayersCharacterAssigned();
			}
			else
			{
				UIMultiplayerNotifications.HideWaitingPlayersCharacterAssigned();
			}
			if (FFSNetwork.IsOnline && FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.AllPlayers.Count == 1)
			{
				UIMultiplayerNotifications.ShowWaitingPlayersJoin();
			}
			else
			{
				UIMultiplayerNotifications.HideWaitingPlayersJoin();
			}
			PlayersJoining = flag;
		}
	}

	private void ValidateCharactersAssignedToSlots()
	{
		if (!Singleton<UINotificationManager>.IsInitialized)
		{
			return;
		}
		if (PlayerRegistry.MyPlayer == null || !PlayerRegistry.MyPlayer.AssignedSlots.Any() || !PlayerRegistry.MyPlayer.MyControllables.Any())
		{
			UIMultiplayerNotifications.HideWaitingAssignAllMySlots();
		}
		else if (FFSNetwork.IsOnline && FFSNetwork.IsClient)
		{
			if (PlayerRegistry.MyPlayer.MyControllables.Count() != PlayerRegistry.MyPlayer.AssignedSlots.Count())
			{
				UIMultiplayerNotifications.ShowWaitingAssignAllMySlots();
			}
			else
			{
				UIMultiplayerNotifications.HideWaitingAssignAllMySlots();
			}
		}
		else if (FFSNetwork.IsOnline && PlayerRegistry.AllPlayers.Exists((NetworkPlayer it) => it.IsClient && it.MyControllables.Count() != it.AssignedSlots.Count()))
		{
			UIMultiplayerNotifications.ShowWaitingAssignAllMySlots();
		}
		else
		{
			UIMultiplayerNotifications.HideWaitingAssignAllMySlots();
		}
	}

	public void UpdateCharacterController(NetworkControllable controllable, NetworkPlayer newController, NetworkPlayer oldController)
	{
		string characterID = null;
		string text = null;
		CharacterState character = Choreographer.GameClient.GetControllableCharacter(controllable.ID).Character;
		if (character != null)
		{
			characterID = character.ClassID.ID;
		}
		if (oldController != null)
		{
			GetService<PartyViewService>()?.HandleOnCharacterOwnershipChange(character);
		}
		CancelVotes();
		if (Singleton<UINotificationManager>.IsInitialized)
		{
			text = I2.Loc.LocalizationManager.GetTranslation(CharacterClassManager.Classes.Single((CCharacterClass s) => s.ID == characterID).LocKey);
			if (FFSNetwork.IsOnline)
			{
				UIMultiplayerNotifications.ShowPlayerControlsCharacter(newController, text);
			}
			RefreshMultiplayerNotifications();
		}
	}

	public void CancelVotes()
	{
		GetService<ScenarioViewService>()?.CancelScenarioReadyCheck();
	}
}
