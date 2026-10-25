using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMoveObstacle : CAbility
{
	public enum MoveObstacleState
	{
		SelectObstacleToMove,
		PreSelectMoveTile,
		SelectMoveToTile,
		PreMovingObstacles,
		RemoveObstacle,
		PlaceObstacle,
		MovedObstacle,
		MoveObstacleDone
	}

	private MoveObstacleState m_State;

	private List<CTile> m_SelectedObstacleTiles;

	private int m_MovingObstacleIndex;

	private string m_MovingPropName;

	private Dictionary<string, int> m_MovedObstaclesDictionary;

	[JsonProperty]
	public int MoveObstacleRange { get; set; }

	[JsonProperty]
	public string MoveObstacleAnimOverload { get; set; }

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return ScenarioManager.ObjectImportType.Obstacle;
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
			if (m_SelectedObstacleTiles != null && m_SelectedObstacleTiles.Count > 0)
			{
				CObjectObstacle cObjectObstacle = m_SelectedObstacleTiles[0].FindObstacleProp();
				if (cObjectObstacle != null)
				{
					return cObjectObstacle.PrefabName;
				}
			}
			return string.Empty;
		}
	}

	public CAbilityMoveObstacle(int moveObstacleRange, string moveObstacleAnimOverload)
	{
		MoveObstacleRange = moveObstacleRange;
		MoveObstacleAnimOverload = moveObstacleAnimOverload;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = MoveObstacleState.SelectObstacleToMove;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
		}
		m_SelectedObstacleTiles = new List<CTile>();
		m_MovedObstaclesDictionary = new Dictionary<string, int>();
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
		List<CTile> list3;
		AbilityData.MiscAbilityData miscAbilityData8;
		switch (m_State)
		{
		case MoveObstacleState.SelectObstacleToMove:
			if (AllTargetsOnMovePath)
			{
				for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart.Count; i++)
				{
					if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
					{
						Point point = CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart[i];
						CTile cTile2 = ScenarioManager.Tiles[point.X, point.Y];
						if (cTile2 != null)
						{
							TilesInRange.Add(cTile2);
						}
					}
				}
			}
			else if (UseSubAbilityTargeting)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentMovePath.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
					if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentMovePath.Value && ParentAbility is CAbilityMove)
					{
						List<CTile> list2 = new List<CTile>();
						for (int j = 0; j < CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart.Count; j++)
						{
							if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(j))
							{
								Point point2 = CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart[j];
								CTile cTile3 = ScenarioManager.Tiles[point2.X, point2.Y];
								list2.Add(cTile3);
								if (cTile3 != null)
								{
									TilesInRange.AddRange(GameState.GetTilesInRange(cTile3.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true));
								}
							}
						}
						TilesInRange = TilesInRange.Distinct().ToList();
						goto IL_04f7;
					}
				}
				if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
						if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
							{
								TilesInRange.AddRange(GameState.GetTilesInRange(inlineSubAbilityTile.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
							}
							TilesInRange = TilesInRange.Distinct().ToList();
							goto IL_04f7;
						}
					}
					TilesInRange = InlineSubAbilityTiles.ToList();
				}
				else if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData6 = MiscAbilityData;
					if (miscAbilityData6 != null && miscAbilityData6.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData7 = MiscAbilityData;
						if (miscAbilityData7 != null && miscAbilityData7.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile item in ParentAbility.TilesSelected)
							{
								TilesInRange.AddRange(GameState.GetTilesInRange(item.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
							}
							TilesInRange = TilesInRange.Distinct().ToList();
							goto IL_04f7;
						}
					}
					TilesInRange = ParentAbility.TilesSelected.ToList();
				}
			}
			else if (AreaEffect == null)
			{
				TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			}
			goto IL_04f7;
		case MoveObstacleState.PreSelectMoveTile:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = MoveObstacleState.SelectMoveToTile;
			Perform();
			break;
		case MoveObstacleState.SelectMoveToTile:
		{
			m_CanUndo = false;
			m_SelectedObstacleTiles.AddRange(m_TilesSelected);
			m_TilesSelected.Clear();
			NumberTargets = m_SelectedObstacleTiles.Count;
			m_NumberTargetsRemaining = NumberTargets;
			Range = MoveObstacleRange;
			TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: true, ignoreBlocked: true);
			List<CTile> list = new List<CTile>();
			foreach (CTile item2 in TilesInRange)
			{
				List<CTile> extraUnblockedTiles = new List<CTile>();
				MF.IsBlockingSpawnPosition(null, null, TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles);
				if (CAbilityFilter.IsValidTile(item2, CAbilityFilter.EFilterTile.EmptyHex, includeInitial: false, null, null, VarsContext) && !MF.IsBlockingSpawnPosition(item2, TilesSelected, TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles))
				{
					list.Add(item2);
				}
			}
			TilesInRange = list.ToList();
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		}
		case MoveObstacleState.PreMovingObstacles:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			AbilityHasHappened = true;
			m_MovingObstacleIndex = 0;
			foreach (CTile selectedObstacleTile in m_SelectedObstacleTiles)
			{
				CTile propTile = selectedObstacleTile;
				CObjectObstacle cObjectObstacle2 = selectedObstacleTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle2 == null)
				{
					cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(selectedObstacleTile.m_ArrayIndex, ref propTile);
				}
				if (cObjectObstacle2 != null)
				{
					ScenarioManager.CurrentScenarioState.Props.Remove(cObjectObstacle2);
				}
			}
			if (IsMergedAbility)
			{
				PhaseManager.StepComplete();
				break;
			}
			m_State = MoveObstacleState.RemoveObstacle;
			Perform();
			break;
		case MoveObstacleState.RemoveObstacle:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				if (m_MovingObstacleIndex < m_SelectedObstacleTiles.Count)
				{
					CTile cTile = m_SelectedObstacleTiles[m_MovingObstacleIndex];
					CObjectObstacle cObjectObstacle = cTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
					m_MovingPropName = cObjectObstacle.PrefabName;
					cObjectObstacle.DestroyProp(0f, sendMessageToClient: false);
					CDestroyObstacle_MessageData message3 = new CDestroyObstacle_MessageData(AnimOverload, TargetingActor)
					{
						m_ActorDestroyingObstacle = TargetingActor,
						m_Tiles = new List<CTile> { cTile },
						m_DestroyDelay = SpawnDelay,
						m_DestroyedProps = new List<CObjectProp> { cObjectObstacle },
						m_DestroyObstacleAbility = this,
						m_OverrideSetLastSelectedTile = true
					};
					ScenarioRuleClient.MessageHandler(message3);
					m_MovedObstaclesDictionary.TryGetValue(cObjectObstacle.PrefabName, out var value);
					m_MovedObstaclesDictionary[cObjectObstacle.PrefabName] = value + 1;
				}
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case MoveObstacleState.PlaceObstacle:
		{
			CPlacingSpawn_MessageData message2 = new CPlacingSpawn_MessageData(MoveObstacleAnimOverload, TargetingActor)
			{
				m_Ability = this,
				m_ActorPlacingSpawn = TargetingActor
			};
			ScenarioRuleClient.MessageHandler(message2);
			List<TileIndex> pathingBlockers = new List<TileIndex>
			{
				new TileIndex(TilesSelected[m_MovingObstacleIndex].m_ArrayIndex)
			};
			CObjectObstacle prop = new CObjectObstacle(m_MovingPropName, ScenarioManager.ObjectImportType.Obstacle, new TileIndex(TilesSelected[m_MovingObstacleIndex].m_ArrayIndex), null, null, pathingBlockers, TargetingActor, TilesSelected[m_MovingObstacleIndex].m_HexMap.MapGuid, ignoresFlyAndJump: false);
			TilesSelected[m_MovingObstacleIndex].SpawnProp(prop, notifyClient: true, SpawnDelay, this);
			break;
		}
		case MoveObstacleState.MovedObstacle:
			{
				m_MovingObstacleIndex++;
				if (m_MovingObstacleIndex <= m_TilesSelected.Count - 1)
				{
					m_State = MoveObstacleState.RemoveObstacle;
					Perform();
				}
				else
				{
					PhaseManager.StepComplete();
				}
				break;
			}
			IL_04f7:
			list3 = new List<CTile>();
			foreach (CTile item3 in TilesInRange)
			{
				if (CAbilityFilter.IsValidTile(item3, CAbilityFilter.EFilterTile.Obstacle, includeInitial: false, null, null, VarsContext))
				{
					list3.Add(item3);
				}
			}
			TilesInRange = list3.ToList();
			TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
			miscAbilityData8 = MiscAbilityData;
			if (miscAbilityData8 != null && miscAbilityData8.ObstacleTypes?.Count > 0)
			{
				TilesInRange.RemoveAll((CTile t) => t != null && !t.m_Props.Any((CObjectProp p) => MiscAbilityData.ObstacleTypes.Contains(p.PropType)));
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message4 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.SingleHexObstacle },
					m_IsPositive = IsPositive(),
					m_SpawnType = ScenarioManager.ObjectImportType.None
				};
				ScenarioRuleClient.MessageHandler(message4);
			}
			break;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message5 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
			}
			else
			{
				CPlayerIsStunned_MessageData message6 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message6);
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
		if (!TilesSelected.Contains(selectedTile))
		{
			if (m_State == MoveObstacleState.SelectObstacleToMove && m_NumberTargetsRemaining > 0)
			{
				if (TilesInRange.Contains(selectedTile) && selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) is CObjectObstacle cObjectObstacle && cObjectObstacle.PathingBlockers.Count <= 1)
				{
					CTile cTile = ScenarioManager.Tiles[cObjectObstacle.ArrayIndex.X, cObjectObstacle.ArrayIndex.Y];
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
			}
			else if (m_State == MoveObstacleState.SelectMoveToTile && m_NumberTargetsRemaining > 0)
			{
				List<CTile> extraUnblockedTiles = new List<CTile>();
				MF.IsBlockingSpawnPosition(null, null, TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles);
				if (TilesInRange.Contains(selectedTile) && !MF.IsBlockingSpawnPosition(selectedTile, m_TilesSelected, TargetingActor, ref extraUnblockedTiles, m_SelectedObstacleTiles))
				{
					m_TilesSelected.Add(selectedTile);
					m_NumberTargetsRemaining--;
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData message2 = new CPlayerSelectedTile_MessageData(TargetingActor)
						{
							m_Ability = this
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
				}
				else
				{
					ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(TargetingActor));
				}
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

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == MoveObstacleState.SelectObstacleToMove || m_State == MoveObstacleState.SelectMoveToTile)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != MoveObstacleState.SelectObstacleToMove)
		{
			return m_State == MoveObstacleState.SelectMoveToTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != MoveObstacleState.SelectObstacleToMove)
			{
				return m_State == MoveObstacleState.SelectMoveToTile;
			}
			return true;
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
		return m_State == MoveObstacleState.MoveObstacleDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = MoveObstacleState.SelectObstacleToMove;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			TilesInRange = m_ValidTilesInAreaEffect;
			TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		}
		m_SelectedObstacleTiles = new List<CTile>();
		m_MovedObstaclesDictionary = new Dictionary<string, int>();
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityMoveObstacle(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, m_MovedObstaclesDictionary));
	}

	public override string GetDescription()
	{
		return "MoveObstacle";
	}

	public bool HasPassedState(MoveObstacleState moveObstacleState)
	{
		return m_State > moveObstacleState;
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
