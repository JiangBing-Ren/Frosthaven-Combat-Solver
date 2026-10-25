using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary;

public class CAbilityCreate : CAbility
{
	public enum CreateState
	{
		SelectObstaclePositions,
		PreCreateObstacles,
		CreatingObstacles,
		CreateObstacles,
		CreateDone
	}

	public class CCreateActorPath
	{
		public List<Point> m_ArrayIndices;

		public List<CTile> m_Waypoints;

		public CTile m_Tile;
	}

	private CreateState m_State;

	private int m_PropsSpawned;

	[JsonProperty]
	public string PropName { get; set; }

	[JsonProperty]
	public string OverridePropName { get; set; }

	private string propCustomLoc => GetMiscVar("PropLocKey").GetString();

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
			if (Enum.TryParse<EPropType>(PropName, out var result))
			{
				return result.ToString();
			}
			return string.Empty;
		}
	}

	public CAbilityCreate(string propName)
	{
		if (TileFilter == CAbilityFilter.EFilterTile.None)
		{
			TileFilter = CAbilityFilter.EFilterTile.EmptyHex;
		}
		PropName = propName;
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = CreateState.SelectObstaclePositions;
		if (IsInlineSubAbility)
		{
			m_CanUndo = false;
		}
		m_PropsSpawned = 0;
		if (AreaEffect != null || Targeting == EAbilityTargeting.Custom)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (NumberTargets == -1 || m_AllTargets)
		{
			m_AllTargets = true;
		}
		else
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
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
		case CreateState.SelectObstaclePositions:
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
					goto IL_0676;
				}
			}
			if (AreaEffect == null && Targeting != EAbilityTargeting.Custom)
			{
				TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: true, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: true);
				List<CTile> list = new List<CTile>();
				foreach (CTile item4 in TilesInRange)
				{
					if (CAbilityFilter.IsValidTile(item4, TileFilter, includeInitial: false, null, null, VarsContext) && !CheckIfWillBeBlocking(item4))
					{
						list.Add(item4);
					}
				}
				TilesInRange = list.ToList();
			}
			goto IL_0676;
		case CreateState.PreCreateObstacles:
			m_CanUndo = false;
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			m_State = CreateState.CreatingObstacles;
			Perform();
			break;
		case CreateState.CreatingObstacles:
			if (m_TilesSelected.Count != 0)
			{
				CPlacingSpawn_MessageData message = new CPlacingSpawn_MessageData(AnimOverload, TargetingActor)
				{
					m_Ability = this,
					m_ActorPlacingSpawn = TargetingActor
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else
			{
				m_State = CreateState.CreateObstacles;
				Perform();
			}
			break;
		case CreateState.CreateObstacles:
			{
				string name = (string.IsNullOrEmpty(OverridePropName) ? PropName : OverridePropName);
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
						CalcRotationAndBlockers(m_TilesSelected, AreaEffect, AreaEffectAngle, AreaEffectFlipped, out var rotation, out var pathingBlockers);
						CObjectObstacle cObjectObstacle = new CObjectObstacle(name, ScenarioManager.ObjectImportType.Obstacle, pathingBlockers[0], null, rotation, pathingBlockers, TargetingActor, m_TilesSelected[0].m_HexMap.MapGuid, ignoresFlyAndJump: false, setPositionToCenterOfAllPathBlockers: true);
						cObjectObstacle.PropCustomLocKey = propCustomLoc;
						m_TilesSelected[0].SpawnProp(cObjectObstacle, notifyClient: true, SpawnDelay, this);
						m_PropsSpawned++;
					}
					else
					{
						foreach (CTile item5 in m_TilesSelected)
						{
							AbilityHasHappened = true;
							List<TileIndex> pathingBlockers2 = new List<TileIndex>
							{
								new TileIndex(item5.m_ArrayIndex)
							};
							float y = 0.5f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(180) + 0.75f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(360);
							CObjectObstacle cObjectObstacle2 = new CObjectObstacle(name, ScenarioManager.ObjectImportType.Obstacle, new TileIndex(item5.m_ArrayIndex), null, new CVector3(0f, y, 0f), pathingBlockers2, TargetingActor, item5.m_HexMap.MapGuid, ignoresFlyAndJump: false);
							cObjectObstacle2.PropCustomLocKey = propCustomLoc;
							item5.SpawnProp(cObjectObstacle2, notifyClient: true, SpawnDelay, this);
							m_PropsSpawned++;
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
				List<CTile> list2 = new List<CTile>();
				foreach (CTile item6 in TilesInRange)
				{
					if (CAbilityFilter.IsValidTile(item6, TileFilter, includeInitial: false, null, null, VarsContext) && !CheckIfWillBeBlocking(item6))
					{
						list2.Add(item6);
					}
				}
				TilesInRange = list2.ToList();
				if (AllTargets)
				{
					foreach (CTile item7 in TilesInRange)
					{
						SelectParentAbilityTiles(item7);
					}
					PhaseManager.StepComplete();
					break;
				}
				goto IL_0676;
			}
			PhaseManager.NextStep();
			return true;
			IL_0676:
			if (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)
			{
				foreach (CTile item8 in TilesInRange)
				{
					SelectParentAbilityTiles(item8);
				}
				if (AbilityHasBeenCancelled)
				{
					return false;
				}
				m_State = CreateState.PreCreateObstacles;
				Perform();
				break;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData cPlayerSelectingObjectPosition_MessageData = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
					m_IsPositive = IsPositive()
				};
				if (GetAbilityBaseCard() is CAbilityCard cAbilityCard)
				{
					cPlayerSelectingObjectPosition_MessageData.m_ShowConsumeBar = cAbilityCard.GetAction(this)?.Augmentations?.Any() == true;
				}
				ScenarioRuleClient.MessageHandler(cPlayerSelectingObjectPosition_MessageData);
				break;
			}
			CAIMoveNew.GetPossibleEmptyTiles(TargetingActor, TilesInRange, out failTile, out failReason, out possibleTiles);
			if (failTile.Count > 0)
			{
				Log.SimpleInfo(TargetingActor?.Class?.ID + " Failed to place " + PropName + " adjacent to " + TargetingActor?.Class?.ID);
				for (int i = 0; i < failTile.Count; i++)
				{
					Log.SimpleInfo("Tile at x:" + failTile[i].m_ArrayIndex.X + " y:" + failTile[i].m_ArrayIndex.Y + ", fail reason=" + failReason[i]);
				}
			}
			CAIMoveNew.SelectTilesBasedOnTargetingFocus(TargetingActor, Range, possibleTiles, Targeting, TilesSelected, NumberTargetsRemaining, AllTargets);
			if (m_TilesSelected.Count > 0)
			{
				m_State = CreateState.PreCreateObstacles;
				Perform();
				return true;
			}
			PhaseManager.StepComplete();
			return true;
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

	public static void CalcRotationAndBlockers(List<CTile> m_TilesSelected, CAreaEffect AreaEffect, float AreaEffectAngle, bool AreaEffectFlipped, out CVector3 rotation, out List<TileIndex> pathingBlockers)
	{
		pathingBlockers = new List<TileIndex>();
		foreach (CTile item in m_TilesSelected)
		{
			pathingBlockers.Add(new TileIndex(item.m_ArrayIndex));
		}
		rotation = null;
		if (AreaEffect != null)
		{
			float num = (float)(AreaEffectFlipped ? 360 : 180) - AreaEffectAngle;
			if (AreaEffectFlipped && pathingBlockers.Count == 2)
			{
				num += 60f;
			}
			rotation = new CVector3(0f, num, 0f);
		}
		else
		{
			float y = 0.5f * (float)SharedClient.GlobalRNG.Next(180) + 0.75f * (float)SharedClient.GlobalRNG.Next(360);
			rotation = new CVector3(0f, y, 0f);
		}
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		InvalidTileReason = null;
		bool flag = false;
		if (!TilesSelected.Contains(selectedTile) && m_State == CreateState.SelectObstaclePositions)
		{
			if (AreaEffect != null)
			{
				if (!m_AreaEffectLocked && m_ValidTilesInAreaEffect.Count == ValidTilesInAreaEffectedIncludingBlocked.Count && m_ValidTilesInAreaEffect.Count == AreaEffect.EnabledHexes.Count())
				{
					foreach (CTile item in m_ValidTilesInAreaEffect)
					{
						if (!CAbilityFilter.IsValidTile(item, TileFilter, includeInitial: false, null, null, VarsContext))
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
					if (!CheckIfWillBeBlocking(selectedTile, m_ValidTilesInAreaEffect, warn: true))
					{
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
			}
			else if (TilesInRange.Contains(selectedTile))
			{
				if (m_TilesSelected.Count < NumberTargets)
				{
					if (!m_TilesSelected.Contains(selectedTile))
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
				CheckIfWillBeBlocking(selectedTile, warn: true);
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
		InvalidTileReason = null;
		bool flag = false;
		bool flag2 = false;
		if (AreaEffect != null && m_AreaEffectLocked)
		{
			m_AreaEffectLocked = false;
			m_TilesSelected.Clear();
			flag2 = true;
		}
		else if (m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			bool flag3 = true;
			foreach (CTile item in m_TilesSelected)
			{
				if (item != selectedTile && CheckIfWillBeBlocking(item, warn: true))
				{
					flag3 = false;
					break;
				}
			}
			if (!flag3)
			{
				m_TilesSelected.Add(selectedTile);
			}
			else
			{
				flag2 = true;
			}
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
		if (m_State == CreateState.SelectObstaclePositions)
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		return m_State == CreateState.SelectObstaclePositions;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == CreateState.SelectObstaclePositions;
		}
		return false;
	}

	public bool SelectParentAbilityTiles(CTile selectedTile)
	{
		if (TilesSelected.Contains(selectedTile))
		{
			return m_CanUndo;
		}
		if (m_State == CreateState.SelectObstaclePositions)
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
				SetCancelAbilityFlag(cancel: true);
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
		return m_State == CreateState.CreateDone;
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCreate(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, propCustomLoc ?? PropName, m_PropsSpawned));
	}

	public override string GetDescription()
	{
		return "Create Obstacle R: " + Range + " N: " + NumberTargets;
	}

	public bool HasPassedState(CreateState createState)
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

	protected override void OnFindBlockingTiles()
	{
		base.OnFindBlockingTiles();
		if (TargetingActor.Type == CActor.EType.Player)
		{
			CPlayerSelectingObjectPosition_MessageData cPlayerSelectingObjectPosition_MessageData = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.EmptyHex },
				m_IsPositive = IsPositive()
			};
			if (GetAbilityBaseCard() is CAbilityCard cAbilityCard)
			{
				cPlayerSelectingObjectPosition_MessageData.m_ShowConsumeBar = cAbilityCard.GetAction(this)?.Augmentations?.Any() == true;
			}
			ScenarioRuleClient.MessageHandler(cPlayerSelectingObjectPosition_MessageData);
		}
	}

	protected override bool ShouldCheckBlocking()
	{
		return true;
	}
}
