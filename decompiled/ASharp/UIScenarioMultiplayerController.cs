using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIScenarioMultiplayerController : Singleton<UIScenarioMultiplayerController>
{
	[SerializeField]
	private UIReadyTrackerBar readyTracker;

	[SerializeField]
	private UIScrollText currentPlayerUI;

	private string characterShownID;

	public CPlayerSelectingToAvoidDamageOrNot_MessageData damageMessage { get; private set; }

	public CPlayerSelectingToToggleShields_MessageData shieldMessage { get; private set; }

	public CAttackSummary.TargetSummary damageSummaryToBurnCardForAlly { get; private set; }

	public CPlayerActor playerActorToBurnCardForAlly { get; private set; }

	public UIScrollText CurrentPlayerUI => currentPlayerUI;

	public void HideMultiplayer()
	{
		UIMultiplayerNotifications.HideAllMultiplayerNotification();
		HidePlayerInfo();
		HideReadyTracker();
		InitiativeTrack.Instance.RefreshCharactersController();
		Singleton<CardsHandManager>.Instance.RefreshConnectionState();
		Singleton<UIScenarioDistributePointsManager>.Instance.Refresh();
		if (damageMessage != null)
		{
			RefreshDamagePhase(force: true);
		}
	}

	public void ShowMultiPlayer()
	{
		if (FFSNetwork.IsOnline)
		{
			InitiativeTrack.Instance.RefreshCharactersController();
			if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				OnCardSelection();
			}
		}
	}

	public void UpdateCharacterController(NetworkControllable controllable)
	{
		string characterIDByControllerID = ControllableRegistry.GetCharacterIDByControllerID(controllable.ID);
		RefreshWaitingNotifications();
		InitiativeTrack.Instance.RefreshCharacterController(characterIDByControllerID);
		readyTracker.RefreshReady(characterIDByControllerID);
		if (characterShownID == characterIDByControllerID)
		{
			ShowPlayerInfo(controllable.Controller);
		}
		if (damageMessage != null && damageMessage.m_ActorBeingAttacked is CPlayerActor cPlayerActor && cPlayerActor.CharacterClass.CharacterID == characterIDByControllerID)
		{
			RefreshDamagePhase(force: true);
		}
	}

	public void RefreshConnectionStatus(NetworkPlayer player)
	{
		for (int i = 0; i < player.MyControllables.Count(); i++)
		{
			string characterIDByControllerID = ControllableRegistry.GetCharacterIDByControllerID(player.MyControllables.ElementAt(i).ID);
			InitiativeTrack.Instance.RefreshCharacterController(characterIDByControllerID);
		}
	}

	public void ShowReadyTracker(bool bringToFront = false, bool showExhausted = false)
	{
		if (bringToFront)
		{
			UIUtility.BringToFront(readyTracker.gameObject);
		}
		List<string> characterIDs = (showExhausted ? (from s in ScenarioManager.Scenario.PlayerActors.Concat(ScenarioManager.Scenario.ExhaustedPlayers)
			select s.CharacterClass.CharacterID).ToList() : ScenarioManager.Scenario.PlayerActors.Select((CPlayerActor s) => s.CharacterClass.CharacterID).ToList());
		readyTracker.ShowCharactersTrackers(characterIDs, (string character) => showExhausted || ScenarioManager.Scenario == null || ScenarioManager.Scenario.PlayerActors.Exists((CPlayerActor e) => e.CharacterClass.ID == character));
		readyTracker.RefreshReady();
	}

	public void UpdateReadyPlayer(NetworkPlayer player, bool isReady)
	{
		readyTracker.RefreshReady(player, isReady);
		if (player.MyControllables.Any((NetworkControllable x) => x.IsAlive))
		{
			Singleton<CombatLogHandler>.Instance.AddLog(string.Format(LocalizationManager.GetTranslation(isReady ? "COMBAT_LOG_READY_PLAYER" : "COMBAT_LOG_UNREADY_PLAYER"), player.Username), CombatLogFilter.MULTIPLAYER);
		}
	}

	public void HideReadyTracker()
	{
		readyTracker.HideTrackers();
	}

	public void ShowPlayerInfo(CPlayerActor actor)
	{
		if (FFSNetwork.IsOnline)
		{
			characterShownID = actor.CharacterClass.CharacterID;
			int controllableID = ControllableRegistry.GetControllableID(actor);
			ShowPlayerInfo(ControllableRegistry.GetController(controllableID));
		}
	}

	private void ShowPlayerInfo(NetworkPlayer player)
	{
		if (player == null || PhaseManager.CurrentPhase == null)
		{
			currentPlayerUI.Hide();
		}
		else if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			currentPlayerUI.ShowText(string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_VIEW_PLAYER_DECK"), player.Username));
		}
		else if (player == PlayerRegistry.MyPlayer)
		{
			currentPlayerUI.ShowText(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_MY_TURN"));
		}
		else
		{
			currentPlayerUI.ShowText(string.Format(LocalizationManager.GetTranslation("GUI_MULTIPLAYER_PLAYER_TURN"), player.Username));
		}
	}

	public void HidePlayerInfo()
	{
		characterShownID = null;
		currentPlayerUI.Hide();
	}

	public void OnCardSelection()
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<CardsHandManager>.Instance.RefreshConnectionState();
			RefreshWaitingNotifications();
			if (Singleton<CardsHandManager>.Instance.CurrentHand?.PlayerActor != null)
			{
				ShowPlayerInfo(Singleton<CardsHandManager>.Instance.CurrentHand.PlayerActor);
			}
			ShowReadyTracker();
		}
	}

	public void OnStartTurn(CPlayerActor actor)
	{
		ShowPlayerInfo(actor);
	}

	public void OnUpdateAbilityHelpText(CAbility ability)
	{
		if (ability.TargetingActor is CPlayerActor actor)
		{
			ShowPlayerInfo(actor);
		}
	}

	public void OnEndTurn()
	{
		HidePlayerInfo();
	}

	public void OnShieldTogglePopup(CPlayerActor playerActor, CPlayerSelectingToToggleShields_MessageData messageData)
	{
		shieldMessage = messageData;
		if (playerActor != null)
		{
			ShowPlayerInfo(playerActor);
		}
		RefreshDamagePhase();
	}

	public void OnTakeDamage(CPlayerActor playerActor, CPlayerSelectingToAvoidDamageOrNot_MessageData messageData)
	{
		damageMessage = messageData;
		if (playerActor != null)
		{
			ShowPlayerInfo(playerActor);
		}
		RefreshDamagePhase();
	}

	public void OnShowChoosePlayerToBurnCard()
	{
		ShowPlayerInfo(GameState.HostCharacterIncludingDead);
	}

	public void OnChooseToBurnCardForAlly(CPlayerActor playerActor, CAttackSummary.TargetSummary summary)
	{
		damageSummaryToBurnCardForAlly = summary;
		playerActorToBurnCardForAlly = playerActor;
		if (playerActor != null)
		{
			ShowPlayerInfo(playerActor);
		}
		else
		{
			ShowPlayerInfo(GameState.HostCharacterIncludingDead);
		}
		RefreshDamagePhase();
	}

	private void RefreshDamagePhase(bool force = false)
	{
		if (damageSummaryToBurnCardForAlly != null)
		{
			Singleton<UIScenarioDistributePointsManager>.Instance.Hide();
			CAttackSummary.TargetSummary targetSummary = damageSummaryToBurnCardForAlly;
			CActor actorToAttack = targetSummary.ActorToAttack;
			CPlayerActor cPlayerActor = playerActorToBurnCardForAlly;
			if (cPlayerActor == null)
			{
				return;
			}
			CActor cActor = ((cPlayerActor != null) ? cPlayerActor : actorToAttack);
			bool flag = false;
			if (!(cActor is CPlayerActor actor))
			{
				if (!(cActor is CHeroSummonActor cHeroSummonActor))
				{
					if (cActor is CEnemyActor)
					{
						flag = FFSNetwork.IsHost;
					}
				}
				else
				{
					flag = cHeroSummonActor.Summoner.IsUnderMyControl();
				}
			}
			else
			{
				flag = actor.IsUnderMyControl();
			}
			if (flag)
			{
				Singleton<TakeDamagePanel>.Instance.ShowBurnCardForAlly(targetSummary.ActorToAttack, cPlayerActor, targetSummary.FinalAttackStrength, targetSummary.Pierce, targetSummary.AttackAbility == null, targetSummary.OriginalAbility, allowBurnCards: true, targetSummary.OriginalAbility?.AbilityType ?? CAbility.EAbilityType.None, targetSummary);
			}
			else
			{
				Singleton<TakeDamagePanel>.Instance.ShowBurnCardForAllyOtherPlayer(targetSummary.ActorToAttack, cPlayerActor, targetSummary.FinalAttackStrength, targetSummary.Pierce, targetSummary.AttackAbility == null, targetSummary.OriginalAbility, targetSummary);
			}
			if (!Singleton<TakeDamagePanel>.Instance.ThisPlayerHasTakeDamageControl || PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
			{
				ActionProcessor.SetState(ActionProcessorStateType.ProcessFreely, ActionPhaseType.ChooseCardsToBurnForAllyMonster, savePreviousState: true);
			}
			else
			{
				ActionProcessor.SetState(ActionProcessorStateType.Halted, ActionPhaseType.ChooseCardsToBurnForAllyMonster, savePreviousState: true);
			}
		}
		else if (shieldMessage != null)
		{
			CActor actorBeingAttacked = shieldMessage.m_ActorBeingAttacked;
			CPlayerActor actorToShowCardsFor = shieldMessage.m_ActorToShowCardsFor;
			CActor cActor2 = ((actorToShowCardsFor != null) ? actorToShowCardsFor : actorBeingAttacked);
			bool flag2 = false;
			if (!(cActor2 is CPlayerActor actor2))
			{
				if (!(cActor2 is CHeroSummonActor cHeroSummonActor2))
				{
					if (cActor2 is CEnemyActor)
					{
						flag2 = FFSNetwork.IsHost;
					}
				}
				else
				{
					flag2 = cHeroSummonActor2.Summoner.IsUnderMyControl();
				}
			}
			else
			{
				flag2 = actor2.IsUnderMyControl();
			}
			if (flag2)
			{
				Singleton<TakeDamagePanel>.Instance.ShowShields(actorBeingAttacked, actorToShowCardsFor, shieldMessage.m_ModifiedStrength, (shieldMessage.m_TargetSummary != null) ? Mathf.Max(0, shieldMessage.m_TargetSummary.Pierce - shieldMessage.m_TargetSummary.Shield) : 0, shieldMessage.m_DamagingAbility, shieldMessage.m_DamageType, shieldMessage.m_TargetSummary);
			}
			else
			{
				Singleton<TakeDamagePanel>.Instance.ShowShieldsOtherPlayer(actorBeingAttacked, actorToShowCardsFor, shieldMessage.m_ModifiedStrength, (shieldMessage.m_TargetSummary != null) ? Mathf.Max(0, shieldMessage.m_TargetSummary.Pierce - shieldMessage.m_TargetSummary.Shield) : 0, shieldMessage.m_DamagingAbility, shieldMessage.m_DamageType, shieldMessage.m_TargetSummary);
			}
		}
		else
		{
			if (damageMessage == null)
			{
				return;
			}
			CActor actorBeingAttacked2 = damageMessage.m_ActorBeingAttacked;
			CPlayerActor actorToShowCardsFor2 = damageMessage.m_ActorToShowCardsFor;
			CActor cActor3 = ((actorToShowCardsFor2 != null) ? actorToShowCardsFor2 : actorBeingAttacked2);
			bool flag3 = false;
			if (!(cActor3 is CPlayerActor actor3))
			{
				if (!(cActor3 is CHeroSummonActor cHeroSummonActor3))
				{
					if (cActor3 is CEnemyActor)
					{
						flag3 = FFSNetwork.IsHost;
					}
				}
				else
				{
					flag3 = cHeroSummonActor3.Summoner.IsUnderMyControl();
				}
			}
			else
			{
				flag3 = actor3.IsUnderMyControl();
			}
			if (FFSNetwork.IsOnline && !flag3)
			{
				Singleton<TakeDamagePanel>.Instance.ShowOtherPlayer(actorBeingAttacked2, actorToShowCardsFor2, damageMessage.m_ModifiedStrength, (damageMessage.m_TargetSummary != null) ? Mathf.Max(0, damageMessage.m_TargetSummary.Pierce - damageMessage.m_TargetSummary.Shield) : 0, damageMessage.m_IsDirectDamage, damageMessage.m_DamagingAbility, damageMessage.m_TargetSummary);
			}
			else if (!Singleton<TakeDamagePanel>.Instance.IsOpen | force)
			{
				Singleton<TakeDamagePanel>.Instance.Show(actorBeingAttacked2, actorToShowCardsFor2, damageMessage.m_ModifiedStrength, (damageMessage.m_TargetSummary != null) ? Mathf.Max(0, damageMessage.m_TargetSummary.Pierce - damageMessage.m_TargetSummary.Shield) : 0, damageMessage.m_IsDirectDamage, damageMessage.m_DamagingAbility, damageMessage.m_AllowBurnCards, damageMessage.m_DamageType, damageMessage.m_TargetSummary);
			}
		}
	}

	public void OnFinishToggleShields(bool hidePlayerInfo = true)
	{
		if (shieldMessage != null)
		{
			Choreographer.s_Choreographer.m_CurrentActor = shieldMessage.m_DamagingAbility.TargetingActor;
			shieldMessage = null;
			if (hidePlayerInfo)
			{
				HidePlayerInfo();
			}
		}
	}

	public void OnFinishTakeDamage(bool hidePlayerInfo = true)
	{
		damageMessage = null;
		if (hidePlayerInfo)
		{
			HidePlayerInfo();
		}
	}

	public void OnFinishBurnCardForAlly(bool hidePlayerInfo = true)
	{
		damageSummaryToBurnCardForAlly = null;
		playerActorToBurnCardForAlly = null;
		if (hidePlayerInfo)
		{
			HidePlayerInfo();
		}
	}

	public void RefreshWaitingNotifications()
	{
		bool flag;
		if (PlayerRegistry.OtherPlayersAreJoining())
		{
			flag = true;
			UIMultiplayerNotifications.ShowWaitingPlayersJoining();
		}
		else
		{
			flag = false;
			UIMultiplayerNotifications.HideWaitingPlayersJoining();
		}
		if (!flag && PlayerRegistry.AllPlayers.Count > 1 && PlayerRegistry.AllPlayers.Exists((NetworkPlayer x) => !x.IsParticipant))
		{
			if (PlayerRegistry.ConnectingUsers != null)
			{
				Log.GUIInfo("Connecting: " + PlayerRegistry.ConnectingUsers.Count + " Players:" + string.Join(", ", PlayerRegistry.AllPlayers.Select((NetworkPlayer it) => it.Username + "-" + it.IsParticipant)));
			}
			UIMultiplayerNotifications.ShowWaitingPlayersCharacterAssigned();
		}
		else
		{
			UIMultiplayerNotifications.HideWaitingPlayersCharacterAssigned();
		}
		if (FFSNetwork.IsHost && PlayerRegistry.JoiningPlayers.Count == 0 && PlayerRegistry.AllPlayers.Count == 1)
		{
			UIMultiplayerNotifications.ShowWaitingPlayersJoin();
		}
		else
		{
			UIMultiplayerNotifications.HideWaitingPlayersJoin();
		}
	}

	public void PingTile(CClientTile tile)
	{
		if (FFSNetwork.IsOnline)
		{
			Singleton<PingManager>.Instance.Ping3DElement(tile.m_TileBehaviour.gameObject, PlayerRegistry.MyPlayer);
			Synchronizer.SendSideAction(GameActionType.PingHex, new TileToken(tile.m_Tile.m_ArrayIndex));
		}
		else
		{
			Singleton<PingManager>.Instance.Ping3DElementSinglePlayer(tile.m_TileBehaviour.gameObject);
		}
	}
}
