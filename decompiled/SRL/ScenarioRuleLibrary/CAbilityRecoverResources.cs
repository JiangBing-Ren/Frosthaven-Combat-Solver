using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityRecoverResources : CAbility
{
	public enum ERecoverResourcesState
	{
		None,
		SelectResourcesToRecover,
		RecoveringResources,
		RecoverResourcesDone
	}

	private ERecoverResourcesState m_State;

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ERecoverResourcesState.SelectResourcesToRecover;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (AllTargetsOnMovePath)
		{
			for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPath.Count; i++)
			{
				if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
				{
					Point point = CAbilityMove.AllPlayerArrayIndexOnPath[i];
					CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
					if (cTile != null)
					{
						TilesInRange.Add(cTile);
					}
				}
			}
		}
		else if (AreaEffect == null)
		{
			TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: true, ignoreBlockedWithActor: false, ignoreLOS: true);
			if (Range > 1)
			{
				TilesInRange.Add(ScenarioManager.GetAdjacentTile(TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y, ScenarioManager.EAdjacentPosition.ECenter));
			}
		}
		if (NumberTargets == -1 || Targeting == EAbilityTargeting.All || Targeting == EAbilityTargeting.AllConnectedRooms)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
		}
		if (UseSubAbilityTargeting)
		{
			if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
			{
				foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
				{
					TileSelected(inlineSubAbilityTile, null);
				}
			}
			else if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
			{
				foreach (CTile item in ParentAbility.TilesSelected)
				{
					TileSelected(item, null);
				}
			}
			m_State = ERecoverResourcesState.RecoveringResources;
		}
		else
		{
			if (TargetingActor.Type != CActor.EType.Player)
			{
				Log.ErrorWithReport("CAbilityRecoverResources does not support non-player actors currently");
				return;
			}
			if (m_AllTargets)
			{
				m_TilesSelected.Clear();
				foreach (CTile item2 in TilesInRange)
				{
					if (item2.FindProp(ScenarioManager.ObjectImportType.Resource) is CObjectResource cObjectResource && ResourcesToTakeFromTargets.ContainsKey(cObjectResource.ResourceData.ID) && !m_TilesSelected.Contains(item2))
					{
						m_TilesSelected.Add(item2);
					}
					CActor actorOnTile = GameState.GetActorOnTile(item2, FilterActor, AbilityFilter, ActorsToIgnore, IsTargetedAbility, false);
					if (actorOnTile != null && AbilityFilter.IsValidTarget(actorOnTile, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, false) && !m_TilesSelected.Contains(item2))
					{
						m_TilesSelected.Add(item2);
						m_ActorsToTarget.Add(actorOnTile);
					}
				}
				if (m_TilesSelected.Count <= 0)
				{
					SetCancelAbilityFlag(cancel: true);
				}
			}
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
		case ERecoverResourcesState.SelectResourcesToRecover:
		{
			CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
				m_IsPositive = IsPositive(),
				m_SpawnType = ScenarioManager.ObjectImportType.None
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case ERecoverResourcesState.RecoveringResources:
		{
			m_AbilityHasHappened = true;
			foreach (CTile item in TilesSelected)
			{
				if (item.FindProp(ScenarioManager.ObjectImportType.Resource) is CObjectResource cObjectResource && ResourcesToTakeFromTargets.ContainsKey(cObjectResource.ResourceData.ID))
				{
					TargetingActor.LootTile(item, asPartOfAbility: true, endOfTurn: false, ScenarioManager.ObjectImportType.Resource);
				}
			}
			foreach (CActor item2 in ActorsToTarget)
			{
				foreach (string key in ResourcesToTakeFromTargets.Keys)
				{
					if (item2.CharacterHasResource(key, ResourcesToTakeFromTargets[key]))
					{
						item2.RemoveCharacterResource(key, ResourcesToTakeFromTargets[key]);
						TargetingActor.AddCharacterResource(key, ResourcesToTakeFromTargets[key]);
					}
				}
			}
			CActorIsRecoveringResources_MessageData message = new CActorIsRecoveringResources_MessageData(TargetingActor)
			{
				m_ActorRecovering = TargetingActor,
				m_RecoverAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
			PhaseManager.NextStep();
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
		if (!AllTargets && m_State == ERecoverResourcesState.SelectResourcesToRecover && !m_TilesSelected.Contains(selectedTile))
		{
			if (selectedTile.FindProp(ScenarioManager.ObjectImportType.Resource) is CObjectResource cObjectResource && ResourcesToTakeFromTargets.ContainsKey(cObjectResource.ResourceData.ID) && !m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
				flag = true;
			}
			CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, ActorsToIgnore, IsTargetedAbility, false);
			if (actorOnTile != null && AbilityFilter.IsValidTarget(actorOnTile, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, false))
			{
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				m_ActorsToTarget.Add(actorOnTile);
				flag = true;
			}
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
		bool flag = false;
		if (!AllTargets && m_State == ERecoverResourcesState.SelectResourcesToRecover && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, ActorsToIgnore, IsTargetedAbility, false);
			if (actorOnTile != null && m_ActorsToTarget.Contains(actorOnTile))
			{
				m_ActorsToTarget.Remove(actorOnTile);
			}
			flag = true;
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void Restart()
	{
		base.Restart();
	}

	public override bool CanClearTargets()
	{
		return m_State == ERecoverResourcesState.SelectResourcesToRecover;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ERecoverResourcesState.SelectResourcesToRecover;
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
		return m_State == ERecoverResourcesState.RecoverResourcesDone;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		CActor targetingActor = TargetingActor;
		if (targetingActor != null && targetingActor.Type == CActor.EType.Enemy)
		{
			_ = ScenarioManager.Scenario.Enemies.Find((CEnemyActor x) => x.ActorGuid == TargetingActor.ActorGuid)?.IsSummon;
		}
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool IsPositive()
	{
		return true;
	}
}
