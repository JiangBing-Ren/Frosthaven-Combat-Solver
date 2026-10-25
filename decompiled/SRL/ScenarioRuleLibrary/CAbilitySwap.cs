using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilitySwap : CAbility
{
	public enum ESwapState
	{
		None,
		SelectTargets,
		CastSwap,
		FinishSwap,
		SwapDone
	}

	private ESwapState m_State;

	private Point m_FirstTargetStartLocation;

	private Point m_SecondTargetStartLocation;

	private List<CActor> m_FirstTargetValidActorsInRange = new List<CActor>();

	private List<CActor> m_SecondTargetValidActorsInRange = new List<CActor>();

	[JsonProperty]
	public CAbilityFilterContainer FirstTargetFilter { get; set; }

	[JsonProperty]
	public CAbilityFilterContainer SecondTargetFilter { get; set; }

	public CAbilitySwap(CAbilityFilterContainer firstTargetFilter = null, CAbilityFilterContainer secondTargetFilter = null)
	{
		FirstTargetFilter = firstTargetFilter;
		SecondTargetFilter = secondTargetFilter;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ESwapState.SelectTargets;
		CAbilityFilterContainer abilityFilter = AbilityFilter;
		if (FirstTargetFilter == null)
		{
			FirstTargetFilter = AbilityFilter;
		}
		if (SecondTargetFilter == null)
		{
			SecondTargetFilter = AbilityFilter;
		}
		AbilityFilter = FirstTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		m_FirstTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		AbilityFilter = SecondTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		RemoveImmuneActorsFromList(ref m_ValidActorsInRange);
		m_SecondTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		if (FirstTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(TargetingActor);
			ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(TargetingActor, ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
		}
		else if (SecondTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(TargetingActor);
			ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(TargetingActor, ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
		}
		AbilityFilter = abilityFilter;
		m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
		if (NumberTargets == -1 && m_ValidActorsInRange.Count == 2)
		{
			m_ActorsToTarget = m_ValidActorsInRange;
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
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
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0089;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_0089;
				}
			}
		}
		switch (m_State)
		{
		case ESwapState.SelectTargets:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
				cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
				cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
				ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			}
			break;
		case ESwapState.CastSwap:
		{
			TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
			ScenarioRuleClient.FirstAbilityStarted();
			AbilityHasHappened = true;
			m_CanUndo = false;
			m_FirstTargetStartLocation = m_ActorsToTarget[0].ArrayIndex;
			m_SecondTargetStartLocation = m_ActorsToTarget[1].ArrayIndex;
			CActorIsCastingSwap_MessageData message2 = new CActorIsCastingSwap_MessageData(TargetingActor)
			{
				m_ActorCasting = TargetingActor,
				m_FirstTarget = m_ActorsToTarget[0],
				m_SecondTarget = m_ActorsToTarget[1],
				m_SwapAbility = this
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case ESwapState.FinishSwap:
		{
			CActorsAreSwapping_MessageData message = new CActorsAreSwapping_MessageData(TargetingActor)
			{
				m_FirstTarget = m_ActorsToTarget[0],
				m_SecondTarget = m_ActorsToTarget[1],
				m_SwapAbility = this
			};
			GameState.LostAdjacency(m_ActorsToTarget[0], m_ActorsToTarget[0].ArrayIndex, m_ActorsToTarget[1].ArrayIndex, this);
			GameState.LostAdjacency(m_ActorsToTarget[1], m_ActorsToTarget[1].ArrayIndex, m_ActorsToTarget[0].ArrayIndex, this);
			Point arrayIndex = m_ActorsToTarget[0].ArrayIndex;
			m_ActorsToTarget[0].ArrayIndex = m_ActorsToTarget[1].ArrayIndex;
			m_ActorsToTarget[1].ArrayIndex = arrayIndex;
			ScenarioRuleClient.MessageHandler(message);
			PhaseManager.StepComplete();
			break;
		}
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message3 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message3);
			}
			else
			{
				CPlayerIsStunned_MessageData message4 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
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
		bool flag = false;
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
		if (actorOnTile != null && ValidActorsInRange.Contains(actorOnTile) && m_State == ESwapState.SelectTargets && m_ActorsToTarget.Count < 2 && !m_ActorsToTarget.Contains(actorOnTile))
		{
			m_ActorsToTarget.Add(actorOnTile);
			if (m_ActorsToTarget.Count < 2)
			{
				if (m_FirstTargetValidActorsInRange.Contains(actorOnTile))
				{
					ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
				}
				else
				{
					ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
				}
				FilterTargetsForFlying(actorOnTile, selectedTile);
			}
			else
			{
				ValidActorsInRange.Clear();
			}
			flag = true;
		}
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override void TileDeselected(CTile selectedTile, List<CTile> optionalTileList)
	{
		CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
		if (m_ActorsToTarget.Contains(actorOnTile))
		{
			m_ActorsToTarget.Remove(actorOnTile);
			if (m_ActorsToTarget.Count > 0)
			{
				if (m_FirstTargetValidActorsInRange.Contains(m_ActorsToTarget[0]))
				{
					ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
				}
				else
				{
					ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
				}
			}
			else
			{
				m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
			}
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public void FilterTargetsForFlying(CActor selectedActor, CTile selectedTile)
	{
		if (selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
		{
			for (int num = ValidActorsInRange.Count - 1; num >= 0; num--)
			{
				CActor cActor = ValidActorsInRange[num];
				if (!cActor.FlyingAfterConditions)
				{
					ValidActorsInRange.Remove(cActor);
				}
			}
		}
		else
		{
			if (selectedActor.FlyingAfterConditions)
			{
				return;
			}
			for (int num2 = ValidActorsInRange.Count - 1; num2 >= 0; num2--)
			{
				CActor cActor2 = ValidActorsInRange[num2];
				if (ScenarioManager.Tiles[cActor2.ArrayIndex.X, cActor2.ArrayIndex.Y].FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
				{
					ValidActorsInRange.Remove(cActor2);
				}
			}
		}
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		CAbilityFilterContainer abilityFilter = AbilityFilter;
		AbilityFilter = FirstTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_FirstTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		AbilityFilter = SecondTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_SecondTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		if (FirstTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(TargetingActor);
			ValidActorsInRange = m_SecondTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(TargetingActor, ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
		}
		else if (SecondTargetFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
		{
			m_ActorsToTarget.Add(TargetingActor);
			ValidActorsInRange = m_FirstTargetValidActorsInRange.ToList();
			FilterTargetsForFlying(TargetingActor, ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
		}
		AbilityFilter = abilityFilter;
		m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
		if (m_State == ESwapState.SelectTargets)
		{
			Perform();
		}
	}

	public override void Restart()
	{
		base.Restart();
		m_State = ESwapState.SelectTargets;
		m_ActorsToTarget.Clear();
		CAbilityFilterContainer abilityFilter = AbilityFilter;
		AbilityFilter = FirstTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_FirstTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		AbilityFilter = SecondTargetFilter;
		SharedAbilityTargeting.GetValidActorsInRange(this);
		m_SecondTargetValidActorsInRange = m_ValidActorsInRange.ToList();
		AbilityFilter = abilityFilter;
		m_ValidActorsInRange = m_FirstTargetValidActorsInRange.Concat(m_SecondTargetValidActorsInRange).Distinct().ToList();
	}

	public override bool CanClearTargets()
	{
		return m_State == ESwapState.SelectTargets;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ESwapState.SelectTargets;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_ActorsToTarget.Count == 2;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == ESwapState.SwapDone;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilitySwap(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, (m_ActorsToTarget.Count > 0) ? m_ActorsToTarget[0].GetPrefabName() : null, (m_ActorsToTarget.Count > 1) ? m_ActorsToTarget[1].GetPrefabName() : null, new TileIndex(m_FirstTargetStartLocation), new TileIndex(m_SecondTargetStartLocation)));
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		for (int i = 0; i < m_ActorsToTarget.Count; i++)
		{
			CActor cActor = m_ActorsToTarget[i];
			CTile cTile = ScenarioManager.Tiles[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
			for (int num = cTile.m_Props.Count - 1; num >= 0; num--)
			{
				if (!CObjectPressurePlate.IsEndTurnPressurePlate(cTile.m_Props[num]))
				{
					cTile.m_Props[num].AutomaticActivate(cActor);
				}
			}
		}
	}

	public override bool IsPositive()
	{
		return true;
	}
}
