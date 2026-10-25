using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class PhaseManager
{
	private static CPhase s_CurrentPhase;

	[StateDump(Skip = true)]
	private static CPhase s_PreviousPhase;

	public static CPhase.PhaseType PhaseType => s_CurrentPhase?.Type ?? CPhase.PhaseType.None;

	public static CPhase Phase => s_CurrentPhase;

	public static CPhase CurrentPhase => s_CurrentPhase;

	public static CPhase PreviousPhase => s_PreviousPhase;

	public static CPhaseAction CurrentPhaseAction => CurrentPhase as CPhaseAction;

	public static bool IsInActionSelectionPhase
	{
		get
		{
			if (PhaseType != CPhase.PhaseType.ActionSelection)
			{
				return PhaseType == CPhase.PhaseType.InfuserMechanicSelection;
			}
			return true;
		}
	}

	public static bool IsInCardsSelectionPhase
	{
		get
		{
			CPhase.PhaseType phaseType = PhaseType;
			return phaseType == CPhase.PhaseType.SelectAbilityCardsOrLongRest || phaseType == CPhase.PhaseType.ShortRest;
		}
	}

	public static CAbilityMove LastMoveAbility(CActor actor)
	{
		return CurrentPhaseAction?.CurrentPhaseAbilities?.Where((CPhaseAction.CPhaseAbility x) => !x.m_Ability.AbilityHasBeenCancelled && x.TargetingActor == actor && x.m_Ability.AbilityType == CAbility.EAbilityType.Move)?.LastOrDefault()?.m_Ability as CAbilityMove;
	}

	public static void Stop()
	{
		s_CurrentPhase = null;
	}

	public static void TileSelected(CTile tile, List<CTile> optionalTileList)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.TileSelected(tile, optionalTileList);
		}
	}

	public static void TileDeselected(CTile tile, List<CTile> optionalTileList)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.TileDeselected(tile, optionalTileList);
		}
	}

	public static void ApplySingleTarget(CActor actor)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.ApplySingleTarget(actor);
		}
	}

	public static void StepComplete(bool passingStep = false)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.StepComplete(passingStep);
		}
	}

	public static void NextStep(bool passing = false)
	{
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.NextStep(passing);
		}
	}

	public static void ApplyPerksWithFlag(EPerkFlag flag)
	{
		foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
		{
			foreach (PerksYMLData item in playerActor.CharacterClass.Perks.Where((PerksYMLData p) => (p.Flags & flag) != 0))
			{
				foreach (CAbility ability in item.Abilities)
				{
					GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(ability, playerActor));
				}
			}
		}
	}

	private static int ModifierPriorityStartScenario(CScenarioModifier modifier)
	{
		if (modifier is CScenarioModifierTriggerAbility)
		{
			return 0;
		}
		if (modifier is CScenarioModifierApplyConditionToActor)
		{
			return 1;
		}
		return 2;
	}

	public static void SetNextPhase(CPhase.PhaseType type)
	{
		Log.GameplayInfo($"Setting Next Phase: {type}    {GameState.InternalCurrentActor?.Class.ID} {GameState.InternalCurrentActor?.ArrayIndex}");
		bool flag = false;
		if (s_CurrentPhase != null)
		{
			s_CurrentPhase.EndPhase();
		}
		CActiveBonus.RefreshAllAuraActiveBonuses();
		switch (type)
		{
		case CPhase.PhaseType.StartScenarioEffects:
			s_CurrentPhase = new CPhaseStartScenarioEffects();
			ScenarioManager.CurrentScenarioState.TryCreateScenarioScripts();
			ApplyPerksWithFlag(EPerkFlag.ApplyActiveBonusesAtScenarioStart);
			foreach (CScenarioModifier item2 in (from x in ScenarioManager.CurrentScenarioState.ScenarioModifiers
				where x.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario
				orderby ModifierPriorityStartScenario(x)
				select x).ToList())
			{
				item2.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			ScenarioManager.CurrentScenarioState.TickPhaseCardScripts(EScenarioModifierTriggerPhase.StartScenario);
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				foreach (CAbility startScenarioAbility in playerActor.CharacterClass.CharacterYML.StartScenarioAbilities)
				{
					GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(startScenarioAbility, playerActor));
				}
			}
			break;
		case CPhase.PhaseType.StartRoundEffects:
			GameState.TurnOrderSoFar.Clear();
			s_CurrentPhase = new CPhaseStartRoundEffects();
			foreach (CSpawner spawner in ScenarioManager.CurrentScenarioState.Spawners)
			{
				spawner.StartRoundReset();
			}
			ScenarioManager.CurrentScenarioState.TryCreateScenarioScripts();
			foreach (CScenarioModifier item3 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound))
			{
				item3.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			ScenarioManager.CurrentScenarioState.TickPhaseCardScripts(EScenarioModifierTriggerPhase.StartRound);
			foreach (CPlayerActor actor2 in ScenarioManager.Scenario.PlayerActors)
			{
				if (actor2.WillBeExhaustedThisRound())
				{
					GameState.ShowToggleActiveBonusSelection(actor2, actor2, null, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.StartRoundIfExhausted, onlyInActionPhase: false);
					GameState.PendingActiveBonuses.AddRange(from x in CActiveBonus.FindApplicableActiveBonuses(actor2, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.StartRoundIfExhausted)
						where x.IsActiveBonusToggledAndNotRestricted(actor2)
						select x);
				}
			}
			foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
			{
				allAliveActor.ActorsAttackedThisRound.Clear();
				allAliveActor.TilesMovedThisRound = 0;
				allAliveActor.AbilityTypesPerformedThisTurn.Clear();
				GameState.PendingActiveBonuses.AddRange(CActiveBonus.FindApplicableActiveBonuses(allAliveActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.StartRoundAbility));
			}
			break;
		case CPhase.PhaseType.PlayerExhausted:
			s_CurrentPhase = new CPhasePlayerExhausted();
			break;
		case CPhase.PhaseType.Autosave:
			s_CurrentPhase = new CPhaseAutosave();
			ScenarioManager.CurrentScenarioState.TryCreateScenarioScripts();
			foreach (CPlayerActor playerActor2 in ScenarioManager.Scenario.PlayerActors)
			{
				CPlayerActor.LogHeroSummonMoveChanged(playerActor2);
			}
			break;
		case CPhase.PhaseType.SelectAbilityCardsOrLongRest:
			s_CurrentPhase = new CPhaseSelectAbilityCardsOrLongRest
			{
				SyncMultiplayer = true
			};
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			GameState.PendingOnLongRestBonuses.Clear();
			GameState.PendingOnShortRestBonuses.Clear();
			GameState.PendingOnRestAbilities.Clear();
			GameState.PendingActiveBonuses.Clear();
			GameState.PendingScenarioModifierAbilities.Clear();
			foreach (CPlayerActor playerActor3 in ScenarioManager.Scenario.PlayerActors)
			{
				playerActor3.CharacterClass.ResetInitiativeBonus(playerActor3);
			}
			foreach (CMonsterClass item4 in MonsterClassManager.MonsterAndObjectClassesWithInitiative)
			{
				item4.ResetInitiativeBonus(null);
			}
			foreach (CScenarioModifier item5 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRoundCardSelection))
			{
				item5.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			break;
		case CPhase.PhaseType.ShortRest:
			s_CurrentPhase = new CPhaseShortRest();
			break;
		case CPhase.PhaseType.MonsterClassesSelectAbilityCards:
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			GameState.SortIntoInitiativeAndIDOrder();
			foreach (CActor initiativeSortedActor in GameState.InitiativeSortedActors)
			{
				if (initiativeSortedActor is CPlayerActor item)
				{
					GameState.PreInitiativeAdjustedPlayerActors.Add(item);
				}
				bool playedThisRound = (initiativeSortedActor.PlayedAtLeastOnceThisRound = false);
				initiativeSortedActor.PlayedThisRound = playedThisRound;
			}
			s_CurrentPhase = new CPhaseSelectMonsterClassAbilityCards();
			foreach (CActor actor in ScenarioManager.Scenario.AllAliveActors)
			{
				GameState.ShowToggleActiveBonusSelection(actor as CPlayerActor, actor, null, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.PreCalcInitiativeAbility, onlyInActionPhase: false);
				IEnumerable<CActiveBonus> collection = from x in CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.PreCalcInitiativeAbility)
					where x.IsActiveBonusToggledAndNotRestricted(actor)
					select x;
				GameState.PendingActiveBonuses.AddRange(collection);
				GameState.ShowToggleActiveBonusSelection(actor as CPlayerActor, actor, null, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.GenericBeforeRevealMonsterCards, onlyInActionPhase: false);
				CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericBeforeRevealMonsterCards, null, actor, actor);
			}
			if (GameState.PendingActiveBonuses.Count > 0)
			{
				StartActiveBonusOrScenarioModifierAbility();
				return;
			}
			if (ScenarioManager.CurrentScenarioState.RoundNumber > 1)
			{
				break;
			}
			GameState.PendingCompanionSummonActors.Clear();
			foreach (CPlayerActor playerActor4 in ScenarioManager.Scenario.PlayerActors)
			{
				if (playerActor4.CharacterClass.CompanionSummonData != null)
				{
					GameState.PendingCompanionSummonActors.Add(playerActor4);
				}
			}
			if (GameState.PendingCompanionSummonActors.Count > 0)
			{
				StartActiveBonusOrScenarioModifierAbility();
				return;
			}
			break;
		case CPhase.PhaseType.CheckForInitiativeAdjustments:
			s_CurrentPhase = new CPhaseCheckForInitiativeAdjustments();
			break;
		case CPhase.PhaseType.CheckForForgoActionActiveBonuses:
			s_CurrentPhase = new CPhaseCheckForForgoActionActiveBonuses();
			break;
		case CPhase.PhaseType.StartTurn:
		{
			s_CurrentPhase = new CPhaseStartTurn();
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			GameState.PendingOnLongRestBonuses.Clear();
			GameState.PendingOnShortRestBonuses.Clear();
			GameState.PendingOnRestAbilities.Clear();
			GameState.PendingActiveBonuses.Clear();
			GameState.PendingScenarioModifierAbilities.Clear();
			CActor internalCurrentActor = GameState.InternalCurrentActor;
			foreach (CActiveBonus item6 in CharacterClassManager.FindAllActiveBonuses())
			{
				item6.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerTurn);
			}
			if (!internalCurrentActor.TurnStarted)
			{
				GameState.ReportStartTurn(internalCurrentActor);
			}
			if (internalCurrentActor.Tokens.HasKey(CCondition.EPositiveCondition.Regenerate) && !internalCurrentActor.IsDead && !internalCurrentActor.IsTakingExtraTurn && !internalCurrentActor.TurnStarted)
			{
				int health = internalCurrentActor.Health;
				CAbility cAbility = CAbilityHeal.CreateDefaultHeal(1, 1, 1, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
				cAbility.SkipAnim = true;
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData
				{
					AutotriggerAbility = true,
					IgnoreStun = true
				};
				cAbility.SetMiscVar("DontCopy", true);
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, internalCurrentActor));
				CRegenerateTriggered_MessageData message = new CRegenerateTriggered_MessageData(internalCurrentActor)
				{
					m_RegenActor = internalCurrentActor,
					m_ActorOriginalHealth = health,
					m_ActorWasAsleep = false
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			if (internalCurrentActor.Tokens.HasKey(CCondition.ENegativeCondition.Wound) && !internalCurrentActor.Tokens.HasKey(CCondition.EPositiveCondition.Regenerate) && !internalCurrentActor.IsDead && !internalCurrentActor.IsTakingExtraTurn && !internalCurrentActor.TurnStarted && !(internalCurrentActor is CEnemyActor { IsImmuneToDamage: not false }))
			{
				int health2 = internalCurrentActor.Health;
				CAbility cAbility2 = CAbilityDamage.CreateDefaultDamage(1, 1, 1, isMonster: false, "", isMultipass: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), "GameStateWoundAbility");
				cAbility2.SkipAnim = true;
				cAbility2.MiscAbilityData = new AbilityData.MiscAbilityData
				{
					AutotriggerAbility = true,
					IgnoreStun = true
				};
				cAbility2.SetMiscVar("DontCopy", true);
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility2, internalCurrentActor));
				CWoundTriggered_MessageData message2 = new CWoundTriggered_MessageData(internalCurrentActor)
				{
					m_WoundedActor = internalCurrentActor,
					m_ActorOriginalHealth = health2,
					m_ActorWasAsleep = false
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			foreach (CScenarioModifier item7 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartTurn))
			{
				if (!GameState.InternalCurrentActor.IsTakingExtraTurn)
				{
					item7.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
				}
			}
			ScenarioManager.CurrentScenarioState.TickPhaseCardScripts(EScenarioModifierTriggerPhase.StartTurn);
			foreach (CPlayerActor playerActor5 in ScenarioManager.Scenario.PlayerActors)
			{
				GameState.ShowItemSelection(playerActor5, GameState.InternalCurrentActor, null, CItem.EItemTrigger.AtStartOfTurn, onlyInActionPhase: false);
			}
			CClass cClass = GameState.InternalCurrentActor.Class;
			CCharacterClass characterClass = cClass as CCharacterClass;
			if (characterClass != null)
			{
				if (GameState.InternalCurrentActor.TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.TopAction && GameState.InternalCurrentActor.TakingExtraTurnOfType != CAbilityExtraTurn.EExtraTurnType.BottomAction)
				{
					List<CActiveBonus> list3 = (from x in characterClass.FindActiveBonuses(GameState.InternalCurrentActor)
						where x.IsSong
						select x).ToList();
					if (list3.Count > 0 && list3.Any((CActiveBonus a) => a.Caster.Class.ID == characterClass.ID))
					{
						GameState.InternalCurrentActor.GainXP(1);
					}
					GameState.ShowToggleActiveBonusSelection(GameState.InternalCurrentActor as CPlayerActor, GameState.InternalCurrentActor, null, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility, onlyInActionPhase: false);
					List<CActiveBonus> list4 = (from x in CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility)
						where !x.IsSong
						select x).ToList();
					list4.AddRange(characterClass.FindCasterActiveBonuses(CActiveBonus.EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns, GameState.InternalCurrentActor));
					list4.AddRange(from x in CharacterClassManager.FindAllSongActiveBonuses(GameState.InternalCurrentActor)
						where x.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility)
						select x);
					list4 = list4.OrderBy((CActiveBonus x) => x.Ability.ActiveBonusData.GetVar("SortOrder").GetInt()).ToList();
					if (list4.Count > 0)
					{
						GameState.PendingActiveBonuses = list4;
					}
				}
			}
			else
			{
				GameState.ShowToggleActiveBonusSelection(GameState.InternalCurrentActor as CPlayerActor, GameState.InternalCurrentActor, null, CAbility.EAbilityType.None, CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility, onlyInActionPhase: false);
				IEnumerable<CActiveBonus> collection2 = from x in CharacterClassManager.FindAllSongActiveBonuses(GameState.InternalCurrentActor)
					where x.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility) && x.Ability.ActiveBonusData.Filter.IsValidTarget(GameState.InternalCurrentActor, x.Actor, isTargetedAbility: false, useTargetOriginalType: false, false)
					select x;
				IEnumerable<CActiveBonus> collection3 = from x in CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.StartTurnAbility)
					where !x.IsSong
					select x;
				List<CActiveBonus> list5 = new List<CActiveBonus>();
				list5.AddRange(collection2);
				list5.AddRange(collection3);
				if (list5.Count > 0)
				{
					GameState.PendingActiveBonuses = list5;
				}
			}
			if (GameState.PendingScenarioModifierAbilities.Count > 0 || GameState.PendingActiveBonuses.Count > 0)
			{
				CStartTurn_MessageData message3 = new CStartTurn_MessageData(GameState.InternalCurrentActor, skipNextPhase: true);
				ScenarioRuleClient.MessageHandler(message3);
				StartActiveBonusOrScenarioModifierAbility(skipNextPhase: true);
				return;
			}
			break;
		}
		case CPhase.PhaseType.ActionSelection:
			if (!GameState.ActorHealthCheck(GameState.InternalCurrentActor, GameState.InternalCurrentActor) && ScenarioManager.Scenario.PlayerActors.Count > 0)
			{
				flag = true;
				GameState.EndTurnCheckNextActorAndMoveToNextPhase();
			}
			else
			{
				s_CurrentPhase = new CPhaseActionSelection();
			}
			break;
		case CPhase.PhaseType.InfuserMechanicSelection:
			if (!GameState.ActorHealthCheck(GameState.InternalCurrentActor, GameState.InternalCurrentActor) && ScenarioManager.Scenario.PlayerActors.Count > 0)
			{
				flag = true;
				GameState.EndTurnCheckNextActorAndMoveToNextPhase();
			}
			else
			{
				s_CurrentPhase = new CPhaseInfuserMechanicSelection();
			}
			break;
		case CPhase.PhaseType.Action:
			s_CurrentPhase = new CPhaseAction();
			break;
		case CPhase.PhaseType.EndTurnLoot:
		{
			s_CurrentPhase = new CPhaseEndTurnLoot();
			bool flag3 = false;
			if (ScenarioManager.Scenario.HasActor(GameState.InternalCurrentActor) && GameState.InternalCurrentActor.Type == CActor.EType.Player && !GameState.InternalCurrentActor.PhasedOut)
			{
				CTile cTile = ScenarioManager.Tiles[GameState.InternalCurrentActor.ArrayIndex.X, GameState.InternalCurrentActor.ArrayIndex.Y];
				List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.Loot);
				List<CLootActiveBonus_LimitLoot> lootLimitBehaviours = new List<CLootActiveBonus_LimitLoot>();
				foreach (CActiveBonus item8 in list)
				{
					if (item8.TryGetBehaviour(out CLootActiveBonus_LimitLoot behaviour))
					{
						lootLimitBehaviours.Add(behaviour);
					}
				}
				List<CObjectProp> list2 = cTile.m_Props.FindAll((CObjectProp x) => x.IsLootable && !x.IgnoreEndOfTurnLooting && x.CanActorLoot(GameState.InternalCurrentActor) && lootLimitBehaviours.All((CLootActiveBonus_LimitLoot y) => y.CanLootObject(GameState.InternalCurrentActor, x.ObjectType)));
				if (list2.Count > 0 && GameState.InternalCurrentActor.LootTile(cTile, asPartOfAbility: false, endOfTurn: true) && ScenarioManager.Scenario.HasActor(GameState.InternalCurrentActor))
				{
					flag3 = true;
					GameState.InternalCurrentActor.ShowLootAnimations("", list2);
				}
			}
			if (!flag3)
			{
				flag = true;
				GameState.NextPhase();
			}
			break;
		}
		case CPhase.PhaseType.AfterEndTurnLoot:
		{
			CPhaseAfterEndTurnLoot cPhaseAfterEndTurnLoot = (CPhaseAfterEndTurnLoot)(s_CurrentPhase = new CPhaseAfterEndTurnLoot());
			foreach (CScenarioModifier item9 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndTurnLoot))
			{
				item9.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
			}
			flag = true;
			if (s_CurrentPhase == cPhaseAfterEndTurnLoot)
			{
				GameState.NextPhase();
			}
			break;
		}
		case CPhase.PhaseType.EndTurn:
		{
			CPhaseEndTurn cPhaseEndTurn = (CPhaseEndTurn)(s_CurrentPhase = new CPhaseEndTurn());
			GameState.CheckAllPressurePlates();
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			GameState.PendingActiveBonuses.Clear();
			GameState.PendingScenarioModifierAbilities.Clear();
			if (!GameState.InternalCurrentActor.IsTakingExtraTurn || GameState.InternalCurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.BothActionsLater || GameState.InternalCurrentActor.TakingExtraTurnOfType == CAbilityExtraTurn.EExtraTurnType.SingleCardAnyActionAtTheEndOfTurn)
			{
				foreach (CScenarioModifier item10 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndTurn))
				{
					item10.PerformScenarioModifier(ScenarioManager.CurrentScenarioState.RoundNumber, GameState.InternalCurrentActor, ScenarioManager.CurrentScenarioState.Players.Count);
				}
				GameState.PendingActiveBonuses.AddRange(CActiveBonus.FindApplicableActiveBonuses(GameState.InternalCurrentActor, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.EndTurnAbility));
				foreach (CActiveBonus item11 in CharacterClassManager.FindAllActiveBonuses().Distinct())
				{
					item11.OnTurnEnded(GameState.InternalCurrentActor);
				}
				if (GameState.PendingScenarioModifierAbilities.Count > 0 || GameState.PendingActiveBonuses.Count > 0)
				{
					StartActiveBonusOrScenarioModifierAbility();
					return;
				}
			}
			ScenarioManager.CurrentScenarioState.TickPhaseCardScripts(EScenarioModifierTriggerPhase.EndTurn);
			if (s_CurrentPhase != cPhaseEndTurn)
			{
				flag = true;
			}
			break;
		}
		case CPhase.PhaseType.EndRound:
		{
			CPhaseEndRound cPhaseEndRound = (CPhaseEndRound)(s_CurrentPhase = new CPhaseEndRound());
			if (!GameState.EndRoundReached)
			{
				GameState.EndRoundReached = true;
				GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
				GameState.PendingActiveBonuses.Clear();
				GameState.PendingScenarioModifierAbilities.Clear();
				foreach (CScenarioModifier item12 in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.EndRound))
				{
					item12.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
				}
				ScenarioManager.CurrentScenarioState.TickPhaseCardScripts(EScenarioModifierTriggerPhase.EndRound);
				CGenericActiveBonus.FireEvent(CActiveBonus.EActiveBonusBehaviourType.GenericEndRound, null, null, null);
				foreach (CActor allAliveActor2 in ScenarioManager.Scenario.AllAliveActors)
				{
					foreach (CActiveBonus item13 in allAliveActor2.FindApplicableActiveBonuses())
					{
						if (item13.HasTracker)
						{
							item13.FinishIfNoRemaining();
						}
					}
					GameState.PendingActiveBonuses.AddRange(CActiveBonus.FindApplicableActiveBonuses(allAliveActor2, CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.EndRoundAbility));
				}
			}
			if (s_CurrentPhase != cPhaseEndRound)
			{
				flag = true;
			}
			if (GameState.PendingScenarioModifierAbilities.Count > 0 || GameState.PendingActiveBonuses.Count > 0)
			{
				StartActiveBonusOrScenarioModifierAbility();
				return;
			}
			break;
		}
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventPhase(type, ESESubTypePhase.PhaseStart, "", doNotSerialize: true));
		Log.GameplayInfo(string.Format("PHASE: {0}    {1} {2}", Log.ColorizeHex(type.ToString(), "00FF00"), GameState.InternalCurrentActor?.Class.ID, GameState.InternalCurrentActor?.ArrayIndex));
		ScenarioManager.CurrentScenarioState.TryCreateScenarioScripts();
		if (!flag)
		{
			s_CurrentPhase.NextStep();
		}
	}

	public static void StartItemAbilities(CItem item)
	{
		GameState.CurrentActionInitiator = GameState.EActionInitiator.ItemCard;
		item.ActionHasHappened = false;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), item.YMLData.Data.Abilities.Select((CAbility s) => CAbility.CopyAbility(s, generateNewID: false)).ToList(), 0), item);
		s_PreviousPhase = s_CurrentPhase;
		SetNextPhase(CPhase.PhaseType.Action);
	}

	public static void EndItemAbilities()
	{
		if (s_PreviousPhase != null)
		{
			if (CurrentPhaseAction?.CurrentPhaseAbility?.m_BaseCard is CItem { SlotState: CItem.EItemSlotState.Useable })
			{
				ElementInfusionBoardManager.RestoreState();
			}
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			if (s_CurrentPhase is CPhaseActionSelection)
			{
				GameState.InternalCurrentActor.Inventory.HighlightUsableItems(null, CItem.EItemTrigger.DuringOwnTurn);
			}
			s_CurrentPhase.NextStep();
		}
	}

	public static void ManuallySetCurrentPhase(CPhase phase)
	{
		if (phase is CPhaseActionSelection && s_PreviousPhase is CPhaseActionSelection)
		{
			s_PreviousPhase = null;
		}
		s_CurrentPhase = phase;
	}

	public static void StartAbilities(List<CAbility> abilities, CBaseCard baseCard, bool fullCopy = false, CActiveBonus fromActiveBonus = null, GameState.EActionInitiator actionInitiator = GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, CActor initiator = null)
	{
		if (CurrentPhase is CPhaseEndTurnLoot || CurrentPhase is CPhaseEndRound || CurrentPhase is CPhaseStartRoundEffects)
		{
			foreach (CAbility ability in abilities)
			{
				GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(ability, ability.TargetingActor ?? GameState.CurrentActor));
			}
			return;
		}
		GameState.CurrentActionInitiator = actionInitiator;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), abilities.Select((CAbility s) => CAbility.CopyAbility(s, generateNewID: false, fullCopy)).ToList(), 0), baseCard, initiator);
		s_PreviousPhase = s_CurrentPhase;
		SetNextPhase(CPhase.PhaseType.Action);
		if (s_CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.CurrentTriggeredActiveBonus = fromActiveBonus;
		}
	}

	public static void StartAbilities(CAbility ability, CActor actor, bool stackInsteadOfInject = false, CActor initiator = null, CItem item = null, bool dontCopy = false)
	{
		StartAbilities(new List<(CAbility, CActor)> { (ability, actor) }, stackInsteadOfInject, reverseOrder: false, initiator, item, dontCopy);
	}

	public static void StartAbilities(List<(CAbility ability, CActor actor)> abilitiesAndActors, bool stackInsteadOfInject = false, bool reverseOrder = false, CActor initiator = null, CItem item = null, bool dontCopy = false)
	{
		List<(CAbility, CActor)> list = abilitiesAndActors.ToList();
		if (CurrentPhaseAction == null)
		{
			if (CurrentPhase is CPhaseEndRound || CurrentPhase is CPhaseStartRoundEffects || CurrentPhase is CPhaseEndTurn)
			{
				GameState.PendingScenarioModifierAbilities.AddRange(abilitiesAndActors.Select(((CAbility ability, CActor actor) x) => new Tuple<CAbility, CActor>(x.ability, x.actor)).ToList());
				return;
			}
			(CAbility, CActor) tuple = list[0];
			list.RemoveAt(0);
			GameState.OverrideCurrentActorForOneAction(tuple.Item2, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
			StartAbility(tuple.Item1, tuple.Item1.GetAbilityBaseCard(tuple.Item2), fullCopy: false, null, asSubability: false, initiator);
		}
		if (CurrentPhaseAction == null)
		{
			foreach (var item5 in list)
			{
				StartAbility(item5.Item1, item5.Item1.GetAbilityBaseCard(item5.Item2), fullCopy: false, null, asSubability: false, initiator);
			}
			return;
		}
		if (reverseOrder & stackInsteadOfInject)
		{
			list.Reverse();
			{
				foreach (var item6 in list)
				{
					CPhaseAction currentPhaseAction = CurrentPhaseAction;
					List<CAbility> nextAbilities = new List<CAbility> { item6.Item1 };
					CActor item2 = item6.Item2;
					CItem item3 = item;
					currentPhaseAction.StackNextAbilities(nextAbilities, item2, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: false, addDirectlyToCurrentAbilities: false, forcedInterrupt: false, interruptRegardlessOfStrength: false, dontCopy, item3);
				}
				return;
			}
		}
		foreach (var item7 in list)
		{
			if (stackInsteadOfInject)
			{
				CPhaseAction currentPhaseAction2 = CurrentPhaseAction;
				List<CAbility> nextAbilities2 = new List<CAbility> { item7.Item1 };
				CActor item4 = item7.Item2;
				CItem item3 = item;
				currentPhaseAction2.StackNextAbilities(nextAbilities2, item4, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: false, addDirectlyToCurrentAbilities: false, forcedInterrupt: false, interruptRegardlessOfStrength: false, dontCopy, item3);
			}
			else
			{
				CurrentPhaseAction.InjectAbility(item7.Item1, item7.Item2, item7.Item1.GetAbilityBaseCard(item7.Item2));
			}
		}
	}

	public static void StartAbility(CAbility ability, CBaseCard baseCard, bool fullCopy = false, CActiveBonus fromActiveBonus = null, bool asSubability = false, CActor initiator = null)
	{
		StartAbilities(new List<CAbility> { ability }, baseCard, fullCopy, fromActiveBonus, asSubability ? GameState.EActionInitiator.AbilityCard : GameState.EActionInitiator.ActionsTriggeredOutsideActionPhase, initiator);
	}

	public static void EndAbilities()
	{
		if (s_PreviousPhase != null)
		{
			CPhase cPhase = s_CurrentPhase;
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			s_CurrentPhase.NextStep();
			cPhase?.EndPhase();
		}
	}

	public static void StartOverrideTurnAbilities(List<CAbility> abilities, CBaseCard baseCard)
	{
		GameState.CurrentActionInitiator = GameState.EActionInitiator.OverrideTurnAction;
		GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), abilities.Select((CAbility s) => CAbility.CopyAbility(s, generateNewID: false)).ToList(), 0), baseCard);
		s_PreviousPhase = s_CurrentPhase;
		SetNextPhase(CPhase.PhaseType.Action);
	}

	public static void EndOverrideTurnAbilities()
	{
		if (s_PreviousPhase != null)
		{
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
			GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
			GameState.EndTurnCheckNextActorAndMoveToNextPhase();
		}
	}

	public static void StartActiveBonusOrScenarioModifierAbility(bool skipNextPhase = false)
	{
		GameState.SkipNextPhase = skipNextPhase;
		GameState.CurrentActionInitiator = GameState.EActionInitiator.ActiveBonus;
		EndActiveBonusOrScenarioModifierAbility(start: true);
	}

	public static bool NextActiveBonusOrScenarioModifier()
	{
		if (GameState.PendingScenarioModifierAbilities != null && GameState.PendingScenarioModifierAbilities.Count > 0)
		{
			GameState.CurrentActionInitiator = GameState.EActionInitiator.ScenarioModifier;
			CAbility item = GameState.PendingScenarioModifierAbilities[0].Item1;
			GameState.OverrideCurrentActor(GameState.PendingScenarioModifierAbilities[0].Item2);
			GameState.PendingScenarioModifierAbilities.RemoveAt(0);
			if (item != null)
			{
				GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), new List<CAbility> { CAbility.CopyAbility(item, generateNewID: false, fullCopy: true) }, 0), null);
				GameState.CurrentAction.Action.Abilities[0].IsScenarioModifierAbility = true;
				s_PreviousPhase = s_CurrentPhase;
				SetNextPhase(CPhase.PhaseType.Action);
				return true;
			}
		}
		else
		{
			if ((GameState.PendingCompanionSummonActors != null) & (GameState.PendingCompanionSummonActors.Count > 0))
			{
				GameState.CurrentActionInitiator = GameState.EActionInitiator.CompanionSummon;
				CPlayerActor cPlayerActor = GameState.PendingCompanionSummonActors[0];
				GameState.PendingCompanionSummonActors.RemoveAt(0);
				GameState.SetCurrentActorForCompanionSummon(cPlayerActor);
				CAbility cAbility = CAbilitySummon.CreateCompanionSummon(cPlayerActor.CharacterClass.CompanionSummonData.ID);
				List<CScenarioModifier> list = ScenarioManager.CurrentScenarioState.ScenarioModifiers.FindAll((CScenarioModifier x) => x.ScenarioModifierType == EScenarioModifierType.OverrideCompanionSummonTiles && x.IsActiveInRound(ScenarioManager.CurrentScenarioState.RoundNumber));
				if (list.Count > 0)
				{
					CScenarioModifierOverrideCompanionSummonTiles cScenarioModifierOverrideCompanionSummonTiles = (CScenarioModifierOverrideCompanionSummonTiles)list[0];
					cAbility.IsSubAbility = true;
					cAbility.IsInlineSubAbility = true;
					cAbility.InlineSubAbilityTiles = new List<CTile>();
					foreach (TileIndex summonTileIndex in cScenarioModifierOverrideCompanionSummonTiles.SummonTileIndexes)
					{
						cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[summonTileIndex.X, summonTileIndex.Y]);
					}
				}
				GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), new List<CAbility> { cAbility }, 0), null);
				s_PreviousPhase = s_CurrentPhase;
				SetNextPhase(CPhase.PhaseType.Action);
				return true;
			}
			if (GameState.PendingActiveBonuses != null && GameState.PendingActiveBonuses.Count > 0)
			{
				CActiveBonus cActiveBonus = GameState.PendingActiveBonuses[0];
				GameState.PendingActiveBonuses.RemoveAt(0);
				if (cActiveBonus.Actor.IsDead)
				{
					return false;
				}
				GameState.CurrentActionInitiator = GameState.EActionInitiator.ActiveBonus;
				if (cActiveBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus)
				{
					bool flag = false;
					CAbility cAbility2 = CAbility.CopyAbility(cAbilityAddActiveBonus.AddAbility, generateNewID: false, fullCopy: true);
					if (cActiveBonus.IsAura && cActiveBonus.Caster != null && cAbility2 is CAbilityTargeting cAbilityTargeting)
					{
						cAbilityTargeting.SetOriginatingFromAura(cActiveBonus.Caster, cAbilityAddActiveBonus.ActiveBonusData.ActiveBonusAnimOverload, cAbilityAddActiveBonus.ActiveBonusData.AuraAnimTriggerAbilityType);
					}
					if (cActiveBonus is CEndTurnAbilityActiveBonus cEndTurnAbilityActiveBonus && cEndTurnAbilityActiveBonus.RequirementsMet(cActiveBonus.Actor) && cEndTurnAbilityActiveBonus.SetupTriggerContext(null, cAbility2, null, cActiveBonus.Actor, cActiveBonus.Actor))
					{
						flag = true;
						cEndTurnAbilityActiveBonus.AddAbility = cAbility2;
						GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cEndTurnAbilityActiveBonus);
					}
					else if (cActiveBonus is CStartTurnAbilityActiveBonus cStartTurnAbilityActiveBonus && cStartTurnAbilityActiveBonus.CanTriggerAbility())
					{
						flag = true;
						cStartTurnAbilityActiveBonus.AddAbility = cAbility2;
						GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cStartTurnAbilityActiveBonus);
						if (cActiveBonus.Ability.ActiveBonusData.TriggerOnCaster || cActiveBonus.Ability.ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.StartTurnAbilityAfterXCasterTurns))
						{
							GameState.OverrideCurrentActorForOneAction(cActiveBonus.Actor, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
							CUpdateCurrentActor_MessageData message = new CUpdateCurrentActor_MessageData(GameState.InternalCurrentActor);
							ScenarioRuleClient.MessageHandler(message);
						}
						CStartTurnActiveBonusTriggered_MessageData message2 = new CStartTurnActiveBonusTriggered_MessageData(GameState.InternalCurrentActor);
						ScenarioRuleClient.MessageHandler(message2);
					}
					else if (cActiveBonus is CStartRoundAbilityActiveBonus cStartRoundAbilityActiveBonus && cStartRoundAbilityActiveBonus.RequirementsMet(cActiveBonus.Actor))
					{
						flag = true;
						cStartRoundAbilityActiveBonus.AddAbility = cAbility2;
						GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cStartRoundAbilityActiveBonus);
						GameState.OverrideCurrentActorForOneAction(cActiveBonus.Actor, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
						CUpdateCurrentActor_MessageData message3 = new CUpdateCurrentActor_MessageData(GameState.InternalCurrentActor);
						ScenarioRuleClient.MessageHandler(message3);
					}
					else if (cActiveBonus is CEndRoundAbilityActiveBonus cEndRoundAbilityActiveBonus && cEndRoundAbilityActiveBonus.RequirementsMet(cActiveBonus.Actor))
					{
						flag = true;
						cEndRoundAbilityActiveBonus.AddAbility = cAbility2;
						GameState.PendingStartEndTurnAbilityBonusTriggers.Add(cEndRoundAbilityActiveBonus);
						GameState.OverrideCurrentActorForOneAction(cActiveBonus.Actor, null, killActorAfterAction: false, null, null, useControllingActorModifierDeck: false, useControllingActorPlayerControl: false, immediatelySetCurrentActor: true);
						CUpdateCurrentActor_MessageData message4 = new CUpdateCurrentActor_MessageData(GameState.InternalCurrentActor);
						ScenarioRuleClient.MessageHandler(message4);
					}
					if (flag)
					{
						GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), new List<CAbility> { cAbility2 }, 0), cActiveBonus.BaseCard);
						s_PreviousPhase = s_CurrentPhase;
						SetNextPhase(CPhase.PhaseType.Action);
						return true;
					}
					return NextActiveBonusOrScenarioModifier();
				}
				if (cActiveBonus.Ability is CAbilityPlaySong cAbilityPlaySong)
				{
					if (cActiveBonus is CEndTurnAbilityActiveBonus item2)
					{
						GameState.PendingStartEndTurnAbilityBonusTriggers.Add(item2);
					}
					else if (cActiveBonus is CStartTurnAbilityActiveBonus item3)
					{
						GameState.PendingStartEndTurnAbilityBonusTriggers.Add(item3);
						CStartTurnActiveBonusTriggered_MessageData message5 = new CStartTurnActiveBonusTriggered_MessageData(GameState.InternalCurrentActor);
						ScenarioRuleClient.MessageHandler(message5);
					}
					List<CAbility> list2 = new List<CAbility>();
					foreach (CSong.SongEffect songEffect in cAbilityPlaySong.Song.SongEffects)
					{
						if (songEffect.SongEffectType != CSong.ESongEffectType.Ability)
						{
							continue;
						}
						foreach (CAbility ability in songEffect.Abilities)
						{
							list2.Add(CAbility.CopyAbility(ability, generateNewID: false));
						}
					}
					GameState.CurrentAction = new GameState.CurrentActorAction(new CAction(new List<ElementInfusionBoardManager.EElement>(), new List<CActionAugmentation>(), list2, 0), cActiveBonus.BaseCard);
					s_PreviousPhase = s_CurrentPhase;
					SetNextPhase(CPhase.PhaseType.Action);
					return true;
				}
			}
		}
		return false;
	}

	public static void EndActiveBonusOrScenarioModifierAbility(bool start = false)
	{
		if (!((s_PreviousPhase != null) | start))
		{
			return;
		}
		if (s_PreviousPhase != null)
		{
			s_CurrentPhase = s_PreviousPhase;
			s_PreviousPhase = null;
		}
		if (NextActiveBonusOrScenarioModifier())
		{
			return;
		}
		GameState.CurrentActionInitiator = GameState.EActionInitiator.None;
		if (CurrentPhase == null || CurrentPhase.Type != CPhase.PhaseType.StartTurn)
		{
			if (GameState.InternalCurrentActor != null)
			{
				ElementInfusionBoardManager.ProcessEndTurnInfusions(GameState.InternalCurrentActor.Type);
			}
			else
			{
				ElementInfusionBoardManager.ProcessEndTurnInfusions();
			}
		}
		GameState.OverrideCurrentActor(GameState.TurnActor);
		if (!GameState.SkipNextPhase)
		{
			s_CurrentPhase.NextStep();
			return;
		}
		GameState.SkipNextPhase = false;
		GameState.NextPhase();
	}

	public static void Reset()
	{
		s_CurrentPhase = null;
		s_PreviousPhase = null;
	}
}
