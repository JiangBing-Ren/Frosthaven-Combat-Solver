using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityCreateShadow : CAbility
{
	public enum CreateShadowState
	{
		SelectShadowPositions,
		PreCreateShadow,
		RemoveShadowPositions,
		CreatingShadow,
		CreateShadow,
		CreateDone
	}

	public bool m_CanClearTargets = true;

	private int m_PropsSpawned;

	private bool _isRemovingShadows;

	private List<CObjectProp> m_cancelShadows = new List<CObjectProp>();

	public List<CObjectProp> NewlyCreatedShadows = new List<CObjectProp>();

	public List<CObjectProp> NewlyRemovedShadows = new List<CObjectProp>();

	public bool FromShadowMark;

	[JsonProperty]
	public string PropName { get; set; }

	public CreateShadowState m_State { get; private set; }

	public override ScenarioManager.ObjectImportType SpawnTypePreview
	{
		get
		{
			ScenarioManager.ObjectImportType spawnTypePreview = base.SpawnTypePreview;
			if (spawnTypePreview != ScenarioManager.ObjectImportType.None)
			{
				return spawnTypePreview;
			}
			return ScenarioManager.ObjectImportType.Shadow;
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
			return EPropType.Shadow.ToString();
		}
	}

	public CAbilityCreateShadow()
	{
		if (TileFilter == CAbilityFilter.EFilterTile.None)
		{
			TileFilter = CAbilityFilter.EFilterTile.EmptyHex;
		}
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		NewlyCreatedShadows.Clear();
		NewlyRemovedShadows.Clear();
		if (targetActor.Type == CActor.EType.HeroSummon)
		{
			GameState.OverrideCurrentActorForOneAction(targetActor, CActor.EType.Player);
		}
		base.Start(targetActor, filterActor, controllingActor);
		m_State = CreateShadowState.SelectShadowPositions;
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
		_isRemovingShadows = false;
		if (AreaEffect != null)
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
		}
		m_NumberTargetsRemaining = NumberTargets;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (UseSubAbilityTargeting && IsInlineSubAbility)
		{
			foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
			{
				if (ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex) == null)
				{
					m_TilesSelected.Add(inlineSubAbilityTile);
				}
			}
			if (m_TilesSelected.Count > 0)
			{
				m_State = CreateShadowState.PreCreateShadow;
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
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
		case CreateShadowState.SelectShadowPositions:
		{
			if (AreaEffect == null)
			{
				List<CTile> list3 = new List<CTile>();
				if (Targeting == EAbilityTargeting.Custom)
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				else
				{
					bool flag = IncludeCasterTile();
					List<CTile> tilesInRange = GameState.GetTilesInRange(TargetingActor.ArrayIndex, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true, noActorsAllowed: false, skipPathingCheck: false, flag);
					if (MiscAbilityData != null && MiscAbilityData.MinRange.HasValue)
					{
						List<CTile> tilesUnderMinRange = new List<CTile>();
						if (MiscAbilityData.MinRange.Value == Range && Range == 1)
						{
							tilesUnderMinRange.Add(TargetingActor.Tile);
						}
						else
						{
							tilesUnderMinRange.AddRange(GameState.GetTilesInRange(TargetingActor.ArrayIndex, MiscAbilityData.MinRange.Value - 1, Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true, noActorsAllowed: false, skipPathingCheck: false, flag));
						}
						tilesInRange.RemoveAll((CTile actor) => tilesUnderMinRange.Contains(actor));
						TilesInRange = tilesInRange;
					}
					else
					{
						TilesInRange = tilesInRange;
						if (flag && !TargetingActor.Tile.m_Props.Any((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow))
						{
							TilesInRange.Add(TargetingActor.Tile);
						}
					}
				}
				foreach (CTile item in TilesInRange)
				{
					if (item.FindPropOfTypes(EPropType.WallObstacle) == null && !item.m_Props.Any((CObjectProp x) => x is CObjectObstacle cObjectObstacle && cObjectObstacle.IgnoresFlyAndJump) && item.IsWalkable && ScenarioScript.current?.TilesCannotStopOnTopOf()?.Contains(item) != true && item.m_HexMap.Revealed && !item.m_Props.Any((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow))
					{
						list3.Add(item);
					}
				}
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list3, this);
				TilesInRange.Clear();
				TilesInRange.AddRange(list3);
				if (TilesInRange.Count == 1)
				{
					TilesSelected.Clear();
					TilesSelected.AddRange(TilesInRange);
				}
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if ((miscAbilityData2 != null && miscAbilityData2.AutotriggerAbility == true) || (TilesInRange.Count == 0 && FromShadowMark))
				{
					m_State = CreateShadowState.PreCreateShadow;
					Perform();
					break;
				}
			}
			CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
				m_IsPositive = IsPositive()
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case CreateShadowState.RemoveShadowPositions:
		{
			m_CanSkip = false;
			HelpBoxTooltipLocKey = "FH/GUI_REMOVE_SHADOWS_HELPBOX";
			CPlayerSelectingObjectPosition_MessageData message3 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
				m_IsPositive = IsPositive(),
				m_SpawnType = ScenarioManager.ObjectImportType.None
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case CreateShadowState.PreCreateShadow:
		{
			List<CObjectProp> source = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow).ToList();
			if (CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour() != null)
			{
				List<CTile> list = source.Select((CObjectProp x) => x.PropTile).ToList();
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list, this);
				source = (from x in list
					select x.FindPropOfTypes(EPropType.Shadow) into x
					where x != null
					select x).ToList();
			}
			int num = source.Count();
			ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData());
			int num2 = ScenarioManager.CurrentScenarioState.ShadowProps.Where((CObjectProp shadow) => shadow is CObjectShadow cObjectShadow && cObjectShadow.WillBeRemoved).Count();
			if (num + m_TilesSelected.Count() - num2 <= 5)
			{
				m_CanUndo = false;
				m_State = CreateShadowState.CreatingShadow;
			}
			else
			{
				m_cancelShadows.Clear();
				foreach (CTile item2 in m_TilesSelected)
				{
					m_cancelShadows.Add(SpawnTerrain(item2, new CVector3(0f, 0f, 0f)));
				}
				List<CTile> list2 = (from shadow in ScenarioManager.CurrentScenarioState.ShadowProps
					where shadow is CObjectShadow cObjectShadow && !cObjectShadow.WillBeRemoved
					select shadow.PropTile).ToList();
				SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list2, this);
				_isRemovingShadows = true;
				TilesInRange.Clear();
				TilesInRange.AddRange(list2);
				m_TilesSelected.Clear();
				NumberTargets = TilesInRange.Count() - 5;
				m_NumberTargetsRemaining = NumberTargets;
				m_State = CreateShadowState.RemoveShadowPositions;
			}
			Perform();
			break;
		}
		case CreateShadowState.CreatingShadow:
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
				m_State = CreateShadowState.CreateShadow;
				Perform();
			}
			break;
		case CreateShadowState.CreateShadow:
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (m_TilesSelected.Count != 0)
			{
				if (_isRemovingShadows)
				{
					List<CObjectProp> shadows = m_TilesSelected.Select((CTile tile) => tile.FindProp(ScenarioManager.ObjectImportType.Shadow)).ToList();
					RemoveShadows(shadows, SpawnDelay + 0.1f);
					_isRemovingShadows = false;
				}
				else
				{
					foreach (CTile item3 in m_TilesSelected)
					{
						CVector3 rotation = new CVector3(0f, 0.5f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(180) + 0.75f * (float)ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(360), 0f);
						SpawnTerrain(item3, rotation, SpawnDelay);
					}
				}
				AbilityHasHappened = true;
				TargetingActor.m_OnCreatedListeners?.Invoke(this);
				CActiveBonus.RefreshAllAuraActiveBonuses(this);
				PhaseManager.NextStep();
			}
			else if (!IsMergedAbility)
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		return false;
		IL_0089:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message4 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message4);
			}
			else
			{
				CPlayerIsStunned_MessageData message5 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message5);
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
		return SpawnShadow(tile, rotation, spawnDelay, this, TargetingActor);
	}

	public static CObjectProp SpawnShadow(CTile tile, CVector3 rotation, float spawnDelay = 0f, CAbilityCreateShadow ability = null, CActor targetingActor = null)
	{
		CObjectShadow cObjectShadow = new CObjectShadow("Shadow", ScenarioManager.ObjectImportType.Shadow, new TileIndex(tile.m_ArrayIndex), null, rotation, targetingActor, tile.m_HexMap.MapGuid);
		tile.SpawnProp(cObjectShadow, notifyClient: true, spawnDelay, ability);
		Log.Info("Spawned shadow " + tile.m_ArrayIndex.ToString());
		if (ability != null)
		{
			ability.NewlyCreatedShadows.Add(cObjectShadow);
			ability.m_PropsSpawned++;
		}
		return cObjectShadow;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		bool flag = false;
		if (FromShadowMark)
		{
			CObjectDoor cObjectDoor = selectedTile.FindDoor();
			if (cObjectDoor != null && cObjectDoor.DoorOpenedByMovingActor && PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && cPhaseAction.PreviousPhaseAbilities.Count() > 0 && cPhaseAction.PreviousPhaseAbilities.Last().m_Ability is CAbilityMove cAbilityMove && cAbilityMove.CurrentMovingActor == TargetingActor && cAbilityMove.DoorsOpened > 0)
			{
				FromShadowMark = false;
				return;
			}
		}
		if (!TilesSelected.Contains(selectedTile))
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
		if (m_State == CreateShadowState.SelectShadowPositions)
		{
			Perform();
		}
	}

	public override bool CanClearTargets()
	{
		if (!m_CanClearTargets || m_State != CreateShadowState.SelectShadowPositions)
		{
			return m_State == CreateShadowState.RemoveShadowPositions;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != CreateShadowState.SelectShadowPositions)
			{
				return m_State == CreateShadowState.RemoveShadowPositions;
			}
			return true;
		}
		return false;
	}

	public bool SelectParentAbilityTiles(CTile selectedTile)
	{
		if (TilesSelected.Contains(selectedTile))
		{
			return m_CanUndo;
		}
		if (m_State == CreateShadowState.SelectShadowPositions)
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
		return m_State == CreateShadowState.CreateDone;
	}

	public override void Undo()
	{
		base.Undo();
		if (m_cancelShadows.Count > 0)
		{
			RemoveShadows(m_cancelShadows);
		}
	}

	private void RemoveShadows(List<CObjectProp> shadows, float spawnDelay = 0f)
	{
		NewlyRemovedShadows.AddRange(shadows);
		RemoveShadows(shadows, this, TargetingActor, spawnDelay);
	}

	public static void RemoveShadows(List<CObjectProp> shadows, CAbility ability, CActor targetingActor, float spawnDelay = 0f)
	{
		foreach (CObjectProp shadow in shadows)
		{
			shadow.DestroyProp(0f, sendMessageToClient: false, forceDestroy: false, ability);
		}
		if (shadows.Count > 0)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityRemoveShadow(new SEventAbility.SEventAbilityBaseData(targetingActor, EAbilityType.RemoveShadow, ESESubTypeAbility.AbilityEnded, ability.GetAbilityBaseCard(), new List<string>(), new List<CActor.EType>(), hasHappened: true, EAbilityFlag.None), CAbilityRemoveShadow.RemoveShadowState.RemoveShadowsDone, shadows.Count));
		}
		CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(ability.AnimOverload, targetingActor)
		{
			m_ActorDestroyingObstacle = targetingActor,
			m_Tiles = shadows.Select((CObjectProp prop) => prop.PropTile).ToList(),
			m_DestroyedProps = shadows,
			m_DestroyDelay = spawnDelay,
			m_DestroyObstacleAbility = ability
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public override void AbilityEnded()
	{
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
		if (!AbilityHasHappened && m_cancelShadows.Count > 0)
		{
			RemoveShadows(m_cancelShadows);
		}
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityCreateShadow(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, PropName, m_PropsSpawned));
	}

	public override string GetDescription()
	{
		return "Create Shadow R: " + Range + " N: " + NumberTargets;
	}

	public bool HasPassedState(CreateShadowState createState)
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
		if (m_State == CreateShadowState.RemoveShadowPositions)
		{
			return m_TilesSelected.Count == NumberTargets;
		}
		if (m_TilesSelected.Any((CTile tile) => tile.FindProps(ScenarioManager.ObjectImportType.Shadow).Count > 0))
		{
			return false;
		}
		return m_TilesSelected.Count > 0;
	}
}
