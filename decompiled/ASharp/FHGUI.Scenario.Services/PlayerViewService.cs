using System;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using FHGUI.Common;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Scenario.DTO;
using FHGUI.Scenario.Modules;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;

namespace FHGUI.Scenario.Services;

public class PlayerViewService : FHScenarioViewService
{
	public CPlayerActor SelectedPlayerActor => Singleton<CardsHandManager>.Instance?.CurrentHand?.PlayerActor;

	public CActor CurrentActor => Choreographer.s_Choreographer?.CurrentActor;

	public CAbility CurrentAbility => Choreographer.s_Choreographer?.m_CurrentAbility;

	private NetworkViewService NetworkService => _view.GetService<NetworkViewService>();

	public PlayerViewService(FHScenarioViewController view)
		: base(view)
	{
	}

	public static bool IsActorUnderMyControl(CActor actor)
	{
		return actor?.IsUnderMyControl() ?? false;
	}

	public static bool IsPlayerSelectedTimeToken(CPlayerActor playerActor)
	{
		if (playerActor == null)
		{
			return false;
		}
		if (playerActor.UsesTimeTokens())
		{
			return playerActor.Agility != CPlayerActor.EAgility.None;
		}
		return true;
	}

	public static bool AreAllOwnedPlayerCardsHandSelectionReady()
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (IsActorUnderMyControl(playerActor) && !IsPlayerCardsHandSelectionReady(playerActor))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsPlayerCardsHandSelectionReady(CPlayerActor playerActor)
	{
		if (playerActor == null)
		{
			return false;
		}
		if (playerActor.PhasedOut)
		{
			return true;
		}
		if (!IsPlayerSelectedTimeToken(playerActor))
		{
			return false;
		}
		if (playerActor.CharacterClass.LongRest || playerActor.CharacterClass.ShortRest)
		{
			return true;
		}
		int num = 2;
		if (playerActor.MustPlayAllRoundCards)
		{
			num = playerActor.MaxCardsClampedToHand;
		}
		if (playerActor.CharacterClass.RoundAbilityCards.Count < num)
		{
			return false;
		}
		return true;
	}

	public bool AreAllPlayersReadyWithCardsHandSelection()
	{
		if (!Choreographer.s_Choreographer.PlayersInValidStartingPositions || Choreographer.s_Choreographer.AnyPlayersInInvalidStartingPositionsForCompanionSummons)
		{
			return false;
		}
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			if (!IsPlayerCardsHandSelectionReady(playerActor))
			{
				return false;
			}
		}
		return true;
	}

	public CPlayerActor GetPlayerActor(ECharacter eCharacter)
	{
		foreach (CPlayerActor partySlotOrderedPlayerActor in GetPartySlotOrderedPlayerActors())
		{
			if (partySlotOrderedPlayerActor.CharacterClass.CharacterModel == eCharacter)
			{
				return partySlotOrderedPlayerActor;
			}
		}
		return null;
	}

	public CPlayerActor GetPlayerActor(int actorID)
	{
		foreach (CPlayerActor partySlotOrderedPlayerActor in GetPartySlotOrderedPlayerActors())
		{
			if (partySlotOrderedPlayerActor.ID == actorID)
			{
				return partySlotOrderedPlayerActor;
			}
		}
		return null;
	}

	public bool CanShortRest(CPlayerActor playerActor)
	{
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(playerActor);
		if (playerActor.CharacterClass.DiscardedAbilityCards.Count > 1 && !hand.IsShortRestSelected() && !playerActor.CharacterClass.HasShortRested)
		{
			return PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
		}
		return false;
	}

	public void SetUseShortRest(CPlayerActor playerActor, bool isSelected, bool fromCardSelected = false)
	{
		if (!CanShortRest(playerActor))
		{
			throw new Exception("Actor '" + playerActor.CharacterClass.ID + "' cannot use short rest!");
		}
		if (FFSNetwork.IsOnline && Singleton<UIReadyToggle>.Instance != null && (Singleton<UIReadyToggle>.Instance.PlayersReady.Contains(PlayerRegistry.MyPlayer) || Singleton<UIReadyToggle>.Instance.IsProgressingBar))
		{
			Log.Info("Discarding short rest button click because this player is already marked as ready");
			return;
		}
		playerActor.CharacterClass.ShortRest = isSelected;
		CardsHandUI hand = Singleton<CardsHandManager>.Instance.GetHand(playerActor);
		if (isSelected)
		{
			hand.DeselectAllCards();
		}
		else
		{
			hand.ToggleHighlight(new List<CardPileType>
			{
				CardPileType.Hand,
				CardPileType.Round
			}, active: false);
		}
		if (!fromCardSelected)
		{
			hand.NetworkSelectedRoundCards();
			hand.UpdateInitiative();
		}
		hand.OnSelectedCardsNumberChanged();
		InitiativeTrack.Instance.CheckRoundAbilityCardsOrLongRestSelected();
		UIEventManager.AddUIEvent(new UIEvent(UIEvent.EUIEventType.ShortRestPressed));
		_view.RefreshView();
	}

	public bool CanUseLongRest(CPlayerActor playerActor)
	{
		if (playerActor.CharacterClass.DiscardedAbilityCards.Count > 1 && !playerActor.CharacterClass.HasLongRested)
		{
			return PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest;
		}
		return false;
	}

	public void SetUseLongRest(CPlayerActor playerActor, bool useLongRest)
	{
		if (!CanShortRest(playerActor))
		{
			throw new Exception("Actor '" + playerActor.CharacterClass.ID + "' cannot use long rest!");
		}
		if (PhaseManager.CurrentPhase.Type == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			playerActor.IsLongRestSelected = useLongRest;
		}
		else
		{
			playerActor.IsLongRestActionSelected = useLongRest;
		}
		playerActor.CharacterClass.LongRest = useLongRest;
		if (useLongRest)
		{
			CAbilityCard abilityCard = Singleton<CardsHandManager>.Instance.GetHand(playerActor).AllCards.First((AbilityCardUI c) => c.IsLongRest).AbilityCard;
			UpdateInitiative(playerActor, abilityCard);
		}
		else
		{
			UpdateInitiative(playerActor);
		}
		ScenarioRuleClient.GetAndReplicateStartRoundDeckState(playerActor, 32, FastReload.CurrentHistoryID);
		_view.RefreshView();
	}

	private void UpdateInitiative(CPlayerActor playerActor, params CAbilityCard[] abilityCards)
	{
		try
		{
			if (playerActor.SelectingCardsForExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.None && !GameState.IsPerformingCardOutOfPhase)
			{
				playerActor.CharacterClass.SetInitiativeAbilityCard((abilityCards.Length != 0) ? abilityCards[^1] : null);
				playerActor.CharacterClass.SetSubInitiativeAbilityCard((abilityCards.Length != 0) ? abilityCards[0] : null);
			}
			InitiativeTrack.Instance.UpdateActors();
		}
		catch (Exception ex)
		{
			Log.GUIException(ex, "An exception occurred within the CardHandsUI.UpdateInitiative().\n");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_UI_00009", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
		}
	}

	public void SetBlinkbladeAgility(CPlayerActor playerActor, CPlayerActor.EAgility agility, bool localOnly = false)
	{
		if (!playerActor.UsesTimeTokens())
		{
			throw new Exception("Tried to switch agility for actor " + playerActor.CharacterClass.ID + ", but UsesTimeTokens() = false");
		}
		if (agility == CPlayerActor.EAgility.None)
		{
			throw new Exception("Tried to switch None agility for actor " + playerActor.CharacterClass.ID);
		}
		if (playerActor.Agility != agility)
		{
			playerActor.Agility = agility;
			_view.RefreshView();
			if (!localOnly && NetworkService.IsOnline)
			{
				int controllableID = ControllableRegistry.GetControllableID(playerActor);
				Synchronizer.ReplicateControllableStateChange(GameActionType.ChangeAgility, ActionProcessor.CurrentPhase, controllableID, (int)playerActor.Agility);
			}
		}
	}

	public void UpdatePartyModule(ScenarioPartyColumnModule module)
	{
		GetPartyInfo(out var listParams, out var partySlotDtos);
		module.Init(listParams, partySlotDtos);
	}

	public void GetPartyInfo(out ScenarioPartyColumnModule.ListParams listParams, out List<ScenarioPartySlotDetailsDTO> partySlotDtos)
	{
		bool isInCardsSelectionPhase = PhaseManager.IsInCardsSelectionPhase;
		listParams = new ScenarioPartyColumnModule.ListParams
		{
			OnViewPartyClick = ViewParty,
			HintedActions = GetPartyColumnSlotInteractions,
			IsHintsContainerShown = isInCardsSelectionPhase
		};
		CActor selectedActor = GetActorMakingChoice();
		partySlotDtos = GetPartySlotOrderedPlayerActors().Select((CPlayerActor playerActor, int index) => new ScenarioPartySlotDetailsDTO(playerActor, index, isInCardsSelectionPhase, selectedActor == playerActor)).ToList();
		if (partySlotDtos.Count < 2)
		{
			listParams.HintedActions = null;
		}
	}

	public CActor GetActorMakingChoice()
	{
		if (Singleton<TakeDamagePanel>.IsInitialized && Singleton<TakeDamagePanel>.InstanceFast.IsOpen)
		{
			return Singleton<TakeDamagePanel>.InstanceFast.PlayerActorMakingChoice;
		}
		if (Choreographer.s_Choreographer.CurrentActor != null)
		{
			return Choreographer.s_Choreographer.CurrentActor;
		}
		if (Singleton<UIUseItemsBar>.IsInitialized && Singleton<UIUseItemsBar>.InstanceFast.HasShownUsableSlots() && Singleton<UIUseItemsBar>.InstanceFast.Actor is CPlayerActor result)
		{
			return result;
		}
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility != null)
		{
			return PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability.TargetingActor;
		}
		if (PhaseManager.CurrentPhase is CPhaseInfuserMechanicSelection cPhaseInfuserMechanicSelection)
		{
			return cPhaseInfuserMechanicSelection.TriggerActor;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.ActionSelection || PhaseManager.IsInCardsSelectionPhase)
		{
			return SelectedPlayerActor;
		}
		return null;
	}

	public IEnumerable<CPlayerActor> GetPartySlotOrderedPlayerActors()
	{
		if (ScenarioManager.Scenario?.PlayerActors == null)
		{
			yield break;
		}
		List<CPlayerActor> allScenarioPlayerActors = ScenarioManager.Scenario.PlayerActors.Concat(ScenarioManager.Scenario.ExhaustedPlayers).ToList();
		CMapParty cMapParty = SaveData.Instance?.Global?.CurrentAdventureData?.AdventureMapState?.MapParty;
		if (cMapParty == null || cMapParty.SelectedCharacters.Count() != allScenarioPlayerActors.Count)
		{
			foreach (CPlayerActor item in allScenarioPlayerActors)
			{
				if (item != null)
				{
					yield return item;
				}
			}
			yield break;
		}
		foreach (CMapCharacter mapCharacter in cMapParty.SelectedCharacters)
		{
			CPlayerActor cPlayerActor = allScenarioPlayerActors.FirstOrDefault((CPlayerActor cpa) => cpa.CharacterClass.ID == mapCharacter.CharacterID);
			if (cPlayerActor != null)
			{
				yield return cPlayerActor;
			}
		}
	}

	private IEnumerable<UIPartySlotHints.HintedActionParams> GetPartyColumnSlotInteractions(PartySlotDTO partySlotDto)
	{
		CPlayerActor playerActor = GetPlayerActor(partySlotDto.Character);
		if (partySlotDto.IsAvailable)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_SELECT",
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_SELECT"),
				ColorSchemeID = "PositiveAction",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					SwitchToPartySlot(playerActor);
				}
			};
		}
		yield return new UIPartySlotHints.HintedActionParams
		{
			HintKey = "GUI_VIEW_CHARACTER",
			HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_VIEW_CHARACTER"),
			ColorSchemeID = "PositiveAction",
			InteractionType = FHInputEventElement.UserInteractionType.RightClick,
			ActionCallback = () =>
			{
				ViewCharacterGeneralInfo(playerActor);
			}
		};
	}

	public void SwitchToPartySlot(CPlayerActor playerActor)
	{
		if (!playerActor.IsDead)
		{
			InitiativeTrack.Instance.Select(playerActor);
		}
	}

	public void ViewCharacterGeneralInfo(CPlayerActor playerActor)
	{
		if (InputFHManager.Controls.UI.ShowInfo.enabled)
		{
			IGeneralInfoData actor = UIGeneralInfoUtils.GetActor(playerActor);
			_view.GeneralInfo.RequestGeneralInfo(actor);
		}
	}

	public void ViewParty()
	{
		Singleton<CardsHandManager>.Instance.ShowAllDecks(openByKey: true);
	}
}
