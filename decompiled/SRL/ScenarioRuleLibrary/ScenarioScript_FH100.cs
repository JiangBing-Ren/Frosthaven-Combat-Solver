using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH100 : ScenarioScript
{
	private List<string> initializedCharacters = new List<string>();

	public static bool IsActive => ScenarioScript.current is ScenarioScript_FH100;

	public override string StartTurnHelpTextOverride => "FH_Scenario_Campaign_100_GUI_TOOLTIP_START_TURN";

	protected override bool Initialize()
	{
		ScenarioManager.CurrentScenarioState.FreeLongRest = true;
		ScenarioManager.CurrentScenarioState.FreeShortRest = true;
		return true;
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(initializedCharacters);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			initializedCharacters = JsonConvert.DeserializeObject<List<string>>(storage);
		}
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		if (_scenarioModifier.ScenarioModifierTriggerPhase != EScenarioModifierTriggerPhase.StartScenario || !(_tickActor is CPlayerActor cPlayerActor) || cPlayerActor.CharacterClass.ID == "HIVEID")
		{
			return;
		}
		foreach (CAbilityCard ownedAbilityCard in cPlayerActor.CharacterClass.OwnedAbilityCards)
		{
			cPlayerActor.CharacterClass.MoveAbilityCard(ownedAbilityCard, cPlayerActor.CharacterClass.GetCardPileList(ownedAbilityCard.CurrentCardPile), cPlayerActor.CharacterClass.GetCardPileList(CBaseCard.ECardPile.Taken), CCharacterClass.GetCardPileListKey(ownedAbilityCard.CurrentCardPile), CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.Taken));
		}
		MoveCardsFromTakenToHand(cPlayerActor, 2, nextToCurrent: false);
		initializedCharacters.Add(cPlayerActor.ActorGuid);
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		if (_tickAbility is CAbilityModeTransfer cAbilityModeTransfer && ScenarioScript.RoundNumber <= 1)
		{
			CPlayerActor cPlayerActor = cAbilityModeTransfer.TargetingActor as CPlayerActor;
			if (initializedCharacters.Contains(cPlayerActor.ActorGuid))
			{
				return;
			}
			foreach (CAbilityCard ownedAbilityCard in cPlayerActor.CharacterClass.OwnedAbilityCards)
			{
				if (ownedAbilityCard.CurrentCardPile != CBaseCard.ECardPile.Activated)
				{
					cPlayerActor.CharacterClass.MoveAbilityCard(ownedAbilityCard, cPlayerActor.CharacterClass.GetCardPileList(ownedAbilityCard.CurrentCardPile), cPlayerActor.CharacterClass.GetCardPileList(CBaseCard.ECardPile.Taken), CCharacterClass.GetCardPileListKey(ownedAbilityCard.CurrentCardPile), CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.Taken));
				}
			}
			MoveCardsFromTakenToHand(cPlayerActor, 2, nextToCurrent: false);
			initializedCharacters.Add(cPlayerActor.ActorGuid);
		}
		else if (_tickAbility is CAbilitySelectAndMoveCard cAbilitySelectAndMoveCard && cAbilitySelectAndMoveCard.GetMiscVar("IsInitialMove").GetBool())
		{
			CPlayerActor cPlayerActor2 = cAbilitySelectAndMoveCard.TargetingActor as CPlayerActor;
			CPlayerActor.EForm defaultForm = cPlayerActor2.CharacterClass.HandAbilityCards.Select((CAbilityCard card) => card.GetAbilityCardYML.RequiredForm).FirstOrDefault();
			if (defaultForm != CPlayerActor.EForm.None && cPlayerActor2.CharacterClass.HandAbilityCards.Concat(cPlayerActor2.CharacterClass.DiscardedAbilityCards).Concat(cPlayerActor2.CharacterClass.LostAbilityCards).Count((CAbilityCard x) => x.GetAbilityCardYML.RequiredForm == defaultForm) < 2)
			{
				MoveCardsFromTakenToHand(cPlayerActor2, 1, nextToCurrent: false);
			}
		}
	}

	private static void MoveCardsFromTakenToHand(CPlayerActor player, int amount, bool nextToCurrent = true, bool forceUsePendingScenarioModifier = false)
	{
		CAbility cAbility = CreateSelectAndMoveAbility(player, amount, "FH_Scenario_Campaign_100_SCENARIO_MODIFIER_4", new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Taken }, CBaseCard.ECardPile.Hand);
		if (!(ScenarioScript.current as ScenarioScript_FH100).initializedCharacters.Contains(player.ActorGuid))
		{
			cAbility.SetMiscVar("IsInitialMove", true);
		}
		if (PhaseManager.CurrentPhaseAction != null && !forceUsePendingScenarioModifier)
		{
			PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, player, killAfter: false, nextToCurrent);
			return;
		}
		GameState.PendingScenarioModifierAbilities.Add(Tuple.Create(cAbility, cAbility.TargetingActor));
		if ((PhaseManager.PhaseType != CPhase.PhaseType.StartScenarioEffects) & nextToCurrent)
		{
			PhaseManager.StartActiveBonusOrScenarioModifierAbility();
		}
	}

	public override CBaseCard.ECardPile OverrideMoveAbilityCardToPile(CActor actor, CBaseCard.ECardPile selectedPile, CAbilityCard abilityCard, string pileToMoveFromName)
	{
		if (selectedPile != CBaseCard.ECardPile.Lost)
		{
			return selectedPile;
		}
		if (PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest)
		{
			GameState.ClearPlayedCards();
		}
		MoveCardsFromTakenToHand((CPlayerActor)actor, 1, PhaseManager.PhaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest, forceUsePendingScenarioModifier: true);
		return CBaseCard.ECardPile.Discarded;
	}

	protected override void TickAvoidedDamageInternal(GameState.EAvoidDamageOption avoidOption, List<CAbilityCard> cardsBurnedToAvoidDamage, List<CActiveBonus> usedPreventDamageBonusesToAvoidDamage)
	{
		switch (avoidOption)
		{
		case GameState.EAvoidDamageOption.Lose1HandCard:
			MoveCardsFromTakenToHand((CPlayerActor)_tickActor, 1);
			break;
		case GameState.EAvoidDamageOption.Lose2DiscardCards:
			MoveCardsFromTakenToHand((CPlayerActor)_tickActor, 2);
			break;
		}
	}

	public override bool IsExhaustedOverride(CPlayerActor actor, out bool isExhausted)
	{
		CCharacterClass characterClass = actor.CharacterClass;
		int num = characterClass.HandAbilityCards.Count + characterClass.RoundAbilityCards.Count + characterClass.ActivatedAbilityCards.Count + characterClass.DiscardedAbilityCards.Count + characterClass.LostAbilityCards.Count + characterClass.PermanentlyLostAbilityCards.Count;
		isExhausted = num >= characterClass.NumberAbilityCardsInBattle;
		return true;
	}

	protected override void OnShortRestCompleteInternal()
	{
		MoveCardsFromTakenToHand((CPlayerActor)_tickActor, 1);
	}

	protected override void OnLongRestCompleteInternal()
	{
		MoveCardsFromTakenToHand((CPlayerActor)_tickActor, 1);
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickAbility is CAbilityRecoverLostCards)
		{
			CCharacterClass characterClass = _tickActor.CharacterClass;
			if (characterClass.HandAbilityCards.Count + characterClass.DiscardedAbilityCards.Count != 0)
			{
				CAbility item = CreateSelectAndMoveAbility(_tickActor, _tickAbility.Strength, "FH_Scenario_Campaign_100_SCENARIO_MODIFIER_6", new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Discarded }, CBaseCard.ECardPile.Taken);
				PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { item }, _tickActor, killAfter: false, stackToNextCurrent: true);
				_tickAbility.SetCancelAbilityFlag(cancel: true);
			}
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		if (!(_tickRoomRevealed.RoomName == "Room_3"))
		{
			return;
		}
		foreach (PlayerState player in ScenarioManager.CurrentScenarioState.Players)
		{
			CCharacterClass characterClass = player.Player.CharacterClass;
			if (characterClass.HandAbilityCards.Count + characterClass.DiscardedAbilityCards.Count != 0)
			{
				CAbility item = CreateSelectAndMoveAbility(player.Player, 1, "FH_Scenario_Campaign_100_SCENARIO_MODIFIER_6", new List<CBaseCard.ECardPile>
				{
					CBaseCard.ECardPile.Hand,
					CBaseCard.ECardPile.Discarded
				}, CBaseCard.ECardPile.Taken);
				PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { item }, player.Player);
			}
		}
	}

	public override string TakenPileTooltipKey()
	{
		return "FH_Scenario_Campaign_100_SCENARIO_MODIFIER_1";
	}

	public override string TakenPileNameKey()
	{
		return "GUI_CARD_SECTION_TAKEN_100";
	}

	public static bool IsCardValidForCurrentSelectAndMove(CAbilityCard AbilityCard)
	{
		CPlayerActor cPlayerActor = (PhaseManager.CurrentPhaseAction.CurrentPhaseAbility.m_Ability as CAbilitySelectAndMoveCard).TargetingActor as CPlayerActor;
		if (cPlayerActor.CharacterClass.ID == "GeminateID")
		{
			CPlayerActor.EForm eForm = cPlayerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.Select((CAbilityCard x) => x.GetAbilityCardYML.RequiredForm).FirstOrDefault();
			if (eForm != CPlayerActor.EForm.None)
			{
				return AbilityCard.GetAbilityCardYML.RequiredForm == eForm;
			}
			return true;
		}
		return true;
	}

	private static CAbility CreateSelectAndMoveAbility(CActor actor, int amount, string hint, List<CBaseCard.ECardPile> fromPiles, CBaseCard.ECardPile toPile)
	{
		List<(string, TValue)> miscVars = new List<(string, TValue)>
		{
			("SelectAndMoveCardPilesToMoveFrom", new TValue(fromPiles)),
			("SelectAndMoveCardPileToMoveTo", new TValue(toPile))
		};
		AbilityData.MiscAbilityData miscAbilityData = new AbilityData.MiscAbilityData
		{
			AutotriggerAbility = true,
			CanSkip = false
		};
		CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.SelectAndMoveCard, amount, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, miscAbilityData, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, hint, null, int.MaxValue, useParentTargeting: false, createInitialState: true, null, toDiscardPile: false, removeWhenDone: false, canExcludeCards: false, miscVars);
		cAbility.TargetingActor = actor;
		cAbility.TargetThisActorAutomatically = actor;
		cAbility.SkipAnim = true;
		return (CAbilitySelectAndMoveCard)cAbility;
	}
}
