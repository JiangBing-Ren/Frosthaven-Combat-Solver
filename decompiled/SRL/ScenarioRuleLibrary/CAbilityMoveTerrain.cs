using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityMoveTerrain : CAbility
{
	public enum EMoveTerrainState
	{
		SelectTerrainToMove,
		PreSelectMoveTile,
		SelectMoveToTile,
		PreMovingTerrain,
		RemoveTerrain,
		PlaceTerrain,
		MovedTerrain,
		MoveTerrainDone
	}

	public bool HightlightOriginTile;

	private EMoveTerrainState m_State;

	private CTile m_SelectedTerrainTile;

	private CTile m_TileToMoveTo;

	private int m_TerrainsToMove;

	private string m_MovingPropName;

	private ScenarioManager.ObjectImportType m_MovingPropType;

	private Dictionary<string, int> m_MovedTerrainDictionary;

	private List<CObjectProp> m_MovedTerrains;

	[JsonProperty]
	private int m_MovingTerrainIndex;

	[JsonProperty]
	private List<CTile> m_MovedTerrainTiles;

	public CTile SelectedTerrainTile => m_SelectedTerrainTile;

	public CTile TileToMoveTo => m_TileToMoveTo;

	[JsonProperty]
	public int MoveTerrainRange { get; set; }

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			if (SelectedTerrainTile != null)
			{
				CObjectProp cObjectProp = SelectedTerrainTile.FindTerrainProp();
				if (cObjectProp != null)
				{
					return cObjectProp.ObjectType;
				}
			}
			return m_MovingPropType;
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
			if (SelectedTerrainTile != null)
			{
				CObjectProp cObjectProp = SelectedTerrainTile.FindTerrainProp();
				if (cObjectProp != null)
				{
					return cObjectProp.PrefabName;
				}
			}
			return string.Empty;
		}
	}

	public CAbilityMoveTerrain(int moveTerrainRange)
	{
		MoveTerrainRange = moveTerrainRange;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = EMoveTerrainState.SelectTerrainToMove;
		if (NumberTargets == -1)
		{
			List<CTile> source = (from terrain in ScenarioManager.CurrentScenarioState.Props
				where IsTerrainType(terrain.ObjectType)
				select terrain.PropTile).ToList();
			NumberTargets = source.Count();
		}
		m_TerrainsToMove = NumberTargets;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_MovingTerrainIndex = 0;
		m_MovedTerrainDictionary = new Dictionary<string, int>();
		m_MovedTerrainTiles = new List<CTile>();
		m_MovedTerrains = new List<CObjectProp>();
		LogEvent(ESESubTypeAbility.AbilityStart);
		if (MiscVars == null)
		{
			MiscVars = new List<(string, TValue)>();
		}
		if (!MiscVars.Any(((string, TValue) p) => p.Item1 == "includeCasterTile"))
		{
			MiscVars.Add(("includeCasterTile", true));
		}
		SetHasStarted(val: true);
	}

	private bool IsTerrainType(ScenarioManager.ObjectImportType type)
	{
		if (type != ScenarioManager.ObjectImportType.TerrainRubble && type != ScenarioManager.ObjectImportType.TerrainWater && type != ScenarioManager.ObjectImportType.TerrainHotCoals && type != ScenarioManager.ObjectImportType.TerrainThorns && type != ScenarioManager.ObjectImportType.TerrainIce)
		{
			return type == ScenarioManager.ObjectImportType.TerrainVisualEffect;
		}
		return true;
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
		if (m_MovingTerrainIndex >= m_TerrainsToMove)
		{
			CObjectProp.CheckRelocateWaterMonsters();
			PhaseManager.NextStep();
			return true;
		}
		List<CTile> list2;
		AbilityData.MiscAbilityData miscAbilityData8;
		switch (m_State)
		{
		case EMoveTerrainState.SelectTerrainToMove:
			if (AllTargetsOnMovePath)
			{
				for (int i = 0; i < CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart.Count; i++)
				{
					if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(i))
					{
						Point point = CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart[i];
						CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
						if (cTile != null)
						{
							TilesInRange.Add(cTile);
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
						List<CTile> list = new List<CTile>();
						for (int j = 0; j < CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart.Count; j++)
						{
							if (MiscAbilityData?.MovePathIndexFilter == null || MiscAbilityData.MovePathIndexFilter.Compare(j))
							{
								Point point2 = CAbilityMove.AllPlayerArrayIndexOnPathIncludingStart[j];
								CTile cTile2 = ScenarioManager.Tiles[point2.X, point2.Y];
								list.Add(cTile2);
								if (cTile2 != null)
								{
									TilesInRange.AddRange(GameState.GetTilesInRange(cTile2.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true));
								}
							}
						}
						TilesInRange = TilesInRange.Distinct().ToList();
						goto IL_052c;
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
							goto IL_052c;
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
							foreach (CTile item2 in ParentAbility.TilesSelected)
							{
								TilesInRange.AddRange(GameState.GetTilesInRange(item2.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
							}
							TilesInRange = TilesInRange.Distinct().ToList();
							goto IL_052c;
						}
					}
					TilesInRange = ParentAbility.TilesSelected.ToList();
				}
			}
			else if (AreaEffect == null && Targeting != EAbilityTargeting.PerformVars)
			{
				if (Targeting == EAbilityTargeting.Custom)
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				else
				{
					TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, skipPathingCheck: false, includeStartingTile: true);
				}
			}
			goto IL_052c;
		case EMoveTerrainState.PreSelectMoveTile:
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = EMoveTerrainState.SelectMoveToTile;
			Perform();
			break;
		case EMoveTerrainState.SelectMoveToTile:
		{
			m_CanUndo = false;
			m_SelectedTerrainTile = m_TilesSelected[0];
			m_TilesSelected.Clear();
			NumberTargets = 1;
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			if (Targeting != EAbilityTargeting.PerformVars)
			{
				if (Targeting == EAbilityTargeting.Custom)
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				else
				{
					TilesInRange = GameState.GetTilesInRange(m_SelectedTerrainTile.m_ArrayIndex, MoveTerrainRange, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
				}
			}
			List<CTile> list4 = new List<CTile>();
			foreach (CTile item3 in TilesInRange)
			{
				if (CAbilityFilter.IsValidTile(item3, CAbilityFilter.EFilterTile.FeaturelessHex, includeInitial: false, null, null, VarsContext))
				{
					list4.Add(item3);
				}
			}
			TilesInRange = list4.ToList();
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message8 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.FeaturelessHex },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message8);
			}
			break;
		}
		case EMoveTerrainState.PreMovingTerrain:
		{
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			CObjectProp item = m_SelectedTerrainTile.FindTerrainProp();
			m_MovedTerrains.Add(item);
			CMovingTerrain_MessageData message7 = new CMovingTerrain_MessageData(AnimOverload, TargetingActor)
			{
				m_Ability = this,
				m_From = m_SelectedTerrainTile,
				m_To = TilesSelected[0]
			};
			ScenarioRuleClient.MessageHandler(message7);
			break;
		}
		case EMoveTerrainState.RemoveTerrain:
			AbilityHasHappened = true;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				if (m_MovingTerrainIndex < m_TerrainsToMove)
				{
					CObjectProp cObjectProp = m_SelectedTerrainTile.FindTerrainProp();
					m_MovingPropName = cObjectProp.PrefabName;
					m_MovingPropType = cObjectProp.ObjectType;
					cObjectProp.DestroyProp(0f, sendMessageToClient: true, forceDestroy: false, this);
					m_MovedTerrainDictionary.TryGetValue(cObjectProp.PrefabName, out var value);
					m_MovedTerrainDictionary[cObjectProp.PrefabName] = value + 1;
					m_State = EMoveTerrainState.PlaceTerrain;
					Perform();
				}
			}
			else
			{
				PhaseManager.NextStep();
			}
			break;
		case EMoveTerrainState.PlaceTerrain:
		{
			CTile cTile3 = TilesSelected[0];
			m_MovedTerrainTiles.Add(cTile3);
			m_MovingTerrainIndex++;
			CActor owner = m_MovedTerrains.LastOrDefault()?.Owner;
			switch (m_MovingPropType)
			{
			case ScenarioManager.ObjectImportType.TerrainHotCoals:
			case ScenarioManager.ObjectImportType.TerrainThorns:
			{
				CObjectHazardousTerrain prop4 = new CObjectHazardousTerrain(m_MovingPropName, m_MovingPropType, new TileIndex(cTile3.m_ArrayIndex), null, null, owner, cTile3.m_HexMap.MapGuid);
				cTile3.SpawnProp(prop4, notifyClient: true, SpawnDelay, this);
				Log.Info("Spawned prop " + m_MovingPropType);
				CCombatLogMessage_MessageData message5 = new CCombatLogMessage_MessageData
				{
					m_MessageKey = "COMBAT_LOG_MOVE_PROP",
					m_Actors = new List<CActor> { TargetingActor },
					m_Params = new List<string> { "Actor_0", "<sprite name=HazardHex>" }
				};
				ScenarioRuleClient.MessageHandler(message5);
				break;
			}
			case ScenarioManager.ObjectImportType.TerrainWater:
			case ScenarioManager.ObjectImportType.TerrainRubble:
			{
				CObjectDifficultTerrain prop3 = new CObjectDifficultTerrain(m_MovingPropName, m_MovingPropType, new TileIndex(cTile3.m_ArrayIndex), null, null, owner, cTile3.m_HexMap.MapGuid, treatAsTrap: false, new CObjectiveFilter());
				cTile3.SpawnProp(prop3, notifyClient: true, SpawnDelay, this);
				Log.Info("Spawned prop " + m_MovingPropType);
				if (m_MovingPropType == ScenarioManager.ObjectImportType.TerrainWater)
				{
					CCombatLogMessage_MessageData message3 = new CCombatLogMessage_MessageData
					{
						m_MessageKey = "COMBAT_LOG_MOVE_PROP",
						m_Actors = new List<CActor> { TargetingActor },
						m_Params = new List<string> { "Actor_0", "<sprite name=WaterHex>" }
					};
					ScenarioRuleClient.MessageHandler(message3);
				}
				else
				{
					CCombatLogMessage_MessageData message4 = new CCombatLogMessage_MessageData
					{
						m_MessageKey = "COMBAT_LOG_MOVE_PROP",
						m_Actors = new List<CActor> { TargetingActor },
						m_Params = new List<string> { "Actor_0", "<sprite name=DifficultHex>" }
					};
					ScenarioRuleClient.MessageHandler(message4);
				}
				break;
			}
			case ScenarioManager.ObjectImportType.TerrainIce:
			{
				CObjectIceTerrain prop2 = new CObjectIceTerrain(m_MovingPropType.ToString(), m_MovingPropType, new TileIndex(cTile3.m_ArrayIndex), null, null, owner, cTile3.m_HexMap.MapGuid);
				cTile3.SpawnProp(prop2, notifyClient: true, SpawnDelay, this);
				CCombatLogMessage_MessageData message2 = new CCombatLogMessage_MessageData
				{
					m_MessageKey = "COMBAT_LOG_MOVE_PROP",
					m_Actors = new List<CActor> { TargetingActor },
					m_Params = new List<string> { "Actor_0", "<sprite name=IceHex>" }
				};
				ScenarioRuleClient.MessageHandler(message2);
				Log.Info("Spawned prop " + m_MovingPropType);
				break;
			}
			case ScenarioManager.ObjectImportType.TerrainVisualEffect:
			{
				CObjectTerrainVisual prop = new CObjectTerrainVisual(m_MovingPropName, m_MovingPropType, new TileIndex(cTile3.m_ArrayIndex), null, null, owner, cTile3.m_HexMap.MapGuid);
				cTile3.SpawnProp(prop, notifyClient: true, SpawnDelay, this);
				Log.Info("Spawned prop " + m_MovingPropType);
				break;
			}
			}
			bool flag = false;
			if (SubAbilities.Count > 0 && SubAbilities.Any((CAbility a) => a.IsInlineSubAbility))
			{
				List<CAbility> list3 = new List<CAbility>();
				foreach (CAbility item4 in SubAbilities.Where((CAbility w) => w.IsInlineSubAbility))
				{
					CAbility cAbility = CAbility.CopyAbility(item4, generateNewID: true, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, VarsContext);
					cAbility.ParentAbility = this;
					cAbility.InlineSubAbilityTiles.Add(cTile3);
					list3.Add(cAbility);
				}
				(PhaseManager.CurrentPhase as CPhaseAction).StackInlineSubAbilities(list3, null, performNow: false, stopPlayerSkipping: false, true, stopPlayerUndo: false, null, ignorePerformNow: true);
				flag = true;
			}
			if (flag)
			{
				m_State = EMoveTerrainState.MovedTerrain;
				CWaitForProgressChoreographer_MessageData message6 = new CWaitForProgressChoreographer_MessageData(TargetingActor)
				{
					WaitTickFrame = 10000,
					WaitActor = TargetingActor,
					ClearEvents = false
				};
				ScenarioRuleClient.MessageHandler(message6);
				return true;
			}
			m_State = EMoveTerrainState.MovedTerrain;
			Perform();
			break;
		}
		case EMoveTerrainState.MovedTerrain:
			{
				CObjectProp.CheckRelocateWaterMonsters();
				if (m_MovingTerrainIndex <= m_TerrainsToMove - 1)
				{
					m_State = EMoveTerrainState.SelectTerrainToMove;
					Perform();
				}
				else
				{
					PhaseManager.StepComplete();
				}
				break;
			}
			IL_052c:
			list2 = new List<CTile>();
			foreach (CTile item5 in TilesInRange)
			{
				if (CAbilityFilter.IsValidTile(item5, CAbilityFilter.EFilterTile.Terrain, includeInitial: false, null, null, VarsContext) && !m_MovedTerrainTiles.Contains(item5))
				{
					list2.Add(item5);
				}
			}
			TilesInRange = list2.ToList();
			TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
			miscAbilityData8 = MiscAbilityData;
			if (miscAbilityData8 != null && miscAbilityData8.TerrainTypes?.Count > 0)
			{
				TilesInRange.RemoveAll((CTile t) => t != null && !t.m_Props.Any((CObjectProp p) => MiscAbilityData.TerrainTypes.Contains(p.PropType)));
			}
			if (Targeting != EAbilityTargeting.PerformVars)
			{
				m_TilesSelected.Clear();
				NumberTargets = 1;
				m_NumberTargetsRemaining = NumberTargets;
				m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Terrain },
					m_IsPositive = IsPositive(),
					m_SpawnType = ScenarioManager.ObjectImportType.None
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message9 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message9);
			}
			else
			{
				CPlayerIsStunned_MessageData message10 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message10);
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
			if (m_State == EMoveTerrainState.SelectTerrainToMove && m_NumberTargetsRemaining > 0)
			{
				if (TilesInRange.Contains(selectedTile))
				{
					CObjectProp cObjectProp = selectedTile.FindTerrainProp();
					if (cObjectProp != null)
					{
						CTile cTile = ScenarioManager.Tiles[cObjectProp.ArrayIndex.X, cObjectProp.ArrayIndex.Y];
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
			}
			else if (m_State == EMoveTerrainState.SelectMoveToTile && m_NumberTargetsRemaining > 0)
			{
				if (TilesInRange.Contains(selectedTile))
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
		if (m_State == EMoveTerrainState.SelectTerrainToMove && Targeting != EAbilityTargeting.PerformVars)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (m_State != EMoveTerrainState.SelectTerrainToMove)
		{
			return m_State == EMoveTerrainState.SelectMoveToTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EMoveTerrainState.SelectTerrainToMove)
			{
				return m_State == EMoveTerrainState.SelectMoveToTile;
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
		return m_State == EMoveTerrainState.MoveTerrainDone;
	}

	public override void Restart()
	{
		base.Restart();
		m_State = EMoveTerrainState.SelectTerrainToMove;
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		m_TilesSelected.Clear();
		if (AreaEffect == null)
		{
			m_ValidTilesInAreaEffect = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			TilesInRange = m_ValidTilesInAreaEffect;
			TilesInRange.RemoveAll((CTile t) => t?.m_Props.Any((CObjectProp p) => p.OverrideDisallowDestroyAndMove || (p.PropHealthDetails != null && p.PropHealthDetails.HasHealth)) ?? false);
		}
		m_MovedTerrainDictionary = new Dictionary<string, int>();
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
		return "MoveTerrain";
	}

	public bool HasPassedState(EMoveTerrainState moveTerrainState)
	{
		return m_State > moveTerrainState;
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
