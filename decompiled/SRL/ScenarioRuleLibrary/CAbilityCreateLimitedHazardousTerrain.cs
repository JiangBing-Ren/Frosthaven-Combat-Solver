using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityCreateLimitedHazardousTerrain : CAbility
{
	public enum CAbilityCreateLimitedHazardousTerrainState
	{
		SelectPropPositions,
		PreCreateProp,
		RemovePropPositions,
		CreatingProp,
		CreateProp,
		CreateDone
	}

	public bool m_CanClearTargets = true;

	private int m_PropsSpawned;

	private bool _isRemovingProps;

	private List<CObjectProp> m_cancelProps = new List<CObjectProp>();

	public const int MaxProps = 6;

	public List<CTile> CachedSpawnTiles = new List<CTile>();

	[JsonProperty]
	public string PropName { get; set; }

	[JsonProperty]
	public string OverridePropName { get; set; }

	public CAbilityCreateLimitedHazardousTerrainState m_State { get; private set; }

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return ScenarioManager.ObjectImportType.TerrainHotCoals;
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
			return EPropType.TerrainHotCoals.ToString();
		}
	}

	public override bool AutoTrigger
	{
		get
		{
			if (m_State == CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions)
			{
				return false;
			}
			return base.AutoTrigger;
		}
	}

	public CAbilityCreateLimitedHazardousTerrain()
	{
		if (TileFilter == CAbilityFilter.EFilterTile.None)
		{
			TileFilter = CAbilityFilter.EFilterTile.EmptyHex;
		}
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		CachedSpawnTiles.Clear();
		if (targetActor.Type == CActor.EType.HeroSummon)
		{
			GameState.OverrideCurrentActorForOneAction(targetActor, CActor.EType.Player);
		}
		base.Start(targetActor, filterActor, controllingActor);
		m_State = CAbilityCreateLimitedHazardousTerrainState.SelectPropPositions;
		m_CanUndo = ((MiscAbilityData != null && MiscAbilityData.CanUndo.HasValue) ? MiscAbilityData.CanUndo.Value : m_CanUndo);
		m_CanSkip = ((MiscAbilityData != null && MiscAbilityData.CanSkip.HasValue) ? MiscAbilityData.CanSkip.Value : m_CanSkip);
		if (VarsContext.GetVar("CanClearTargets").isValid)
		{
			m_CanClearTargets = VarsContext.GetVar("CanClearTargets").GetBool();
		}
		if (IsInlineSubAbility)
		{
			m_CanUndo = false;
		}
		m_PropsSpawned = 0;
		_isRemovingProps = false;
		if (AreaEffect != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
		}
		else
		{
			NumberTargets = Math.Min(NumberTargets, 6);
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (UseSubAbilityTargeting && IsInlineSubAbility)
		{
			foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
			{
				if (CAbilityFilter.IsValidTile(inlineSubAbilityTile, TileFilter))
				{
					m_TilesSelected.Add(inlineSubAbilityTile);
				}
			}
			if (m_TilesSelected.Count > 0)
			{
				m_State = CAbilityCreateLimitedHazardousTerrainState.PreCreateProp;
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	private bool IsPropValid(CObjectProp prop)
	{
		if (prop.PropType == EPropType.TerrainHotCoals)
		{
			return prop.Owner == TargetingActor;
		}
		return false;
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
		case CAbilityCreateLimitedHazardousTerrainState.SelectPropPositions:
		{
			if (AreaEffect == null)
			{
				if (Targeting == EAbilityTargeting.Custom)
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				else
				{
					List<CTile> tilesInRange = GameState.GetTilesInRange(TargetingActor.ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true, noActorsAllowed: false, skipPathingCheck: false, includeStartingTile: true);
					if (MiscAbilityData != null && MiscAbilityData.MinRange.HasValue)
					{
						List<CTile> tilesUnderMinRange = new List<CTile>();
						if (MiscAbilityData.MinRange.Value == Range && Range == 1)
						{
							tilesUnderMinRange.Add(TargetingActor.Tile);
						}
						else
						{
							tilesUnderMinRange.AddRange(GameState.GetTilesInRange(TargetingActor.ArrayIndex, MiscAbilityData.MinRange.Value - 1, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true, noActorsAllowed: false, skipPathingCheck: false, includeStartingTile: true));
						}
						tilesInRange.RemoveAll((CTile actor) => tilesUnderMinRange.Contains(actor));
						TilesInRange = tilesInRange;
					}
					else
					{
						TilesInRange = tilesInRange;
						if (!TargetingActor.Tile.m_Props.Any((CObjectProp prop) => IsPropValid(prop)))
						{
							TilesInRange.Add(TargetingActor.Tile);
						}
					}
				}
				List<CTile> list = new List<CTile>();
				foreach (CTile item in TilesInRange)
				{
					if (item != null && ScenarioManager.PathFinder.GetNode(item.m_ArrayIndex).Walkable && item.m_HexMap.Revealed && CAbilityFilter.IsValidTile(item, TileFilter) && !item.m_Props.Any((CObjectProp prop) => IsPropValid(prop)))
					{
						list.Add(item);
					}
				}
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list, this);
				TilesInRange.Clear();
				TilesInRange.AddRange(list);
				if (AllTargets)
				{
					TilesSelected.Clear();
					TilesSelected.AddRange(TilesInRange);
				}
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.AutotriggerAbility == true)
				{
					if (!AllTargets && TilesInRange.Count > 0)
					{
						TilesSelected.Clear();
						TilesSelected.Add(TilesInRange[0]);
					}
					m_State = CAbilityCreateLimitedHazardousTerrainState.PreCreateProp;
					Perform();
					break;
				}
			}
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
		case CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions:
		{
			m_CanSkip = false;
			HelpBoxTooltipLocKey = "FH/GUI_REMOVE_LIMITED_HAZARDOUS_TERRAIN_HELPBOX";
			CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
				m_IsPositive = IsPositive(),
				m_SpawnType = ScenarioManager.ObjectImportType.None
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case CAbilityCreateLimitedHazardousTerrainState.PreCreateProp:
		{
			List<CObjectProp> list2 = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => IsPropValid(prop)).ToList();
			if (CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour() != null)
			{
				List<CTile> list3 = list2.Select((CObjectProp x) => x.PropTile).ToList();
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list3, this);
				list2 = (from prop in list3.SelectMany((CTile x) => x.m_Props)
					where prop != null && IsPropValid(prop)
					select prop).ToList();
			}
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			CachedSpawnTiles.Clear();
			CachedSpawnTiles.AddRange(TilesSelected);
			int num = m_TilesSelected.Count();
			int num2 = list2.Count + num;
			if (num2 <= 6)
			{
				m_CanUndo = false;
				m_State = CAbilityCreateLimitedHazardousTerrainState.CreatingProp;
			}
			else
			{
				m_cancelProps.Clear();
				foreach (CTile item2 in m_TilesSelected)
				{
					m_cancelProps.Add(SpawnTerrain(item2, new CVector3(0f, 0f, 0f)));
				}
				List<CTile> list4 = list2.Select((CObjectProp prop) => prop.PropTile).Concat(CachedSpawnTiles).ToList();
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list4, this);
				_isRemovingProps = true;
				TilesInRange.Clear();
				TilesInRange.AddRange(list4);
				m_TilesSelected.Clear();
				NumberTargets = num2 - 6;
				m_NumberTargetsRemaining = NumberTargets;
				m_State = CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions;
			}
			Perform();
			break;
		}
		case CAbilityCreateLimitedHazardousTerrainState.CreatingProp:
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
				m_State = CAbilityCreateLimitedHazardousTerrainState.CreateProp;
				Perform();
			}
			break;
		case CAbilityCreateLimitedHazardousTerrainState.CreateProp:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				bool flag = false;
				if (_isRemovingProps)
				{
					List<CObjectProp> props = m_TilesSelected.SelectMany((CTile tile) => tile.m_Props.Where((CObjectProp prop) => IsPropValid(prop))).ToList();
					RemoveProps(props, SpawnDelay + 0.1f);
					_isRemovingProps = false;
					foreach (CTile item3 in m_TilesSelected)
					{
						CachedSpawnTiles.Remove(item3);
					}
				}
				else
				{
					foreach (CTile cachedSpawnTile in CachedSpawnTiles)
					{
						CVector3 rotation = new CVector3(0f, 0.5f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(180) + 0.75f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(360), 0f);
						SpawnTerrain(cachedSpawnTile, rotation, SpawnDelay);
						flag |= StackInlineSubAbilities(null, cachedSpawnTile);
					}
				}
				AbilityHasHappened = true;
				TargetingActor.m_OnCreatedListeners?.Invoke(this);
				CActiveBonus.RefreshAllAuraActiveBonuses(this);
				if (flag)
				{
					m_State = CAbilityCreateLimitedHazardousTerrainState.CreateDone;
					PhaseManager.StepComplete();
					return true;
				}
				PhaseManager.NextStep();
			}
			else if (!IsMergedAbility)
			{
				PhaseManager.NextStep();
			}
			return true;
		case CAbilityCreateLimitedHazardousTerrainState.CreateDone:
			PhaseManager.NextStep();
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

	public override bool ApplyToActor(CActor actor)
	{
		if (base.ApplyToActor(actor))
		{
			CBaseCard abilityBaseCard = GetAbilityBaseCard();
			if (ActiveBonusData.OverrideAsSong)
			{
				actor.AddAugmentOrSong(this, TargetingActor);
			}
			else if (abilityBaseCard != null)
			{
				abilityBaseCard.AddActiveBonus(this, actor, TargetingActor);
			}
			else
			{
				Log.ErrorWithReport("Unable to find base ability card for ability " + Name);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(actor);
			}
		}
		return true;
	}

	public void AddSelectedTiles(List<CTile> tiles)
	{
		m_TilesSelected.AddRange(tiles);
	}

	private CObjectProp SpawnTerrain(CTile tile, CVector3 rotation, float spawnDelay = 0f)
	{
		return SpawnTerrain(tile, rotation, spawnDelay, TargetingActor, this, OverridePropName);
	}

	public static CObjectProp SpawnTerrain(CTile tile, CVector3 rotation, float spawnDelay, CActor actor, CAbilityCreateLimitedHazardousTerrain ability, string OverridePropName = null)
	{
		CObjectHazardousTerrain cObjectHazardousTerrain = new CObjectHazardousTerrain(string.IsNullOrEmpty(OverridePropName) ? "TerrainHotCoals" : OverridePropName, ScenarioManager.ObjectImportType.TerrainHotCoals, new TileIndex(tile.m_ArrayIndex), null, rotation, actor, tile.m_HexMap.MapGuid);
		tile.SpawnProp(cObjectHazardousTerrain, notifyClient: true, spawnDelay, ability);
		Log.Info("Spawned hazardous terrain " + tile.m_ArrayIndex.ToString());
		if (ability != null)
		{
			ability.m_PropsSpawned++;
		}
		return cObjectHazardousTerrain;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (!TilesSelected.Contains(selectedTile) && (!AllTargets || m_State == CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions))
		{
			if (TilesInRange.Contains(selectedTile))
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
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
						cPlayerSelectedTile_MessageData.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
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
		if (!AllTargets || m_State == CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions)
		{
			if (m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Remove(selectedTile);
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
		if (m_State == CAbilityCreateLimitedHazardousTerrainState.SelectPropPositions)
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (!m_CanClearTargets || m_State != CAbilityCreateLimitedHazardousTerrainState.SelectPropPositions)
		{
			return m_State == CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != CAbilityCreateLimitedHazardousTerrainState.SelectPropPositions)
			{
				return m_State == CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions;
			}
			return true;
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
		return m_State == CAbilityCreateLimitedHazardousTerrainState.CreateDone;
	}

	public override void Undo()
	{
		base.Undo();
		if (m_cancelProps.Count > 0)
		{
			RemoveProps(m_cancelProps);
		}
	}

	private void RemoveProps(List<CObjectProp> Props, float spawnDelay = 0f)
	{
		if (Props.Count > 0)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (CObjectProp Prop in Props)
			{
				Prop.DestroyProp(0f, sendMessageToClient: false, forceDestroy: false, this);
				if (!dictionary.ContainsKey(Prop.PrefabName))
				{
					dictionary[Prop.PrefabName] = 0;
				}
				dictionary[Prop.PrefabName]++;
			}
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityDestroyObstacle(new SEventAbility.SEventAbilityBaseData(TargetingActor, ESESubTypeAbility.AbilityEnded, this), CAbilityDestroyObstacle.DestroyObstacleState.DestroyObstaclesDone, dictionary));
		}
		CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(AnimOverload, TargetingActor)
		{
			m_ActorDestroyingObstacle = TargetingActor,
			m_Tiles = Props.Select((CObjectProp prop) => prop.PropTile).ToList(),
			m_DestroyedProps = Props,
			m_DestroyDelay = spawnDelay,
			m_DestroyObstacleAbility = this
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		if (!AbilityHasHappened && m_cancelProps.Count > 0)
		{
			RemoveProps(m_cancelProps);
		}
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCreateLimitedHazardousTerrain(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, PropName, m_PropsSpawned));
	}

	public override string GetDescription()
	{
		return "Create Prop R: " + Range + " N: " + NumberTargets;
	}

	public bool HasPassedState(CAbilityCreateLimitedHazardousTerrainState createState)
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
		if (m_State == CAbilityCreateLimitedHazardousTerrainState.RemovePropPositions)
		{
			return m_TilesSelected.Count == NumberTargets;
		}
		if (m_TilesSelected.Any((CTile tile) => tile.m_Props.Count((CObjectProp prop) => IsPropValid(prop)) > 0))
		{
			return false;
		}
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
}
