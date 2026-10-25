using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityPerformFromShadow : CAbility
{
	public enum EPerformFromShadowState
	{
		SelectShadowToPerformForm,
		PrePerformFromShadow,
		PerformFromShadow,
		PerformFromShadowDone
	}

	private EPerformFromShadowState m_State;

	private List<CAbility> m_Abilities;

	public List<CObjectProp> m_UsedShadows = new List<CObjectProp>();

	public int m_ShadowsToUse;

	public bool m_RemoveWhenDone;

	public List<CTile> m_ShadowsAtStart;

	public List<CAbility> Abilities => m_Abilities;

	public CAbilityPerformFromShadow(List<CAbility> abilities, bool removeWhenDone = false)
	{
		m_Abilities = abilities;
		m_RemoveWhenDone = removeWhenDone;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		if (m_ShadowsAtStart == null)
		{
			m_ShadowsAtStart = (from shadow in ScenarioManager.CurrentScenarioState.Props
				where shadow.ObjectType == ScenarioManager.ObjectImportType.Shadow
				select shadow.PropTile).ToList();
		}
		m_State = EPerformFromShadowState.SelectShadowToPerformForm;
		if (NumberTargets == -1)
		{
			NumberTargets = m_ShadowsAtStart.Count();
			m_ShadowsToUse = NumberTargets;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else if (NumberTargets > 0 && m_ShadowsToUse == 0)
		{
			m_ShadowsToUse = NumberTargets;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
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
		if (m_Abilities.Count > 0 && m_Abilities[0] is CAbilityAttack && TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm))
		{
			CPlayerIsDisarmed_MessageData message = new CPlayerIsDisarmed_MessageData(TargetingActor);
			ScenarioRuleClient.MessageHandler(message);
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_00d7;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_00d7;
				}
			}
		}
		switch (m_State)
		{
		case EPerformFromShadowState.SelectShadowToPerformForm:
		{
			if (TargetingActor.Type != CActor.EType.Player)
			{
				break;
			}
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			HelpBoxTooltipLocKey = "FH/GUI_SELECT_SHADOW_PERFORM_HELPBOX";
			TilesInRange = GameState.GetTilesInRange(TargetingActor.ArrayIndex, 99).ToList();
			List<CTile> list = new List<CTile>();
			foreach (CTile item in TilesInRange)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				CObjectProp cObjectProp = item.m_Props.Find((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow);
				if (item.m_HexMap.Revealed && cObjectProp != null && cNode.Walkable && !m_UsedShadows.Contains(cObjectProp) && m_ShadowsAtStart.Contains(item))
				{
					list.Add(item);
				}
			}
			SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list, this);
			TilesInRange = list.ToList();
			if (GetMiscVar("ContinuingAfterUse").GetBool() && TilesInRange.Count == 0)
			{
				SetCancelAbilityFlag(cancel: true, forceImmediate: true);
				Perform();
				break;
			}
			m_TilesSelected.Clear();
			NumberTargets = 1;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Shadow },
				m_IsPositive = IsPositive()
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case EPerformFromShadowState.PrePerformFromShadow:
		{
			m_CanUndo = false;
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			CObjectShadow cObjectShadow = m_TilesSelected[0].FindProp(ScenarioManager.ObjectImportType.Shadow) as CObjectShadow;
			m_UsedShadows.Add(cObjectShadow);
			if (m_RemoveWhenDone)
			{
				cObjectShadow.WillBeRemoved = true;
			}
			if (IsMergedAbility)
			{
				PhaseManager.StepComplete();
				break;
			}
			m_State = EPerformFromShadowState.PerformFromShadow;
			Perform();
			break;
		}
		case EPerformFromShadowState.PerformFromShadow:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				foreach (CAbility ability in m_Abilities)
				{
					ability.TargetingOverrideArrayIndex = m_TilesSelected[0].m_ArrayIndex;
					ability.AddParentContext(VarsContext);
					ability.AnimOverload = AnimOverload;
				}
				if (m_RemoveWhenDone)
				{
					CAbilityRemoveShadow cAbilityRemoveShadow = CAbility.CreateAbility(EAbilityType.RemoveShadow, CAbilityFilterContainer.CreateDefaultFilter(), isMonster: false, isTargetedAbility: true, 0, 0, -1) as CAbilityRemoveShadow;
					cAbilityRemoveShadow.MiscAbilityData = new AbilityData.MiscAbilityData();
					cAbilityRemoveShadow.MiscAbilityData.AutotriggerAbility = true;
					cAbilityRemoveShadow.MiscAbilityData.AlsoTargetSelf = true;
					cAbilityRemoveShadow.MiscAbilityData.CanSkip = false;
					cAbilityRemoveShadow.MiscAbilityData.CanUndo = false;
					cAbilityRemoveShadow.MiscAbilityData.StartIfParentHasHappened = true;
					cAbilityRemoveShadow.SkipAnim = true;
					cAbilityRemoveShadow.m_DestroyDelay = 0f;
					cAbilityRemoveShadow.TargetingOverrideArrayIndex = m_TilesSelected[0].m_ArrayIndex;
					m_Abilities.Last().AddSubAbility(cAbilityRemoveShadow);
					PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbilityRemoveShadow }, TargetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true);
				}
				AbilityHasHappened = true;
				if (m_ShadowsToUse - 1 > 0)
				{
					CAbilityPerformFromShadow cAbilityPerformFromShadow = CAbility.CopyAbility(this, generateNewID: false, fullCopy: true, copyCurrentOverrides: true, applyCurrentStatIsBasedOnX: false, CAbilityFilter.CurrentVarsContext) as CAbilityPerformFromShadow;
					cAbilityPerformFromShadow.m_UsedShadows = m_UsedShadows;
					cAbilityPerformFromShadow.m_ShadowsToUse = m_ShadowsToUse - 1;
					cAbilityPerformFromShadow.m_ShadowsAtStart = m_ShadowsAtStart;
					cAbilityPerformFromShadow.SetMiscVar("ContinuingAfterUse", true);
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbilityPerformFromShadow }, TargetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true);
				}
				m_ShadowsAtStart = null;
				PhaseManager.CurrentPhaseAction.StackNextAbilities(m_Abilities, TargetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true);
				PhaseManager.StepComplete();
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		}
		return false;
		IL_00d7:
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
		if (!TilesSelected.Contains(selectedTile) && m_State == EPerformFromShadowState.SelectShadowToPerformForm && m_NumberTargetsRemaining > 0 && TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Shadow) is CObjectShadow cObjectShadow)
		{
			CTile cTile = ScenarioManager.Tiles[cObjectShadow.ArrayIndex.X, cObjectShadow.ArrayIndex.Y];
			if (cTile != null)
			{
				m_TilesSelected.Add(cTile);
				m_NumberTargetsRemaining--;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
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
		if (m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			m_NumberTargetsRemaining++;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
				{
					m_Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override bool CanClearTargets()
	{
		return m_State == EPerformFromShadowState.SelectShadowToPerformForm;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EPerformFromShadowState.SelectShadowToPerformForm;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EPerformFromShadowState.PerformFromShadowDone;
	}

	public override void Restart()
	{
		base.Restart();
		if (m_ShadowsAtStart == null)
		{
			m_ShadowsAtStart = (from shadow in ScenarioManager.CurrentScenarioState.Props
				where shadow.ObjectType == ScenarioManager.ObjectImportType.Shadow
				select shadow.PropTile).ToList();
		}
		m_State = EPerformFromShadowState.SelectShadowToPerformForm;
		if (NumberTargets == -1)
		{
			NumberTargets = m_ShadowsAtStart.Count();
			m_ShadowsToUse = NumberTargets;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else if (NumberTargets > 0 && m_ShadowsToUse == 0)
		{
			m_ShadowsToUse = NumberTargets;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		if (m_ShadowsToUse <= 0)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		LogEvent(ESESubTypeAbility.AbilityRestart);
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
	}

	public override string GetDescription()
	{
		return "PerformFromShadow";
	}

	public bool HasPassedState(EPerformFromShadowState moveShadowState)
	{
		return m_State > moveShadowState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public static void RemoveTilesReserevdByPerformFromShadow(List<CTile> tiles)
	{
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbilities == null)
		{
			return;
		}
		foreach (CAbilityPerformFromShadow ab in (from x in PhaseManager.CurrentPhaseAction.CurrentAndPreviousPhaseAbilities
			where x.m_Ability is CAbilityPerformFromShadow && x.m_Ability.HasStarted && x.m_Ability.AbilityHasHappened
			select x.m_Ability as CAbilityPerformFromShadow).ToList())
		{
			tiles.RemoveAll((CTile tile) => ab.m_UsedShadows.Any((CObjectProp shadow) => tile.m_Props.Contains(shadow)));
		}
	}
}
