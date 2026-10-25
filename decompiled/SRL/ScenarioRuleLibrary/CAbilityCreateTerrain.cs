using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary;

public class CAbilityCreateTerrain : CAbility
{
	public enum CreateTerrainState
	{
		SelectTerrainPositions,
		PreCreateTerrain,
		CreatingTerrain,
		CreateTerrain,
		CreateDone
	}

	private CreateTerrainState m_State;

	private int m_PropsSpawned;

	[JsonProperty]
	private ScenarioManager.ObjectImportType _terrainType;

	[JsonIgnore]
	[StateDump(Skip = true)]
	private List<CObjectProp> terrainSpawned;

	[JsonProperty]
	public string PropName { get; set; }

	[JsonProperty]
	public string OverridePropName { get; set; }

	public ScenarioManager.ObjectImportType TerrainType => _terrainType;

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return TerrainType;
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
			if (Enum.TryParse<EPropType>(PropName, out var result))
			{
				return result.ToString();
			}
			return string.Empty;
		}
	}

	public CAbilityCreateTerrain(string propName)
	{
		terrainSpawned = new List<CObjectProp>();
		if (TileFilter == CAbilityFilter.EFilterTile.None)
		{
			TileFilter = CAbilityFilter.EFilterTile.FeaturelessHex;
		}
		PropName = propName;
		if (Enum.TryParse<ScenarioManager.ObjectImportType>(PropName, out var result))
		{
			_terrainType = result;
		}
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		terrainSpawned = new List<CObjectProp>();
		m_State = CreateTerrainState.SelectTerrainPositions;
		if (IsInlineSubAbility)
		{
			m_CanUndo = false;
		}
		m_PropsSpawned = 0;
		if (AreaEffect != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (GetMiscVar("HideInvalidTiles").GetBool())
		{
			TilesInRange.RemoveAll((CTile x) => !CAbilityFilter.IsValidTile(x, TileFilter, includeInitial: false, null, parentContext: VarsContext, vars: MiscVars));
		}
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
		}
		else
		{
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
		List<CTile> failTile;
		List<string> failReason;
		List<CTile> possibleTiles;
		switch (m_State)
		{
		case CreateTerrainState.SelectTerrainPositions:
			if (UseSubAbilityTargeting)
			{
				if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData3 = MiscAbilityData;
						if (miscAbilityData3 != null && miscAbilityData3.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
							{
								TilesInRange.AddRange(GameState.GetTilesInRange(inlineSubAbilityTile.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: true));
							}
							TilesInRange = TilesInRange.Distinct().ToList();
							foreach (CTile inlineSubAbilityTile2 in InlineSubAbilityTiles)
							{
								TilesInRange.Remove(inlineSubAbilityTile2);
							}
							goto IL_038c;
						}
					}
					TilesInRange = InlineSubAbilityTiles.ToList();
				}
				else if (ParentAbility != null && ParentAbility.TilesSelected != null && ParentAbility.TilesSelected.Count > 0)
				{
					AbilityData.MiscAbilityData miscAbilityData4 = MiscAbilityData;
					if (miscAbilityData4 != null && miscAbilityData4.AllTargetsAdjacentToParentTargets.HasValue)
					{
						AbilityData.MiscAbilityData miscAbilityData5 = MiscAbilityData;
						if (miscAbilityData5 != null && miscAbilityData5.AllTargetsAdjacentToParentTargets.Value)
						{
							foreach (CTile item in ParentAbility.TilesSelected)
							{
								TilesInRange.AddRange(GameState.GetTilesInRange(item.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: false, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: true));
							}
							TilesInRange = TilesInRange.Distinct().ToList();
							foreach (CTile item2 in ParentAbility.TilesSelected)
							{
								TilesInRange.Remove(item2);
							}
							goto IL_038c;
						}
					}
					TilesInRange = ParentAbility.TilesSelected.ToList();
				}
				goto IL_038c;
			}
			if (IsMergedAbility)
			{
				AbilityData.MiscAbilityData miscAbilityData6 = MiscAbilityData;
				if (miscAbilityData6 == null || !miscAbilityData6.IgnoreMergedAbilityTargetSelection.HasValue)
				{
					if (ParentAbility != null && ParentAbility is CAbilityMerged cAbilityMerged)
					{
						CAbility mergedWithAbility = cAbilityMerged.GetMergedWithAbility(this);
						if (mergedWithAbility != null && mergedWithAbility.TilesSelected != null && mergedWithAbility.TilesSelected.Count > 0)
						{
							TilesInRange.Clear();
							foreach (CTile item3 in mergedWithAbility.TilesSelected)
							{
								AbilityData.MiscAbilityData miscAbilityData7 = MiscAbilityData;
								if (miscAbilityData7 != null && miscAbilityData7.UseMergedWithAbilityTiles.HasValue)
								{
									AbilityData.MiscAbilityData miscAbilityData8 = MiscAbilityData;
									if (miscAbilityData8 != null && miscAbilityData8.UseMergedWithAbilityTiles.Value)
									{
										TilesInRange.Add(item3);
										continue;
									}
								}
								CActor cActor = ScenarioManager.Scenario.FindActorAt(item3.m_ArrayIndex);
								if (cActor != null)
								{
									m_ActorsToIgnore.Add(cActor);
								}
								TilesInRange.AddRange(GameState.GetTilesInRange(item3.m_ArrayIndex, Range, Targeting, emptyTilesOnly: false));
							}
						}
					}
					goto IL_06d5;
				}
			}
			if (Targeting == EAbilityTargeting.Custom)
			{
				if (!GetMiscVar("DontClearAreaEffect").GetBool())
				{
					AreaEffect = null;
				}
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
			else if (AreaEffect == null)
			{
				bool flag = IncludeCasterTile();
				TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, includeStartingTile: flag, ignoreLOS: MiscAbilityData?.IgnoreLOS ?? false);
				List<CTile> list2 = new List<CTile>();
				foreach (CTile item4 in TilesInRange)
				{
					if (CAbilityFilter.IsValidTile(item4, TileFilter, includeInitial: false, null, null, VarsContext))
					{
						list2.Add(item4);
					}
				}
				TilesInRange = list2.ToList();
			}
			if (GetMiscVar("HideInvalidTiles").GetBool())
			{
				TilesInRange.RemoveAll((CTile x) => !CAbilityFilter.IsValidTile(x, TileFilter, includeInitial: false, null, parentContext: VarsContext, vars: MiscVars));
			}
			goto IL_06d5;
		case CreateTerrainState.PreCreateTerrain:
			m_CanUndo = false;
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = CreateTerrainState.CreatingTerrain;
			Perform();
			break;
		case CreateTerrainState.CreatingTerrain:
			if (m_TilesSelected.Count != 0)
			{
				CCreatingTerrain_MessageData message = new CCreatingTerrain_MessageData(AnimOverload, TargetingActor)
				{
					m_Ability = this,
					m_Tiles = TilesSelected,
					m_ActorCreatingTerrain = TargetingActor
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				m_State = CreateTerrainState.CreateTerrain;
				Perform();
			}
			break;
		case CreateTerrainState.CreateTerrain:
			{
				if (TargetingActor.Type == CActor.EType.Player)
				{
					TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
					ScenarioRuleClient.FirstAbilityStarted();
				}
				if (m_TilesSelected.Count != 0 && m_PropsSpawned == 0)
				{
					if (AreaEffect != null)
					{
						AbilityHasHappened = true;
						List<TileIndex> list = new List<TileIndex>();
						foreach (CTile item5 in m_TilesSelected)
						{
							list.Add(new TileIndex(item5.m_ArrayIndex));
						}
						CVector3 cVector = null;
						if (AreaEffect != null)
						{
							float y = 180f - AreaEffectAngle;
							cVector = new CVector3(0f, y, 0f);
						}
						else
						{
							float y2 = 0.5f * (float)SharedClient.GlobalRNG.Next(180) + 0.75f * (float)SharedClient.GlobalRNG.Next(360);
							cVector = new CVector3(0f, y2, 0f);
						}
						foreach (CTile item6 in m_TilesSelected)
						{
							SpawnTerrain(item6, cVector);
						}
					}
					else
					{
						foreach (CTile item7 in m_TilesSelected)
						{
							AbilityHasHappened = true;
							CVector3 rotation = new CVector3(0f, 0.5f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(180) + 0.75f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(360), 0f);
							SpawnTerrain(item7, rotation);
						}
					}
					if (GetMiscVar("LinkProps").GetBool())
					{
						foreach (CObjectProp item8 in terrainSpawned)
						{
							foreach (CObjectProp item9 in terrainSpawned)
							{
								if (item9 != item8)
								{
									item8.LinkToOtherProp(item9);
								}
							}
						}
					}
					TargetingActor.m_OnCreatedListeners?.Invoke(this);
					PhaseManager.NextStep();
				}
				else if (!IsMergedAbility)
				{
					PhaseManager.NextStep();
				}
				return true;
			}
			IL_038c:
			if (TilesInRange.Count > 0)
			{
				List<CTile> list3 = new List<CTile>();
				foreach (CTile item10 in TilesInRange)
				{
					if (CAbilityFilter.IsValidTile(item10, TileFilter, includeInitial: false, null, null, VarsContext))
					{
						list3.Add(item10);
					}
				}
				TilesInRange = list3.ToList();
				if (AllTargets)
				{
					foreach (CTile item11 in TilesInRange)
					{
						SelectParentAbilityTiles(item11);
					}
					PhaseManager.StepComplete();
					break;
				}
				goto IL_06d5;
			}
			PhaseManager.NextStep();
			return true;
			IL_06d5:
			if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)
			{
				foreach (CTile item12 in TilesInRange)
				{
					SelectParentAbilityTiles(item12);
				}
				m_State = CreateTerrainState.PreCreateTerrain;
				Perform();
				break;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData cPlayerSelectingObjectPosition_MessageData = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { TileFilter },
					m_IsPositive = IsPositive()
				};
				if (GetAbilityBaseCard() is CAbilityCard cAbilityCard)
				{
					cPlayerSelectingObjectPosition_MessageData.m_ShowConsumeBar = cAbilityCard.GetAction(this)?.Augmentations?.Any() == true;
				}
				ScenarioRuleClient.MessageHandler(cPlayerSelectingObjectPosition_MessageData);
				break;
			}
			if (AllTargets)
			{
				m_TilesSelected.AddRange(TilesInRange);
				break;
			}
			CAIMoveNew.GetPossibleEmptyTiles(TargetingActor, TilesInRange, out failTile, out failReason, out possibleTiles);
			if (failTile.Count > 0)
			{
				Log.SimpleInfo(TargetingActor?.Class?.ID + " Failed to place " + PropName + " adjacent to " + TargetingActor?.Class?.ID);
				for (int num = 0; num < failTile.Count; num++)
				{
					Log.SimpleInfo("Tile at x:" + failTile[num].m_ArrayIndex.X + " y:" + failTile[num].m_ArrayIndex.Y + ", fail reason=" + failReason[num]);
				}
			}
			CAIMoveNew.SelectTilesBasedOnTargetingFocus(TargetingActor, Range, possibleTiles, Targeting, TilesSelected, NumberTargetsRemaining, AllTargets);
			m_State = CreateTerrainState.PreCreateTerrain;
			Perform();
			break;
		}
		return false;
		IL_0089:
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

	public void AddSelectedTiles(List<CTile> tiles)
	{
		m_TilesSelected.AddRange(tiles);
	}

	public static CObjectProp SpawnTerrain(ScenarioManager.ObjectImportType TerrainType, CTile tile, CVector3 rotation = null, CActor TargetingActor = null, float SpawnDelay = 0f, string OverridePropName = null, CAbility fromAbility = null)
	{
		string name = (string.IsNullOrEmpty(OverridePropName) ? TerrainType.ToString() : OverridePropName);
		switch (TerrainType)
		{
		case ScenarioManager.ObjectImportType.TerrainHotCoals:
		case ScenarioManager.ObjectImportType.TerrainThorns:
		{
			CObjectHazardousTerrain cObjectHazardousTerrain = new CObjectHazardousTerrain(name, TerrainType, new TileIndex(tile.m_ArrayIndex), null, rotation, TargetingActor, tile.m_HexMap.MapGuid);
			tile.SpawnProp(cObjectHazardousTerrain, notifyClient: true, SpawnDelay, fromAbility);
			Log.Info("Spawned prop " + TerrainType);
			return cObjectHazardousTerrain;
		}
		case ScenarioManager.ObjectImportType.TerrainWater:
		case ScenarioManager.ObjectImportType.TerrainRubble:
		{
			CObjectDifficultTerrain cObjectDifficultTerrain = new CObjectDifficultTerrain(name, TerrainType, new TileIndex(tile.m_ArrayIndex), null, rotation, TargetingActor, tile.m_HexMap.MapGuid, treatAsTrap: false, new CObjectiveFilter());
			tile.SpawnProp(cObjectDifficultTerrain, notifyClient: true, SpawnDelay, fromAbility);
			Log.Info("Spawned prop " + TerrainType);
			return cObjectDifficultTerrain;
		}
		case ScenarioManager.ObjectImportType.TerrainIce:
		{
			CObjectIceTerrain cObjectIceTerrain = new CObjectIceTerrain(TerrainType.ToString(), TerrainType, new TileIndex(tile.m_ArrayIndex), null, rotation, TargetingActor, tile.m_HexMap.MapGuid);
			tile.SpawnProp(cObjectIceTerrain, notifyClient: true, SpawnDelay, fromAbility);
			Log.Info("Spawned prop " + TerrainType);
			return cObjectIceTerrain;
		}
		case ScenarioManager.ObjectImportType.TerrainVisualEffect:
		{
			CObjectTerrainVisual cObjectTerrainVisual = new CObjectTerrainVisual(name, TerrainType, new TileIndex(tile.m_ArrayIndex), null, rotation, TargetingActor, tile.m_HexMap.MapGuid);
			tile.SpawnProp(cObjectTerrainVisual, notifyClient: true, SpawnDelay, fromAbility);
			Log.Info("Spawned prop " + TerrainType);
			return cObjectTerrainVisual;
		}
		default:
			return null;
		}
	}

	private CObjectProp SpawnTerrain(CTile tile, CVector3 rotation)
	{
		CObjectProp cObjectProp = SpawnTerrain(TerrainType, tile, rotation, TargetingActor, SpawnDelay, OverridePropName, this);
		if (cObjectProp != null)
		{
			terrainSpawned.Add(cObjectProp);
		}
		m_PropsSpawned++;
		return cObjectProp;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (!TilesSelected.Contains(selectedTile) && m_State == CreateTerrainState.SelectTerrainPositions)
		{
			if (AreaEffect != null)
			{
				if (!m_AreaEffectLocked && m_ValidTilesInAreaEffect.Count == ValidTilesInAreaEffectedIncludingBlocked.Count && m_ValidTilesInAreaEffect.Count == AreaEffect.EnabledHexes.Count())
				{
					foreach (CTile item in m_ValidTilesInAreaEffect)
					{
						if (!CAbilityFilter.IsValidTile(item, TileFilter, includeInitial: false, null, parentContext: VarsContext, vars: MiscVars) || CheckIfWillBeBlocking(item))
						{
							if (flag)
							{
								Perform();
							}
							base.TileSelected(selectedTile, optionalTileList);
							LogEvent(ESESubTypeAbility.AbilityTileSelected);
							return;
						}
					}
					m_TilesSelected.AddRange(m_ValidTilesInAreaEffect);
					m_AreaEffectLocked = true;
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
						cPlayerSelectedTile_MessageData.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
					}
				}
			}
			else if (TilesInRange.Contains(selectedTile))
			{
				if (m_TilesSelected.Count < NumberTargets)
				{
					if (!m_TilesSelected.Contains(selectedTile) && CAbilityFilter.IsValidTile(selectedTile, TileFilter, includeInitial: false, null, parentContext: VarsContext, vars: MiscVars))
					{
						m_TilesSelected.Add(selectedTile);
						flag = true;
					}
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData2 = new CPlayerSelectedTile_MessageData(TargetingActor);
						cPlayerSelectedTile_MessageData2.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData2);
						m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
						m_NumberTargetsRemaining--;
					}
				}
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CInvalidSpawnPosition_MessageData(TargetingActor));
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
		bool flag2 = false;
		if (AreaEffect != null && m_AreaEffectLocked)
		{
			m_AreaEffectLocked = false;
			m_TilesSelected.Clear();
			flag2 = true;
		}
		else
		{
			if (m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Remove(selectedTile);
			}
			flag2 = true;
		}
		if (flag2)
		{
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
				cPlayerSelectedTile_MessageData.m_Ability = this;
				ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				m_NumberTargetsRemaining++;
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

	public override void ClearTargets()
	{
		base.ClearTargets();
		if (m_State == CreateTerrainState.SelectTerrainPositions)
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == CreateTerrainState.SelectTerrainPositions;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == CreateTerrainState.SelectTerrainPositions;
		}
		return false;
	}

	public bool SelectParentAbilityTiles(CTile selectedTile)
	{
		if (TilesSelected.Contains(selectedTile))
		{
			return m_CanUndo;
		}
		if (m_State == CreateTerrainState.SelectTerrainPositions)
		{
			List<CTile> extraUnblockedTiles = new List<CTile>();
			MF.IsBlockingSpawnPosition(null, null, TargetingActor, ref extraUnblockedTiles);
			if (!MF.IsBlockingSpawnPosition(selectedTile, m_TilesSelected, TargetingActor, ref extraUnblockedTiles))
			{
				if (!m_TilesSelected.Contains(selectedTile))
				{
					m_TilesSelected.Add(selectedTile);
				}
				m_CanUndo = false;
				LogEvent(ESESubTypeAbility.AbilityTileSelected);
			}
			else
			{
				LogEvent(ESESubTypeAbility.AbilityTileSelected);
				PhaseManager.StepComplete();
			}
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
		return m_State == CreateTerrainState.CreateDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCreateTerrain(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, PropName, m_PropsSpawned));
	}

	public override string GetDescription()
	{
		return "Create Terrain R: " + Range + " N: " + NumberTargets;
	}

	public bool HasPassedState(CreateTerrainState createState)
	{
		return m_State > createState;
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

	public override bool InitValueFromVars(string key, TValue expressionValue, TValue value)
	{
		if (key == "OverridePropName")
		{
			OverridePropName = value.GetString();
			if (GetMiscVar("SpawnPropPreview").isUnknown)
			{
				SetMiscVar("SpawnPropPreview", OverridePropName);
			}
			return true;
		}
		return base.InitValueFromVars(key, expressionValue, value);
	}

	public override bool CanSkipUntergetableCheck()
	{
		return true;
	}

	public override CAbility GetQueuedAbility(CActor targetingActor)
	{
		CAbility cAbility = this;
		if ((PropName == "TerrainThorns" || PropName == "TerrainHotCoals") && targetingActor.Class.ID == "PyroclastID")
		{
			cAbility = CAbility.CopyAbility(this, generateNewID: false, fullCopy: false, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, null, EAbilityType.CreateLimitedHazardousTerrain);
			if (cAbility.Vars == null)
			{
				cAbility.Vars = new List<(string, TValue)>();
			}
			cAbility.Vars.Add(("OverridePropName", new TValue("PyroclastHazardTerrain")));
		}
		return cAbility;
	}
}
