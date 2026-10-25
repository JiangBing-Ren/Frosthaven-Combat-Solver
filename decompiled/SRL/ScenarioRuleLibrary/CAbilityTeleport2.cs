using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityTeleport2 : CAbility
{
	public enum ETeleport2State
	{
		None,
		SelectTeleportTarget,
		ActorIsSelectingTeleportTile,
		ActorIsTeleporting,
		ActorHasTeleported,
		ActorFinalizeTeleport,
		CheckForNextTarget,
		TeleportDone
	}

	[Serializable]
	public class Teleported2ActorStats
	{
		public string Name;

		public int DistanceMoved;

		public TileIndex MovedFromPoint;

		public TileIndex MovedToPoint;

		public Teleported2ActorStats(string name, int distanceMoved, TileIndex movedFromPoint, TileIndex movedToPoint)
		{
			Name = name;
			DistanceMoved = distanceMoved;
			MovedFromPoint = movedFromPoint;
			MovedToPoint = movedToPoint;
		}
	}

	public static readonly ETeleport2State[] Teleport2States = (ETeleport2State[])Enum.GetValues(typeof(ETeleport2State));

	public static bool s_LastTeleport2AbilityWasExecuted;

	private ETeleport2State m_State;

	private Teleported2ActorStats m_ActorStats;

	public CActor ActorToTeleport;

	public CTile SourceTile;

	public CTile DestinationTile;

	public bool UseFade()
	{
		return GetMiscVar("UseFade").GetBool();
	}

	public static CAbility CreateDefaultTeleport2(int strength, int range, int numberOfTargets, bool isMonster, string animationOverload = "", string animationOverload2 = "", CAbilityFilterContainer abilityfilter = null)
	{
		return CAbility.CreateAbility(EAbilityType.Teleport2, strength, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, abilityfilter ?? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), animationOverload, animationOverload2, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultTeleport2", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		s_LastTeleport2AbilityWasExecuted = false;
		base.Start(targetingActor, filterActor, controllingActor);
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || TargetingActor.Type != CActor.EType.Player || IsInlineSubAbility)
		{
			m_CanUndo = false;
		}
		if (TargetThisActorAutomatically != null)
		{
			if (!CAbility.ImmuneToAbility(TargetThisActorAutomatically, this))
			{
				ValidActorsInRange = new List<CActor> { TargetThisActorAutomatically };
				ActorToTeleport = TargetThisActorAutomatically;
				if (TargetingActor.Type == CActor.EType.Player)
				{
					m_State = ETeleport2State.ActorIsSelectingTeleportTile;
				}
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		else if (UseSubAbilityTargeting)
		{
			if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
			{
				TilesInRange = InlineSubAbilityTiles.ToList();
				foreach (CTile inlineSubAbilityTile in InlineSubAbilityTiles)
				{
					CActor cActor = ScenarioManager.Scenario.FindActorAt(inlineSubAbilityTile.m_ArrayIndex);
					if (cActor != null && !m_ValidActorsInRange.Contains(cActor) && AbilityFilter.IsValidTarget(cActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, false))
					{
						m_ValidActorsInRange.Add(cActor);
					}
				}
			}
			else if (ParentAbility.AreaEffect != null || ParentAbility.AreaEffectBackup != null)
			{
				m_ValidTilesInAreaEffect = ParentAbility.ValidTilesInAreaAffected.ToList();
				ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffect);
				m_ActorsToTarget = ValidActorsInRange.ToList();
			}
			else if (AbilityFilter.Equals(ParentAbility.AbilityFilter) && ParentAbility?.ActorsTargeted != null)
			{
				ValidActorsInRange = ParentAbility.ActorsTargeted.ToList();
			}
			else
			{
				SharedAbilityTargeting.GetValidActorsInRange(this);
			}
			NumberTargets = ValidActorsInRange.Count;
		}
		else
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		for (int num = ValidActorsInRange.Count - 1; num >= 0; num--)
		{
			CActor cActor2 = ValidActorsInRange[num];
			if (cActor2 != null)
			{
				bool flag = false;
				if (cActor2.IsDead)
				{
					flag = true;
				}
				if (cActor2.Deactivated)
				{
					flag = true;
				}
				if (CAbility.ImmuneToAbility(cActor2, this))
				{
					flag = true;
				}
				if (cActor2.Tokens.HasKey(CCondition.EPositiveCondition.Immovable))
				{
					flag = true;
				}
				if (flag)
				{
					ValidActorsInRange.Remove(cActor2);
				}
			}
		}
		if (UseSubAbilityTargeting)
		{
			if (ValidActorsInRange.Count <= 0)
			{
				SetCancelAbilityFlag(cancel: true);
				if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction && ParentAbility != null && ParentAbility is CAbilityAttack cAbilityAttack && !cPhaseAction.CurrentPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability != this && (x.m_Ability.AbilityType == EAbilityType.Push || x.m_Ability.AbilityType == EAbilityType.Pull || x.m_Ability.AbilityType == EAbilityType.Teleport2)))
				{
					CActor cActor3 = ((cAbilityAttack.OriginalTargetingActor != null) ? cAbilityAttack.OriginalTargetingActor : cAbilityAttack.TargetingActor);
					if (TargetThisActorAutomatically != null)
					{
						if (ScenarioManager.Scenario.HasActor(cActor3) && ScenarioManager.Scenario.HasActor(TargetThisActorAutomatically) && !ActorsTargeted.Contains(TargetThisActorAutomatically))
						{
							cActor3.ApplyRetaliateToAttack(TargetThisActorAutomatically, cAbilityAttack);
						}
					}
					else
					{
						foreach (CActor item in ParentAbility?.ActorsTargeted)
						{
							if (ScenarioManager.Scenario.HasActor(cActor3) && ScenarioManager.Scenario.HasActor(item) && !ActorsTargeted.Contains(item))
							{
								cActor3.ApplyRetaliateToAttack(item, cAbilityAttack);
							}
						}
					}
				}
			}
			else if (m_ValidActorsInRange.Count == 1)
			{
				ActorToTeleport = m_ValidActorsInRange[0];
				if (TargetingActor.Type == CActor.EType.Player)
				{
					m_State = ETeleport2State.ActorIsSelectingTeleportTile;
				}
			}
		}
		if (ValidActorsInRange.Count <= 0 && (TargetingActor.Type != CActor.EType.Player || !CanSkip))
		{
			SetCancelAbilityFlag(cancel: true);
		}
		if ((Flags & EAbilityFlag.AdjacentToAttackTargets) != 0 && !CanSkip && GetAttackTargetsAdjacentTiles().Count == 0)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		if (UseSubAbilityTargeting || IsModifierAbility)
		{
			NumberTargets = ValidActorsInRange.Count;
		}
		if (NumberTargets == -1)
		{
			m_NumberTargetsRemaining = ValidActorsInRange.Count;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (ActorToTeleport == null || TargetingActor.Type != CActor.EType.Player)
		{
			m_State = ((Strength > 0) ? ETeleport2State.SelectTeleportTarget : ETeleport2State.ActorHasTeleported);
		}
		SourceTile = null;
		DestinationTile = null;
		m_ActorStats = null;
		m_ActorsToTarget.Clear();
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	public override void Restart()
	{
		base.Restart();
		s_LastTeleport2AbilityWasExecuted = false;
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		Log.SimpleInfo("[TELEPORT_2_ABILITY] - Performing state: " + m_State);
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText())
		{
			return true;
		}
		if (!ProcessIfDead)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_00aa;
			}
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
				{
					goto IL_00aa;
				}
			}
		}
		switch (m_State)
		{
		case ETeleport2State.SelectTeleportTarget:
			if (TargetingActor.Type != CActor.EType.Player)
			{
				if (AreaEffect == null)
				{
					int num = ValidActorsInRange.Count - 1;
					while (num >= 0 && ValidActorsInRange.Count > m_NumberTargetsRemaining)
					{
						ValidActorsInRange.RemoveAt(num);
						num--;
					}
				}
				if (ValidActorsInRange.Count > 0)
				{
					ActorToTeleport = GetNextTarget();
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[ActorToTeleport.ArrayIndex.X, ActorToTeleport.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[ActorToTeleport.ArrayIndex.X, ActorToTeleport.ArrayIndex.Y]);
					}
					TValue miscVar = GetMiscVar("TeleportToTile");
					if (!miscVar.isUnknown)
					{
						DestinationTile = miscVar.GetObject<CTile>();
						if (DestinationTile == null)
						{
							m_CancelAbility = true;
						}
					}
					else
					{
						DestinationTile = TilesInRange.FirstOrDefault();
					}
					m_State = ETeleport2State.ActorIsTeleporting;
					Perform();
				}
				else
				{
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.NextStep();
				}
			}
			else if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				m_CanUndo = true;
				ActorToTeleport = TargetingActor;
				m_State = ETeleport2State.ActorIsSelectingTeleportTile;
				Perform();
			}
			else
			{
				ScenarioRuleClient.MessageHandler(new CActorIsSelectingTargetingFocus_MessageData(TargetingActor)
				{
					m_IsPositive = false,
					m_TargetingAbility = this
				});
			}
			break;
		case ETeleport2State.ActorIsSelectingTeleportTile:
			if (ActorToTeleport == null || !ScenarioManager.Scenario.HasActor(ActorToTeleport))
			{
				CheckNextTarget();
			}
			else if (TargetingActor.Type == CActor.EType.Player && DestinationTile != null)
			{
				Log.SimpleInfo("[TELEPORT_2_ABILITY] - target location: " + ActorToTeleport.ArrayIndex.ToString());
				Point arrayIndex = DestinationTile.m_ArrayIndex;
				Log.SimpleInfo("[TELEPORT_2_ABILITY] - teleport destination: " + arrayIndex.ToString());
				Point arrayIndex2 = ActorToTeleport.ArrayIndex;
				Point arrayIndex3 = DestinationTile.m_ArrayIndex;
				m_ActorStats = new Teleported2ActorStats("Teleport", Point.AxialDistance(Point.OddRToAxial(arrayIndex2), Point.OddRToAxial(arrayIndex3)), new TileIndex(ActorToTeleport.ArrayIndex), new TileIndex(DestinationTile.m_ArrayIndex));
				m_State = ETeleport2State.ActorIsTeleporting;
				Perform();
			}
			else
			{
				DestinationTile = null;
				List<CTile> teleportTiles = GetTeleportTiles(ActorToTeleport.ArrayIndex, Strength);
				if (teleportTiles.Count == 0 || teleportTiles.All((CTile tile) => !tile.IsWalkable))
				{
					m_CanSkip = true;
				}
				CActorIsSelectingTeleport2Tile_MessageData message = new CActorIsSelectingTeleport2Tile_MessageData(TargetingActor)
				{
					m_Teleport2Ability = this
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			break;
		case ETeleport2State.ActorIsTeleporting:
		{
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			SourceTile = ScenarioManager.Tiles[ActorToTeleport.ArrayIndex.X, ActorToTeleport.ArrayIndex.Y];
			ActorToTeleport.SetTempVar("LastTileBeforeForceMove", new TValue(SourceTile));
			if (DestinationTile != null)
			{
				ActorToTeleport.ArrayIndex = DestinationTile.m_ArrayIndex;
			}
			if (TargetingActor.Type == CActor.EType.Player && DestinationTile == null)
			{
				CheckNextTarget();
				break;
			}
			CActorIsTeleporting2_MessageData message3 = new CActorIsTeleporting2_MessageData(TargetingActor)
			{
				m_Teleport2Ability = this,
				m_Waypoints = new List<CTile> { DestinationTile },
				m_ActorToTeleport = ActorToTeleport,
				AnimOverload = AnimOverload
			};
			ScenarioRuleClient.MessageHandler(message3);
			break;
		}
		case ETeleport2State.ActorHasTeleported:
		{
			s_LastTeleport2AbilityWasExecuted = true;
			CActorHasTeleported2_MessageData message2 = new CActorHasTeleported2_MessageData(TargetingActor)
			{
				m_Teleport2Ability = this,
				m_Waypoints = new List<CTile> { DestinationTile },
				m_ActorToTeleport = ActorToTeleport,
				m_DestinationTile = DestinationTile,
				AnimOverload = AnimOverload2
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case ETeleport2State.ActorFinalizeTeleport:
			AbilityHasHappened = true;
			ActivatePropsOnTeleport(ActorToTeleport, DestinationTile, SourceTile, this);
			CActiveBonus.RefreshAllAuraActiveBonuses();
			ActorToTeleport.CalculateAttackStrengthForUI();
			ActorToTeleport.m_OnMovedListeners?.Invoke(this, ActorToTeleport, new List<CActor>(), newActorCarried: false, 1, finalMovement: true, DestinationTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), DestinationTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1, ActorToTeleport.PrevArrayIndex);
			if (ParentAbility != null && ParentAbility is CAbilityAttack { OriginalTargetingActor: var cActor } cAbilityAttack)
			{
				if (cActor == null)
				{
					cActor = cAbilityAttack.TargetingActor;
				}
				CActor cActor2 = cActor;
				if (ScenarioManager.Scenario.HasActor(cActor2) && ScenarioManager.Scenario.HasActor(ActorToTeleport))
				{
					cActor2.ApplyRetaliateToAttack(ActorToTeleport, cAbilityAttack);
				}
			}
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(ActorToTeleport);
			}
			PhaseManager.StepComplete();
			return true;
		case ETeleport2State.CheckForNextTarget:
			CheckNextTarget();
			return true;
		}
		return false;
		IL_00aa:
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
		m_CanUndo = true;
		m_State = ETeleport2State.TeleportDone;
		return true;
	}

	public static void ActivatePropsOnTeleport(CActor ActorToTeleport, CTile DestinationTile, CTile SourceTile, CAbility ability = null)
	{
		GameState.LostAdjacency(ActorToTeleport, SourceTile.m_ArrayIndex, DestinationTile.m_ArrayIndex, ability);
		if (ActorToTeleport.CanActivatePressurePlates() && SourceTile.FindProp(ScenarioManager.ObjectImportType.PressurePlate) is CObjectPressurePlate { ActivatesOnEndTurn: false, PressurePlateType: CObjectPressurePlate.EPressurePlateType.ActivateWhileOccupied } cObjectPressurePlate)
		{
			cObjectPressurePlate.Deactivate();
		}
		for (int num = DestinationTile.m_Props.Count - 1; num >= 0; num--)
		{
			CObjectProp cObjectProp = DestinationTile.m_Props[num];
			if (!CObjectProp.TryChooseToSkipTrap(ActorToTeleport, cObjectProp))
			{
				bool flag = false;
				if (cObjectProp.WillActivationKillActor(ActorToTeleport))
				{
					flag = true;
				}
				if (cObjectProp.ObjectType != ScenarioManager.ObjectImportType.PressurePlate && cObjectProp.ObjectType != ScenarioManager.ObjectImportType.Portal)
				{
					if (cObjectProp is CObjectDoor cObjectDoor)
					{
						cObjectDoor.SetDoorOpenedByMovingActor(ActorToTeleport);
					}
					else
					{
						cObjectProp.AutomaticActivate(ActorToTeleport);
					}
				}
				else if (DestinationTile.m_Props[num] is CObjectPressurePlate cObjectPressurePlate2 && ActorToTeleport.CanActivatePressurePlates() && (cObjectPressurePlate2.PressurePlateType == CObjectPressurePlate.EPressurePlateType.ActivateOnce || (ActorToTeleport.ArrayIndex.X == cObjectPressurePlate2.ArrayIndex.X && ActorToTeleport.ArrayIndex.Y == cObjectPressurePlate2.ArrayIndex.Y && !flag)))
				{
					cObjectProp.AutomaticActivate(ActorToTeleport);
				}
			}
		}
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		Log.SimpleInfo("[TELEPORT_2_ABILITY] - Tile selected");
		bool flag = false;
		if (m_State == ETeleport2State.SelectTeleportTarget)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(selectedTile.m_ArrayIndex);
			if (cActor != null && cActor != ActorToTeleport && !ActorsTargeted.Contains(cActor) && ValidActorsInRange.Contains(cActor))
			{
				if (ActorToTeleport != null)
				{
					CTile item = ScenarioManager.Tiles[ActorToTeleport.ArrayIndex.X, ActorToTeleport.ArrayIndex.Y];
					m_ActorsToTarget.Remove(ActorToTeleport);
					m_TilesSelected.Remove(item);
				}
				ActorToTeleport = cActor;
				m_ActorsToTarget.Add(cActor);
				m_TilesSelected.Add(selectedTile);
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
				}
				if (TargetingActor.Type != CActor.EType.Player)
				{
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[ActorToTeleport.ArrayIndex.X, ActorToTeleport.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[ActorToTeleport.ArrayIndex.X, ActorToTeleport.ArrayIndex.Y]);
					}
					m_State = ETeleport2State.ActorIsTeleporting;
					flag = true;
				}
				else if (TargetingActor.Type == CActor.EType.Player && UseSubAbilityTargeting)
				{
					m_State = ETeleport2State.ActorIsSelectingTeleportTile;
					flag = true;
				}
			}
		}
		else if (m_State == ETeleport2State.ActorIsSelectingTeleportTile)
		{
			DestinationTile = selectedTile;
			m_CanUndo = false;
			flag = true;
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
		Log.SimpleInfo("[TELEPORT_2_ABILITY] - Tile deselected");
		bool flag = false;
		if (m_State == ETeleport2State.SelectTeleportTarget && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			ValidActorsInRange.Add(ActorToTeleport);
			m_ActorsToTarget.Remove(ActorToTeleport);
			ActorToTeleport = null;
			flag = true;
		}
		if (flag && TargetingActor.Type == CActor.EType.Player)
		{
			CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
			{
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message);
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
		ActorToTeleport = null;
		if (m_State == ETeleport2State.SelectTeleportTarget)
		{
			Perform();
		}
	}

	public static bool IsClosedDoor(CTile tile)
	{
		bool result = false;
		if (((tile.m_HexMap != null && tile.m_Hex2Map != null) || tile.FindProp(ScenarioManager.ObjectImportType.Door) != null) && ((tile.m_HexMap != null && !tile.m_HexMap.Revealed) || (tile.m_Hex2Map != null && !tile.m_Hex2Map.Revealed) || !ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].IsBridgeOpen))
		{
			result = true;
		}
		return result;
	}

	public override bool CanClearTargets()
	{
		if (GetTeleportTiles(TargetingActor.ArrayIndex, Range).Count == 1)
		{
			return false;
		}
		if (m_State != ETeleport2State.SelectTeleportTarget)
		{
			return m_State == ETeleport2State.ActorIsSelectingTeleportTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != ETeleport2State.SelectTeleportTarget)
			{
				return m_State == ETeleport2State.ActorIsSelectingTeleportTile;
			}
			return true;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		if (m_State == ETeleport2State.SelectTeleportTarget)
		{
			return m_ActorsToTarget.Count > 0;
		}
		return true;
	}

	public override bool RequiresWaypointSelection()
	{
		return m_State == ETeleport2State.ActorIsSelectingTeleportTile;
	}

	public CActor GetNextTarget()
	{
		if (m_ValidActorsInRange.Count > 0)
		{
			CActor result = m_ValidActorsInRange[0];
			m_ValidActorsInRange.RemoveAt(0);
			return result;
		}
		return null;
	}

	public override void Update()
	{
		if (m_State == ETeleport2State.ActorIsTeleporting && !ScenarioManager.Scenario.HasActor(ActorToTeleport))
		{
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(ActorToTeleport);
			}
			CheckNextTarget();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState && m_State != ETeleport2State.TeleportDone)
		{
			Log.SimpleInfo("[TELEPORT_2_ABILITY] - Ability complete moving state from: " + m_State.ToString() + " to: " + (m_State + 1));
			m_State++;
		}
		else
		{
			Log.SimpleInfo("[TELEPORT_2_ABILITY] - Ability complete check not moving state");
		}
		return m_State == ETeleport2State.TeleportDone;
	}

	public override string GetDescription()
	{
		return "Teleport2(" + Strength + ")";
	}

	private List<CTile> GetAttackTargetsAdjacentTiles()
	{
		List<CTile> list = new List<CTile>();
		foreach (CActor item in CPhaseAction.TargetsListDamagedInPrevAttackThisAction)
		{
			if (item.IsDead)
			{
				continue;
			}
			foreach (CTile item2 in GameState.GetTilesInRange(item.ArrayIndex, 1))
			{
				if (item2.FindProp(ScenarioManager.ObjectImportType.Door) == null)
				{
					IEnumerable<CActor> enumerable = from x in ScenarioManager.Scenario.FindActorsAt(item2.m_ArrayIndex)
						where x != TargetingActor
						select x;
					if (enumerable == null || !enumerable.Any())
					{
						list.Add(item2);
					}
				}
			}
		}
		return list;
	}

	private List<CTile> GetAlliesAdjacentTiles(int range)
	{
		List<CTile> list = new List<CTile>();
		List<CActor> list2 = new List<CActor>();
		list2.AddRange(ScenarioManager.Scenario.AllAllyMonsters);
		list2.AddRange(ScenarioManager.Scenario.AllPlayers);
		list2.AddRange(ScenarioManager.Scenario.AllHeroSummons);
		list2.Remove(TargetingActor);
		foreach (CActor item in list2.Where((CActor actor) => ScenarioManager.GetTileDistance(TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y, actor.ArrayIndex.X, actor.ArrayIndex.Y) <= range))
		{
			if (item.IsDead)
			{
				continue;
			}
			foreach (CTile item2 in GameState.GetTilesInRange(item.ArrayIndex, 1))
			{
				IEnumerable<CActor> enumerable = from x in ScenarioManager.Scenario.FindActorsAt(item2.m_ArrayIndex)
					where x != TargetingActor
					select x;
				if ((enumerable == null || !enumerable.Any()) && ScenarioManager.GetTileDistance(TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y, item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y) <= range)
				{
					list.Add(item2);
				}
			}
		}
		return list;
	}

	public List<CTile> GetTeleportTiles(Point origin, int range)
	{
		List<CTile> list = _GetTeleportTiles(origin, range);
		RemoveInvalidTiles(list);
		SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(list, this);
		return list;
	}

	private void RemoveInvalidTiles(List<CTile> tiles)
	{
		CTile originTile = TargetingActor?.Tile;
		ScenarioScript currentScenarioScript = ScenarioScript.current;
		if (currentScenarioScript != null)
		{
			tiles.RemoveAll((CTile tile) => !currentScenarioScript.IsTeleportValid(originTile, tile) || currentScenarioScript.NoTeleportMapGuids.Contains(tile.m_HexMap.MapGuid));
		}
		tiles.RemoveAll((CTile tile) => tile.m_HexMap != TargetingActor.Tile.m_HexMap && !tile.m_HexMap.CanTeleportToFromAnotherMap);
	}

	private List<CTile> _GetTeleportTiles(Point origin, int range)
	{
		List<CTile> list = null;
		TilesInRange.Clear();
		if ((Flags & EAbilityFlag.AdjacentToAttackTargets) != 0)
		{
			list = GetAttackTargetsAdjacentTiles();
			list.RemoveAll((CTile tile) => tile.IsBlocked);
			TilesInRange.AddRange(list);
			return list;
		}
		if ((Flags & EAbilityFlag.AdjacentToAllies) != 0)
		{
			list = GetAlliesAdjacentTiles(range);
			list.RemoveAll((CTile tile) => tile.IsBlocked);
			TilesInRange.AddRange(list);
			return list;
		}
		if ((Flags & EAbilityFlag.TargetStartingPositions) != 0)
		{
			list = ScenarioManager.StartingTiles;
			list.RemoveAll((CTile tile) => tile.IsBlocked || tile.LivingActor != null);
			TilesInRange.AddRange(list);
			return list;
		}
		list = TilesInRange;
		CTile originTile = ScenarioManager.GetTile(origin);
		if (GetVar("TargetShadows") != TValue.Unknown && GetVar("TargetShadows").GetBool())
		{
			if (GetVar("TargetTile") != TValue.Unknown)
			{
				CTile item = GetVar("TargetTile").GetObject<CTile>();
				list.Add(item);
			}
			else
			{
				List<CTile> list2 = ScenarioManager.CurrentScenarioState.ShadowProps.Select((CObjectProp prop) => prop.PropTile).Where((CTile tile) =>
				{
					List<CActor> actorsOnTile = GameState.GetActorsOnTile(tile, TargetingActor, CAbilityFilterContainer.CreateDefaultFilter(), null, isTargetedAbility: true, false);
					return (actorsOnTile.Count() == 0 || actorsOnTile.First() == TargetingActor) && !tile.IsBlocked;
				}).ToList();
				ScenarioScript currentScenarioScript = ScenarioScript.current;
				if (currentScenarioScript != null)
				{
					list2 = list2.Where((CTile tile) => currentScenarioScript.IsTeleportValid(originTile, tile) && !currentScenarioScript.NoTeleportMapGuids.Contains(tile.m_HexMap.MapGuid)).ToList();
				}
				foreach (CTile item2 in list2)
				{
					CNode cNode = ScenarioManager.PathFinder.Nodes[item2.m_ArrayIndex.X, item2.m_ArrayIndex.Y];
					if (cNode.Walkable && !cNode.Blocked)
					{
						list.Add(item2);
					}
				}
			}
		}
		if (GetVar("TargetAdjacentToShadows") != TValue.Unknown && GetVar("TargetAdjacentToShadows").GetBool())
		{
			foreach (CTile item3 in ScenarioManager.CurrentScenarioState.ShadowProps.Select((CObjectProp prop) => prop.PropTile))
			{
				foreach (CTile item4 in GameState.GetTilesInRange(item3.m_ArrayIndex, 1, EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, TargetingActor.Type == CActor.EType.Player))
				{
					IEnumerable<CActor> enumerable = from x in ScenarioManager.Scenario.FindActorsAt(item4.m_ArrayIndex)
						where x != TargetingActor
						select x;
					if (enumerable == null || !enumerable.Any())
					{
						list.Add(item4);
					}
				}
			}
			list = list.Where((CTile tile) => ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Walkable && !ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y].Blocked).ToList();
			ScenarioScript currentScenarioScript2 = ScenarioScript.current;
			if (currentScenarioScript2 != null)
			{
				list = list.Where((CTile tile) => currentScenarioScript2.IsTeleportValid(originTile, tile) && !currentScenarioScript2.NoTeleportMapGuids.Contains(tile.m_HexMap.MapGuid)).ToList();
			}
		}
		list.RemoveAll((CTile tile) => tile.m_HexMap != TargetingActor.Tile.m_HexMap && !tile.m_HexMap.CanTeleportToFromAnotherMap);
		if (GetVar("TargetShadows") != TValue.Unknown || GetVar("TargetAdjacentToShadows") != TValue.Unknown)
		{
			ScenarioScript.current?.ValidateAbilityTargeting(this);
			return list;
		}
		TValue miscVar = GetMiscVar("IsValidTeleportTile");
		foreach (CTile item5 in GameState.GetTilesInRange(origin, range))
		{
			if (ScenarioManager.PathFinder.GetNode(item5.m_ArrayIndex).Blocked || (!item5.m_HexMap.CanTeleportToFromAnotherMap && item5.m_HexMap != TargetingActor.Tile.m_HexMap))
			{
				continue;
			}
			List<CActor> list3 = ScenarioManager.Scenario.FindActorsAt(item5.m_ArrayIndex);
			if (list3 != null && list3.Count > 0 && list3.All((CActor a) => !a.IgnoreActorCollision && !a.Deactivated && a != TargetingActor))
			{
				continue;
			}
			if (!miscVar.isNull && !miscVar.isUnknown)
			{
				CVars cVars = new CVars();
				cVars.Set("tile", new TValue(item5));
				cVars.Set("Tile", new TValue(item5));
				CVarsContext context = new CVarsContext(cVars, VarsContext);
				if (!miscVar.GetBool(context))
				{
					continue;
				}
			}
			list.Add(item5);
		}
		ScenarioScript.current?.ValidateAbilityTargeting(this);
		return list;
	}

	private void CheckNextTarget()
	{
		Log.SimpleInfo("[TELEPORT_2_ABILITY] - Checking for next target");
		if (ActorToTeleport != null)
		{
			GameState.ActorHealthCheck(TargetingActor, ActorToTeleport);
			ActorsTargeted.Add(ActorToTeleport);
			m_ActorsToTarget.Remove(ActorToTeleport);
			if (ResourcesToTakeFromTargets != null && ResourcesToTakeFromTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in ResourcesToTakeFromTargets)
				{
					if (ActorToTeleport.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
					{
						ActorToTeleport.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
						TargetingActor.AddCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
					}
				}
			}
			if (ResourcesToGiveToTargets != null && ResourcesToGiveToTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToGiveToTarget in ResourcesToGiveToTargets)
				{
					if (TargetingActor.CharacterHasResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value))
					{
						TargetingActor.RemoveCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
						ActorToTeleport.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
					}
				}
			}
		}
		m_NumberTargetsRemaining--;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (m_NumberTargetsRemaining > 0)
		{
			m_State = ETeleport2State.SelectTeleportTarget;
			ValidActorsInRange.Remove(ActorToTeleport);
			ActorToTeleport = null;
			SourceTile = null;
			DestinationTile = null;
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			Perform();
		}
		else if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == EAbilityType.Teleport2)
		{
			PhaseManager.NextStep();
		}
	}

	public override void AbilityEnded()
	{
		Log.SimpleInfo("[PULL ABILITY] - Ability Ended");
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		List<Teleported2ActorStats> list = new List<Teleported2ActorStats>();
		if (m_ActorStats != null)
		{
			list.Add(m_ActorStats);
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTeleport2(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, list));
	}

	public bool HasPassedState(ETeleport2State teleport2State)
	{
		return m_State > teleport2State;
	}

	public override bool IsPositive()
	{
		return false;
	}
}
