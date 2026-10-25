using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityTeleport : CAbility
{
	public enum ETeleportState
	{
		None,
		ActorIsSelectingTeleportActor,
		ActorIsSelectingTeleportTile,
		ActorCastingTeleport,
		ActorIsTeleporting,
		ActorHasBeenTeleported,
		ActorFinalizeTeleport,
		TeleportDone
	}

	public enum ETeleportType
	{
		None,
		TeleportToTileInRange,
		TeleportToPropTypeRandomly,
		TeleportToCaster,
		TeleportToPropTypeInOrder
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class TeleportData
	{
		[JsonProperty]
		public ETeleportType TeleportType;

		[JsonProperty]
		public EPropType PropType;

		[JsonProperty]
		public bool MoveOtherThingsOffTiles;

		[JsonProperty]
		public bool ShouldOpenDoorsToTeleportedLocation;

		public TeleportData Copy()
		{
			return new TeleportData
			{
				TeleportType = TeleportType,
				PropType = PropType,
				MoveOtherThingsOffTiles = MoveOtherThingsOffTiles,
				ShouldOpenDoorsToTeleportedLocation = ShouldOpenDoorsToTeleportedLocation
			};
		}

		public static TeleportData DefaultTeleportData()
		{
			return new TeleportData
			{
				TeleportType = ETeleportType.TeleportToTileInRange,
				PropType = EPropType.None,
				MoveOtherThingsOffTiles = false,
				ShouldOpenDoorsToTeleportedLocation = false
			};
		}
	}

	public static readonly ETeleportState[] TeleportStates = (ETeleportState[])Enum.GetValues(typeof(ETeleportState));

	public static readonly ETeleportType[] TeleportTypes = (ETeleportType[])Enum.GetValues(typeof(ETeleportType));

	private CActor m_CurrentTarget;

	private ETeleportState m_State;

	private CTile m_StartingTile;

	private CTile m_TargetingTile;

	private List<CObjectProp> m_PropsToTeleportTo = new List<CObjectProp>();

	private bool m_FoundPropTiles;

	private bool m_TargetFlying;

	public CActor CurrentTarget
	{
		get
		{
			return m_CurrentTarget;
		}
		set
		{
			m_CurrentTarget = value;
			if (m_CurrentTarget != null)
			{
				m_StartingTile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
				m_TargetFlying = m_CurrentTarget.FlyingAfterConditions;
				if (m_TargetFlying)
				{
					return;
				}
				List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(m_CurrentTarget, EAbilityType.Move);
				if (list == null)
				{
					return;
				}
				{
					foreach (CActiveBonus item in list)
					{
						if (item is CMoveActiveBonus cMoveActiveBonus && cMoveActiveBonus.TryGetBehaviour(out CMoveActiveBonus_BuffMove behaviour) && behaviour.ReferenceFly(this, CurrentTarget).HasValue)
						{
							m_TargetFlying |= behaviour.ReferenceFly(this, CurrentTarget).Value;
						}
					}
					return;
				}
			}
			m_StartingTile = null;
		}
	}

	[JsonProperty]
	public TeleportData AbilityTeleportData { get; set; }

	public CAbilityTeleport(TeleportData teleportData)
	{
		if (teleportData == null)
		{
			teleportData = TeleportData.DefaultTeleportData();
		}
		AbilityTeleportData = teleportData;
	}

	public static CAbility CreateDefaultTeleport(TeleportData teleportData)
	{
		return CAbility.CreateAbility(EAbilityType.Teleport, 1, useSpecialBaseStat: false, 1, 1, 0, EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), "", null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultTeleport", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster: false, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, teleportData: teleportData, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		base.Start(targetActor, filterActor, controllingActor);
		m_State = ETeleportState.ActorIsSelectingTeleportActor;
		m_TargetingTile = null;
		if (IsInlineSubAbility && InlineSubAbilityTiles != null && InlineSubAbilityTiles.Count > 0)
		{
			CurrentTarget = TargetingActor;
			TilesInRange.AddRange(InlineSubAbilityTiles);
			if (TilesInRange.Count == 1)
			{
				m_TargetingTile = TilesInRange[0];
				m_State = ETeleportState.ActorIsTeleporting;
			}
			else
			{
				m_State = ETeleportState.ActorIsSelectingTeleportTile;
			}
		}
		else
		{
			SharedAbilityTargeting.GetValidActorsInRange(this);
		}
		if (NumberTargets == -1)
		{
			m_AllTargets = true;
			m_NumberTargetsRemaining = ValidActorsInRange.Count;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		else
		{
			m_NumberTargetsRemaining = NumberTargets;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		}
		if (AreaEffect == null && !AllTargets)
		{
			int num = ValidActorsInRange.Count - 1;
			while (num >= 0 && ValidActorsInRange.Count > NumberTargets)
			{
				ValidActorsInRange.RemoveAt(num);
				num--;
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
		case ETeleportState.ActorIsSelectingTeleportActor:
			if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true))
			{
				CurrentTarget = TargetingActor;
				m_State = ETeleportState.ActorIsSelectingTeleportTile;
				Perform();
			}
			else if (TargetingActor.Type != CActor.EType.Player)
			{
				CurrentTarget = null;
				for (int num3 = m_ValidActorsInRange.Count - 1; num3 >= 0; num3--)
				{
					CActor cActor2 = m_ValidActorsInRange[0];
					m_ValidActorsInRange.RemoveAt(0);
					if (ScenarioManager.Scenario.HasActor(cActor2))
					{
						CurrentTarget = cActor2;
						m_State = ETeleportState.ActorIsSelectingTeleportTile;
						Perform();
						break;
					}
				}
				if (CurrentTarget == null)
				{
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.NextStep();
				}
			}
			else if (m_ValidActorsInRange.Count > 0)
			{
				ScenarioRuleClient.MessageHandler(new CActorIsSelectingTargetingFocus_MessageData(TargetingActor)
				{
					m_IsPositive = false,
					m_TargetingAbility = this
				});
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
				PhaseManager.NextStep();
			}
			break;
		case ETeleportState.ActorIsSelectingTeleportTile:
			TilesInRange.Clear();
			switch (AbilityTeleportData.TeleportType)
			{
			case ETeleportType.TeleportToTileInRange:
				TilesInRange = GameState.GetTilesInRange(TargetingActor, Range, Targeting, emptyTilesOnly: true, ignoreBlocked: true, null, ignorePathLength: true, ignoreBlockedWithActor: false, ignoreLOS: true, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: true);
				break;
			case ETeleportType.TeleportToPropTypeRandomly:
				foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop.PropType == AbilityTeleportData.PropType)
					{
						CTile cTile2 = ScenarioManager.Tiles[prop.ArrayIndex.X, prop.ArrayIndex.Y];
						if (cTile2 != null && ScenarioManager.PathFinder.Nodes[cTile2.m_ArrayIndex.X, cTile2.m_ArrayIndex.Y].Walkable)
						{
							TilesInRange.Add(cTile2);
						}
					}
				}
				break;
			case ETeleportType.TeleportToPropTypeInOrder:
				if (m_FoundPropTiles)
				{
					break;
				}
				foreach (CObjectProp prop2 in ScenarioManager.CurrentScenarioState.Props)
				{
					if (prop2.PropType == AbilityTeleportData.PropType)
					{
						CTile cTile3 = ScenarioManager.Tiles[prop2.ArrayIndex.X, prop2.ArrayIndex.Y];
						if (cTile3 != null && ScenarioManager.PathFinder.Nodes[cTile3.m_ArrayIndex.X, cTile3.m_ArrayIndex.Y].Walkable)
						{
							m_PropsToTeleportTo.Add(prop2);
							TilesInRange.Add(cTile3);
						}
					}
				}
				break;
			case ETeleportType.TeleportToCaster:
			{
				CTile cTile = ScenarioManager.Tiles[TargetingActor.ArrayIndex.X, TargetingActor.ArrayIndex.Y];
				for (int j = 0; j < 5; j++)
				{
					List<CTile> allUnblockedTilesFromOrigin2 = ScenarioManager.GetAllUnblockedTilesFromOrigin(cTile, j + 1);
					allUnblockedTilesFromOrigin2.Remove(cTile);
					List<CTile> list3 = new List<CTile>();
					foreach (CTile item in allUnblockedTilesFromOrigin2)
					{
						CTile propTile2 = null;
						if (ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable && CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref propTile2) == null && ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex) == null)
						{
							list3.Add(item);
						}
					}
					if (list3.Count > 0)
					{
						TilesInRange.AddRange(list3);
						break;
					}
				}
				break;
			}
			}
			if (TilesInRange.Count == 0)
			{
				Log.ErrorWithReport("Could not find any tiles to teleport to");
				PhaseManager.StepComplete();
				return true;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerSelectingObjectPosition_MessageData message3 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
					m_IsPositive = IsPositive(),
					m_SpawnType = ScenarioManager.ObjectImportType.None
				};
				ScenarioRuleClient.MessageHandler(message3);
				break;
			}
			switch (AbilityTeleportData.TeleportType)
			{
			case ETeleportType.TeleportToPropTypeRandomly:
				m_TargetingTile = TilesInRange[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(TilesInRange.Count)];
				break;
			case ETeleportType.TeleportToPropTypeInOrder:
			{
				CTile cTile4 = null;
				if (m_PropsToTeleportTo.Count > 0)
				{
					foreach (CObjectProp item2 in m_PropsToTeleportTo)
					{
						CTile cTile5 = ScenarioManager.Tiles[item2.ArrayIndex.X, item2.ArrayIndex.Y];
						if (cTile5.m_HexMap.Revealed || (cTile5.m_Hex2Map != null && cTile5.m_Hex2Map.Revealed && ScenarioManager.Scenario.FindActorAt(cTile5.m_ArrayIndex) == null && !CurrentTarget.TeleportedToPropGuids.Contains(item2.PropGuid)))
						{
							cTile4 = cTile5;
							break;
						}
					}
					if (cTile4 != null)
					{
						m_TargetingTile = cTile4;
					}
					else
					{
						CurrentTarget.TeleportedToPropGuids.Clear();
					}
					break;
				}
				Log.ErrorWithReport("Could not find any prop tiles to teleport to");
				PhaseManager.StepComplete();
				return true;
			}
			case ETeleportType.TeleportToCaster:
			{
				List<CTile> list4 = new List<CTile>();
				int num2 = int.MaxValue;
				foreach (CTile item3 in TilesInRange)
				{
					int tileDistance = ScenarioManager.GetTileDistance(CurrentTarget.ArrayIndex.X, CurrentTarget.ArrayIndex.Y, item3.m_ArrayIndex.X, item3.m_ArrayIndex.Y);
					if (tileDistance < num2)
					{
						num2 = tileDistance;
						list4.Clear();
					}
					if (tileDistance == num2)
					{
						list4.Add(item3);
					}
				}
				m_TargetingTile = list4[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list4.Count)];
				break;
			}
			}
			if (CurrentTarget == TargetingActor)
			{
				m_State = ETeleportState.ActorIsTeleporting;
			}
			else
			{
				m_State = ETeleportState.ActorCastingTeleport;
			}
			Perform();
			break;
		case ETeleportState.ActorCastingTeleport:
		{
			CActorIsApplyingConditionActiveBonus_MessageData message4 = new CActorIsApplyingConditionActiveBonus_MessageData(AnimOverload, TargetingActor)
			{
				m_Ability = this,
				m_ActorsAppliedTo = new List<CActor> { CurrentTarget }
			};
			ScenarioRuleClient.MessageHandler(message4);
			break;
		}
		case ETeleportState.ActorIsTeleporting:
			AbilityHasHappened = true;
			m_CanUndo = false;
			if (CurrentTarget != null && m_TargetingTile != null)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
				if (AbilityTeleportData.ShouldOpenDoorsToTeleportedLocation)
				{
					OpenDoorsToTeleportedLocation(m_TargetingTile);
				}
				CTile propTile = null;
				if (AbilityTeleportData.MoveOtherThingsOffTiles)
				{
					EnsureTileIsClearForTeleport(m_TargetingTile, ref propTile, m_CurrentTarget);
				}
				else
				{
					CActor cActor = ScenarioManager.Scenario.FindActorAt(m_TargetingTile.m_ArrayIndex);
					if (!ScenarioManager.PathFinder.Nodes[m_TargetingTile.m_ArrayIndex.X, m_TargetingTile.m_ArrayIndex.Y].Walkable || (cActor != null && !cActor.IsDead) || (!m_TargetFlying && CObjectProp.FindPropWithPathingBlocker(m_TargetingTile.m_ArrayIndex, ref propTile) != null))
					{
						for (int i = 0; i < 5; i++)
						{
							List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(m_TargetingTile, i + 1);
							allUnblockedTilesFromOrigin.Remove(m_TargetingTile);
							List<CTile> list2 = new List<CTile>();
							foreach (CTile item4 in allUnblockedTilesFromOrigin)
							{
								if (ScenarioManager.PathFinder.Nodes[item4.m_ArrayIndex.X, item4.m_ArrayIndex.Y].Walkable && ScenarioManager.Scenario.FindActorAt(item4.m_ArrayIndex) == null && CObjectProp.FindPropWithPathingBlocker(item4.m_ArrayIndex, ref propTile) == null)
								{
									list2.Add(item4);
								}
							}
							if (list2.Count > 0)
							{
								m_TargetingTile = list2[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(list2.Count)];
								break;
							}
						}
					}
				}
				CActorIsTeleporting_MessageData message2 = new CActorIsTeleporting_MessageData(TargetingActor)
				{
					m_ActorTeleporting = CurrentTarget,
					m_TeleportAbility = this
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			else
			{
				Log.ErrorWithReport("Unable to teleport, no teleport target or target tile set");
				PhaseManager.StepComplete();
			}
			break;
		case ETeleportState.ActorHasBeenTeleported:
		{
			GameState.LostAdjacency(CurrentTarget, CurrentTarget.ArrayIndex, m_TargetingTile.m_ArrayIndex, this);
			CActorHasTeleported_MessageData message = new CActorHasTeleported_MessageData(TargetingActor)
			{
				m_EndLocation = m_TargetingTile.m_ArrayIndex,
				m_StartLocation = CurrentTarget.ArrayIndex,
				m_ActorTeleported = CurrentTarget,
				m_TeleportAbility = this
			};
			CurrentTarget.ArrayIndex = m_TargetingTile.m_ArrayIndex;
			ScenarioRuleClient.MessageHandler(message);
			return false;
		}
		case ETeleportState.ActorFinalizeTeleport:
			if (CurrentTarget != null)
			{
				List<CObjectProp> list = m_TargetingTile.m_Props.ToList();
				for (int num = list.Count - 1; num >= 0; num--)
				{
					CObjectProp cObjectProp = list[num];
					if (cObjectProp is CObjectDoor cObjectDoor)
					{
						cObjectDoor.ForceActivate(m_CurrentTarget);
					}
					else
					{
						cObjectProp.Activate(m_CurrentTarget);
					}
				}
				ActorsTargeted.Add(CurrentTarget);
				m_ActorsToTarget.Remove(CurrentTarget);
			}
			m_NumberTargetsRemaining--;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			if (m_NumberTargetsRemaining > 0)
			{
				m_State = ETeleportState.ActorIsSelectingTeleportActor;
				ValidActorsInRange.Remove(CurrentTarget);
				CurrentTarget = null;
				Perform();
			}
			else
			{
				PhaseManager.NextStep();
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
		bool flag = false;
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (m_State == ETeleportState.ActorIsSelectingTeleportActor)
			{
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
				if (ValidActorsInRange.Contains(actorOnTile))
				{
					if (CurrentTarget != null)
					{
						m_ActorsToTarget.Remove(CurrentTarget);
					}
					CurrentTarget = actorOnTile;
					m_ActorsToTarget.Add(CurrentTarget);
					flag = true;
				}
			}
			else if (m_State == ETeleportState.ActorIsSelectingTeleportTile && TilesInRange.Contains(selectedTile) && m_TargetingTile != selectedTile && GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible) == null && selectedTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) == null)
			{
				m_TilesSelected.Remove(m_TargetingTile);
				m_TargetingTile = selectedTile;
				m_TilesSelected.Add(selectedTile);
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
		if (m_State == ETeleportState.ActorIsSelectingTeleportActor)
		{
			CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
			if (CurrentTarget == actorOnTile)
			{
				m_ActorsToTarget.Remove(CurrentTarget);
				CurrentTarget = null;
				flag = true;
			}
		}
		else if (m_State == ETeleportState.ActorIsSelectingTeleportTile && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(m_TargetingTile);
			m_TargetingTile = null;
			flag = true;
		}
		if (flag)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override bool CanClearTargets()
	{
		return m_State == ETeleportState.ActorIsSelectingTeleportTile;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			return m_State == ETeleportState.ActorIsSelectingTeleportTile;
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
		return m_State == ETeleportState.TeleportDone;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityTeleport(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, (m_StartingTile != null) ? new TileIndex(m_StartingTile.m_ArrayIndex) : null, new TileIndex(TargetingActor.ArrayIndex)));
	}

	public override void Reset()
	{
		base.Reset();
		m_PropsToTeleportTo.Clear();
	}

	public override void AbilityEnded()
	{
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override bool IsCurrentlyTargetingActors()
	{
		return m_State == ETeleportState.ActorIsSelectingTeleportActor;
	}

	public static void EnsureTileIsClearForTeleport(CTile teleportTile, ref CTile refTile, CActor actorTeleporting, string animOverload = null)
	{
		CActor cActor = ScenarioManager.Scenario.FindActorAt(teleportTile.m_ArrayIndex);
		CObjectObstacle cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(teleportTile.m_ArrayIndex, ref refTile);
		bool flag = cActor == null;
		bool flag2 = cObjectObstacle == null;
		if (cActor == null && cObjectObstacle == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			List<CTile> allUnblockedTilesFromOrigin = ScenarioManager.GetAllUnblockedTilesFromOrigin(teleportTile, i + 1);
			allUnblockedTilesFromOrigin.Remove(teleportTile);
			List<CTile> list = new List<CTile>();
			foreach (CTile item in allUnblockedTilesFromOrigin)
			{
				if (ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable && item.m_Props.Count == 0 && ScenarioManager.Scenario.FindActorAt(item.m_ArrayIndex) == null && CObjectProp.FindPropWithPathingBlocker(item.m_ArrayIndex, ref refTile) == null)
				{
					list.Add(item);
				}
			}
			CTile randStartingTile = ScenarioManager.StartingTiles[ScenarioManager.CurrentScenarioState.ScenarioRNG.Next(ScenarioManager.StartingTiles.Count)];
			list.Sort((CTile t1, CTile t2) => ScenarioManager.GetTileDistance(t1, randStartingTile).CompareTo(ScenarioManager.GetTileDistance(t2, randStartingTile)));
			if (list.Count > 0 && !flag)
			{
				CTile cTile = list[0];
				TeleportActorToNewTile(cTile, cActor, animOverload);
				list.Remove(cTile);
				flag = true;
			}
			if (list.Count > 0 && !flag2)
			{
				CTile cTile2 = list[0];
				TeleportObstacleToNewTile(cTile2, cObjectObstacle, actorTeleporting, teleportTile);
				list.Remove(cTile2);
				flag2 = true;
			}
			if (flag & flag2)
			{
				break;
			}
		}
	}

	public static void TeleportActorToNewTile(CTile teleportTile, CActor actorToTeleport, string animOverload = null)
	{
		Point arrayIndex = actorToTeleport.ArrayIndex;
		actorToTeleport.ArrayIndex = teleportTile.m_ArrayIndex;
		CActorHasTeleported_MessageData message = new CActorHasTeleported_MessageData(actorToTeleport)
		{
			m_EndLocation = teleportTile.m_ArrayIndex,
			m_StartLocation = arrayIndex,
			m_ActorTeleported = actorToTeleport,
			m_TeleportAbility = null,
			AnimOverload = animOverload
		};
		ScenarioRuleClient.MessageHandler(message);
	}

	public static void TeleportObstacleToNewTile(CTile newTile, CObjectObstacle obstacleToTeleport, CActor replacingActor, CTile oldTile)
	{
		string prefabName = obstacleToTeleport.PrefabName;
		float num = 0f;
		obstacleToTeleport.DestroyProp(0f, sendMessageToClient: false);
		CDestroyObstacle_MessageData message = new CDestroyObstacle_MessageData(string.Empty, replacingActor)
		{
			m_ActorDestroyingObstacle = replacingActor,
			m_Tiles = new List<CTile> { oldTile },
			m_DestroyDelay = num,
			m_DestroyedProps = new List<CObjectProp> { obstacleToTeleport },
			m_DestroyObstacleAbility = null,
			m_OverrideSetLastSelectedTile = true
		};
		ScenarioRuleClient.MessageHandler(message);
		CObjectObstacle prop = new CObjectObstacle(pathingBlockers: new List<TileIndex>
		{
			new TileIndex(newTile.m_ArrayIndex)
		}, name: prefabName, type: ScenarioManager.ObjectImportType.Obstacle, arrayIndex: new TileIndex(newTile.m_ArrayIndex), position: null, rotation: null, owner: replacingActor, mapGuid: newTile.m_HexMap.MapGuid, ignoresFlyAndJump: false);
		newTile.SpawnProp(prop, notifyClient: true, num);
	}

	public static void OpenDoorsToTeleportedLocation(CTile teleportedTile)
	{
		if (teleportedTile.m_HexMap.Revealed)
		{
			return;
		}
		for (int i = 0; i < ScenarioManager.CurrentScenarioState.DoorProps.Count; i++)
		{
			CTile cTile = ScenarioManager.Tiles[ScenarioManager.CurrentScenarioState.DoorProps[i].ArrayIndex.X, ScenarioManager.CurrentScenarioState.DoorProps[i].ArrayIndex.Y];
			if (teleportedTile.IsMapShared(cTile) && ((cTile.m_HexMap != null && cTile.m_HexMap.Revealed) || (cTile.m_Hex2Map != null && cTile.m_Hex2Map.Revealed)))
			{
				((CObjectDoor)ScenarioManager.CurrentScenarioState.DoorProps[i]).ForceActivate(null);
			}
		}
	}

	public override bool IsPositive()
	{
		return true;
	}

	public override bool EnoughTargetsSelected()
	{
		if (m_State == ETeleportState.ActorIsSelectingTeleportActor)
		{
			return m_ActorsToTarget.Count > 0;
		}
		if (m_State == ETeleportState.ActorIsSelectingTeleportTile)
		{
			return m_TilesSelected.Count > 0;
		}
		return true;
	}
}
