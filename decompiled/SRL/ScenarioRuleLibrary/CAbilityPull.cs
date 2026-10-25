using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityPull : CAbility
{
	public enum EPullState
	{
		None,
		SelectTileToPullTowards,
		PostSelectTileToPullTowards,
		SelectPullTarget,
		ActorIsSelectingPullTile,
		ActorIsPulling,
		ActorHasPulled,
		ActorFinalizePull,
		CheckForNextTarget,
		PullDone
	}

	public enum EPullType
	{
		None,
		PullTowardsActor,
		PullTowardsTile,
		PullTowardsAlly
	}

	[Serializable]
	public class PulledActorStats
	{
		public string Name;

		public string ID;

		public int DistanceMoved;

		public TileIndex MovedFromPoint;

		public TileIndex MovedToPoint;

		public PulledActorStats(string name, string id, int distanceMoved, TileIndex movedFromPoint, TileIndex movedToPoint)
		{
			Name = name;
			ID = id;
			DistanceMoved = distanceMoved;
			MovedFromPoint = movedFromPoint;
			MovedToPoint = movedToPoint;
		}
	}

	public static readonly EPullType[] PullTypes = (EPullType[])Enum.GetValues(typeof(EPullType));

	public static bool s_LastPullAbilityWasExecuted;

	public static readonly EPullState[] PullStates = (EPullState[])Enum.GetValues(typeof(EPullState));

	private EPullState m_State;

	private CTile m_PlayerDestination;

	private List<CTile> m_PlayerWaypoints;

	public List<Point> m_AbilityPath;

	public List<Point> m_CachedAbilityPath;

	private CActor m_CurrentTarget;

	private bool m_TileSelected;

	private Dictionary<CActor, PulledActorStats> m_ActorDistanceMovedDictionary;

	private CAbilityFilterContainer m_OriginalFilter;

	public Point m_StartingPoint;

	private int m_AbilityPathProgressIndex;

	public int RemainingPulls { get; set; }

	public int DistanceMoved { get; set; }

	public bool MustMove { get; set; }

	public CActor CurrentTarget => m_CurrentTarget;

	public Point PullToPoint { get; set; }

	public CTile TryPullToThisTileAutomatically { get; set; }

	public EPullState State => m_State;

	[JsonProperty]
	public EPullType PullType { get; set; }

	public List<Point> GetAbilityPath()
	{
		if (m_CachedAbilityPath != null)
		{
			return m_CachedAbilityPath;
		}
		return m_AbilityPath;
	}

	public CAbilityPull(EPullType pullType)
	{
		PullType = pullType;
	}

	public override void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		base.CopyInterruptedProperties(destAbility, strength, initialCopy, forced);
		CAbilityPull cAbilityPull = (CAbilityPull)destAbility;
		cAbilityPull.RemainingPulls = RemainingPulls;
		cAbilityPull.DistanceMoved = DistanceMoved;
		cAbilityPull.MustMove = MustMove;
		cAbilityPull.PullToPoint = PullToPoint;
		cAbilityPull.PullType = PullType;
		cAbilityPull.m_State = m_State;
		cAbilityPull.m_PlayerDestination = m_PlayerDestination;
		cAbilityPull.m_AbilityPath = m_AbilityPath?.ToList();
		cAbilityPull.m_CachedAbilityPath = m_CachedAbilityPath?.ToList();
		cAbilityPull.m_CurrentTarget = m_CurrentTarget;
		cAbilityPull.m_TileSelected = m_TileSelected;
		cAbilityPull.m_ActorDistanceMovedDictionary = new Dictionary<CActor, PulledActorStats>(m_ActorDistanceMovedDictionary);
		cAbilityPull.m_OriginalFilter = m_OriginalFilter;
		cAbilityPull.m_StartingPoint = m_StartingPoint;
		cAbilityPull.m_AbilityPathProgressIndex = m_AbilityPathProgressIndex;
		if (State == EPullState.ActorFinalizePull)
		{
			cAbilityPull.PrepForReselectingTiles();
		}
	}

	public void PrepForReselectingTiles()
	{
		if (CurrentTarget != null && !CurrentTarget.IsDead)
		{
			m_PlayerWaypoints = null;
			m_PlayerDestination = null;
			m_State = EPullState.ActorIsSelectingPullTile;
			if (m_CachedAbilityPath != null && m_CachedAbilityPath.Count > 0)
			{
				int abilityPathProgressIndex = m_AbilityPathProgressIndex;
				RemainingPulls = ModifiedStrength() - abilityPathProgressIndex;
			}
		}
	}

	public static CAbility CreateDefaultPull(int strength, int range, int numberOfTargets, bool isMonster, string animationOverload = "", CAbilityFilterContainer abilityfilter = null)
	{
		return CAbility.CreateAbility(EAbilityType.Pull, strength, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, abilityfilter ?? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), animationOverload, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultPull", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	private void RefreshNumberOfTargets()
	{
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
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		Log.SimpleInfo("[PULL ABILITY] - Ability Started");
		s_LastPullAbilityWasExecuted = false;
		base.Start(targetingActor, filterActor, controllingActor);
		if (TargetingActor.IsMirrorActor() && !IsMirrorAbility)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffForcedMovement))
		{
			m_ModifiedStrength += item.ReferenceStrength(this, TargetingActor);
		}
		m_OriginalFilter = AbilityFilter;
		RemainingPulls = m_ModifiedStrength;
		PullToPoint = targetingActor.ArrayIndex;
		CTile cTile = GetMiscVar("PullToPoint").GetObject<CTile>();
		if (cTile != null)
		{
			PullToPoint = cTile.m_ArrayIndex;
		}
		m_CanUndo = false;
		m_CanSkip = false;
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || TargetingActor.Type != CActor.EType.Player || IsInlineSubAbility)
		{
			m_CanUndo = false;
		}
		SetTargets();
		if (UseSubAbilityTargeting || IsModifierAbility)
		{
			NumberTargets = ValidActorsInRange.Count;
		}
		RefreshNumberOfTargets();
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (PullType.Equals(EPullType.PullTowardsTile) || PullType.Equals(EPullType.PullTowardsAlly))
		{
			m_State = EPullState.SelectTileToPullTowards;
		}
		else if (m_CurrentTarget == null || TargetingActor.Type != CActor.EType.Player || TryPullToThisTileAutomatically != null)
		{
			if (Strength <= 0)
			{
				m_State = EPullState.ActorHasPulled;
			}
			else
			{
				m_State = EPullState.SelectPullTarget;
			}
		}
		m_PlayerWaypoints = null;
		m_PlayerDestination = null;
		m_ActorsToTarget.Clear();
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, PulledActorStats>();
		if (AbilityHasBeenCancelled)
		{
			CAbilityAttack.ApplyRetaliateToParentAttack(this, TargetThisActorAutomatically);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	private void SetTargets()
	{
		if (TargetThisActorAutomatically != null)
		{
			if (!CAbility.ImmuneToAbility(TargetThisActorAutomatically, this))
			{
				ValidActorsInRange = new List<CActor> { TargetThisActorAutomatically };
				m_CurrentTarget = TargetThisActorAutomatically;
				if (TargetingActor.Type == CActor.EType.Player && TryPullToThisTileAutomatically == null)
				{
					m_State = EPullState.ActorIsSelectingPullTile;
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
			else if (ParentAbility?.AreaEffect != null || ParentAbility?.AreaEffectBackup != null)
			{
				m_ValidTilesInAreaEffect = ParentAbility.ValidTilesInAreaAffected.ToList();
				ValidActorsInRange = GetValidActorsInArea(m_ValidTilesInAreaEffect);
				m_ActorsToTarget = ValidActorsInRange.ToList();
			}
			else if (AbilityFilter.Equals(ParentAbility?.AbilityFilter) && ParentAbility?.ActorsTargeted != null)
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
				if (cActor2.IsDead || cActor2.IsDowned)
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
		if (PullType == EPullType.PullTowardsActor)
		{
			foreach (CActor allAdjacentActor in ScenarioManager.GetAllAdjacentActors(TargetingActor, useRealArrayIndex: false))
			{
				if (ValidActorsInRange.Contains(allAdjacentActor))
				{
					ValidActorsInRange.Remove(allAdjacentActor);
				}
			}
		}
		if (UseSubAbilityTargeting)
		{
			if (ValidActorsInRange.Count <= 0)
			{
				SetCancelAbilityFlag(cancel: true);
			}
			else if (m_ValidActorsInRange.Count == 1)
			{
				m_CurrentTarget = m_ValidActorsInRange[0];
				if (TargetingActor.Type == CActor.EType.Player)
				{
					m_State = EPullState.ActorIsSelectingPullTile;
				}
			}
		}
		if (TargetingActor.Type == CActor.EType.Player)
		{
			return;
		}
		if (ValidActorsInRange.Count <= 0)
		{
			SetCancelAbilityFlag(cancel: true);
			return;
		}
		List<CActor> list = m_ValidActorsInRange.ToList();
		list.Sort((CActor x, CActor y) => (SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(x, TargetingActor) < SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(y, TargetingActor)) ? 1 : GameState.s_ActorInitiativeComparer.Compare(x, y));
		ValidActorsInRange = list;
	}

	public override void Restart()
	{
		base.Restart();
		s_LastPullAbilityWasExecuted = false;
	}

	public bool CanBePerformOnActor(CActor actor)
	{
		if (ScenarioScript.current != null && ScenarioScript.current.AllowPullWhenNoPushTiles(actor))
		{
			return true;
		}
		return GetPullTiles(actor, actor.Type, actor.ArrayIndex, PullToPoint, PullType, Strength, this, allowAlliesToMoveThroughAllies: false).Count > 0;
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		Log.SimpleInfo("[PULL ABILITY] - Performing state: " + m_State);
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
		case EPullState.SelectTileToPullTowards:
		{
			m_CanUndo = true;
			m_CanSkip = true;
			if (PullType == EPullType.PullTowardsAlly)
			{
				TilesInRange = (from actor in ScenarioManager.Scenario.AllAliveActors
					where CActor.AreActorsAllied(TargetingActor.Type, actor.Type, TargetingActor, actor)
					select actor.Tile).ToList();
				AbilityFilter = CAbilityFilterContainer.CreateDefaultFriendlyFilter();
				CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
				{
					m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Actor },
					m_IsPositive = IsPositive()
				};
				ScenarioRuleClient.MessageHandler(message);
				break;
			}
			TilesInRange = GameState.GetTilesInRange(TargetingActor.ArrayIndex, Range, EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true);
			AbilityFilter = new CAbilityFilterContainer();
			if (MiscVars == null)
			{
				MiscVars = new List<(string, TValue)>();
			}
			MiscVars.Insert(0, ("includeCasterTile", true));
			PullToPoint = default;
			CPlayerSelectingObjectPosition_MessageData message2 = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.None },
				m_IsPositive = IsPositive()
			};
			ScenarioRuleClient.MessageHandler(message2);
			break;
		}
		case EPullState.PostSelectTileToPullTowards:
		{
			m_CanUndo = false;
			m_CanSkip = false;
			if (ShowCancelledHelpText())
			{
				m_State = EPullState.PullDone;
				return true;
			}
			if (PullType == EPullType.PullTowardsTile)
			{
				AbilityFilter = m_OriginalFilter;
				MiscVars.RemoveAt(0);
			}
			ValidActorsInRange = GameState.GetActorsInRange(PullToPoint, FilterActor, Range, ActorsToIgnore, AbilityFilter, null, null, IsTargetedAbility, null, MiscAbilityData?.CanTargetInvisible);
			if (PullType != EPullType.PullTowardsAlly)
			{
				TilesInRange = GameState.GetTilesInRange(PullToPoint, Range, EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true);
				RefreshNumberOfTargets();
			}
			CActor cActor = ScenarioManager.Scenario.FindActorAt(PullToPoint);
			if (cActor != null && AbilityFilter.IsValidTarget(cActor, TargetingActor, IsTargetedAbility, useTargetOriginalType: false, MiscAbilityData?.CanTargetInvisible))
			{
				ValidActorsInRange.Add(cActor);
			}
			if (m_AllTargets)
			{
				NumberTargets = ValidActorsInRange.Count;
				m_NumberTargetsRemaining = NumberTargets;
				m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
				m_AllTargets = false;
			}
			CActorSelectedTileToPullTowards message5 = new CActorSelectedTileToPullTowards(GetVar("PullTowardsAnimOverload").GetString(), TargetingActor)
			{
				m_PullAbility = this,
				m_TileToPullTowards = ScenarioManager.Tiles[PullToPoint.X, PullToPoint.Y]
			};
			ScenarioRuleClient.MessageHandler(message5);
			AbilityFilter = m_OriginalFilter;
			if (m_CurrentTarget != null)
			{
				m_State = EPullState.SelectPullTarget;
			}
			break;
		}
		case EPullState.SelectPullTarget:
			m_CanUndo = !AbilityHasHappened;
			m_CanSkip = true;
			if (TargetingActor.Type != CActor.EType.Player || TryPullToThisTileAutomatically != null)
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
					m_CurrentTarget = GetNextTarget();
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPullState.ActorIsPulling;
					Perform();
				}
				else
				{
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.NextStep();
				}
			}
			else
			{
				if (ValidActorsInRange.Count == 0 && (TargetingActor.Type != CActor.EType.Player || (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value)))
				{
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.NextStep();
				}
				if (TargetThisActorAutomatically == null && !UseSubAbilityTargeting && PullType == EPullType.None)
				{
					SharedAbilityTargeting.GetValidActorsInRange(this);
				}
				ScenarioRuleClient.MessageHandler(new CActorIsSelectingTargetingFocus_MessageData(TargetingActor)
				{
					m_IsPositive = false,
					m_TargetingAbility = this
				});
			}
			break;
		case EPullState.ActorIsSelectingPullTile:
			m_CanUndo = !AbilityHasHappened;
			m_CanSkip = !MustMove;
			if (ShowCancelledHelpText())
			{
				m_State = EPullState.PullDone;
				return true;
			}
			if (m_CurrentTarget == null || !ScenarioManager.Scenario.HasActor(m_CurrentTarget) || !CanBePerformOnActor(m_CurrentTarget))
			{
				m_CanSkip = true;
				CheckNextTarget();
				break;
			}
			if (CAbility.ImmuneToAbility(m_CurrentTarget, this))
			{
				m_PlayerDestination = m_CurrentTarget.Tile;
				m_TileSelected = true;
				m_PlayerWaypoints = new List<CTile> { m_PlayerDestination };
			}
			if (TargetingActor.Type == CActor.EType.Player && m_PlayerDestination != null)
			{
				Log.SimpleInfo("[PULL ABILITY] - Player location: " + m_CurrentTarget.ArrayIndex.ToString());
				Log.SimpleInfo("[PULL ABILITY] - Player destination: " + m_PlayerDestination.m_ArrayIndex.ToString());
				Log.SimpleInfo("[PULL ABILITY] - Generating path using passed in waypoints");
				string text = "[PULL ABILITY] - In waypoints: ";
				foreach (CTile playerWaypoint in m_PlayerWaypoints)
				{
					text = text + "\n" + playerWaypoint.m_ArrayIndex.ToString();
				}
				Log.SimpleInfo(text);
				CAbilityPush.GetNextPathingPositionUsingExistingWaypoints(m_CurrentTarget, m_PlayerDestination, jump: false, fly: false, m_PlayerWaypoints, out m_PlayerWaypoints, out m_AbilityPath, ignoreDifficultTerrain: true, ignoreNegativeTiles: false, 0, ignoreMoveCost: true, logFailure: true);
				string text2 = "[PULL ABILITY] - Out waypoints: ";
				foreach (CTile playerWaypoint2 in m_PlayerWaypoints)
				{
					text2 = text2 + "\n" + playerWaypoint2.m_ArrayIndex.ToString();
				}
				Log.SimpleInfo(text2);
				string text3 = "[PULL ABILITY] - Out ability path: ";
				foreach (Point item2 in m_AbilityPath)
				{
					text3 = text3 + "\n" + item2.ToString();
				}
				Log.SimpleInfo(text3);
				m_CachedAbilityPath = m_AbilityPath.ToList();
				for (int num4 = 0; num4 < m_AbilityPath.Count; num4++)
				{
					Point point2 = m_AbilityPath[num4];
					CTile cTile2 = ScenarioManager.Tiles[point2.X, point2.Y];
					bool flag2 = false;
					for (int num5 = cTile2.m_Props.Count - 1; num5 >= 0; num5--)
					{
						if (cTile2.m_Props[num5].ObjectType != ScenarioManager.ObjectImportType.Door && cTile2.m_Props[num5].WillActivationKillActor(m_CurrentTarget))
						{
							flag2 = true;
							break;
						}
					}
					if (PathShouldEndDueToClosingDoor(num4) || GameState.WillMovingOnTileEndMovement(cTile2, CurrentTarget, this))
					{
						flag2 = true;
					}
					if (flag2)
					{
						Log.SimpleInfo("[PULL ABILITY] - Truncating pull path as prop activation will kill target along the way");
						m_PlayerDestination = cTile2;
						for (int num6 = num4 + 1; num6 < m_AbilityPath.Count; num6++)
						{
							CTile item = ScenarioManager.Tiles[m_AbilityPath[num6].X, m_AbilityPath[num6].Y];
							m_PlayerWaypoints.Remove(item);
						}
						m_AbilityPath.RemoveRange(num4 + 1, m_AbilityPath.Count - (num4 + 1));
						break;
					}
				}
				UpdateCurrentTargetStat(m_AbilityPath.Count, m_CurrentTarget.ArrayIndex, m_PlayerDestination.m_ArrayIndex);
				Log.SimpleInfo("[PULL ABILITY] - Updating Current Pull Target (" + m_CurrentTarget.GetPrefabName() + " " + m_CurrentTarget.ID + ") array index to: " + m_PlayerDestination.m_ArrayIndex.ToString());
				m_CurrentTarget.ArrayIndex = m_PlayerDestination.m_ArrayIndex;
				m_State = EPullState.ActorIsPulling;
				Perform();
			}
			else
			{
				m_TileSelected = false;
				if (!AbilityHasHappened)
				{
					CRefreshFirstAbility_MessageData message3 = new CRefreshFirstAbility_MessageData(TargetingActor)
					{
						m_IsFirstAbility = CPhaseAction.IsFirstAbility(this)
					};
					ScenarioRuleClient.MessageHandler(message3);
				}
				CActorIsSelectingPullTile_MessageData message4 = new CActorIsSelectingPullTile_MessageData(TargetingActor)
				{
					m_PullAbility = this
				};
				ScenarioRuleClient.MessageHandler(message4);
			}
			break;
		case EPullState.ActorIsPulling:
			m_CanUndo = false;
			m_CanSkip = false;
			if (ShowCancelledHelpText())
			{
				return true;
			}
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (TargetingActor.Type == CActor.EType.Player && TryPullToThisTileAutomatically == null)
			{
				m_StartingPoint = m_CurrentTarget.PrevArrayIndex;
				m_AbilityPathProgressIndex = 0;
			}
			else
			{
				m_StartingPoint = m_CurrentTarget.ArrayIndex;
				m_AbilityPathProgressIndex = 0;
			}
			CurrentTarget.SetTempVar("LastTileBeforeForceMove", new TValue(ScenarioManager.GetTile(m_StartingPoint)));
			if (TargetingActor.Type == CActor.EType.Player && !m_TileSelected && TryPullToThisTileAutomatically == null)
			{
				CheckNextTarget();
			}
			else if (TargetingActor.Type != CActor.EType.Player || TryPullToThisTileAutomatically != null)
			{
				m_CurrentTarget.Pull(PullToPoint, RemainingPulls, PullType, out m_AbilityPath, TryPullToThisTileAutomatically, this);
				if (m_CurrentTarget.ArrayIndex != m_StartingPoint)
				{
					CActorIsPulling_MessageData cActorIsPulling_MessageData = new CActorIsPulling_MessageData(TargetingActor);
					cActorIsPulling_MessageData.m_PullAbility = this;
					cActorIsPulling_MessageData.m_Waypoints = m_CurrentTarget.AIMoveFocusWaypoints;
					cActorIsPulling_MessageData.AnimOverload = AnimOverload;
					ScenarioRuleClient.MessageHandler(cActorIsPulling_MessageData);
				}
				else
				{
					m_State = EPullState.ActorHasPulled;
					ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
				}
			}
			else
			{
				CActorIsPulling_MessageData cActorIsPulling_MessageData2 = new CActorIsPulling_MessageData(TargetingActor);
				cActorIsPulling_MessageData2.m_PullAbility = this;
				cActorIsPulling_MessageData2.m_Waypoints = m_PlayerWaypoints;
				cActorIsPulling_MessageData2.AnimOverload = AnimOverload;
				ScenarioRuleClient.MessageHandler(cActorIsPulling_MessageData2);
			}
			break;
		case EPullState.ActorHasPulled:
		{
			s_LastPullAbilityWasExecuted = true;
			LogEvent(ESESubTypeAbility.ApplyToActor);
			CActorHasPulled_MessageData cActorHasPulled_MessageData = new CActorHasPulled_MessageData(TargetingActor);
			cActorHasPulled_MessageData.m_PullAbility = this;
			cActorHasPulled_MessageData.m_Waypoints = m_PlayerWaypoints;
			ScenarioRuleClient.MessageHandler(cActorHasPulled_MessageData);
			break;
		}
		case EPullState.ActorFinalizePull:
		{
			AbilityHasHappened = s_LastPullAbilityWasExecuted;
			for (int num2 = 0; num2 < m_AbilityPath.Count; num2++)
			{
				Point firstPoint = ((num2 == 0) ? m_StartingPoint : m_AbilityPath[num2 - 1]);
				GameState.LostAdjacency(m_CurrentTarget, firstPoint, m_AbilityPath[num2], this);
			}
			m_CurrentTarget.SetTempVar("LastForceMovePath", new TValue(m_AbilityPath.ToList()));
			if (m_CurrentTarget.CanActivatePressurePlates() && ScenarioManager.Tiles[m_StartingPoint.X, m_StartingPoint.Y].FindProp(ScenarioManager.ObjectImportType.PressurePlate) is CObjectPressurePlate { ActivatesOnEndTurn: false, PressurePlateType: CObjectPressurePlate.EPressurePlateType.ActivateWhileOccupied } cObjectPressurePlate)
			{
				cObjectPressurePlate.Deactivate();
			}
			Point prevPosition = m_StartingPoint;
			while (m_AbilityPathProgressIndex < m_AbilityPath.Count)
			{
				Point point = m_AbilityPath[m_AbilityPathProgressIndex];
				m_AbilityPathProgressIndex++;
				CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
				for (int num3 = cTile.m_Props.Count - 1; num3 >= 0; num3--)
				{
					CObjectProp cObjectProp = cTile.m_Props[num3];
					if (!CObjectProp.TryChooseToSkipTrap(m_CurrentTarget, cObjectProp))
					{
						bool flag = false;
						if (cObjectProp.WillActivationKillActor(m_CurrentTarget))
						{
							flag = true;
							m_CurrentTarget.ArrayIndex = point;
							UpdateCurrentTargetStat(m_AbilityPathProgressIndex, m_StartingPoint, point);
						}
						if (cObjectProp.ObjectType != ScenarioManager.ObjectImportType.PressurePlate && cObjectProp.ObjectType != ScenarioManager.ObjectImportType.Portal)
						{
							cObjectProp.AutomaticActivate(m_CurrentTarget);
							if (TargetingActor != null && cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Trap)
							{
								SEventLogMessageHandler.AddEventLogMessage(new SEventActorForcedIntoTrap(GetAbilityBaseCard().ID, TargetingActor.Class.ID));
							}
						}
						else if (cObjectProp is CObjectPressurePlate cObjectPressurePlate2 && m_CurrentTarget.CanActivatePressurePlates())
						{
							if (cObjectPressurePlate2.PressurePlateType == CObjectPressurePlate.EPressurePlateType.ActivateOnce || (m_CurrentTarget.ArrayIndex.X == cObjectPressurePlate2.ArrayIndex.X && m_CurrentTarget.ArrayIndex.Y == cObjectPressurePlate2.ArrayIndex.Y && !flag))
							{
								cObjectProp.AutomaticActivate(m_CurrentTarget);
							}
							else if (!cObjectPressurePlate2.Activated && cObjectPressurePlate2.PressurePlateType == CObjectPressurePlate.EPressurePlateType.ActivateWhileOccupied && !cObjectPressurePlate2.ActivatesOnEndTurn)
							{
								cObjectProp.Activate(m_CurrentTarget);
								cObjectProp.Deactivate();
							}
						}
					}
				}
				m_CurrentTarget.ArrayIndex = point;
				UpdateCurrentTargetStat(m_AbilityPathProgressIndex, m_StartingPoint, point);
				CActiveBonus.RefreshAllAuraActiveBonuses();
				m_CurrentTarget.CalculateAttackStrengthForUI();
				m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, new List<CActor>(), newActorCarried: false, 1, m_AbilityPathProgressIndex >= m_AbilityPath.Count, cTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), cTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1, prevPosition);
				prevPosition = point;
				if (m_CurrentTarget.Health <= 0)
				{
					break;
				}
			}
			DistanceMoved = m_AbilityPath.Count;
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(m_CurrentTarget);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(m_CurrentTarget);
			}
			PhaseManager.StepComplete();
			return true;
		}
		case EPullState.CheckForNextTarget:
			if (ShowCancelledHelpText())
			{
				return true;
			}
			CheckNextTarget();
			return true;
		}
		return false;
		IL_00aa:
		if (TargetingActor.Type == CActor.EType.Player)
		{
			if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				CPlayerIsSleeping_MessageData message6 = new CPlayerIsSleeping_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message6);
			}
			else
			{
				CPlayerIsStunned_MessageData message7 = new CPlayerIsStunned_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message7);
			}
		}
		else
		{
			PhaseManager.NextStep();
		}
		return true;
	}

	private bool PathShouldEndDueToClosingDoor(int currentIdx)
	{
		if (currentIdx + 1 >= m_AbilityPath.Count)
		{
			return false;
		}
		if (!m_CurrentTarget.CanActivatePressurePlates())
		{
			return false;
		}
		if (!(m_CurrentTarget.Tile.FindProp(ScenarioManager.ObjectImportType.PressurePlate) is CObjectPressurePlate { ActivatesOnEndTurn: false, PressurePlateType: CObjectPressurePlate.EPressurePlateType.ActivateWhileOccupied } cObjectPressurePlate))
		{
			return false;
		}
		Point point = m_AbilityPath[currentIdx + 1];
		CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
		for (int i = 0; i < cTile.m_Props.Count; i++)
		{
			if (cTile.m_Props[i].ObjectType == ScenarioManager.ObjectImportType.Door)
			{
				CObjectDoor cObjectDoor = cTile.m_Props[i] as CObjectDoor;
				if (!cObjectDoor.Activated || !cObjectDoor.LinkedPressurePlates.Contains(cObjectPressurePlate))
				{
					return false;
				}
				int num = cObjectDoor.LinkedPressurePlates.Where((CObjectPressurePlate p) => p.GetConfigForPartySize(ScenarioManager.CurrentScenarioState?.Players.Count ?? 1) != ScenarioManager.EPerPartySizeConfig.Hidden && p.Activated).Count();
				int doorPressurePlateLockCount = cObjectDoor.DoorPressurePlateLockCount;
				if (num - 1 < doorPressurePlateLockCount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		Log.SimpleInfo("[PULL ABILITY] - Tile selected");
		bool flag = false;
		if (m_State == EPullState.SelectTileToPullTowards)
		{
			if (PullType == EPullType.PullTowardsAlly)
			{
				CActor actorOnTile = GameState.GetActorOnTile(selectedTile, FilterActor, AbilityFilter, new List<CActor>(), IsTargetedAbility, MiscAbilityData?.CanTargetInvisible);
				if (PullToPoint != selectedTile.m_ArrayIndex && actorOnTile != null && CActor.AreActorsAllied(TargetingActor.Type, actorOnTile.Type, TargetingActor, actorOnTile))
				{
					PullToPoint = selectedTile.m_ArrayIndex;
					m_TilesSelected.Clear();
					m_TilesSelected.Add(selectedTile);
					if (TargetingActor.Type == CActor.EType.Player)
					{
						CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
						cPlayerSelectedTile_MessageData.m_Ability = this;
						ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
						CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
						{
							m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Actor },
							m_IsPositive = IsPositive()
						};
						ScenarioRuleClient.MessageHandler(message);
					}
				}
			}
			else if (PullToPoint != selectedTile.m_ArrayIndex)
			{
				PullToPoint = selectedTile.m_ArrayIndex;
				m_TilesSelected.Clear();
				m_TilesSelected.Add(selectedTile);
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData2 = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData2.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData2);
					if (IsCurrentlyTargetingActors())
					{
						ScenarioRuleClient.MessageHandler(new CActorIsSelectingTargetingFocus_MessageData(TargetingActor)
						{
							m_IsPositive = false,
							m_TargetingAbility = this
						});
					}
				}
			}
		}
		else if (m_State == EPullState.SelectPullTarget)
		{
			CActor cActor = ScenarioManager.Scenario.FindActorAt(selectedTile.m_ArrayIndex);
			if (cActor != null && cActor != m_CurrentTarget && !ActorsTargeted.Contains(cActor) && ValidActorsInRange.Contains(cActor))
			{
				if (m_CurrentTarget != null)
				{
					CTile item = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
					m_ActorsToTarget.Remove(m_CurrentTarget);
					m_TilesSelected.Remove(item);
				}
				m_CurrentTarget = cActor;
				m_ActorsToTarget.Add(cActor);
				m_TilesSelected.Add(selectedTile);
				if (TargetingActor.Type == CActor.EType.Player)
				{
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData3 = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData3.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData3);
				}
				if (TargetingActor.Type != CActor.EType.Player)
				{
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPullState.ActorIsPulling;
					flag = true;
				}
				else if (TargetingActor.Type == CActor.EType.Player && UseSubAbilityTargeting)
				{
					m_State = EPullState.ActorIsSelectingPullTile;
					flag = true;
				}
			}
		}
		else if (m_State == EPullState.ActorIsSelectingPullTile)
		{
			m_TileSelected = true;
			m_PlayerDestination = selectedTile;
			m_PlayerWaypoints = optionalTileList;
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
		Log.SimpleInfo("[PULL ABILITY] - Tile deselected");
		bool flag = false;
		if (m_State == EPullState.SelectTileToPullTowards)
		{
			if (PullToPoint == selectedTile.m_ArrayIndex)
			{
				m_TilesSelected.Remove(selectedTile);
				PullToPoint = new Point(0, 0);
				flag = true;
			}
			CPlayerSelectingObjectPosition_MessageData message = new CPlayerSelectingObjectPosition_MessageData(this, TargetingActor)
			{
				m_TileFilter = new List<CAbilityFilter.EFilterTile> { CAbilityFilter.EFilterTile.Actor },
				m_IsPositive = IsPositive()
			};
			ScenarioRuleClient.MessageHandler(message);
		}
		else if (m_State == EPullState.SelectPullTarget && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			ValidActorsInRange.Add(m_CurrentTarget);
			m_ActorsToTarget.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			flag = true;
		}
		if (flag && TargetingActor.Type == CActor.EType.Player)
		{
			CPlayerSelectedTile_MessageData message2 = new CPlayerSelectedTile_MessageData(TargetingActor)
			{
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message2);
			if (IsCurrentlyTargetingActors())
			{
				ScenarioRuleClient.MessageHandler(new CActorIsSelectingTargetingFocus_MessageData(TargetingActor)
				{
					m_IsPositive = false,
					m_TargetingAbility = this
				});
			}
		}
		if (0 != 0)
		{
			Perform();
		}
		base.TileDeselected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileDeselected);
	}

	public override void AbilityPassStep()
	{
		if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
			{
				m_State = EPullState.PullDone;
				return;
			}
		}
		if (m_State == EPullState.SelectTileToPullTowards && PullType == EPullType.PullTowardsAlly)
		{
			m_State = EPullState.PullDone;
		}
		if (m_State == EPullState.SelectPullTarget)
		{
			m_State = EPullState.ActorFinalizePull;
		}
	}

	public override void ClearTargets()
	{
		int undoNumberTargetsRemaining = m_UndoNumberTargetsRemaining;
		base.ClearTargets();
		m_CurrentTarget = null;
		m_ActorsToTarget.Clear();
		if (m_State == EPullState.SelectPullTarget)
		{
			m_NumberTargetsRemaining = undoNumberTargetsRemaining;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			Perform();
		}
		else if (m_State == EPullState.SelectTileToPullTowards)
		{
			PullToPoint = new Point(0, 0);
			Perform();
		}
		else if (m_State == EPullState.ActorIsSelectingPullTile)
		{
			m_State = EPullState.SelectPullTarget;
			m_NumberTargetsRemaining = undoNumberTargetsRemaining;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
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
		if (m_State == EPullState.SelectTileToPullTowards || m_State == EPullState.SelectPullTarget || m_State == EPullState.ActorIsSelectingPullTile)
		{
			return m_ActorsToTarget.Count > 0;
		}
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EPullState.SelectTileToPullTowards && m_State != EPullState.SelectPullTarget)
			{
				return m_State == EPullState.ActorIsSelectingPullTile;
			}
			return true;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		if (m_State != EPullState.SelectPullTarget)
		{
			return true;
		}
		return m_ActorsToTarget.Count > 0;
	}

	public override bool RequiresWaypointSelection()
	{
		return m_State == EPullState.ActorIsSelectingPullTile;
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
		if (m_State == EPullState.ActorIsPulling && !ScenarioManager.Scenario.HasActor(m_CurrentTarget))
		{
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(m_CurrentTarget);
			}
			CheckNextTarget();
		}
	}

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (!dontMoveState && m_State != EPullState.PullDone)
		{
			Log.SimpleInfo("[PULL ABILITY] - Ability complete moving state from: " + m_State.ToString() + " to: " + (m_State + 1));
			m_State++;
		}
		else
		{
			Log.SimpleInfo("[PULL ABILITY] - Ability complete check not moving state");
		}
		return m_State == EPullState.PullDone;
	}

	public override string GetDescription()
	{
		return "Pull(" + Strength + ")";
	}

	public static List<CTile> GetPullTiles(CActor actorToPull, CActor.EType targetType, Point pullFromPoint, Point pullToPoint, EPullType pullType, int remainingPulls = int.MaxValue, CAbility ability = null, bool allowAlliesToMoveThroughAllies = true)
	{
		List<CTile> pullTiles_New = GetPullTiles_New(actorToPull, pullFromPoint, pullToPoint, remainingPulls, allowAlliesToMoveThroughAllies);
		SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(pullTiles_New, ability);
		return pullTiles_New;
	}

	private static List<CTile> GetPullTiles_New(CActor actorToPull, Point pullFromPoint, Point pullToPoint, int remainingPulls, bool allowAlliesToMoveThroughAllies = true)
	{
		List<CTile> tiles = new List<CTile>();
		if (remainingPulls == 0)
		{
			return tiles;
		}
		List<Point> pathSoFar = new List<Point>();
		pathSoFar.Add(pullFromPoint);
		GetPullTilesRecursive_New(actorToPull, pullFromPoint, pullToPoint, remainingPulls, ref pathSoFar, ref tiles, allowAlliesToMoveThroughAllies);
		return tiles;
	}

	private static void GetPullTilesRecursive_New(CActor actorToPull, Point pullFromPoint, Point pullToPoint, int remainingPulls, ref List<Point> pathSoFar, ref List<CTile> tiles, bool allowAlliesToMoveThroughAllies = true)
	{
		int strength = 1;
		CAbilityMove.GetMoveBonuses(actorToPull, out var jump, out var fly, out var _, out var _, ref strength);
		bool flag = false;
		List<Point> list = new List<Point>();
		if (pathSoFar.Count > 1 && ScenarioManager.PathFinder.GetNode(pathSoFar[pathSoFar.Count - 1]).IsIceTerrain && !fly && !jump)
		{
			Point point = pathSoFar[pathSoFar.Count - 2];
			Point ptNext = pathSoFar[pathSoFar.Count - 1];
			CPathFinderContext cPathFinderContext = ScenarioManager.PathFinder.FloodFill_ToRange(actorToPull, point, 1, fly, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, ignoreDoorLocks: false, (CPathFinderContext context) =>
			{
				CAIMoveNew.AddDoorsToPathFindingContext(context, disallowDoorways: false, shouldPathThroughDoors: false);
			}, isCalculatingDistance: false, ignoreCannotStop: true, EAbilityType.Pull);
			CPathFinderContext.TNode nodePrev = cPathFinderContext.GetNode(point);
			if (cPathFinderContext.SlideOnIce(ref ptNext, point, ref nodePrev, out var numSlides) != CPathFinder.EAdjacentPosition.None && numSlides > 0)
			{
				list.Add(ptNext);
				flag = true;
			}
		}
		if (!flag)
		{
			ScenarioManager.PathFinder.FloodFill_ToRange(actorToPull, pullFromPoint, 1, fly, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, ignoreDoorLocks: false, (CPathFinderContext context) =>
			{
				CAIMoveNew.AddDoorsToPathFindingContext(context, disallowDoorways: false, shouldPathThroughDoors: false);
			}, isCalculatingDistance: false, ignoreCannotStop: true, EAbilityType.Pull);
		}
		int num = Point.AxialDistance(Point.OddRToAxial(pullToPoint), Point.OddRToAxial(pullFromPoint));
		Point.AxialDistance(Point.OddRToAxial(pullToPoint), Point.OddRToAxial(actorToPull.ArrayIndex));
		foreach (Point item in list)
		{
			bool flag2 = true;
			int num2 = num;
			List<Point> list2 = CActor.FindCharacterPath(actorToPull, pullFromPoint, item, fly, ignoreMoveCost: true, out var foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: true, jump, fly);
			if (!foundPath)
			{
				continue;
			}
			pathSoFar.Add(item);
			bool flag3 = flag;
			foreach (Point item2 in list2)
			{
				int num3 = Point.AxialDistance(Point.OddRToAxial(pullToPoint), Point.OddRToAxial(item2));
				if ((num3 < num2) | flag3)
				{
					num2 = num3;
					flag3 = ScenarioManager.PathFinder.GetNode(item2).IsIceTerrain && !fly && !jump;
					continue;
				}
				flag2 = false;
				break;
			}
			if (!flag2)
			{
				pathSoFar.RemoveAt(pathSoFar.Count - 1);
				continue;
			}
			if (!tiles.Contains(ScenarioManager.Tiles[item.X, item.Y]) && (remainingPulls > 0 || ((remainingPulls == 0) & flag)))
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(item);
				if (cActor == null || cActor.DoesNotBlock || (allowAlliesToMoveThroughAllies && CustomAbility.IsAlly(cActor, actorToPull)))
				{
					tiles.Add(ScenarioManager.Tiles[item.X, item.Y]);
				}
			}
			if (remainingPulls > 0)
			{
				int remainingPulls2 = ((ScenarioManager.PathFinder.GetNode(item).IsIceTerrain && !fly && !jump) ? remainingPulls : (remainingPulls - 1));
				GetPullTilesRecursive_New(actorToPull, item, pullToPoint, remainingPulls2, ref pathSoFar, ref tiles, allowAlliesToMoveThroughAllies);
			}
			pathSoFar.RemoveAt(pathSoFar.Count - 1);
		}
	}

	private static List<CTile> GetPullTiles_Old(CActor actorToPull, CActor.EType targetType, Point pullFromPoint, Point pullToPoint, EPullType pullType, int remainingPulls)
	{
		List<CTile> tiles = new List<CTile>();
		List<Point> list = CActor.FindCharacterPath(actorToPull, pullFromPoint, pullToPoint, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, actorToPull?.FlyingAfterConditions ?? false);
		if (!foundPath || list.Count == 0)
		{
			return tiles;
		}
		GetPullTilesRecursive_Old(actorToPull, ScenarioManager.Tiles[pullToPoint.X, pullToPoint.Y], ScenarioManager.Tiles[pullFromPoint.X, pullFromPoint.Y], ref tiles, list.Count, targetType, remainingPulls);
		if (pullType.Equals(EPullType.PullTowardsTile))
		{
			bool flag = false;
			if (actorToPull != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(actorToPull, out var _, out var fly, out var _, out var _, ref strength);
				flag = fly;
			}
			if (tiles.Count > 0 || list.Count == 1)
			{
				CTile cTile = ScenarioManager.Tiles[pullToPoint.X, pullToPoint.Y];
				CNode cNode = ScenarioManager.PathFinder.Nodes[cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y];
				CActor cActor = ScenarioManager.Scenario.FindActorAt(cTile.m_ArrayIndex);
				if (cNode.Walkable && (!cNode.Blocked | flag) && (cTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) == null || !((CObjectObstacle)cTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump) && cActor == null)
				{
					tiles.Add(ScenarioManager.Tiles[pullToPoint.X, pullToPoint.Y]);
				}
			}
		}
		return tiles;
	}

	private static void GetPullTilesRecursive_Old(CActor actorToPull, CTile pullToTile, CTile currentTile, ref List<CTile> tiles, int distance, CActor.EType targetType, int remainingPulls)
	{
		List<CTile> list = new List<CTile>();
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			CActor.FindCharacterPath(actorToPull, currentTile.m_ArrayIndex, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, actorToPull?.FlyingAfterConditions ?? false);
			if (!foundPath)
			{
				continue;
			}
			List<Point> list2 = CActor.FindCharacterPath(actorToPull, pullToTile.m_ArrayIndex, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath2, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, actorToPull?.FlyingAfterConditions ?? false);
			bool flag = IsClosedDoor(allAdjacentTile);
			bool flag2 = false;
			if (actorToPull != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(actorToPull, out var _, out var fly, out var _, out var _, ref strength);
				flag2 = fly;
			}
			if (!(!flag & foundPath2) || list2.Count >= distance || tiles.Contains(allAdjacentTile) || !(!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked | flag2) || (allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) != null && ((CObjectObstacle)allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump))
			{
				continue;
			}
			List<CActor> list3 = ScenarioManager.Scenario.FindActorsAt(allAdjacentTile.m_ArrayIndex);
			bool flag3 = false;
			bool flag4 = false;
			foreach (CActor item in list3)
			{
				if (item != null && !item.DoesNotBlock)
				{
					if (!CActor.AreActorsAllied(item.Type, targetType, item, null) && !item.IsDowned && remainingPulls > 1)
					{
						flag3 = true;
					}
					else if (CActor.AreActorsAllied(item.Type, targetType, item, null))
					{
						flag4 = true;
					}
				}
			}
			if (!flag3)
			{
				list.Add(allAdjacentTile);
				if (!flag4)
				{
					tiles.Add(allAdjacentTile);
				}
			}
		}
		if (remainingPulls - 1 <= 0)
		{
			return;
		}
		foreach (CTile item2 in list)
		{
			GetPullTilesRecursive_Old(actorToPull, pullToTile, item2, ref tiles, distance - 1, targetType, remainingPulls - 1);
		}
	}

	private void CheckNextTarget()
	{
		Log.SimpleInfo("[PULL ABILITY] - Checking for next target");
		if (m_CurrentTarget != null)
		{
			GameState.ActorHealthCheck(TargetingActor, m_CurrentTarget);
			ActorsTargeted.Add(m_CurrentTarget);
			m_ActorsToTarget.Remove(m_CurrentTarget);
			if (ResourcesToTakeFromTargets != null && ResourcesToTakeFromTargets.Count > 0)
			{
				foreach (KeyValuePair<string, int> resourcesToTakeFromTarget in ResourcesToTakeFromTargets)
				{
					if (m_CurrentTarget.CharacterHasResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value))
					{
						m_CurrentTarget.RemoveCharacterResource(resourcesToTakeFromTarget.Key, resourcesToTakeFromTarget.Value);
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
						m_CurrentTarget.AddCharacterResource(resourcesToGiveToTarget.Key, resourcesToGiveToTarget.Value);
					}
				}
			}
		}
		m_NumberTargetsRemaining--;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		if (m_NumberTargetsRemaining > 0)
		{
			m_State = EPullState.SelectPullTarget;
			ValidActorsInRange.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			m_PlayerDestination = null;
			RemainingPulls = m_ModifiedStrength;
			DistanceMoved = 0;
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			Perform();
		}
		else if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == EAbilityType.Pull)
		{
			PhaseManager.NextStep(!AbilityHasHappened);
		}
	}

	public override void AbilityEnded()
	{
		Log.SimpleInfo("[PULL ABILITY] - Ability Ended");
		foreach (CActor item in ActorsTargeted)
		{
			CAbilityAttack.ApplyRetaliateToParentAttack(this, item);
		}
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		List<PulledActorStats> actorsDistanceMoved = m_ActorDistanceMovedDictionary.Values.ToList();
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityPull(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, actorsDistanceMoved));
	}

	public bool HasPassedState(EPullState pullState)
	{
		return m_State > pullState;
	}

	private void UpdateCurrentTargetStat(int movedCount, Point movedFrom, Point movedTo)
	{
		if (m_ActorDistanceMovedDictionary.TryGetValue(m_CurrentTarget, out var value))
		{
			value.DistanceMoved = movedCount;
		}
		else
		{
			value = new PulledActorStats(m_CurrentTarget.ActorLocKey(), (m_CurrentTarget is CEnemyActor cEnemyActor) ? (" " + cEnemyActor.ID) : "", movedCount, new TileIndex(movedFrom), new TileIndex(movedTo));
		}
		m_ActorDistanceMovedDictionary[m_CurrentTarget] = value;
	}

	public override bool IsPositive()
	{
		if (m_State == EPullState.SelectTileToPullTowards && PullType == EPullType.PullTowardsAlly)
		{
			return true;
		}
		return false;
	}

	public override CAbilityOverride OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null, CActor targetActor = null, bool fromModifier = false)
	{
		int originalRange = m_OriginalRange;
		CAbilityOverride result = base.OverrideAbilityValues(abilityOverride, perform, item, filter, targetActor, fromModifier);
		if (originalRange != m_OriginalRange && TargetThisActorAutomatically == null)
		{
			SetTargets();
		}
		RefreshNumberOfTargets();
		return result;
	}

	public override void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		int originalRange = m_OriginalRange;
		base.UndoOverride(abilityOverride, perform, item);
		if (originalRange != m_OriginalRange && TargetThisActorAutomatically == null)
		{
			SetTargets();
		}
		RefreshNumberOfTargets();
	}

	public override bool IsCurrentlyTargetingActors()
	{
		if (State == EPullState.SelectTileToPullTowards)
		{
			return false;
		}
		if (State == EPullState.SelectPullTarget)
		{
			return true;
		}
		return base.IsCurrentlyTargetingActors();
	}
}
