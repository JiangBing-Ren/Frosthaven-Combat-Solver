using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityActivateSpawner : CAbility
{
	public enum EActivateSpawnerState
	{
		SelectSpawnerPosition,
		ActivateSpawner,
		ActivateSpawnerDone
	}

	private EActivateSpawnerState m_State;

	private List<CInteractableSpawner> m_SpawnersToActivate;

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return ScenarioManager.ObjectImportType.Spawner;
		}
	}

	public override string SpawnPropPreview
	{
		get
		{
			string spawnPropPreview = base.SpawnPropPreview;
			if (!string.IsNullOrEmpty(spawnPropPreview))
			{
				return spawnPropPreview;
			}
			return EPropType.Spawner.ToString();
		}
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetingActor, filterActor, controllingActor);
		m_State = EActivateSpawnerState.SelectSpawnerPosition;
		m_SpawnersToActivate = new List<CInteractableSpawner>();
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
			TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
		}
		if (NumberTargets == -1 || Targeting == EAbilityTargeting.All || Targeting == EAbilityTargeting.AllConnectedRooms)
		{
			m_AllTargets = true;
		}
		else
		{
			m_AllTargets = false;
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
		case EActivateSpawnerState.SelectSpawnerPosition:
			if (UseSubAbilityTargeting)
			{
				if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
				{
					foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
					{
						TileSelected(inlineSubAbilityTile, null);
					}
					break;
				}
				if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
				{
					foreach (CTile item5 in ParentAbility.TilesSelected)
					{
						TileSelected(item5, null);
					}
					break;
				}
				PhaseManager.NextStep();
				return true;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (m_AllTargets)
				{
					m_TilesSelected.Clear();
					foreach (CTile item6 in TilesInRange)
					{
						foreach (CSpawner spawner in item6.m_Spawners)
						{
							if (!(spawner is CInteractableSpawner { IsActive: false } cInteractableSpawner))
							{
								continue;
							}
							if (!m_SpawnersToActivate.Contains(cInteractableSpawner))
							{
								m_SpawnersToActivate.Add(cInteractableSpawner);
							}
							if (!m_TilesSelected.Contains(item6))
							{
								m_TilesSelected.Add(item6);
							}
							if (spawner.PathingBlockers == null || spawner.PathingBlockers.Count <= 0)
							{
								continue;
							}
							foreach (TileIndex pathingBlocker in spawner.PathingBlockers)
							{
								CTile item = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
								if (!m_TilesSelected.Contains(item))
								{
									m_TilesSelected.Add(item);
								}
							}
						}
						CTile spawnerTile = null;
						CInteractableSpawner cInteractableSpawner2 = CObjectProp.FindSpawnerWithPathingBlocker(item6.m_ArrayIndex, ref spawnerTile);
						if (cInteractableSpawner2 == null || cInteractableSpawner2.IsActive)
						{
							continue;
						}
						if (!m_SpawnersToActivate.Contains(cInteractableSpawner2))
						{
							m_SpawnersToActivate.Add(cInteractableSpawner2);
						}
						if (!m_TilesSelected.Contains(item6))
						{
							m_TilesSelected.Add(item6);
						}
						if (cInteractableSpawner2.PathingBlockers == null || cInteractableSpawner2.PathingBlockers.Count <= 0)
						{
							continue;
						}
						foreach (TileIndex pathingBlocker2 in cInteractableSpawner2.PathingBlockers)
						{
							CTile item2 = ScenarioManager.Tiles[pathingBlocker2.X, pathingBlocker2.Y];
							if (!m_TilesSelected.Contains(item2))
							{
								m_TilesSelected.Add(item2);
							}
						}
					}
					if (m_TilesSelected.Count <= 0)
					{
						SetCancelAbilityFlag(cancel: true);
						if (IsMergedAbility)
						{
							PhaseManager.StepComplete();
						}
						else
						{
							PhaseManager.NextStep();
						}
					}
				}
				if (!AbilityHasBeenCancelled)
				{
					CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
					{
						m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.DeactiveSpawner },
						m_IsPositive = IsPositive()
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				break;
			}
			m_TilesSelected.Clear();
			foreach (CTile item7 in TilesInRange)
			{
				foreach (CSpawner spawner2 in item7.m_Spawners)
				{
					if (!(spawner2 is CInteractableSpawner { IsActive: false } cInteractableSpawner3))
					{
						continue;
					}
					if (!m_SpawnersToActivate.Contains(cInteractableSpawner3))
					{
						m_SpawnersToActivate.Add(cInteractableSpawner3);
					}
					if (!m_TilesSelected.Contains(item7))
					{
						m_TilesSelected.Add(item7);
					}
					if (spawner2.PathingBlockers == null || spawner2.PathingBlockers.Count <= 0)
					{
						continue;
					}
					foreach (TileIndex pathingBlocker3 in spawner2.PathingBlockers)
					{
						CTile item3 = ScenarioManager.Tiles[pathingBlocker3.X, pathingBlocker3.Y];
						if (!m_TilesSelected.Contains(item3))
						{
							m_TilesSelected.Add(item3);
						}
					}
				}
				CTile spawnerTile2 = null;
				CInteractableSpawner cInteractableSpawner4 = CObjectProp.FindSpawnerWithPathingBlocker(item7.m_ArrayIndex, ref spawnerTile2);
				if (cInteractableSpawner4 != null && !cInteractableSpawner4.IsActive)
				{
					if (!m_SpawnersToActivate.Contains(cInteractableSpawner4))
					{
						m_SpawnersToActivate.Add(cInteractableSpawner4);
					}
					if (!m_TilesSelected.Contains(item7))
					{
						m_TilesSelected.Add(item7);
					}
					if (cInteractableSpawner4.PathingBlockers != null && cInteractableSpawner4.PathingBlockers.Count > 0)
					{
						foreach (TileIndex pathingBlocker4 in cInteractableSpawner4.PathingBlockers)
						{
							CTile item4 = ScenarioManager.Tiles[pathingBlocker4.X, pathingBlocker4.Y];
							if (!m_TilesSelected.Contains(item4))
							{
								m_TilesSelected.Add(item4);
							}
						}
					}
				}
				if (m_SpawnersToActivate.Count >= m_NumberTargetsRemaining)
				{
					break;
				}
			}
			m_State = EActivateSpawnerState.ActivateSpawner;
			Perform();
			break;
		case EActivateSpawnerState.ActivateSpawner:
			m_CanUndo = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				AbilityHasHappened = true;
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count > 0)
			{
				CActivateOrDeactivateSpawner_MessageData message = new CActivateOrDeactivateSpawner_MessageData(AnimOverload, TargetingActor)
				{
					m_ActorDeactivatingSpawner = TargetingActor,
					m_Tiles = m_TilesSelected,
					m_ActivateOrDeactivateSpawnerAbility = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			foreach (CInteractableSpawner item8 in m_SpawnersToActivate)
			{
				item8.SetActive(active: true);
			}
			break;
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
		if (!AllTargets && !TilesSelected.Contains(selectedTile) && TilesSelected.Count < NumberTargets && m_State == EActivateSpawnerState.SelectSpawnerPosition && TilesInRange.Contains(selectedTile))
		{
			foreach (CSpawner spawner in selectedTile.m_Spawners)
			{
				if (!(spawner is CInteractableSpawner { IsActive: false } cInteractableSpawner))
				{
					continue;
				}
				if (!m_SpawnersToActivate.Contains(cInteractableSpawner))
				{
					m_SpawnersToActivate.Add(cInteractableSpawner);
				}
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				if (spawner.PathingBlockers == null || spawner.PathingBlockers.Count <= 0)
				{
					continue;
				}
				foreach (TileIndex pathingBlocker in spawner.PathingBlockers)
				{
					CTile item = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					if (!m_TilesSelected.Contains(item))
					{
						m_TilesSelected.Add(item);
					}
				}
			}
			CTile spawnerTile = null;
			CInteractableSpawner cInteractableSpawner2 = CObjectProp.FindSpawnerWithPathingBlocker(selectedTile.m_ArrayIndex, ref spawnerTile);
			if (cInteractableSpawner2 != null && !cInteractableSpawner2.IsActive)
			{
				if (!m_SpawnersToActivate.Contains(cInteractableSpawner2))
				{
					m_SpawnersToActivate.Add(cInteractableSpawner2);
				}
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				if (cInteractableSpawner2.PathingBlockers != null && cInteractableSpawner2.PathingBlockers.Count > 0)
				{
					foreach (TileIndex pathingBlocker2 in cInteractableSpawner2.PathingBlockers)
					{
						CTile item2 = ScenarioManager.Tiles[pathingBlocker2.X, pathingBlocker2.Y];
						if (!m_TilesSelected.Contains(item2))
						{
							m_TilesSelected.Add(item2);
						}
					}
				}
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
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
		if (!AllTargets && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			foreach (CSpawner spawner in selectedTile.m_Spawners)
			{
				if (!(spawner is CInteractableSpawner item))
				{
					continue;
				}
				m_SpawnersToActivate.Remove(item);
				if (spawner.PathingBlockers == null || spawner.PathingBlockers.Count <= 0)
				{
					continue;
				}
				foreach (TileIndex pathingBlocker in spawner.PathingBlockers)
				{
					CTile item2 = ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y];
					m_TilesSelected.Remove(item2);
				}
			}
			CTile spawnerTile = null;
			CInteractableSpawner cInteractableSpawner = CObjectProp.FindSpawnerWithPathingBlocker(selectedTile.m_ArrayIndex, ref spawnerTile);
			if (cInteractableSpawner != null)
			{
				m_SpawnersToActivate.Remove(cInteractableSpawner);
				if (cInteractableSpawner.PathingBlockers != null && cInteractableSpawner.PathingBlockers.Count > 0)
				{
					foreach (TileIndex pathingBlocker2 in cInteractableSpawner.PathingBlockers)
					{
						CTile item3 = ScenarioManager.Tiles[pathingBlocker2.X, pathingBlocker2.Y];
						m_TilesSelected.Remove(item3);
					}
				}
			}
			if (m_State == EActivateSpawnerState.SelectSpawnerPosition && TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
			}
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == EActivateSpawnerState.SelectSpawnerPosition)
		{
			Perform();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == EActivateSpawnerState.ActivateSpawnerDone;
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
		return "ActivateSpawner";
	}

	public override bool CanClearTargets()
	{
		return m_State == EActivateSpawnerState.SelectSpawnerPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == EActivateSpawnerState.SelectSpawnerPosition;
		}
		return false;
	}

	public bool HasPassedState(EActivateSpawnerState deactivateState)
	{
		return m_State > deactivateState;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}
}
