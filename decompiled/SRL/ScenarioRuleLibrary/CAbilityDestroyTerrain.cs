using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityDestroyTerrain : CAbility
{
	public enum DestroyObstacleState
	{
		SelectObstaclesPosition,
		PreDestroyObstacleProcessing,
		DestroyObstacles,
		DestroyObstaclesDone
	}

	private DestroyObstacleState m_State;

	private Dictionary<string, int> m_destroyedObstaclesDictionary;

	private int m_NumberOfTilesBlockedDestroyed;

	public DestroyObstacleState State
	{
		get
		{
			return m_State;
		}
		set
		{
			m_State = value;
		}
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = DestroyObstacleState.SelectObstaclesPosition;
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
						m_ValidTilesInAreaEffect.Add(cTile);
					}
				}
			}
		}
		else if (UseSubAbilityTargeting)
		{
			if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
			{
				m_ValidTilesInAreaEffect.AddRange(ParentAbility.InlineSubAbilityTiles);
			}
			else if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
			{
				if (ParentAbility.AreaEffect != null)
				{
					m_ValidTilesInAreaEffect.AddRange(ParentAbility.ValidTilesInAreaEffectedIncludingBlocked);
				}
				else
				{
					m_ValidTilesInAreaEffect.AddRange(ParentAbility.TilesSelected);
				}
			}
		}
		else if (AreaEffect == null)
		{
			if (Range == 0)
			{
				m_ValidTilesInAreaEffect = new List<CTile>();
				m_ValidTilesInAreaEffect.Add(ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y]);
			}
			else
			{
				m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			TilesInRange = m_ValidTilesInAreaEffect.ToList();
		}
		else
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, ScenarioManager.Tiles[targetActor.ArrayIndex.X, targetActor.ArrayIndex.Y], AreaEffect, 0f, flipped: false, AreaEffectFlipAxis, GameState.GetTilesInRange(targetActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true), ref m_ValidTilesInAreaEffectIncludingBlocked, ref m_FlagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks);
		}
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if (!prop.OverrideDisallowDestroyAndMove && (prop.PropHealthDetails == null || !prop.PropHealthDetails.HasHealth))
			{
				continue;
			}
			foreach (CTile item in from x in prop.GetTiles()
				select ScenarioManager.GetTile(x))
			{
				m_ValidTilesInAreaEffect.Remove(item);
				TilesInRange.Remove(item);
			}
		}
		ExpandTileListWithLinkedProps(m_ValidTilesInAreaEffect);
		m_destroyedObstaclesDictionary = new Dictionary<string, int>();
		m_NumberOfTilesBlockedDestroyed = 0;
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
		case DestroyObstacleState.SelectObstaclesPosition:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				if (m_AllTargets)
				{
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
					else if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)
					{
						PhaseManager.StepComplete();
						break;
					}
				}
				if (!AbilityHasBeenCancelled)
				{
					List<CAbilityFilter.EFilterTile> tileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Terrain };
					CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
					{
						m_TileFilter = tileFilter,
						m_IsPositive = IsPositive(),
						m_SpawnType = ScenarioManager.ObjectImportType.None
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
				break;
			}
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				if (item.FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle)
				{
					if (!m_TilesSelected.Contains(item))
					{
						m_TilesSelected.Add(item);
					}
					m_State = DestroyObstacleState.DestroyObstacles;
					Perform();
					break;
				}
			}
			m_State = DestroyObstacleState.DestroyObstaclesDone;
			Perform();
			break;
		case DestroyObstacleState.PreDestroyObstacleProcessing:
			foreach (CTile item2 in m_TilesSelected)
			{
				CObjectProp cObjectProp2 = item2.FindTerrainProp();
				if (cObjectProp2 != null)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(cObjectProp2);
				}
			}
			PhaseManager.StepComplete();
			break;
		case DestroyObstacleState.DestroyObstacles:
		{
			m_CanUndo = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			bool faceObstacle = !IsMergedAbility;
			List<CObjectProp> list = new List<CObjectProp>();
			foreach (CTile item3 in m_TilesSelected)
			{
				AbilityHasHappened = true;
				CObjectProp cObjectProp = item3.FindTerrainProp();
				if (cObjectProp != null)
				{
					if (ParentAbility is CAbilityMerged cAbilityMerged && cAbilityMerged.GetMergedWithAbility(this).AbilityHasBeenCancelled)
					{
						faceObstacle = true;
					}
					m_destroyedObstaclesDictionary.TryGetValue(cObjectProp.PrefabName, out var value);
					m_destroyedObstaclesDictionary[cObjectProp.PrefabName] = value + 1;
					m_NumberOfTilesBlockedDestroyed++;
					cObjectProp.DestroyProp(0f, sendMessageToClient: false, forceDestroy: false, this);
					list.Add(cObjectProp);
				}
			}
			CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(AnimOverload, TargetingActor, faceObstacle)
			{
				m_ActorDestroyingObstacle = TargetingActor,
				m_Tiles = m_TilesSelected.ToList(),
				m_DestroyedProps = list.ToList(),
				m_DestroyDelay = SpawnDelay,
				m_DestroyObstacleAbility = this
			};
			ScenarioRuleClient.MessageHandler(message);
			return true;
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
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (!AllTargets && !TilesSelected.Contains(selectedTile) && m_State == DestroyObstacleState.SelectObstaclesPosition && m_NumberTargetsRemaining > 0 && m_ValidTilesInAreaEffect.Contains(selectedTile) && selectedTile.FindTerrainProp() != null)
			{
				m_TilesSelected.Add(selectedTile);
				m_TilesSelected.AddRange(GetLinkedTiles(selectedTile));
				m_NumberTargetsRemaining--;
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
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
		if (!AllTargets)
		{
			using (new CActiveBonus.ActiveBonusCalculationScope())
			{
				if (m_TilesSelected.Contains(selectedTile))
				{
					selectedTile.FindTerrainProp();
					m_TilesSelected.Remove(selectedTile);
					foreach (CTile linkedTile in GetLinkedTiles(selectedTile))
					{
						m_TilesSelected.Remove(linkedTile);
					}
					m_NumberTargetsRemaining++;
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
						cPlayerSelectedTile_MessageData.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
					}
				}
			}
			if (flag)
			{
				Perform();
			}
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == DestroyObstacleState.SelectObstaclesPosition)
		{
			Restart();
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == DestroyObstacleState.SelectObstaclesPosition;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == DestroyObstacleState.SelectObstaclesPosition;
		}
		return false;
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState)
		{
			m_State++;
		}
		return m_State == DestroyObstacleState.DestroyObstaclesDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = DestroyObstacleState.SelectObstaclesPosition;
		m_TilesSelected.Clear();
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
						m_ValidTilesInAreaEffect.Add(cTile);
					}
				}
			}
		}
		else if (AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			TilesInRange = m_ValidTilesInAreaEffect;
		}
		m_ValidTilesInAreaEffect.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		if (NumberTargets == OriginalTargetCount)
		{
			m_AllTargets = false;
		}
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
			foreach (CTile item in m_ValidTilesInAreaEffect)
			{
				if (item.FindTerrainProp() != null)
				{
					CTile cTile2 = item;
					if (cTile2 != null && !m_TilesSelected.Contains(cTile2))
					{
						m_TilesSelected.Add(cTile2);
					}
				}
			}
		}
		m_destroyedObstaclesDictionary = new Dictionary<string, int>();
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_NumberOfTilesBlockedDestroyed = 0;
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		GameState.UpdateObstaclesDestroyedThisTurn(m_NumberOfTilesBlockedDestroyed);
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDestroyTerrain(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, m_destroyedObstaclesDictionary));
	}

	public override string GetDescription()
	{
		return "DestroyTerrain";
	}

	public bool HasPassedState(DestroyObstacleState destroyState)
	{
		return m_State > destroyState;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return false;
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		return m_TilesSelected.Count > 0;
	}

	public override bool ShouldCheckLinkedTiles()
	{
		return true;
	}
}
