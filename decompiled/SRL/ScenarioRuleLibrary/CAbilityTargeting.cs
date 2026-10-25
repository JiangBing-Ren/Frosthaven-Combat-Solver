using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityTargeting : CAbility
{
	public enum TargetingState
	{
		None,
		ActorIsSelectingAoE,
		ActorIsSelectingTargetingFocus,
		PreProcessingBeforeActorApplies,
		ActorIsApplying,
		ApplyToActors,
		CheckForOneTargetAtATime,
		InlineSubAbilityCheck,
		Done
	}

	protected TargetingState m_State;

	protected bool m_OneTargetAtATime;

	protected bool m_SkipWaitForProgressChoreographer;

	protected CActor m_CameraFocusActor;

	private CActor m_LastActorSelected;

	public bool OneTargetAtATime => m_OneTargetAtATime;

	public bool AreaEffectSelected { get; private set; }

	public int ActiveBonusAddTargetBuff { get; private set; }

	public bool OriginatesFromAnAura { get; private set; }

	public CActor OriginatingAuraOwner { get; private set; }

	public string OriginatingAuraAnimOverload { get; private set; }

	public CActiveBonus.EAuraTriggerAbilityAnimType AuraTriggerAbilityAnimType { get; private set; }

	public TargetingState State => m_State;

	public CAbilityTargeting(EAbilityType abilityType)
	{
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		if (m_ValidActorsInRange.Count <= 0)
		{
			if (ActorsToTarget != null)
			{
				return ActorsToTarget.Count > 0;
			}
			return false;
		}
		return true;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		bool abilityStartListenersInvoked = AbilityStartListenersInvoked;
		base.Start(targetActor, filterActor, controllingActor);
		if (OneTargetAtATime || Targeting == EAbilityTargeting.Custom)
		{
			AreaEffect = null;
		}
		if (AreaEffect != null)
		{
			m_State = TargetingState.ActorIsSelectingAoE;
		}
		else
		{
			m_State = TargetingState.ActorIsSelectingTargetingFocus;
		}
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		m_ImmuneActorsInRange = new List<CActor>();
		if (UseSubAbilityTargeting || UseParentTargets)
		{
			if (AreaEffect == null && ParentAbility?.AreaEffect != null)
			{
				AreaEffect = ParentAbility.AreaEffect.Copy();
			}
			if (GetVar("ForceSubAbilityTargetingToSelectAOE").GetBool())
			{
				m_State = TargetingState.ActorIsSelectingAoE;
			}
			if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				m_ValidActorsInRange = new List<CActor> { TargetingActor };
				RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
				m_ActorsToTarget.AddRange(ValidActorsInRange);
			}
			else
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.AllTargetsAdjacentToParentMovePath.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentMovePath.Value && ParentAbility is CAbilityMove)
					{
						TilesInRange.Clear();
						m_ValidActorsInRange = new List<CActor>();
						for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPath.Count; i++)
						{
							if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
							{
								Point point = CAbilityMove.AllPlayerArrayIndexOnPath[i];
								CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
								if (cTile != null)
								{
									TilesInRange.AddRange(GameState.GetTilesInRange(cTile.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
								}
							}
						}
						TilesInRange = TilesInRange.Distinct().ToList();
						foreach (CTile item in TilesInRange)
						{
							if (item != null)
							{
								CActor actorOnTile = GameState.GetActorOnTile(item, TargetingActor, AbilityFilter, m_ActorsToIgnore, IsTargetedAbility, MiscAbilityData?.CanTargetInvisible, originalTargetType: false, CanSkipUntergetableCheck());
								if (actorOnTile != null && !m_ValidActorsInRange.Contains(actorOnTile))
								{
									m_ValidActorsInRange.Add(actorOnTile);
								}
							}
						}
						goto IL_0508;
					}
				}
				if (AreaEffect != null && ParentAbility != null)
				{
					if (ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
					{
						m_ValidTilesInAreaEffect = ParentAbility.ValidTilesInAreaAffected.Where(ValidTilePredicate).ToList();
						m_ValidTilesInAreaEffectIncludingBlocked = ParentAbility.ValidTilesInAreaEffectedIncludingBlocked.Where(ValidTilePredicate).ToList();
						m_ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffectIncludingBlocked);
						if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self) && !m_ValidActorsInRange.Contains(TargetingActor))
						{
							m_ValidActorsInRange.Add(TargetingActor);
						}
					}
				}
				else if (ParentAbility?.ActorsTargeted != null)
				{
					if (AbilityFilter.Equals(ParentAbility.AbilityFilter))
					{
						ValidActorsInRange = ParentAbility.ActorsTargeted.Where((CActor x) => !x.IsDead).ToList();
					}
					else
					{
						ValidActorsInRange = new List<CActor>();
						foreach (CActor item2 in ParentAbility.ActorsTargeted)
						{
							if (!item2.IsDead && AbilityFilter.IsValidTarget(item2, filterActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible, CanSkipUntergetableCheck()))
							{
								ValidActorsInRange.Add(item2);
							}
						}
					}
				}
				else if (InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
				{
					foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
					{
						CActor cActor = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
						if (cActor != null)
						{
							m_ValidActorsInRange.Add(cActor);
						}
					}
				}
			}
			goto IL_0508;
		}
		if (AreaEffect != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
			NumberTargets = m_ValidActorsInRange.Count;
			TilesInRange = GameState.GetTilesInRange(targetActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, MiscAbilityData?.IgnoreLOS ?? false);
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else if (targetActor != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else
		{
			ValidActorsInRange = new List<CActor>();
		}
		goto IL_05d9;
		IL_05d9:
		if (ParentAbility != null)
		{
			AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
			if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentTargets.HasValue)
			{
				AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
				if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentTargets.Value)
				{
					if (targetActor != null)
					{
						TilesInRange.Clear();
						m_ValidActorsInRange = new List<CActor>();
					}
					if (ParentAbility is CAbilityCreate cAbilityCreate)
					{
						foreach (CTile item3 in cAbilityCreate.TilesSelected)
						{
							TilesInRange.AddRange(from x in GameState.GetTilesInRange(item3.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, MiscAbilityData?.IgnoreLOS ?? false)
								where !TilesInRange.Contains(x)
								select x);
							m_ValidActorsInRange.AddRange(from x in GameState.GetActorsInRange(item3.m_ArrayIndex, TargetingActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible)
								where !m_ValidActorsInRange.Contains(x)
								select x);
						}
					}
					else if (ParentAbility is CAbilityCreateTerrain cAbilityCreateTerrain)
					{
						foreach (CTile item4 in cAbilityCreateTerrain.TilesSelected)
						{
							TilesInRange.AddRange(from x in GameState.GetTilesInRange(item4.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, MiscAbilityData?.IgnoreLOS ?? false)
								where !TilesInRange.Contains(x)
								select x);
							m_ValidActorsInRange.AddRange(from x in GameState.GetActorsInRange(item4.m_ArrayIndex, TargetingActor, Range, m_ActorsToIgnore, AbilityFilter, AreaEffect, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible)
								where !m_ValidActorsInRange.Contains(x)
								select x);
						}
					}
					else
					{
						for (int num = 0; num < ParentAbility.ActorsTargeted.Count; num++)
						{
							CActor cActor2 = ParentAbility.ActorsTargeted[num];
							if (cActor2 == null)
							{
								continue;
							}
							AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
							if (miscAbilityData5 != null && miscAbilityData5.UseParentTargets.HasValue)
							{
								AbilityData.MiscAbilityData miscAbilityData6 = MiscAbilityData;
								if (miscAbilityData6 != null && miscAbilityData6.UseParentTargets.Value)
								{
									goto IL_08cd;
								}
							}
							m_ActorsToIgnore.Add(cActor2);
							goto IL_08cd;
							IL_08cd:
							TilesInRange.AddRange(from x in GameState.GetTilesInRange(cActor2.ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, MiscAbilityData?.IgnoreLOS ?? false)
								where !TilesInRange.Contains(x)
								select x);
							m_ValidActorsInRange.AddRange(from x in GameState.GetActorsInRange(cActor2.ArrayIndex, TargetingActor, Range, m_ActorsToIgnore, AbilityFilter, null, m_ValidTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible)
								where !m_ValidActorsInRange.Contains(x)
								select x);
						}
					}
					m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
					if (UseSubAbilityTargeting || UseParentTargets)
					{
						NumberTargets = m_ValidActorsInRange.Count;
					}
				}
			}
		}
		if (UseSubAbilityTargeting && ValidActorsInRange.Count <= 0)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(targetActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, MiscAbilityData?.IgnoreLOS ?? false);
			NumberTargets = ValidActorsInRange.Count;
			if (!m_OneTargetAtATime || NumberTargets <= 1)
			{
				m_AllTargets = true;
			}
		}
		else
		{
			m_AllTargets = false;
		}
		if (NumberTargets == 0 && CancelIfNoValid)
		{
			NumberTargets = 1;
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.AddTarget).ToList();
		ActiveBonusAddTargetBuff = 0;
		if (list != null)
		{
			ActiveBonusAddTargetBuff = list.Sum((CActiveBonus x) => x.ReferenceStrength(this, null));
			if (AreaEffect == null)
			{
				NumberTargets += ActiveBonusAddTargetBuff;
			}
		}
		m_NumberTargetsRemaining = NumberTargets + ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_SkipWaitForProgressChoreographer = false;
		if (TargetingActor == null || TargetingActor.Type != CActor.EType.Player)
		{
			m_CanUndo = false;
		}
		else if (PhaseManager.CurrentPhaseAction != null)
		{
			CPhaseAction currentPhaseAction = PhaseManager.CurrentPhaseAction;
			if (currentPhaseAction.PreviousPhaseAbilities != null && currentPhaseAction.PreviousPhaseAbilities.Count > 0)
			{
				m_CanUndo = !currentPhaseAction.PreviousPhaseAbilities.Last().IsCostAbility;
			}
		}
		if (TargetingActor.Type == CActor.EType.Player && !IsPositive() && (MiscAbilityData == null || !MiscAbilityData.NegativeAbilityIsOptional.HasValue || !MiscAbilityData.NegativeAbilityIsOptional.Value) && m_ValidActorsInRange.Count > 0 && (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Ally) || AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self) || AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Companion)))
		{
			foreach (CActor item5 in m_ValidActorsInRange)
			{
				if (CActor.AreActorsAllied(TargetingActor.Type, item5.Type, TargetingActor, item5) || item5 == TargetingActor)
				{
					m_CanSkip = false;
					break;
				}
			}
		}
		if (!AbilityStartListenersInvoked)
		{
			AbilityStartListenersInvoked = true;
			TargetingActor.m_OnAbilityTargetingStartListeners?.Invoke(this);
		}
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		if (!abilityStartListenersInvoked)
		{
			TryAutoSelectTiles();
			LogEvent(ESESubTypeAbility.AbilityStart);
		}
		SetHasStarted(val: true);
		return;
		IL_0508:
		m_ValidActorsInRange = m_ValidActorsInRange.Distinct().ToList();
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		m_ActorsToTarget = m_ValidActorsInRange.ToList();
		NumberTargets = m_ValidActorsInRange.Count;
		goto IL_05d9;
		bool ValidTilePredicate(CTile tile)
		{
			if (Targeting == EAbilityTargeting.BlueHexesInArea)
			{
				return (ParentAbility.FlagsOfValidTiles[tile] & CAreaEffect.EHexFlag.Ally) != 0;
			}
			return true;
		}
	}

	public virtual bool PreProcessBeforeActorApplies()
	{
		return true;
	}

	public virtual bool ActorIsApplying(CActor actorApplying, List<CActor> actorsAppliedTo)
	{
		LogEvent(ESESubTypeAbility.ActorIsApplying);
		foreach (CActor item in actorsAppliedTo)
		{
			Strength = m_ModifiedStrength;
			SetStatBasedOnXFromTarget(item, actorApplying, StatIsBasedOnXEntries, AbilityFilter);
		}
		return true;
	}

	public virtual void OnActorIsApplying()
	{
		m_CanUndo = false;
		if (TargetingActor.Type == CActor.EType.Player)
		{
			TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			ScenarioRuleClient.FirstAbilityStarted();
		}
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
		if (!ProcessIfDead && m_State < TargetingState.CheckForOneTargetAtATime)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0095;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0095;
				}
			}
		}
		switch (m_State)
		{
		case TargetingState.ActorIsSelectingAoE:
			if (TargetingActor.Type == CActor.EType.Player && !AutoTrigger)
			{
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData2 = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData2.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData2.m_IsPositive = IsPositive();
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData2);
			}
			else
			{
				m_State = TargetingState.ActorIsSelectingTargetingFocus;
				Perform();
			}
			break;
		case TargetingState.ActorIsSelectingTargetingFocus:
		{
			if (m_OneTargetAtATime)
			{
				if (TargetingActor.Type == CActor.EType.Player && AreaEffect != null)
				{
					AreaEffect = null;
					AreaEffectSelected = true;
					m_NumberTargetsRemaining = ValidActorsInRange.Count;
					m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
					if (ValidActorsInRange.Count == 1)
					{
						TileSelected(ScenarioManager.Tiles[ValidActorsInRange[0].ArrayIndex.X, ValidActorsInRange[0].ArrayIndex.Y], null);
						PhaseManager.StepComplete();
						return true;
					}
					ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
				}
				for (int num = ValidActorsInRange.Count - 1; num >= 0; num--)
				{
					if (ValidActorsInRange[num].IsDead)
					{
						ValidActorsInRange.RemoveAt(num);
					}
				}
			}
			if (ValidActorsInRange.Count == 0 && AreaEffect == null && (TargetingActor.Type != CActor.EType.Player || m_AbilityHasHappened))
			{
				PhaseManager.NextStep();
				return true;
			}
			CActor.EType eType = TargetingActor.Type;
			AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
			if (miscAbilityData2 != null && miscAbilityData2.UseOriginalActor == true)
			{
				eType = TargetingActor.OriginalType;
			}
			if (UseSubAbilityTargeting)
			{
				if (m_ActorsToTarget.Count > 0)
				{
					m_State = TargetingState.PreProcessingBeforeActorApplies;
					Perform();
					break;
				}
				PhaseManager.NextStep();
				return true;
			}
			if (eType != CActor.EType.Player || AutoTrigger)
			{
				if (AreaEffect == null)
				{
					SortValidActorsInRange(TargetingActor, ref m_ValidActorsInRange);
					if (!AllTargets)
					{
						int num2 = ValidActorsInRange.Count - 1;
						while (num2 >= 0 && ValidActorsInRange.Count > NumberTargets)
						{
							ValidActorsInRange.RemoveAt(num2);
							num2--;
						}
					}
				}
				if (Targeting != EAbilityTargeting.Custom || ActorsToTarget.Count == 0)
				{
					m_ActorsToTarget.Clear();
					m_ActorsToTarget.AddRange(ValidActorsInRange);
					bool flag2 = false;
					AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
					if (miscAbilityData3 != null && miscAbilityData3.AlsoTargetSelf == true)
					{
						m_ActorsToTarget.Add(TargetingActor);
						NumberTargets++;
						flag2 = true;
					}
					if (MiscAbilityData?.AlsoTargetAdjacent != null)
					{
						CAbilityFilter alsoTargetAdjacent = MiscAbilityData.AlsoTargetAdjacent;
						foreach (CActor allAdjacentActor in ScenarioManager.GetAllAdjacentActors(TargetingActor))
						{
							if (alsoTargetAdjacent.IsValidTarget(allAdjacentActor, TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false))
							{
								m_ActorsToTarget.Add(allAdjacentActor);
								NumberTargets++;
							}
						}
						flag2 = true;
					}
					if (flag2)
					{
						m_ActorsToTarget = m_ActorsToTarget.Distinct().ToList();
					}
				}
				m_TilesSelected.Clear();
				foreach (CActor item2 in m_ActorsToTarget)
				{
					m_TilesSelected.Add(ScenarioManager.Tiles[item2.ArrayIndex.X, item2.ArrayIndex.Y]);
				}
				m_State = TargetingState.PreProcessingBeforeActorApplies;
				Perform();
				break;
			}
			bool flag3 = false;
			if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || m_AllTargets)
			{
				m_ActorsToTarget.AddRange(ValidActorsInRange);
				flag3 = true;
			}
			else if (m_OneTargetAtATime && !m_AllTargets)
			{
				m_ActorsToTarget.Clear();
			}
			AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
			if (miscAbilityData4 != null && miscAbilityData4.AlsoTargetSelf == true)
			{
				m_ActorsToTarget.Add(TargetingActor);
				NumberTargets++;
				flag3 = true;
			}
			if (MiscAbilityData?.AlsoTargetAdjacent != null)
			{
				CAbilityFilter alsoTargetAdjacent2 = MiscAbilityData.AlsoTargetAdjacent;
				foreach (CActor allAdjacentActor2 in ScenarioManager.GetAllAdjacentActors(TargetingActor))
				{
					if (alsoTargetAdjacent2.IsValidTarget(allAdjacentActor2, TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false))
					{
						m_ActorsToTarget.Add(allAdjacentActor2);
						NumberTargets++;
					}
				}
				flag3 = true;
			}
			if (flag3)
			{
				m_ActorsToTarget = m_ActorsToTarget.Distinct().ToList();
			}
			TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
			cActorIsSelectingTargetingFocus_MessageData.m_CanUndo = (miscAbilityData5 == null || miscAbilityData5.CanUndo != false) && cActorIsSelectingTargetingFocus_MessageData.m_CanUndo;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = IsPositive();
			cActorIsSelectingTargetingFocus_MessageData.m_CameraFocusActor = ((m_CameraFocusActor != null) ? m_CameraFocusActor : TargetingActor);
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case TargetingState.PreProcessingBeforeActorApplies:
			if (PreProcessBeforeActorApplies())
			{
				m_State = TargetingState.ActorIsApplying;
				Perform();
			}
			break;
		case TargetingState.ActorIsApplying:
			OnActorIsApplying();
			foreach (CActor item3 in m_ActorsToTarget)
			{
				CTile item = ScenarioManager.Tiles[item3.ArrayIndex.X, item3.ArrayIndex.Y];
				if (!m_TilesSelected.Contains(item))
				{
					TilesSelected.Add(item);
				}
			}
			if (ActorIsApplying(TargetingActor, m_ActorsToTarget))
			{
				m_State = TargetingState.ApplyToActors;
				Perform();
			}
			break;
		case TargetingState.ApplyToActors:
		{
			m_CanUndo = false;
			AbilityHasHappened = true;
			if ((Flags & EAbilityFlag.ModifiesTrophyTokens) != 0 && TargetingActor is CPlayerActor cPlayerActor)
			{
				cPlayerActor.SetTrophyTokens(cPlayerActor.GetTrophyTokens() + Tokens);
			}
			List<CActor> list2 = new List<CActor>();
			if (m_OneTargetAtATime && m_LastActorSelected != null)
			{
				list2 = new List<CActor> { m_LastActorSelected };
				m_LastActorSelected = null;
			}
			else if (AreaEffect == null)
			{
				if (AllTargetsOnAttackPath && m_ActorsToTarget.Count > 0)
				{
					List<Point> list3 = ScenarioManager.PathFinder.FindPath(TargetingActor, TargetingActor.ArrayIndex, m_ActorsToTarget[0].ArrayIndex, Range > 1, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, IgnoreDoorLocks);
					if (foundPath)
					{
						foreach (Point point in list3)
						{
							CActor cActor = ValidActorsInRange.Find((CActor x) => x.ArrayIndex == point);
							if (cActor != null && !m_ActorsToTarget.Contains(cActor))
							{
								m_ActorsToTarget.Add(cActor);
							}
						}
					}
				}
				List<CActor> list4 = list2;
				List<CActor> collection = (ValidActorsInRange = m_ActorsToTarget.Where((CActor w) => !m_ActorsToIgnore.Contains(w)).ToList());
				list4.AddRange(collection);
			}
			else
			{
				list2.AddRange(ValidActorsInRange);
			}
			list2 = list2.Distinct().ToList();
			List<CActor> list6 = new List<CActor>();
			foreach (CActor item4 in list2)
			{
				bool flag = ApplyToActor(item4);
				if (flag)
				{
					list6.Add(item4);
				}
				if (flag && this is CAbilityCondition)
				{
					TargetingActor.m_OnConditionApplyToActorListeners?.Invoke(this, item4);
					item4.m_OnConditionApplyToActorListeners?.Invoke(this, item4);
				}
				if (!GetVar("MultipleApplyToOneActor").GetBool())
				{
					m_ActorsToIgnore.Add(item4);
					if (AbilityInterruptCopy != null)
					{
						CopyInterruptedProperties(AbilityInterruptCopy, AbilityInterruptCopy.Strength, initialCopy: false, forced: false);
					}
				}
				if (!m_OneTargetAtATime)
				{
					continue;
				}
				m_UndoNumberTargetsRemaining--;
				if (!GetVar("MultipleApplyToOneActor").GetBool())
				{
					ValidActorsInRange.Remove(item4);
					if (AbilityInterruptCopy != null)
					{
						CopyInterruptedProperties(AbilityInterruptCopy, AbilityInterruptCopy.Strength, initialCopy: false, forced: false);
					}
				}
				if (!flag)
				{
					PhaseManager.StepComplete();
					return true;
				}
				break;
			}
			if (list6.Count > 0)
			{
				CBaseCard cBaseCard = TargetingActor.FindCardWithAbility(this);
				if (cBaseCard != null)
				{
					if (!ActiveBonusData.IsAura && list2 != null && list2.Count > 0 && PositiveConditions.Count == 0 && LogApplyToAfterApplied())
					{
						CAbilityTargetingCombatLog_MessageData message = new CAbilityTargetingCombatLog_MessageData(AnimOverload, TargetingActor)
						{
							m_Ability = this,
							m_ActorsAppliedTo = list6.ToList()
						};
						ScenarioRuleClient.MessageHandler(message);
						foreach (CMessageData logAfterAppliedMessage in m_LogAfterAppliedMessages)
						{
							ScenarioRuleClient.MessageHandler(logAfterAppliedMessage);
						}
						m_LogAfterAppliedMessages.Clear();
					}
					if (PositiveConditions.Count > 0)
					{
						List<CActiveBonus> list7 = cBaseCard.ActiveBonuses.Where((CActiveBonus activeBonus) => activeBonus.Ability == this).ToList().FindAll((CActiveBonus x) => x.IsAura && x.BespokeBehaviour.BehaviourType != CActiveBonus.EActiveBonusBehaviourType.OverrideAbility);
						List<CActor> list8 = new List<CActor>();
						foreach (CActiveBonus item5 in list7)
						{
							foreach (CActor item6 in item5.ValidActorsInRangeOfAura)
							{
								if (!list8.Contains(item6) && !list2.Contains(item6))
								{
									list8.Add(item6);
								}
							}
						}
						if (LogApplyToAfterApplied())
						{
							CAbilityTargetingCombatLog_MessageData message2 = new CAbilityTargetingCombatLog_MessageData(AnimOverload, TargetingActor)
							{
								m_Ability = this,
								m_ActorsAppliedTo = ((list8.Count > 0) ? list8 : list2)
							};
							ScenarioRuleClient.MessageHandler(message2);
							foreach (CMessageData logAfterAppliedMessage2 in m_LogAfterAppliedMessages)
							{
								ScenarioRuleClient.MessageHandler(logAfterAppliedMessage2);
							}
							m_LogAfterAppliedMessages.Clear();
						}
						LogEvent(ESESubTypeAbility.AbilityPerform);
						foreach (CActor item7 in list8)
						{
							if (m_PositiveConditions.Count > 0)
							{
								ProcessPositiveStatusEffects(item7);
							}
						}
					}
				}
				if (m_SkipWaitForProgressChoreographer || (TargetingActor is CPlayerActor && (AbilityType.Equals(EAbilityType.RecoverDiscardedCards) || AbilityType.Equals(EAbilityType.RecoverLostCards) || AbilityType.Equals(EAbilityType.LoseCards) || AbilityType.Equals(EAbilityType.DiscardCards) || AbilityType.Equals(EAbilityType.IncreaseCardLimit) || AbilityType.Equals(EAbilityType.ExtraTurn) || AbilityType.Equals(EAbilityType.LoseGoalChestReward) || AbilityType.Equals(EAbilityType.TransferDooms) || AbilityType.Equals(EAbilityType.MoveActiveBonusTracker)) && Strength < int.MaxValue) || AbilityType.Equals(EAbilityType.AddDoom))
				{
					if (m_SkipWaitForProgressChoreographer)
					{
						m_State++;
					}
					return true;
				}
				if (AbilityType.Equals(EAbilityType.ControlActor))
				{
					m_State++;
				}
			}
			CWaitForProgressChoreographer_MessageData message3 = new CWaitForProgressChoreographer_MessageData(TargetingActor)
			{
				WaitActor = TargetingActor,
				WaitTickFrame = 10000,
				ClearEvents = SkipAnim
			};
			ScenarioRuleClient.MessageHandler(message3);
			return true;
		}
		case TargetingState.CheckForOneTargetAtATime:
			if (m_OneTargetAtATime && (m_NumberTargetsRemaining > 0 || AreaEffectSelected) && ValidActorsInRange.Count > 0)
			{
				m_State = TargetingState.ActorIsSelectingTargetingFocus;
				Perform();
			}
			else
			{
				PhaseManager.StepComplete();
			}
			break;
		case TargetingState.InlineSubAbilityCheck:
			if (SubAbilities.Count > 0 && SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
			{
				List<CAbility> list = new List<CAbility>();
				foreach (CAbility item8 in SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
				{
					foreach (CActor item9 in ActorsTargeted)
					{
						if (item8.MiscAbilityData == null || !item8.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.HasValue || !item8.MiscAbilityData.InlineSubAbilityOnKilledTargetsOnly.Value || item9.IsDead)
						{
							CAbility cAbility = CAbility.CopyAbility(item8, generateNewID: true, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
							cAbility.ParentAbility = this;
							cAbility.InlineSubAbilityTiles.Add(ScenarioManager.Tiles[item9.ArrayIndex.X, item9.ArrayIndex.Y]);
							cAbility.TargetThisActorAutomatically = item9;
							list.Add(cAbility);
						}
					}
				}
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list, null, performNow: false, stopPlayerSkipping: false, true);
				m_State = TargetingState.Done;
				PhaseManager.StepComplete();
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case TargetingState.Done:
			PhaseManager.NextStep();
			break;
		}
		return true;
		IL_0095:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message4 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
			else
			{
				CPlayerIsStunned_MessageData message5 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0061;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0061;
				}
			}
		}
		InvalidTileReason = null;
		bool flag = GetMiscVar("ShouldPerformOnTileSelected").GetBool();
		if (AreaEffect != null)
		{
			bool flag2 = false;
			List<CActor> validActorsInArea = GetValidActorsInArea(ValidTilesInAreaEffectedIncludingBlocked);
			if (validActorsInArea.Count > 0)
			{
				flag2 = true;
				m_AreaEffectLocked = true;
				ValidActorsInRange = validActorsInArea;
			}
			if (!flag2)
			{
				base.TileSelected(selectedTile, optionalTileList);
				LogEvent(ESESubTypeAbility.AbilityTileSelected);
				return;
			}
		}
		Log.Trace("ValidTilesInAreaEffectedIncludingBlocked count: " + ValidTilesInAreaEffectedIncludingBlocked.Count);
		Log.Trace("ValidActorsInRange Count: " + ValidActorsInRange.Count);
		Log.Trace("m_ActorsToTarget Count: " + m_ActorsToTarget.Count + " m_NumberTargets: " + NumberTargets);
		CAbilityFilter.CurrentVarsContext = VarsContext;
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible, originalTargetType: false, CanSkipUntergetableCheck());
		CAbilityFilter.CurrentVarsContext = null;
		if (actorOnTile != null && (m_OneTargetAtATime ? (m_ActorsToTarget.Count <= 1) : (m_ActorsToTarget.Count < NumberTargets)))
		{
			if (!m_OneTargetAtATime || !ActorsTargeted.Contains(actorOnTile) || GetVar("MultipleApplyToOneActor").GetBool())
			{
				m_LastActorSelected = actorOnTile;
				if (m_ActorsToTarget.Contains(m_LastActorSelected))
				{
					if (!m_TilesSelected.Contains(selectedTile))
					{
						TilesSelected.Add(selectedTile);
					}
				}
				else if (m_State == TargetingState.ActorIsSelectingTargetingFocus || m_State == TargetingState.ActorIsSelectingAoE)
				{
					if (m_OneTargetAtATime && m_LastActorSelected != null)
					{
						if (m_ActorsToTarget.Count > 0)
						{
							m_NumberTargetsRemaining++;
						}
						TilesSelected.Clear();
						m_ActorsToTarget.Clear();
					}
					if (SharedAbilityTargeting.TileSelected(this, selectedTile, AreaEffect, TargetingActor, FilterActor, Range, ref m_ValidActorsInRange, m_ActorsToIgnore, AbilityFilter, m_OneTargetAtATime ? m_NumberTargetsRemaining : NumberTargets, ref m_NumberTargetsRemaining, m_ValidTilesInAreaEffectIncludingBlocked, ref m_ActorsToTarget, ref m_TilesSelected, IsTargetedAbility))
					{
						if (AreaEffect != null && m_AreaEffectLocked)
						{
							m_ActorsToTarget.AddRange(m_ValidActorsInRange);
						}
						if ((m_State == TargetingState.ActorIsSelectingAoE || m_State == TargetingState.ActorIsSelectingTargetingFocus) && AreaEffect != null && ActiveBonusAddTargetBuff != 0)
						{
							m_AreaEffectBackup = AreaEffect;
							AreaEffect = null;
							m_ActorsToTarget = new List<CActor>(ValidActorsInRange);
							m_ActorsToIgnore = new List<CActor>(ValidActorsInRange);
							ValidActorsInRange = GameState.GetActorsInRange(TargetingActor, FilterActor, Range, m_ActorsToIgnore, AbilityFilter, null, AllPossibleTilesInAreaEffect, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
							CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
							cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
							cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = IsPositive();
							ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
						}
					}
					else
					{
						TryShowInvalidTargetReason(selectedTile);
					}
				}
			}
		}
		else if (selectedTile.LivingActor != null)
		{
			TryShowInvalidTargetReason(selectedTile);
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
		return;
		IL_0061:
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	private void TryShowInvalidTargetReason(CTile selectedTile)
	{
		if (AbilityFilter == null)
		{
			return;
		}
		foreach (CAbilityFilter abilityFilter in AbilityFilter.AbilityFilters)
		{
			if (abilityFilter.FilterVars == null || abilityFilter.FilterVars.Count <= 0)
			{
				continue;
			}
			foreach (CActor livingActor in selectedTile.LivingActors)
			{
				if (!abilityFilter.CheckVars(livingActor, TargetingActor, null, out var failReason) && failReason != null)
				{
					string text = GetMiscVar(failReason + "InvalidText").GetString();
					if (!string.IsNullOrEmpty(text))
					{
						InvalidTileReason = text;
						return;
					}
				}
			}
		}
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0061;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0061;
				}
			}
		}
		bool flag = GetMiscVar("ShouldPerformOnTileSelected").GetBool();
		if (AreaEffect != null && m_AreaEffectLocked)
		{
			m_AreaEffectLocked = false;
		}
		else if (SharedAbilityTargeting.TileDeselected(this, selectedTile, m_OneTargetAtATime ? m_NumberTargetsRemaining : NumberTargets, ref m_NumberTargetsRemaining, ref m_ActorsToTarget, ref m_TilesSelected))
		{
			bool flag2 = (MiscAbilityData != null && MiscAbilityData.AllTargetsAdjacentToParentTargets.HasValue && MiscAbilityData.AllTargetsAdjacentToParentTargets.Value) || UseParentTargets;
			if (AreaEffectBackup != null)
			{
				AreaEffect = m_AreaEffectBackup;
				m_AreaEffectBackup = null;
				m_State = TargetingState.ActorIsSelectingTargetingFocus;
				m_ActorsToIgnore.Clear();
				m_NumberTargetsRemaining = NumberTargets + ActiveBonusAddTargetBuff;
				if (!flag2)
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
			}
			m_AreaEffectLocked = false;
			if (!flag2)
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
			flag = true;
		}
		m_LastActorSelected = ((!m_OneTargetAtATime && m_ActorsToTarget.Count > 0) ? m_ActorsToTarget[m_ActorsToTarget.Count - 1] : null);
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
		return;
		IL_0061:
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == TargetingState.ActorIsSelectingTargetingFocus)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State == TargetingState.ActorIsSelectingTargetingFocus)
		{
			if (OriginalTargetCount == -1)
			{
				return OneTargetAtATime;
			}
			return true;
		}
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != TargetingState.ActorIsSelectingAoE)
			{
				return m_State == TargetingState.ActorIsSelectingTargetingFocus;
			}
			return true;
		}
		return false;
	}

	public virtual void SortValidActorsInRange(CActor actorApplying, ref List<CActor> validActorsInRange)
	{
		if (ShouldSortValidActorsInRangeByInitiative())
		{
			validActorsInRange.Sort((CActor x, CActor y) => new GameState.ActorInitiativeComparer().Compare(x, y));
		}
	}

	public virtual bool ShouldSortValidActorsInRangeByInitiative()
	{
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			bool flag = State == TargetingState.ActorIsSelectingAoE;
			m_State++;
			if (flag)
			{
				m_State++;
			}
		}
		return m_State >= TargetingState.Done;
	}

	public override void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		base.CopyInterruptedProperties(destAbility, strength, initialCopy, forced);
		CAbilityTargeting obj = destAbility as CAbilityTargeting;
		obj.m_State = m_State;
		obj.m_OneTargetAtATime = m_OneTargetAtATime;
		obj.m_SkipWaitForProgressChoreographer = m_SkipWaitForProgressChoreographer;
		obj.AreaEffectSelected = AreaEffectSelected;
		obj.ActiveBonusAddTargetBuff = ActiveBonusAddTargetBuff;
		obj.OriginatesFromAnAura = OriginatesFromAnAura;
		obj.OriginatingAuraOwner = OriginatingAuraOwner;
		obj.OriginatingAuraAnimOverload = OriginatingAuraAnimOverload;
		obj.AuraTriggerAbilityAnimType = AuraTriggerAbilityAnimType;
		obj.m_CameraFocusActor = m_CameraFocusActor;
		obj.m_LastActorSelected = m_LastActorSelected;
	}

	public override void Restart()
	{
		base.Restart();
		if (ActorsTargeted == null)
		{
			ActorsTargeted = new List<CActor>();
		}
		ActorsTargeted.Clear();
		if (m_OneTargetAtATime && AreaEffect != null)
		{
			m_State = TargetingState.ActorIsSelectingAoE;
		}
		else
		{
			m_State = TargetingState.ActorIsSelectingTargetingFocus;
		}
		m_ActorsToTarget = new List<CActor>();
		if (UseSubAbilityTargeting)
		{
			if (AreaEffect == null && ParentAbility?.AreaEffect != null)
			{
				AreaEffect = ParentAbility.AreaEffect.Copy();
			}
			if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				m_ValidActorsInRange = new List<CActor> { TargetingActor };
				m_ActorsToTarget.AddRange(ValidActorsInRange);
			}
			else if (AreaEffect != null)
			{
				m_ValidTilesInAreaEffect = ParentAbility.ValidTilesInAreaAffected.ToList();
				m_ValidTilesInAreaEffectIncludingBlocked = ParentAbility.ValidTilesInAreaEffectedIncludingBlocked.ToList();
				ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffectIncludingBlocked);
				m_ActorsToTarget.AddRange(ValidActorsInRange);
			}
			else if (ParentAbility?.ActorsTargeted != null)
			{
				if (AbilityFilter.Equals(ParentAbility.AbilityFilter))
				{
					ValidActorsInRange = ParentAbility.ActorsTargeted.ToList();
				}
				else
				{
					ValidActorsInRange = new List<CActor>();
					foreach (CActor item in ParentAbility.ActorsTargeted)
					{
						if (AbilityFilter.IsValidTarget(item, FilterActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible, CanSkipUntergetableCheck()))
						{
							ValidActorsInRange.Add(item);
						}
					}
				}
			}
			m_ActorsToTarget = m_ValidActorsInRange.ToList();
			NumberTargets = ValidActorsInRange.Count;
		}
		else if (TargetingActor != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		else
		{
			ValidActorsInRange = new List<CActor>();
		}
		if (NumberTargets == OriginalTargetCount)
		{
			m_AllTargets = false;
		}
		if (NumberTargets == -1 || m_AllTargets)
		{
			if (!m_OneTargetAtATime)
			{
				m_AllTargets = true;
			}
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false);
			NumberTargets = ValidActorsInRange.Count;
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (AutoSelectTiles && !m_HasAutoSelected)
		{
			foreach (CActor item2 in ValidActorsInRange)
			{
				TileSelected(item2.Tile, null);
			}
			m_HasAutoSelected = true;
		}
		m_SkipWaitForProgressChoreographer = false;
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public bool HasPassedState(TargetingState targetingState)
	{
		return m_State > targetingState;
	}

	public override bool EnoughTargetsSelected()
	{
		if (GetVar("NeedsOverride").GetBool() && CurrentOverrides.Count == 0)
		{
			return false;
		}
		int num = GetMiscVar("MinTargets").GetInt(VarsContext);
		if (num > 0 && m_ActorsToTarget.Count < num)
		{
			return false;
		}
		return m_ActorsToTarget.Count > 0;
	}

	public override bool MaxTargetsSelected()
	{
		if (m_OneTargetAtATime)
		{
			return m_ActorsToTarget.Count > 0;
		}
		return base.MaxTargetsSelected();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor secondaryActor = null;
		if (ActorsTargeted.Count > 0)
		{
			secondaryActor = ActorsTargeted[ActorsTargeted.Count - 1];
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTargeting(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this, secondaryActor), m_State));
	}

	public void SetOriginatingFromAura(CActor actorOwningAura, string auraAnimOverload, CActiveBonus.EAuraTriggerAbilityAnimType auraTriggerAbilityAnimType)
	{
		OriginatesFromAnAura = true;
		OriginatingAuraOwner = actorOwningAura;
		OriginatingAuraAnimOverload = auraAnimOverload ?? string.Empty;
		AuraTriggerAbilityAnimType = auraTriggerAbilityAnimType;
	}

	protected virtual bool LogApplyToAfterApplied()
	{
		return true;
	}

	protected override void LogPositiveCondition(CCondition.EPositiveCondition key, CActor target)
	{
		CConditionCombatLog_MessageData item = new CConditionCombatLog_MessageData(TargetingActor, key, CCondition.ENegativeCondition.NA, target);
		m_LogAfterAppliedMessages.Add(item);
	}

	protected override void LogNegativeCondition(CCondition.ENegativeCondition key, CActor target)
	{
		CConditionCombatLog_MessageData item = new CConditionCombatLog_MessageData(TargetingActor, CCondition.EPositiveCondition.NA, key, target);
		m_LogAfterAppliedMessages.Add(item);
	}

	public override CAbility CreateInterruptCopy(CActor targetingActor, bool killAfter, bool forced, bool interruptRegardlessOfStrength = false)
	{
		CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
		TargetingActor = TargetingActor;
		if ((NumberTargetsRemaining - 1 > 0 && (OneTargetAtATime || State != TargetingState.ApplyToActors)) | interruptRegardlessOfStrength)
		{
			CAbility cAbility = CAbility.CopyAbility(this, generateNewID: false, fullCopy: true);
			CopyInterruptedProperties(cAbility, Strength, initialCopy: true, forced);
			PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities.Remove(currentPhaseAbility);
			return cAbility;
		}
		return null;
	}
}
