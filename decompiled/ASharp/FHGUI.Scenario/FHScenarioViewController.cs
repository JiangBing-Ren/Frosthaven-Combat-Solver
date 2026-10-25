using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.Common;
using FHGUI.Scenario.Services;
using FHGUI.Scenario.States;
using FHRL;
using Libs.GameConsole;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace FHGUI.Scenario;

public class FHScenarioViewController : Singleton<FHScenarioViewController>
{
	public enum StateOperationType
	{
		None,
		Show,
		Update,
		Hide,
		Clear
	}

	public FHScenarioViewModulesCatalogue Modules;

	[Header("Common")]
	public UIGeneralInfoModule GeneralInfo;

	public UIContextualTutorialModule ContextualTutorial;

	public ScenarioMechanicsInfoModule ScenarioMechanics;

	[SerializeField]
	private UIHexInfoPanel m_HexInfoPanel;

	[SerializeField]
	private ActorStatPanel m_ActorStatPanel;

	[SerializeField]
	private UIHoverInfoHints m_HoverHints;

	[ConsoleVariable(Alias = "new_ui", Persistent = true)]
	public static bool NewUIEnabled;

	private bool _newUIEnabled;

	private readonly Dictionary<Type, FHScenarioViewService> _services = new Dictionary<Type, FHScenarioViewService>();

	private readonly PhaseViewLayer _phaseLayer = new PhaseViewLayer();

	private readonly ReactionViewLayer _reactionLayer = new ReactionViewLayer();

	private readonly InfoViewLayer _infoLayer = new InfoViewLayer();

	private bool _isDirty;

	private bool _shouldUpdateParty;

	private bool _shouldUpdateAttackModifiers;

	private bool _shouldUpdateReactionModules;

	private CPlayerActor _prevSelectedPlayer;

	private CActor _prevActingActor;

	private List<CMessageData> _currentTurnMessageData = new List<CMessageData>();

	private bool _isTurnStarted;

	private Coroutine m_WaitForGeneralInfoCoroutine;

	public IReadOnlyList<CMessageData> CurrentTurnMessageData => _currentTurnMessageData;

	public bool IsTurnStarted => _isTurnStarted;

	protected override void AwakeInit()
	{
		_newUIEnabled = NewUIEnabled;
		GameConsole.Context.Add(this);
		base.AwakeInit();
		PlayerRegistry.OnSlotOwnershipChanged = (SlotOwnershipChangedEvent)Delegate.Combine(PlayerRegistry.OnSlotOwnershipChanged, new SlotOwnershipChangedEvent(OnSlotOwnershipChanged));
		Modules.Init();
		InitServices();
		UpdateNewUIEnabled(force: true);
	}

	private void Start()
	{
		DoLayerStackOperation(StateOperationType.Show);
		_isDirty = true;
	}

	private void OnEnable()
	{
		GeneralInfo.OnShow += OnGeneralInfoOpen;
	}

	private void OnDisable()
	{
		GeneralInfo.OnShow -= OnGeneralInfoOpen;
		if (m_WaitForGeneralInfoCoroutine != null)
		{
			StopCoroutine(m_WaitForGeneralInfoCoroutine);
			m_WaitForGeneralInfoCoroutine = null;
		}
	}

	protected override void OnDestroy()
	{
		GameAction.ExecuteActionHandler = null;
		PlayerRegistry.OnSlotOwnershipChanged = (SlotOwnershipChangedEvent)Delegate.Remove(PlayerRegistry.OnSlotOwnershipChanged, new SlotOwnershipChangedEvent(OnSlotOwnershipChanged));
		GameConsole.Context.Remove(this);
	}

	private void InitServices()
	{
		_services.Add(typeof(PlayerViewService), new PlayerViewService(this));
		_services.Add(typeof(NetworkViewService), new NetworkViewService(this));
		_services.Add(typeof(ActorViewService), new ActorViewService(this));
		_services.Add(typeof(ActionsViewService), new ActionsViewService(this));
		_services.Add(typeof(UndoActionViewService), new UndoActionViewService(this));
		_services.Add(typeof(ScenarioMechanicViewService), new ScenarioMechanicViewService(this));
		_services.Add(typeof(ReactionViewService), new ReactionViewService(this));
	}

	public TService GetService<TService>() where TService : FHScenarioViewService
	{
		if (!_services.TryGetValue(typeof(TService), out var value))
		{
			return null;
		}
		return (TService)value;
	}

	private void OnGeneralInfoOpen(IGeneralInfoData _)
	{
		Choreographer.s_Choreographer?.readyButton?.HideProgressBar();
	}

	[ConsoleCommand(Command = "print_msgq", Description = "Print current turn CMessageData message Queue", RequiredContext = typeof(FHScenarioViewController))]
	public static void PrintTurnMessageQueue(IConsole console)
	{
		FHScenarioViewController instance = Singleton<FHScenarioViewController>.Instance;
		console.WriteLine($"StartTurn {instance._isTurnStarted}. Messages {instance._currentTurnMessageData.Count}");
		foreach (CMessageData currentTurnMessageDatum in instance._currentTurnMessageData)
		{
			console.WriteLine(currentTurnMessageDatum.m_Type.ToString());
		}
	}

	public void ProcessMessage(CMessageData messageData)
	{
		ProcessMessageOnLayerStack(messageData);
	}

	public void WinScenario()
	{
	}

	public void LoseScenario()
	{
	}

	public void ShowDetailedInfo(AbilityCardUI cardUI)
	{
		_infoLayer.Set(new CardInfoViewState(cardUI), StateOperationType.Show);
	}

	public void ClearDetailedInfo()
	{
		_infoLayer.Set(null, StateOperationType.Clear);
	}

	public void RefreshView()
	{
		_isDirty = true;
		_shouldUpdateParty = true;
		_shouldUpdateAttackModifiers = true;
		_shouldUpdateReactionModules = true;
	}

	private void Update()
	{
		int num;
		int num2;
		if (m_ActorStatPanel != null)
		{
			num = (m_ActorStatPanel.isActiveAndEnabled ? 1 : 0);
			if (num != 0)
			{
				num2 = 1;
				goto IL_003e;
			}
		}
		else
		{
			num = 0;
		}
		num2 = ((m_HexInfoPanel != null && m_HexInfoPanel.isActiveAndEnabled) ? 1 : 0);
		goto IL_003e;
		IL_003e:
		bool flag = (byte)num2 != 0;
		CHeroSummonActor cHeroSummonActor = ((num != 0) ? (m_ActorStatPanel.ActorShown as CHeroSummonActor) : null);
		bool flag2 = CanDeactivateSummon(cHeroSummonActor);
		if ((bool)m_HoverHints)
		{
			m_HoverHints.Init(isPrimaryActive: false, flag2, flag && InputFHManager.Controls.UI.ShowInfo.enabled);
		}
		if (InputFHManager.Controls.UI.ShowInfo.WasPressedThisFrame() && m_WaitForGeneralInfoCoroutine == null)
		{
			m_WaitForGeneralInfoCoroutine = StartCoroutine(WaitForGeneralInfo());
		}
		if (flag2 && InputFHManager.GetMouseDown(MouseButton.Right))
		{
			if (FFSNetwork.IsOnline && PlayerRegistry.OtherPlayersAreJoining())
			{
				UIMultiplayerNotifications.ShowSelectedCardWhileWaitingForPlayersToJoin();
			}
			else
			{
				Singleton<CardsHandManager>.Instance.DiscardActiveAbilityCard(cHeroSummonActor.Summoner, cHeroSummonActor.FindSummonCard(), null);
			}
		}
	}

	private static bool CanDeactivateSummon(CHeroSummonActor summon)
	{
		if (summon == null || !summon.IsUnderMyControl())
		{
			return false;
		}
		CActor currentActor = Choreographer.s_Choreographer.CurrentActor;
		if (currentActor == null && PhaseManager.IsInCardsSelectionPhase)
		{
			return true;
		}
		if (currentActor != summon && currentActor != summon.Summoner)
		{
			if (currentActor.IsHeroSummonByDefault())
			{
				return ((CHeroSummonActor)currentActor).Summoner == summon.Summoner;
			}
			return false;
		}
		return true;
	}

	private IEnumerator WaitForGeneralInfo()
	{
		if (GeneralInfo.CurrentModule == null)
		{
			float camRot = ((CameraController.s_CameraController.m_Camera == null) ? 0f : CameraController.s_CameraController.m_Camera.transform.rotation.eulerAngles.y);
			while (!InputFHManager.Controls.UI.ShowInfo.WasReleasedThisFrame())
			{
				yield return new WaitForEndOfFrame();
			}
			if (Math.Abs(((CameraController.s_CameraController.m_Camera == null) ? 0f : CameraController.s_CameraController.m_Camera.transform.rotation.eulerAngles.y) - camRot) < 4f)
			{
				GeneralInfo.TryShowGeneralInfoUnderCursor();
			}
		}
		else
		{
			GeneralInfo.CloseInfo();
		}
		m_WaitForGeneralInfoCoroutine = null;
	}

	private void LateUpdate()
	{
		CheckSelectionAndActingActors();
		if (_shouldUpdateParty)
		{
			Modules.SimpleActionBox.Refresh(immediate: true);
			Modules.PartyModule.gameObject.SetActive(value: true);
			GetService<PlayerViewService>().UpdatePartyModule(Modules.PartyModule);
			_shouldUpdateParty = false;
		}
		if (_shouldUpdateAttackModifiers)
		{
			GetService<ActionsViewService>().UpdateAttackModifiersModule(Modules.attackSummary);
			_shouldUpdateAttackModifiers = false;
		}
		if (_shouldUpdateReactionModules)
		{
			GetService<ReactionViewService>().UpdateReactionModules();
			_shouldUpdateReactionModules = false;
		}
		UpdateIfDirty();
	}

	private void UpdateIfDirty()
	{
		UpdateNewUIEnabled();
		if (!_isDirty || !NewUIEnabled)
		{
			return;
		}
		List<IUIViewModule> list = GetLayerStackModules().ToList();
		foreach (IUIViewModule allModule in Modules.AllModules)
		{
			bool active = list.Contains(allModule);
			allModule.GameObject.SetActive(active);
		}
		Modules.StateInfo.GameObject.SetActive(value: true);
		Modules.StateInfo.SetStateInfo(_phaseLayer.CurrentPhase, _phaseLayer.State, _infoLayer.State, _reactionLayer.State);
		DoLayerStackOperation(StateOperationType.Update);
		_isDirty = false;
	}

	private void UpdateNewUIEnabled(bool force = false)
	{
		bool flag = false;
		if (NewUIEnabled != _newUIEnabled)
		{
			_newUIEnabled = NewUIEnabled;
			flag = true;
		}
		if (!(flag | force))
		{
			return;
		}
		if (_newUIEnabled)
		{
			_isDirty = true;
			GameAction.ExecuteActionHandler = OnGameAction;
			return;
		}
		foreach (IUIViewModule allModule in Modules.AllModules)
		{
			allModule.GameObject.SetActive(value: false);
		}
		GameAction.ExecuteActionHandler = null;
	}

	private void OnGameAction(GameAction action, ref bool actionValid, ref bool executionFinished)
	{
		if (!OnGameActionRecieved(action))
		{
			GameAction.DefaultActionHandle(action, ref actionValid, ref executionFinished);
		}
	}

	private bool OnGameActionRecieved(GameAction action)
	{
		NetworkPlayer player = PlayerRegistry.GetPlayer(action.PlayerID);
		switch (action.ActionType)
		{
		case GameActionType.ReadyUpPlayer:
			GetService<NetworkViewService>().HandleNetworkPlayerReadyChanged(player, ready: true);
			break;
		case GameActionType.UnreadyPlayer:
			GetService<NetworkViewService>().HandleNetworkPlayerReadyChanged(player, ready: false);
			break;
		case GameActionType.ConfirmAction:
			GetService<NetworkViewService>().ConfirmAction(localExec: true);
			return true;
		case GameActionType.ReadyProceed:
			GetService<NetworkViewService>().GameProceed(localExec: true);
			return true;
		case GameActionType.ChangeAgility:
		{
			CPlayerActor.EAgility supplementaryDataIDMin = (CPlayerActor.EAgility)action.SupplementaryDataIDMin;
			CPlayerActor playerActor = GetService<PlayerViewService>().GetPlayerActor(action.ActorID);
			GetService<PlayerViewService>().SetBlinkbladeAgility(playerActor, supplementaryDataIDMin);
			break;
		}
		}
		return false;
	}

	private void OnSlotOwnershipChanged(PartySlotID slotid, NetworkPlayer player)
	{
		RefreshView();
	}

	private void ProcessMessageOnLayerStack(CMessageData messageData)
	{
		CheckShouldUpdateParty(messageData);
		_shouldUpdateAttackModifiers |= GetService<ActionsViewService>().CheckAdvantageChoice(messageData, Modules.attackSummary);
		if (!NewUIEnabled)
		{
			return;
		}
		if (_isTurnStarted)
		{
			if (messageData.m_Type == CMessageData.MessageType.EndTurn || messageData.m_Type == CMessageData.MessageType.EndedExtraTurn)
			{
				_isTurnStarted = false;
				_currentTurnMessageData.Clear();
			}
			else
			{
				_currentTurnMessageData.Add(messageData);
			}
		}
		else if (messageData.m_Type == CMessageData.MessageType.StartTurn)
		{
			_isTurnStarted = true;
		}
		_isDirty |= _phaseLayer.ProcessMessage(messageData);
		_isDirty |= _reactionLayer.ProcessMessage(messageData);
		_isDirty |= _infoLayer.ProcessMessage(messageData);
		_isDirty = true;
	}

	private void CheckShouldUpdateParty(CMessageData messageData)
	{
		if (messageData.m_Type != CMessageData.MessageType.SRLQueueDebugLog)
		{
			_shouldUpdateParty = true;
		}
	}

	private void CheckSelectionAndActingActors()
	{
		CPlayerActor cPlayerActor = Singleton<CardsHandManager>.Instance.CurrentHand?.PlayerActor;
		CActor currentActor = Choreographer.s_Choreographer.CurrentActor;
		if (_prevSelectedPlayer != cPlayerActor || _prevActingActor != currentActor)
		{
			_shouldUpdateParty = true;
		}
		_prevSelectedPlayer = cPlayerActor;
		_prevActingActor = currentActor;
	}

	private void DoLayerStackOperation(StateOperationType operationType)
	{
		_phaseLayer.Do(operationType);
		_reactionLayer.Do(operationType);
		_infoLayer.Do(operationType);
	}

	private IEnumerable<IUIViewModule> GetLayerStackModules()
	{
		return _phaseLayer.GetStateModules().Concat(_reactionLayer.GetStateModules().Concat(_infoLayer.GetStateModules()));
	}

	public bool IsAnyReactionModuleActive()
	{
		if (Modules.ReactionActiveBonusTracker.isActiveAndEnabled)
		{
			return true;
		}
		if (Modules.ReactionFrozenFistRecoverFromSelection.isActiveAndEnabled)
		{
			return true;
		}
		return false;
	}

	public static bool IsBackendWaitingForReaction()
	{
		if (!GameState.WaitingForGenericConfirmation && !GameState.WaitingForMercenarySpecialMechanicSlotChoice && !GameState.WaitingForPlayerActorToContinueAbility && !GameState.WaitingForPlayerToSelectDamageResponse)
		{
			return GameState.WaitingForPlayerActorToAvoidDamageResponse;
		}
		return true;
	}

	public void OnEvent(string eventKey, params object[] parameters)
	{
		if (GetService<ScenarioMechanicViewService>().Validate(ScenarioMechanics.MechanicsData, eventKey, parameters, out var isNew))
		{
			if (ScenarioMechanics.isActiveAndEnabled)
			{
				ScenarioMechanics.Init(Singleton<FHScenarioViewController>.InstanceFast.GetService<ScenarioMechanicViewService>().GetData(ScenarioMechanics.MechanicsData));
			}
			if (isNew)
			{
				Singleton<ScenarioInfoModule>.InstanceFast?.UpdateNewRuleNotification(active: true);
			}
		}
	}
}
