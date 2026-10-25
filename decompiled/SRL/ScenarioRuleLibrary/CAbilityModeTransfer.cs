using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityModeTransfer : CAbility
{
	public enum EModeUse
	{
		Transfer = 1,
		ActivateMode,
		DeactivateMode
	}

	public enum EModeTransferState
	{
		SelectTargetSummon,
		TransferIngToTarget,
		TransferToTarget,
		SelectModeCardToSummonOrDeactivate,
		ClearModeBonuses,
		SummonModeActor,
		SelectModeCardToActivate,
		ActivateModeAbilities,
		Done
	}

	[JsonProperty]
	public int ModeAmountToActivate;

	[JsonProperty]
	public EModeUse ModeUsage;

	[JsonProperty]
	public bool DontSummon;

	public EModeTransferState m_State;

	public CHeroSummonActor ActorBeingSummoned;

	private CTile oldPosition;

	private List<CAbilityCard> validCards;

	private CAction selectedModeCardAction;

	private CAbilityCard summonCardOfKilledActor;

	private int idOfModeToActivate = -1;

	private int idOfSummonToKill = -1;

	private bool hadModeActivated;

	private bool skipTransferAbilitiesAnim;

	private bool wasStunnedAtStart;

	public CAbilityCard SelectedModeCard { get; private set; }

	private CPlayerActor playerActor
	{
		get
		{
			if (TargetingActor is CPlayerActor result)
			{
				return result;
			}
			return TargetingActor.GetMirrorActor() as CPlayerActor;
		}
	}

	public CAbilityCard LastDeactivatedModeCard { get; private set; }

	public CAbilityCard LastActivatedModeCard { get; private set; }

	public CHeroSummonActor LastSummonKilled { get; private set; }

	public CHeroSummonActor LastSummonSummoned { get; private set; }

	public void SetSelectedModeCard(CAbilityCard card)
	{
		SelectedModeCard = card;
	}

	public override void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		base.CopyInterruptedProperties(destAbility, strength, initialCopy, forced);
		(destAbility as CAbilityModeTransfer).wasStunnedAtStart = wasStunnedAtStart;
	}

	public CAbilityModeTransfer(EModeUse modeUsage, bool dontSummon)
	{
		ModeAmountToActivate = 1;
		ModeUsage = modeUsage;
		DontSummon = dontSummon;
		NumberTargets = 1;
		AbilityFilter = new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.OwnSummon);
		if (modeUsage == EModeUse.Transfer && Targeting != EAbilityTargeting.Custom)
		{
			Targeting = EAbilityTargeting.All;
		}
	}

	public bool UseFade()
	{
		return false;
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		wasStunnedAtStart = TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun);
		validCards = null;
		idOfSummonToKill = -1;
		idOfModeToActivate = -1;
		hadModeActivated = TargetingActor.Type == CActor.EType.Player && playerActor.CharacterClass.ActivatedAbilityCards.Any((CAbilityCard x) => x.HasModeActive());
		if (string.IsNullOrEmpty(AnimOverload) && ModeUsage == EModeUse.Transfer)
		{
			AnimOverload = "Transfer";
		}
		if (string.IsNullOrEmpty(AnimOverload2))
		{
			AnimOverload2 = "Assemble";
		}
		if (ModeUsage == EModeUse.Transfer)
		{
			m_ValidActorsInRange = new List<CActor>();
			if (Targeting == EAbilityTargeting.Custom)
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
			else
			{
				m_ValidActorsInRange = new List<CActor>(playerActor.GetAllHeroSummons());
			}
			SharedAbilityTargeting.RemoveInvalidActorsForAbilityMirrored(m_ValidActorsInRange, this);
			for (int num = m_ValidActorsInRange.Count - 1; num >= 0; num--)
			{
				CActor cActor = m_ValidActorsInRange[num];
				if (!playerActor.FlyingAfterConditions)
				{
					CNode node = ScenarioManager.PathFinder.GetNode(cActor.ArrayIndex);
					if (node.Blocked || !node.Walkable)
					{
						m_ValidActorsInRange.RemoveAt(num);
						continue;
					}
				}
				bool flag = true;
				foreach (ScenarioScript scenarioScript in ScenarioManager.CurrentScenarioState.ScenarioScripts)
				{
					if (!scenarioScript.IsTeleportValid(TargetingActor.Tile, cActor.Tile))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					m_ValidActorsInRange.RemoveAt(num);
				}
			}
			skipTransferAbilitiesAnim = true;
			SetHasStarted(val: true);
			m_State = EModeTransferState.SelectTargetSummon;
		}
		else if (ModeUsage == EModeUse.ActivateMode)
		{
			if (SelectedModeCard == null)
			{
				m_State = EModeTransferState.SelectModeCardToActivate;
				CPlayerActor cPlayerActor = (CPlayerActor)TargetingActor;
				validCards = cPlayerActor.CharacterClass.HandAbilityCards.Where((CAbilityCard card) => card.TopAction.ModeAbilities.Count > 0 || card.BottomAction.ModeAbilities.Count > 0).ToList();
			}
			else
			{
				m_State = EModeTransferState.ActivateModeAbilities;
			}
		}
		else if (ModeUsage == EModeUse.DeactivateMode)
		{
			m_State = EModeTransferState.SelectModeCardToSummonOrDeactivate;
			CPlayerActor cPlayerActor2 = (CPlayerActor)TargetingActor;
			validCards = cPlayerActor2.CharacterClass.ActivatedAbilityCards.Where((CAbilityCard card) => card.TopAction.IsModeActive(card) || card.BottomAction.IsModeActive(card)).ToList();
		}
		SetHasStarted(val: true);
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if ((m_State == EModeTransferState.SelectModeCardToActivate || m_State == EModeTransferState.SelectModeCardToSummonOrDeactivate) && validCards != null && validCards.Count == 0)
		{
			CNoValidCardsForAbility_MessageData message = new CNoValidCardsForAbility_MessageData(TargetingActor, this);
			ScenarioRuleClient.MessageHandler(message);
			return false;
		}
		bool startedOnDeathAbility;
		if (!ProcessIfDead)
		{
			if (!TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				if (!wasStunnedAtStart)
				{
					goto IL_011e;
				}
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData != null)
				{
					bool? ignoreStun = miscAbilityData.IgnoreStun;
					startedOnDeathAbility = true;
					if (ignoreStun == startedOnDeathAbility)
					{
						goto IL_011e;
					}
				}
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
				{
					CPlayerIsSleeping_MessageData message2 = new CPlayerIsSleeping_MessageData(TargetingActor);
					ScenarioRuleClient.MessageHandler(message2);
				}
				else
				{
					CPlayerIsStunned_MessageData message3 = new CPlayerIsStunned_MessageData(TargetingActor);
					ScenarioRuleClient.MessageHandler(message3);
				}
			}
			else
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		goto IL_011e;
		IL_011e:
		switch (m_State)
		{
		case EModeTransferState.SelectTargetSummon:
		{
			CHeroSummonActor cHeroSummonActor = GetMiscVar("AutoKillSummon").GetObject<CHeroSummonActor>();
			if (cHeroSummonActor == null && AutoTrigger)
			{
				cHeroSummonActor = ValidActorsInRange.FirstOrDefault() as CHeroSummonActor;
			}
			if (cHeroSummonActor != null)
			{
				ActorsToTarget.Clear();
				ActorsToTarget.Add(cHeroSummonActor);
				m_State = EModeTransferState.TransferIngToTarget;
				Perform();
				break;
			}
			if (TargetingActor.Type != CActor.EType.Player && TargetingActor.OriginalType == CActor.EType.Player)
			{
				SetCancelAbilityFlag(cancel: true);
				Perform();
				return false;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
			cActorIsSelectingTargetingFocus_MessageData.m_CanUndo = (miscAbilityData2 == null || miscAbilityData2.CanUndo != false) && cActorIsSelectingTargetingFocus_MessageData.m_CanUndo;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = IsPositive();
			cActorIsSelectingTargetingFocus_MessageData.m_CameraFocusActor = TargetingActor;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EModeTransferState.TransferIngToTarget:
		{
			if (ActorsToTarget.Count == 0)
			{
				SetCancelAbilityFlag(cancel: true);
				Perform();
				return false;
			}
			CActorIsModeTransfering_MessageData message6 = new CActorIsModeTransfering_MessageData(TargetingActor)
			{
				AnimOverload = AnimOverload,
				m_ModeTransferAbility = this,
				m_TeleportToTile = (ActorsToTarget[0] as CHeroSummonActor).Tile
			};
			ScenarioRuleClient.MessageHandler(message6);
			break;
		}
		case EModeTransferState.TransferToTarget:
		{
			if (ActorsToTarget.Count == 0)
			{
				SetCancelAbilityFlag(cancel: true);
				Perform();
				return false;
			}
			AbilityHasHappened = true;
			CHeroSummonActor cHeroSummonActor2 = (LastSummonKilled = ActorsToTarget[0] as CHeroSummonActor);
			CAbilityCard cAbilityCard3 = cHeroSummonActor2.FindSummonCard();
			summonCardOfKilledActor = cAbilityCard3;
			int num = cHeroSummonActor2.MaxHealth - cHeroSummonActor2.Health;
			CTile tile = cHeroSummonActor2.Tile;
			oldPosition = TargetingActor.Tile;
			AbilityHasHappened = true;
			idOfSummonToKill = playerActor.CharacterClass.ActivatedCards.IndexOf(summonCardOfKilledActor);
			GameState.TeleportActor(TargetingActor, tile, tryReveal: false, null, invokeOnMovedListeners: false, useAnim: false);
			CActorHasModeTransfered_MessageData message4 = new CActorHasModeTransfered_MessageData(TargetingActor)
			{
				m_ModeTransferAbility = this,
				m_DestinationTile = tile
			};
			ScenarioRuleClient.MessageHandler(message4);
			CCombatLogMessage_MessageData message5 = new CCombatLogMessage_MessageData
			{
				m_MessageKey = "COMBAT_LOG_PERFORM_TRANSFER",
				m_Actors = new List<CActor> { TargetingActor, cHeroSummonActor2 },
				m_Params = new List<string> { "Actor_0", "Actor_1" }
			};
			ScenarioRuleClient.MessageHandler(message5);
			GameState.KillActor(TargetingActor, cHeroSummonActor2, CActor.ECauseOfDeath.SummonActiveBonusCancelled, out startedOnDeathAbility, this, actorWasAsleep: false, null, instantlyDeletePrefab: true, dontLog: true);
			if (num > 0)
			{
				GameState.ActorBeenDamaged(TargetingActor, num, checkIfPlayerCanAvoidDamage: true, TargetingActor, null, EAbilityType.None, 0, isTrapDamage: false, isTerrainDamage: false, cannotPreventDamageWithActiveBonus: false, pierceInvulnerable: false, 0, triggerOnDamageListeners: true, 0, skipHitAnimation: true);
			}
			List<CCondition.EPositiveCondition> list2 = new List<CCondition.EPositiveCondition>();
			List<CCondition.ENegativeCondition> list3 = new List<CCondition.ENegativeCondition>();
			foreach (PositiveConditionPair checkPositiveToken in cHeroSummonActor2.Tokens.CheckPositiveTokens)
			{
				cHeroSummonActor2.Tokens.RemovePositiveToken(checkPositiveToken.PositiveCondition, checkPositiveToken.ConditionDecTrigger);
				list2.Add(checkPositiveToken.PositiveCondition);
			}
			foreach (NegativeConditionPair checkNegativeToken in cHeroSummonActor2.Tokens.CheckNegativeTokens)
			{
				cHeroSummonActor2.Tokens.RemoveNegativeToken(checkNegativeToken.NegativeCondition, checkNegativeToken.ConditionDecTrigger);
				list3.Add(checkNegativeToken.NegativeCondition);
			}
			if (list2.Count > 0 || list3.Count > 0)
			{
				CAbility cAbility = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: false, "", null, list2, list3);
				cAbility.Start(TargetingActor, TargetingActor);
				cAbility.ApplyToActor(TargetingActor);
			}
			foreach (CObjectProp item in TargetingActor.Tile.m_Props.ToList())
			{
				if (item.WillActivate(TargetingActor, actorIsJumping: false, TargetingActor.FlyingAfterConditions))
				{
					item.AutomaticActivate(TargetingActor);
				}
			}
			TargetingActor.m_OnMovedListeners?.Invoke(this, TargetingActor, new List<CActor>(), newActorCarried: false, 1, finalMovement: false, tile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), tile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1, oldPosition.m_ArrayIndex);
			PhaseManager.CurrentPhaseAction.StepComplete();
			break;
		}
		case EModeTransferState.SelectModeCardToSummonOrDeactivate:
		{
			CAbilityCard cAbilityCard2 = GetMiscVar("AutoDeactivateModeCard").GetObject<CAbilityCard>();
			if (cAbilityCard2 != null)
			{
				SelectedModeCard = cAbilityCard2;
				PhaseManager.CurrentPhaseAction.StepComplete();
				return false;
			}
			if (TargetingActor is CPlayerActor cPlayerActor)
			{
				List<CAbilityCard> list = cPlayerActor.CharacterClass.ActivatedAbilityCards.Where((CAbilityCard x) => x.SelectedAction != null && x.SelectedAction.IsModeActive(x)).ToList();
				if (list.Count > 1)
				{
					CSelectCardForMode_MessageData cSelectCardForMode_MessageData2 = new CSelectCardForMode_MessageData(AnimOverload, TargetingActor);
					cSelectCardForMode_MessageData2.m_ActorReplayingCard = TargetingActor;
					cSelectCardForMode_MessageData2.m_Ability = this;
					cSelectCardForMode_MessageData2.m_ValidCards = list.Select((CAbilityCard c) => c.ID).ToList();
					cSelectCardForMode_MessageData2.m_PileToChooseFrom = CBaseCard.ECardPile.Activated;
					ScenarioRuleClient.MessageHandler(cSelectCardForMode_MessageData2);
					return false;
				}
				if (list.Count > 0)
				{
					SelectedModeCard = list[0];
					PhaseManager.CurrentPhaseAction.StepComplete();
					return false;
				}
			}
			m_State = EModeTransferState.Done;
			PhaseManager.CurrentPhaseAction.StepComplete();
			break;
		}
		case EModeTransferState.ClearModeBonuses:
			return DeactivateMode(TargetingActor, SelectedModeCard, this);
		case EModeTransferState.SummonModeActor:
			ActorBeingSummoned = null;
			if (!DontSummon)
			{
				SummonModeActor(TargetingActor, selectedModeCardAction, SelectedModeCard, this, oldPosition);
			}
			if (summonCardOfKilledActor != null)
			{
				if (summonCardOfKilledActor.CurrentCardPile != CBaseCard.ECardPile.Hand)
				{
					playerActor.CharacterClass.MoveAbilityCard(summonCardOfKilledActor, playerActor.CharacterClass.GetCardPileList(summonCardOfKilledActor.CurrentCardPile), playerActor.CharacterClass.RoundAbilityCards, summonCardOfKilledActor.CurrentCardPile.ToString(), "RoundAbilityCards");
				}
				m_State = EModeTransferState.ActivateModeAbilities;
				SelectedModeCard = summonCardOfKilledActor;
				Perform();
				return false;
			}
			m_State = EModeTransferState.Done;
			PhaseManager.CurrentPhaseAction.StepComplete();
			return false;
		case EModeTransferState.SelectModeCardToActivate:
		{
			CAbilityCard cAbilityCard = GetMiscVar("AutoActivateModeCard").GetObject<CAbilityCard>();
			if (cAbilityCard != null)
			{
				SelectedModeCard = cAbilityCard;
				PhaseManager.CurrentPhaseAction.StepComplete();
			}
			else
			{
				CSelectCardForMode_MessageData cSelectCardForMode_MessageData = new CSelectCardForMode_MessageData(AnimOverload, TargetingActor);
				cSelectCardForMode_MessageData.m_ActorReplayingCard = TargetingActor;
				cSelectCardForMode_MessageData.m_Ability = this;
				cSelectCardForMode_MessageData.m_ValidCards = validCards.Select((CAbilityCard c) => c.ID).ToList();
				cSelectCardForMode_MessageData.m_PileToChooseFrom = CBaseCard.ECardPile.Hand;
				ScenarioRuleClient.MessageHandler(cSelectCardForMode_MessageData);
			}
			return false;
		}
		case EModeTransferState.ActivateModeAbilities:
			ActivateMode(TargetingActor, SelectedModeCard, this);
			return false;
		case EModeTransferState.Done:
			PhaseManager.NextStep();
			return true;
		}
		return false;
	}

	public static bool SummonModeActor(CActor hiveActor, CAction selectedModeCardAction, CBaseAbilityCard SelectedModeCard, CAbilityModeTransfer modeAbility, CTile oldPosition)
	{
		CAbilitySummon cAbilitySummon = CAbility.CopyAbility(selectedModeCardAction.Abilities.SingleOrDefault((CAbility x) => x.AbilityType == EAbilityType.Summon), generateNewID: false) as CAbilitySummon;
		cAbilitySummon.SkipAnim = modeAbility.skipTransferAbilitiesAnim;
		bool summoned = false;
		bool cancelAbility = false;
		List<CActor> list = new List<CActor>();
		if (CAbilitySummon.TrySpawnSummon(hiveActor, cAbilitySummon.SelectedSummonYMLData?.ID, cAbilitySummon.SelectedMonsterYMLData?.ID, new List<CTile> { oldPosition }, out var _, cAbilitySummon.AnimOverload, cAbilitySummon, null, list, out var _, ref summoned, ref cancelAbility, hiveActor?.GetMirrorAbilitiesActor()))
		{
			modeAbility.ActorBeingSummoned = list[0] as CHeroSummonActor;
			if (hiveActor is CPlayerActor cPlayerActor && modeAbility.idOfModeToActivate >= 0)
			{
				cPlayerActor.CharacterClass.ActivatedCards.Remove(SelectedModeCard);
				cPlayerActor.CharacterClass.ActivatedCards.Insert(Math.Max(0, Math.Min(cPlayerActor.CharacterClass.ActivatedCards.Count - 1, modeAbility.idOfModeToActivate)), SelectedModeCard);
			}
			if (modeAbility?.SelectedModeCard != null)
			{
				modeAbility.SelectedModeCard.SetSelectedAction(selectedModeCardAction);
				modeAbility.SelectedModeCard.ActionHasHappened = true;
				modeAbility.LastSummonSummoned = modeAbility.ActorBeingSummoned;
			}
			CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
			{
				m_MessageKey = "COMBAT_LOG_SUMMON",
				m_Actors = new List<CActor> { hiveActor, modeAbility.ActorBeingSummoned },
				m_Params = new List<string> { "Actor_0", "Actor_1" }
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		return false;
	}

	public static bool DeactivateMode(CActor hiveActor, CAbilityCard SelectedModeCard, CAbilityModeTransfer modeAbility = null, bool advancePhase = true)
	{
		if (SelectedModeCard == null)
		{
			return false;
		}
		CPlayerActor cPlayerActor = modeAbility.playerActor;
		if (modeAbility != null)
		{
			modeAbility.AbilityHasHappened = true;
			modeAbility.idOfModeToActivate = cPlayerActor.CharacterClass.ActivatedCards.IndexOf(SelectedModeCard);
			modeAbility.selectedModeCardAction = SelectedModeCard.SelectedAction;
			modeAbility.LastDeactivatedModeCard = SelectedModeCard;
		}
		foreach (CActiveBonus item in SelectedModeCard.ActiveBonuses.Where((CActiveBonus x) => SelectedModeCard.ModeAbilities().Any((CAbility mode) => mode.ID == x.Ability.ID)).ToList())
		{
			CClass.CancelActiveBonus(item);
		}
		if (modeAbility != null && hiveActor.Type == CActor.EType.Player && SelectedModeCard.CurrentCardPile != CBaseCard.ECardPile.None)
		{
			modeAbility.AbilityHasHappened = true;
			if (modeAbility.ModeUsage == EModeUse.DeactivateMode || modeAbility.DontSummon)
			{
				cPlayerActor.CharacterClass.MoveAbilityCard(SelectedModeCard, cPlayerActor.CharacterClass.GetCardPileList(SelectedModeCard.CurrentCardPile), cPlayerActor.CharacterClass.DiscardedAbilityCards, SelectedModeCard.CurrentCardPile.ToString() + "AbilityCards", "DiscardedAbilityCards");
				if (modeAbility.ModeUsage == EModeUse.DeactivateMode)
				{
					modeAbility.m_State = EModeTransferState.Done;
				}
			}
			else
			{
				cPlayerActor.CharacterClass.MoveAbilityCard(SelectedModeCard, cPlayerActor.CharacterClass.GetCardPileList(SelectedModeCard.CurrentCardPile), null, SelectedModeCard.CurrentCardPile.ToString() + "AbilityCards", null);
			}
		}
		if (advancePhase)
		{
			PhaseManager.CurrentPhaseAction.StepComplete();
		}
		return false;
	}

	public static bool ActivateMode(CActor hiveActor, CAbilityCard SelectedModeCard, CAbilityModeTransfer modeAbility = null, bool advancePhase = true)
	{
		CAction cAction = null;
		if (SelectedModeCard == null)
		{
			return false;
		}
		if (SelectedModeCard.TopAction.ModeAbilities.Count > 0)
		{
			cAction = SelectedModeCard.TopAction;
		}
		else
		{
			if (SelectedModeCard.BottomAction.ModeAbilities.Count <= 0)
			{
				modeAbility?.SetCancelAbilityFlag(cancel: true);
				if (advancePhase)
				{
					PhaseManager.NextStep();
				}
				return true;
			}
			cAction = SelectedModeCard.BottomAction;
		}
		if (modeAbility != null)
		{
			modeAbility.AbilityHasHappened = true;
			modeAbility.selectedModeCardAction = cAction;
			modeAbility.LastActivatedModeCard = SelectedModeCard;
		}
		CPlayerActor cPlayerActor = hiveActor as CPlayerActor;
		if (cPlayerActor != null && SelectedModeCard.CurrentCardPile != CBaseCard.ECardPile.None)
		{
			cPlayerActor.CharacterClass.MoveAbilityCard(SelectedModeCard, cPlayerActor.CharacterClass.GetCardPileList(SelectedModeCard.CurrentCardPile), null, SelectedModeCard.CurrentCardPile.ToString() + "AbilityCards", null);
			SelectedModeCard.CurrentCardPile = CBaseCard.ECardPile.Activated;
		}
		SelectedModeCard.SetSelectedAction(cAction);
		List<CAbility> list = new List<CAbility>();
		foreach (CAbility modeAbility2 in cAction.ModeAbilities)
		{
			CAbility cAbility = CAbility.CopyAbility(modeAbility2, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, modeAbility.VarsContext);
			cAbility.CachedAbilityBaseCard = SelectedModeCard;
			cAbility.SkipAnim = modeAbility.skipTransferAbilitiesAnim;
			list.Add(cAbility);
			if (modeAbility2.SubAbilities == null)
			{
				continue;
			}
			foreach (CAbility subAbility in modeAbility2.SubAbilities)
			{
				if (!subAbility.IsInlineSubAbility)
				{
					CAbility cAbility2 = CAbility.CopyAbility(subAbility, generateNewID: false);
					cAbility2.ParentAbility = modeAbility2;
					list.Add(cAbility2);
				}
			}
		}
		if (modeAbility != null && modeAbility.idOfSummonToKill != -1 && cPlayerActor != null)
		{
			cPlayerActor.CharacterClass.ActivatedCards.Remove(SelectedModeCard);
			cPlayerActor.CharacterClass.ActivatedCards.Insert(Math.Min(cPlayerActor.CharacterClass.ActivatedCards.Count, modeAbility.idOfSummonToKill), SelectedModeCard);
		}
		CModeCardChanged_MessageData message = new CModeCardChanged_MessageData(SelectedModeCard, hiveActor);
		ScenarioRuleClient.MessageHandler(message);
		if (!modeAbility.hadModeActivated && cAction.ActionXP > 0)
		{
			CustomAbility.StackXPAbility(cAction.ActionXP, modeAbility.playerActor, modeAbility.VarsContext);
		}
		PhaseManager.CurrentPhaseAction.StackNextAbilities(list, hiveActor, killAfter: false, stackToNextCurrent: true);
		if (advancePhase)
		{
			PhaseManager.CurrentPhaseAction.StepComplete();
		}
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (m_State == EModeTransferState.SelectTargetSummon)
		{
			SharedAbilityTargeting.TileSelected(this, selectedTile, AreaEffect, TargetingActor, FilterActor, Range, ref m_ValidActorsInRange, m_ActorsToIgnore, AbilityFilter, NumberTargets, ref m_NumberTargetsRemaining, m_ValidTilesInAreaEffectIncludingBlocked, ref m_ActorsToTarget, ref m_TilesSelected, IsTargetedAbility);
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		base.TileDeselected(selectedTile, optionalTileList);
		bool flag = false;
		if (m_State == EModeTransferState.SelectTargetSummon && SharedAbilityTargeting.TileDeselected(this, selectedTile, m_NumberTargetsRemaining, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected))
		{
			flag = true;
		}
		if (flag)
		{
			Perform();
		}
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override bool EnoughTargetsSelected()
	{
		return m_ActorsToTarget.Count > 0;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == TargetingActor.ActorGuid)?.IsSummon;
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbility(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this, defaultAction: false, (int)ModeUsage)));
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == EModeTransferState.SelectTargetSummon)
		{
			Perform();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState && m_State == EModeTransferState.SelectModeCardToSummonOrDeactivate && SelectedModeCard == null)
		{
			return false;
		}
		if (!dontMoveState && m_State < EModeTransferState.Done)
		{
			m_State++;
		}
		return m_State == EModeTransferState.Done;
	}

	public override bool CanClearTargets()
	{
		return m_State == EModeTransferState.SelectTargetSummon;
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		if (m_State == EModeTransferState.SelectTargetSummon && ValidActorsInRange.Count == 0)
		{
			return false;
		}
		return base.CanBePerformed(performer);
	}
}
