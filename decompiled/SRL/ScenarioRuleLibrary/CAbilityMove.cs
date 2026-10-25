using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.Utils;
using Newtonsoft.Json;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Achievments;

namespace ScenarioRuleLibrary;

public class CAbilityMove : CAbility
{
	public enum EMoveState
	{
		None,
		PreMoveBuffTargeting,
		ApplyMoveBuff,
		CheckForAIToggleBonuses,
		ActorIsSelectingMoveTile,
		ActorIsMoving,
		ActorIsMovingB,
		ActorHasMoved,
		UnableToMove,
		CheckForPlayerIdle,
		MoveDone
	}

	public enum EMoveRestrictionType
	{
		None,
		StraightLineOnly,
		MustEndNextToObstacle,
		MustEndOnShadow,
		MustEndOnSpecificTiles,
		MustEndNextToEnemy
	}

	public static readonly EMoveState[] MoveStates = (EMoveState[])Enum.GetValues(typeof(EMoveState));

	public static readonly EMoveRestrictionType[] MoveRestrictionTypes = (EMoveRestrictionType[])Enum.GetValues(typeof(EMoveRestrictionType));

	public const string DEFAULT_MOVE_NAME = "DefaultMove";

	public List<Point> FixedPath;

	public bool DontApplyBonuses;

	public List<CNavLink> ShadowNavLinks = new List<CNavLink>();

	public static CActor s_LastMover;

	private static List<CActor> s_AllTargetActorsOnPath = new List<CActor>();

	private static List<Point> s_AllPlayerArrayIndexOnPath = new List<Point>();

	private static List<Point> s_AllPlayerArrayIndexOnPathIncludingRepeats = new List<Point>();

	private static List<Point> s_AllPlayerArrayIndexOnPathIncludingStart = new List<Point>();

	public static List<Point> s_AllArrayIndicesAlongPath = new List<Point>();

	private int m_InitialMoveCount;

	private int m_MoveCount;

	private int m_RemainingWaterTilesThatCostZero;

	public EMoveState m_State;

	private List<CTile> m_PlayerWaypoints;

	private List<CTile> m_AllSelectedPlayerWaypoints;

	[JsonProperty]
	private bool m_Jump;

	[JsonProperty]
	private bool m_Fly;

	[JsonProperty]
	private bool m_IgnoreDifficultTerrain;

	[JsonProperty]
	private bool m_IgnoreHazardousTerrain;

	[JsonProperty]
	private bool m_IgnoreBlockedTileMoveCost;

	[JsonProperty]
	private bool m_CarryOtherActorsOnHex;

	private int m_AlreadyMoved;

	private int m_AlreadyMovedWaterTiles;

	private List<Point> m_TilesMoved = new List<Point>();

	private List<Point> m_PlayerArrayIndexPath = new List<Point>();

	[JsonProperty]
	private bool m_IsDefaultMove;

	private bool m_AIFinishedMove;

	private CActor m_CurrentTarget;

	private Point m_StartingPoint;

	[StateDump(Skip = true)]
	[JsonIgnore]
	private Point m_OriginalStartingPoint;

	private bool m_MergedAbilityMoved;

	private CTile m_LastTileChecked;

	private List<CMap> m_RoomRevealsPendingChecks;

	private List<CActor> m_ActorsToCarry = new List<CActor>();

	private bool m_NewActorCarried;

	private bool m_MoveStartListenersInvoked;

	private Point m_PositionBeforeInterrupt;

	public int m_AlreadyMovedAtStart;

	private Point m_MoveFromPoint;

	private Point m_MoveToPoint;

	private List<Point> m_OutFloodedPoints = new List<Point>();

	private List<CObjectProp> m_PropsTriedToActivate = new List<CObjectProp>();

	private bool m_InterruptedByTeleport;

	public Point? m_FreeTileAwayFromCaster;

	[StateDump(Skip = true)]
	[JsonIgnore]
	public List<CActiveBonus> m_LockedActiveBonuses = new List<CActiveBonus>();

	public int MoveCount
	{
		get
		{
			return m_InitialMoveCount;
		}
		set
		{
			m_InitialMoveCount = value;
		}
	}

	public int MoveCountAfterLimit => Math.Min(MoveCount, GetMiscVar("MaxStrength").GetInt(null, MoveCount));

	public int RemainingMoves
	{
		get
		{
			return m_MoveCount;
		}
		set
		{
			m_MoveCount = value;
		}
	}

	public int RemainingMovesAfterLimit => Math.Min(RemainingMoves, GetMiscVar("MaxStrength").GetInt(null, RemainingMoves));

	public int RemainingWaterTilesThatCostZero => m_RemainingWaterTilesThatCostZero;

	public bool Jump
	{
		get
		{
			return m_Jump;
		}
		set
		{
			m_Jump = value;
		}
	}

	public bool Fly
	{
		get
		{
			return m_Fly;
		}
		set
		{
			m_Fly = value;
		}
	}

	public bool IgnoreDifficultTerrain
	{
		get
		{
			if (m_IgnoreDifficultTerrain)
			{
				return true;
			}
			return GetMiscVar("IgnoreDifficultTerrain").GetBool(VarsContext);
		}
		set
		{
			m_IgnoreDifficultTerrain = value;
		}
	}

	public bool IgnoreHazardousTerrain
	{
		get
		{
			return m_IgnoreHazardousTerrain;
		}
		set
		{
			m_IgnoreHazardousTerrain = value;
		}
	}

	public bool IgnoreBlockedTileMoveCost
	{
		get
		{
			return m_IgnoreBlockedTileMoveCost;
		}
		set
		{
			m_IgnoreBlockedTileMoveCost = value;
		}
	}

	public int WaterTilesThatCostZero => GetMiscVar("WaterTilesThatCostZero").GetInt();

	public bool CarryOtherActorsOnHex
	{
		get
		{
			return m_CarryOtherActorsOnHex;
		}
		set
		{
			m_CarryOtherActorsOnHex = value;
		}
	}

	public List<CActor> ActorsToCarry
	{
		get
		{
			return m_ActorsToCarry;
		}
		set
		{
			m_ActorsToCarry = value;
		}
	}

	[JsonProperty]
	public CAIFocusOverrideDetails AIFocusOverride { get; private set; }

	public bool IsDefaultMove => m_IsDefaultMove;

	public bool HasMoved => m_TilesMoved.Count > 0;

	public List<Point> TilesMoved => m_TilesMoved;

	public List<Point> PlayerArrayIndexPath => m_PlayerArrayIndexPath;

	public CActor CurrentMovingActor => m_CurrentTarget;

	public int? RestrictRange => MiscAbilityData?.RestrictMoveRange;

	public Point? RestrictPoint => ControllingActor?.ArrayIndex;

	public Point StartingPoint => m_StartingPoint;

	public CTile StartTile => ScenarioManager.GetTile(m_StartingPoint);

	public List<CTile> AllSelectedPlayerWaypoints => m_AllSelectedPlayerWaypoints;

	[JsonProperty]
	public EMoveRestrictionType MoveRestrictionType { get; set; }

	public List<Point> AllArrayIndexOnPathIncludingRepeatsCopy { get; set; }

	public int DoorsOpened { get; set; }

	public static List<CActor> AllTargetActorsOnPath => s_AllTargetActorsOnPath;

	public static List<Point> AllPlayerArrayIndexOnPath => s_AllPlayerArrayIndexOnPath;

	public static List<Point> AllPlayerArrayIndexOnPathIncludingRepeats => s_AllPlayerArrayIndexOnPathIncludingRepeats;

	public static List<Point> AllPlayerArrayIndexOnPathIncludingStart => s_AllPlayerArrayIndexOnPathIncludingStart;

	public bool MoveStartListenersInvoked
	{
		get
		{
			return m_MoveStartListenersInvoked;
		}
		set
		{
			m_MoveStartListenersInvoked = value;
		}
	}

	public CTile m_PlayerDestination { get; private set; }

	public List<Point> TilesMovedThisInstance { get; private set; } = new List<Point>();

	public CTile OriginalStartTile => ScenarioManager.GetTile(m_OriginalStartingPoint);

	public CAbilityMove(bool jump, bool fly, bool isDefaultMove, EMoveRestrictionType moveRestrictionType, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool ignoreBlockedTileMoveCost = false, bool carryOtherActorsOnHex = false, CAIFocusOverrideDetails aiFocusOverride = null)
	{
		MoveRestrictionType = moveRestrictionType;
		m_Jump = jump;
		m_Fly = fly;
		m_IgnoreDifficultTerrain = ignoreDifficultTerrain;
		m_IgnoreHazardousTerrain = ignoreHazardousTerrain;
		m_IgnoreBlockedTileMoveCost = ignoreBlockedTileMoveCost;
		m_CarryOtherActorsOnHex = carryOtherActorsOnHex;
		AIFocusOverride = aiFocusOverride;
		m_NewActorCarried = false;
		m_IsDefaultMove = isDefaultMove;
	}

	public static CAbility CreateDefaultMove(int moveAmount, bool isMonster, int range = 0, bool jump = false, bool fly = false, bool ignoreDifficultTerrain = false, bool ignoreHazardousTerrain = false, bool ignoreBlockedTileMoveCost = false, bool carryOtherActorsOnHex = false, bool isDefault = true, CAIFocusOverrideDetails focusOverrideDetails = null)
	{
		CAbility cAbility = CAbility.CreateAbility(EAbilityType.Move, moveAmount, useSpecialBaseStat: false, range, 0, 0, EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), string.Empty, null, null, null, attackSourcesOnly: false, jump, fly, ignoreDifficultTerrain, ignoreHazardousTerrain, ignoreBlockedTileMoveCost, carryOtherActorsOnHex, focusOverrideDetails, EMoveRestrictionType.None, null, null, "", "", "", "DefaultMove", new List<CEnhancement>(), null, null, multiPassAttack: true, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, isDefault: isDefault, statIsBasedOnXEntries: new List<AbilityData.StatIsBasedOnXData>(), moveObstacleRange: 0, moveObstacleAnimOverload: string.Empty, slotsToRefresh: new List<CItem.EItemSlot>(), slotStatesToRefresh: new List<CItem.EItemSlotState>(), addActiveBonusAbility: null, overrideAugmentAttackType: EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: new List<EAbilityType>(), cardAction: ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
		cAbility.SetMiscVar("PrintedProperties", new TValue(new List<object> { "Move" }));
		return cAbility;
	}

	public static List<Point> PathArrayIndices()
	{
		if (s_LastMover == null)
		{
			return null;
		}
		List<Point> list = ((s_LastMover.Type == CActor.EType.Player) ? s_AllPlayerArrayIndexOnPathIncludingRepeats : CActorStatic.s_LastCalculatedPathAI?.m_ArrayIndices);
		if (list == null)
		{
			list = new List<Point>();
		}
		return list;
	}

	public override bool CanBePerformed(CActor performer = null)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (TargetingActor == null)
			{
				return false;
			}
			m_OutFloodedPoints.Clear();
			ScenarioManager.PathFinder.FloodFill_ToRange(TargetingActor, TargetingActor.ArrayIndex, Strength, ignoreBlocked: false, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, WaterTilesThatCostZero, null, null, m_OutFloodedPoints, null, IgnoreDoorLocks);
			foreach (Point outFloodedPoint in m_OutFloodedPoints)
			{
				CActor.FindCharacterPath(TargetingActor, TargetingActor.ArrayIndex, outFloodedPoint, m_Fly || m_Jump, ignoreMoveCost: false, out var foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, WaterTilesThatCostZero, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, m_Jump, m_Fly);
				if (foundPath)
				{
					return true;
				}
			}
			if (ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.ShadowBuff))
			{
				return true;
			}
			if (ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.BuffMove))
			{
				return true;
			}
			return false;
		}
	}

	public override void Start(CActor targetActor, CActor filterActor, CActor controllingActor = null)
	{
		if (ActiveBonusData.HasBehaviour(CActiveBonus.EActiveBonusBehaviourType.MoveScript))
		{
			base.Start(targetActor, filterActor, controllingActor);
			return;
		}
		CAbilityMove cAbilityMove = GetMiscVar("ResumedFromAbility").GetObject<CAbilityMove>();
		SetMiscVar("ResumedFromAbility", null);
		int strength = Strength;
		targetActor.AIMoveFocusActors?.Clear();
		targetActor.AIMoveFocusWaypoints?.Clear();
		targetActor.AIMoveFocusPath = new List<Point>();
		ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == targetActor.ActorGuid);
		if (AIFocusOverride != null)
		{
			actorState?.SetAIFocusOverride(AIFocusOverride);
		}
		else if (actorState != null && actorState.AIFocusOverride?.IsTemporary == true)
		{
			actorState.ResetAIFocusOverride();
		}
		base.Start(targetActor, filterActor, controllingActor);
		if (controllingActor != null && !NoBonuses && controllingActor != TargetingActor)
		{
			foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(controllingActor, EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffForcedMovement))
			{
				m_ModifiedStrength += item.ReferenceStrength(this, TargetingActor);
			}
		}
		m_State = ((TargetingActor.Type != CActor.EType.Player) ? EMoveState.CheckForAIToggleBonuses : EMoveState.ActorIsSelectingMoveTile);
		m_CurrentTarget = TargetingActor;
		m_IgnoreDifficultTerrain |= TargetingActor.IgnoreDifficultTerrain || (TargetingActor.MonsterClass?.IsWaterMonster ?? false);
		m_IgnoreHazardousTerrain |= TargetingActor.IgnoreHazardousTerrain;
		GetBonuses();
		if (TilesMoved.Count == 0)
		{
			TargetingActor.HasFreeTileAwayFromCaster(out m_FreeTileAwayFromCaster);
		}
		m_InitialMoveCount = ((FixedPath != null) ? FixedPath.Count : m_ModifiedStrength);
		m_MoveCount = m_InitialMoveCount;
		if (cAbilityMove != null)
		{
			m_StartingPoint = cAbilityMove.m_StartingPoint;
			m_OriginalStartingPoint = cAbilityMove.m_OriginalStartingPoint;
			SetMiscVar("ResumedAtPosition", new TValue(targetActor.Tile));
			AbilityHasHappened = TilesMoved.Count > 0;
			int num = (m_ModifiedStrength = strength);
			int strength2 = (RemainingMoves = num);
			Strength = strength2;
		}
		else
		{
			m_StartingPoint = targetActor.ArrayIndex;
			m_OriginalStartingPoint = m_StartingPoint;
		}
		m_AlreadyMoved = 0;
		m_AlreadyMovedWaterTiles = 0;
		m_RemainingWaterTilesThatCostZero = WaterTilesThatCostZero;
		if (cAbilityMove == null)
		{
			m_TilesMoved.Clear();
		}
		m_PlayerDestination = null;
		if (ActiveBonusData != null && ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			m_State = EMoveState.PreMoveBuffTargeting;
			m_ActorsToTarget.Clear();
			m_ActorsToTarget.Add(TargetingActor);
			m_ValidActorsInRange = new List<CActor>();
			m_ValidActorsInRange.Add(TargetingActor);
		}
		else
		{
			s_LastMover = null;
			s_AllTargetActorsOnPath.Clear();
			s_AllPlayerArrayIndexOnPath.Clear();
			s_AllPlayerArrayIndexOnPathIncludingStart.Clear();
			s_AllPlayerArrayIndexOnPathIncludingStart.Add(targetActor.ArrayIndex);
			s_AllPlayerArrayIndexOnPathIncludingRepeats.Clear();
			if (cAbilityMove != null)
			{
				s_AllPlayerArrayIndexOnPathIncludingRepeats.AddRange(m_TilesMoved);
			}
			s_AllArrayIndicesAlongPath.Clear();
			m_RoomRevealsPendingChecks = new List<CMap>();
			if (MoveCount <= 0 && (UseSubAbilityTargeting || TargetingActor.Type != CActor.EType.Player))
			{
				m_State = EMoveState.ActorHasMoved;
			}
		}
		m_PlayerWaypoints = null;
		if (!MoveStartListenersInvoked)
		{
			MoveStartListenersInvoked = true;
			if (targetActor.m_OnMoveStartListeners != null)
			{
				targetActor.m_OnMoveStartListeners?.Invoke(this);
			}
		}
		if (TargetingActor.Tile.LivingActors.Count((CActor x) => !x.Deactivated && x.IsTransientBlocker) > 1 || (!TargetingActor.FlyingAfterConditions && TargetingActor.Tile.m_Props.Any((CObjectProp p) => p is CObjectObstacle cObjectObstacle && (cObjectObstacle.RuntimeAttachedActor == null || cObjectObstacle.RuntimeAttachedActor.IsTransientBlocker))))
		{
			SetCanSkip(canSkip: false);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
		if (cAbilityMove != null && MoveRestrictionType.Equals(EMoveRestrictionType.MustEndOnShadow) && !GameState.GetTilesInRange(TargetingActor, RemainingMovesAfterLimit, EAbilityTargeting.Range, emptyTilesOnly: false, Jump || Fly, null, ignorePathLength: false, ignoreBlockedWithActor: false, ignoreLOS: false, emptyOpenDoorTiles: false, ignoreMoveCost: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, WaterTilesThatCostZero, TargetingActor is CPlayerActor, includeTargetPosition: false, skipPathingCheck: false, includeStartingTile: true).Any((CTile x) => x.FindPropOfType<CObjectShadow>() != null))
		{
			SetCancelAbilityFlag(cancel: true);
		}
	}

	public override void Restart()
	{
		base.Restart();
		CRefreshFirstAbility_MessageData cRefreshFirstAbility_MessageData = new CRefreshFirstAbility_MessageData(TargetingActor);
		cRefreshFirstAbility_MessageData.m_IsFirstAbility = CPhaseAction.IsFirstAbility(this);
		ScenarioRuleClient.MessageHandler(cRefreshFirstAbility_MessageData);
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		if (GetMiscVar("ResumedFromAbility").GetObject<CAbilityMove>() == null)
		{
			m_ModifiedStrength = Strength;
			GetBonuses();
			m_InitialMoveCount = m_ModifiedStrength;
			m_MoveCount = m_InitialMoveCount - m_AlreadyMoved;
			m_RemainingWaterTilesThatCostZero = WaterTilesThatCostZero - m_AlreadyMovedWaterTiles;
		}
		else
		{
			SetMiscVar("ResumedFromAbility", null);
		}
		if (MoveCountAfterLimit <= 0 && HasMoved)
		{
			m_State = EMoveState.ActorHasMoved;
		}
		else if (TargetingActor.m_OnMoveStartListeners != null)
		{
			TargetingActor.m_OnMoveStartListeners(this);
		}
	}

	public override void Undo()
	{
		base.Undo();
		ClearShadowNavLinks();
	}

	public override bool CanApplyActiveBonusTogglesTo()
	{
		if (NoBonuses)
		{
			return false;
		}
		if (ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			return false;
		}
		return !DontApplyBonuses;
	}

	public void GetBonuses(CActor actor = null)
	{
		if (!CanApplyActiveBonusTogglesTo())
		{
			return;
		}
		if (Original != null && Original is CAbilityMove { Jump: not false })
		{
			m_Jump = true;
		}
		if (actor == null)
		{
			actor = TargetingActor;
		}
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.Move);
		if (list != null)
		{
			foreach (CActiveBonus item in list.Except(m_LockedActiveBonuses))
			{
				m_ModifiedStrength += item.ReferenceStrength(this, actor);
				if (item is CMoveActiveBonus cMoveActiveBonus && cMoveActiveBonus.TryGetBehaviour(out CMoveActiveBonus_BuffMove behaviour))
				{
					if (behaviour.ReferenceJump(this, actor).HasValue)
					{
						m_Jump = behaviour.ReferenceJump(this, actor).Value;
					}
					if (behaviour.ReferenceFly(this, actor).HasValue)
					{
						m_Fly = behaviour.ReferenceFly(this, actor).Value;
					}
					if (behaviour.ReferenceIgnoreDifficultTerrain(this, actor).HasValue)
					{
						m_IgnoreDifficultTerrain = behaviour.ReferenceIgnoreDifficultTerrain(this, actor).Value;
					}
					if (behaviour.ReferenceIgnoreHazardousTerrain(this, actor).HasValue)
					{
						m_IgnoreHazardousTerrain = behaviour.ReferenceIgnoreHazardousTerrain(this, actor).Value;
					}
				}
			}
			foreach (CActiveBonus item2 in list)
			{
				m_ModifiedStrength *= item2.ReferenceStrengthScalar(this, actor);
			}
			foreach (CActiveBonus item3 in list)
			{
				int num = item3.Ability.GetMiscVar("MaxMovement").GetInt(null, -1);
				if (num != -1)
				{
					m_ModifiedStrength = Math.Min(m_ModifiedStrength, num);
				}
			}
		}
		CAbilityAttack cAbilityAttack = null;
		if (actor.Type == CActor.EType.HeroSummon && IsDefaultMove)
		{
			CAbility cAbility = CAbilityAttack.CreateDefaultAttack(1, Range, 1, isMonster: false);
			((CAbilityAttack)cAbility).ProcessSongOverridesAndAbilities(actor, CSong.ESongActivationType.AbilityStart, temporaryOverrides: true);
			Range = ((CAbilityAttack)cAbility).Range;
			cAbilityAttack = cAbility as CAbilityAttack;
		}
		else if (actor is CPlayerActor && actor.IsMonsterType)
		{
			foreach (CPhaseAction.CPhaseAbility remainingPhaseAbility in (PhaseManager.CurrentPhase as CPhaseAction).RemainingPhaseAbilities)
			{
				if (remainingPhaseAbility.m_Ability.AbilityType == EAbilityType.Attack)
				{
					Range = remainingPhaseAbility.m_Ability.Range;
					cAbilityAttack = remainingPhaseAbility.m_Ability as CAbilityAttack;
					break;
				}
			}
		}
		if (cAbilityAttack != null)
		{
			CActor targetingActor = cAbilityAttack.TargetingActor;
			cAbilityAttack.TargetingActor = TargetingActor;
			CAbilityAttack.GetAttackBonuses(TargetingActor, cAbilityAttack, out var _, out var range, out var _, out var _, out var _);
			Range += range;
			cAbilityAttack.TargetingActor = targetingActor;
		}
		if (actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.HeroSummonClass.SummonYML.TreatAsTrap)
		{
			m_Fly = true;
		}
		if (actor.Tokens.HasKey(CCondition.ENegativeCondition.StopFlying))
		{
			m_Fly = false;
		}
	}

	public override void ActiveBonusToggled(CActor actor, CActiveBonus activeBonus)
	{
		if (activeBonus.ShouldRestartAbilityOnToggle(this))
		{
			Restart();
		}
	}

	public static void GetMoveBonuses(CActor actor, out bool jump, out bool fly, out bool ignoreDifficultTerrain, out bool ignoreHazardousTerrain, ref int strength)
	{
		CAbilityMove ability = (CAbilityMove)CreateDefaultMove(1, isMonster: false);
		jump = false;
		fly = actor.FlyingAfterConditions;
		ignoreDifficultTerrain = false;
		ignoreHazardousTerrain = false;
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, EAbilityType.Move);
		if (list == null)
		{
			return;
		}
		foreach (CActiveBonus item in list)
		{
			strength += item.ReferenceStrength(ability, actor);
			if (item is CMoveActiveBonus cMoveActiveBonus && cMoveActiveBonus.TryGetBehaviour(out CMoveActiveBonus_BuffMove behaviour))
			{
				if (behaviour.ReferenceJump(ability, actor).HasValue)
				{
					jump = behaviour.ReferenceJump(ability, actor).Value;
				}
				if (behaviour.ReferenceFly(ability, actor).HasValue)
				{
					fly = behaviour.ReferenceFly(ability, actor).Value;
				}
				if (behaviour.ReferenceIgnoreDifficultTerrain(ability, actor).HasValue)
				{
					ignoreDifficultTerrain = behaviour.ReferenceIgnoreDifficultTerrain(ability, actor).Value;
				}
				if (behaviour.ReferenceIgnoreHazardousTerrain(ability, actor).HasValue)
				{
					ignoreHazardousTerrain = behaviour.ReferenceIgnoreHazardousTerrain(ability, actor).Value;
				}
			}
		}
		foreach (CActiveBonus item2 in list)
		{
			strength *= item2.ReferenceStrengthScalar(ability, actor);
		}
	}

	private bool AbortIfMoverKilled()
	{
		if (!ScenarioManager.Scenario.HasActor(m_CurrentTarget))
		{
			PhaseManager.NextStep();
			return true;
		}
		return false;
	}

	public override CAbility CreateInterruptCopy(CActor targetingActor, bool killAfter, bool forced, bool interruptRegardlessOfStrength = false)
	{
		if (AbilityInterruptCopy != null)
		{
			return null;
		}
		int remainingMovesAfterLimit = RemainingMovesAfterLimit;
		CActor targetingActor2 = TargetingActor;
		CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
		LocoStop(cancelAbilityOnStop: true);
		TargetingActor = targetingActor2;
		if ((!m_AIFinishedMove && remainingMovesAfterLimit > 0) | interruptRegardlessOfStrength)
		{
			CAbility cAbility = CAbility.CopyAbility(this, generateNewID: false);
			CopyInterruptedProperties(cAbility, remainingMovesAfterLimit, initialCopy: true, forced);
			PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities.Remove(currentPhaseAbility);
			return cAbility;
		}
		return null;
	}

	public override void ForceInterrupt(bool fromTeleport = false)
	{
		m_StartingPoint = TargetingActor.ArrayIndex;
		base.ForceInterrupt(fromTeleport);
		if (fromTeleport)
		{
			m_InterruptedByTeleport = true;
			SetCancelAbilityFlag(cancel: true);
		}
	}

	public override void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		base.CopyInterruptedProperties(destAbility, strength, initialCopy, forced);
		CAbilityMove cAbilityMove = (CAbilityMove)destAbility;
		cAbilityMove.m_OriginalStartingPoint = m_OriginalStartingPoint;
		cAbilityMove.MiscAbilityData = ((MiscAbilityData != null) ? MiscAbilityData.Copy() : null);
		cAbilityMove.DontApplyBonuses = true;
		cAbilityMove.m_InitialMoveCount = m_InitialMoveCount;
		cAbilityMove.m_MoveCount = m_MoveCount;
		cAbilityMove.m_RemainingWaterTilesThatCostZero = m_RemainingWaterTilesThatCostZero;
		cAbilityMove.m_State = m_State;
		cAbilityMove.m_PlayerDestination = m_PlayerDestination;
		cAbilityMove.m_PlayerWaypoints = m_PlayerWaypoints?.ToList();
		cAbilityMove.m_AllSelectedPlayerWaypoints = m_AllSelectedPlayerWaypoints?.ToList();
		cAbilityMove.m_Jump = m_Jump;
		cAbilityMove.m_Fly = m_Fly;
		cAbilityMove.m_IgnoreDifficultTerrain = m_IgnoreDifficultTerrain;
		cAbilityMove.m_IgnoreHazardousTerrain = m_IgnoreHazardousTerrain;
		cAbilityMove.m_IgnoreBlockedTileMoveCost = m_IgnoreBlockedTileMoveCost;
		cAbilityMove.m_CarryOtherActorsOnHex = m_CarryOtherActorsOnHex;
		cAbilityMove.m_AlreadyMoved = m_AlreadyMoved;
		cAbilityMove.m_AlreadyMovedWaterTiles = m_AlreadyMovedWaterTiles;
		cAbilityMove.m_TilesMoved = m_TilesMoved?.ToList();
		cAbilityMove.m_PlayerArrayIndexPath = m_PlayerArrayIndexPath?.ToList();
		cAbilityMove.m_IsDefaultMove = m_IsDefaultMove;
		cAbilityMove.m_AIFinishedMove = m_AIFinishedMove;
		cAbilityMove.m_CurrentTarget = m_CurrentTarget;
		cAbilityMove.m_StartingPoint = m_StartingPoint;
		cAbilityMove.m_MergedAbilityMoved = m_MergedAbilityMoved;
		cAbilityMove.m_LastTileChecked = m_LastTileChecked;
		cAbilityMove.m_RoomRevealsPendingChecks = m_RoomRevealsPendingChecks?.ToList();
		cAbilityMove.m_ActorsToCarry = m_ActorsToCarry?.ToList();
		cAbilityMove.m_NewActorCarried = m_NewActorCarried;
		cAbilityMove.m_MoveStartListenersInvoked = m_MoveStartListenersInvoked;
		cAbilityMove.m_ActorsToTarget = m_ActorsToTarget?.ToList();
		cAbilityMove.m_CanUndo = m_CanUndo;
		cAbilityMove.m_CanSkip = m_CanSkip;
		if (initialCopy)
		{
			cAbilityMove.m_PositionBeforeInterrupt = TargetingActor.ArrayIndex;
			cAbilityMove.m_AlreadyMovedAtStart = m_AlreadyMoved;
		}
		CacheTilesMovedThisInstance(cAbilityMove);
		if (s_AllPlayerArrayIndexOnPathIncludingRepeats != null && s_AllPlayerArrayIndexOnPathIncludingRepeats.Count > 0)
		{
			cAbilityMove.SetMiscVar("WaypointsSoFar", new TValue(s_AllPlayerArrayIndexOnPathIncludingRepeats.ToList()));
		}
	}

	private void CacheTilesMovedThisInstance(CAbilityMove moveAbility)
	{
		if (TilesMoved != null)
		{
			moveAbility.TilesMovedThisInstance = new List<Point>();
			for (int i = m_AlreadyMovedAtStart; i < TilesMoved.Count; i++)
			{
				moveAbility.TilesMovedThisInstance.Add(TilesMoved[i]);
			}
			if (moveAbility.TilesMovedThisInstance.Count == 0 || moveAbility.TilesMovedThisInstance.Last() != TargetingActor.ArrayIndex)
			{
				moveAbility.TilesMovedThisInstance.Add(TargetingActor.ArrayIndex);
				moveAbility.m_AlreadyMovedAtStart++;
			}
		}
	}

	public void CreateResumeCopy(Action<CAbilityMove> modifyAction = null, bool locoStop = true)
	{
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		CAbilityMove cAbilityMove = CAbility.CopyAbility(this, generateNewID: false, fullCopy: false, copyCurrentOverrides: true) as CAbilityMove;
		if (cAbilityMove.MiscAbilityData == null)
		{
			cAbilityMove.MiscAbilityData = new AbilityData.MiscAbilityData();
		}
		cAbilityMove.ParentAbility = ParentAbility;
		cAbilityMove.StatIsBasedOnXEntries.Clear();
		cAbilityMove.Strength = RemainingMovesAfterLimit;
		cAbilityMove.WasInterrupted = false;
		cAbilityMove.WasForcedInterrupted = false;
		cAbilityMove.TilesMoved.Clear();
		cAbilityMove.AbilityHasHappened = AbilityHasHappened;
		cAbilityMove.m_LockedActiveBonuses = (from x in TargetingActor.FindApplicableActiveBonuses(EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffMove)
			where x.ToggledBonus || !x.Ability.ActiveBonusData.IsToggleBonus
			select x).Concat(m_LockedActiveBonuses).Distinct().ToList();
		if (TilesMoved != null)
		{
			cAbilityMove.TilesMoved.AddRange(TilesMoved);
			cAbilityMove.AbilityHasHappened |= TilesMoved.Count > 0;
		}
		cAbilityMove.MiscAbilityData.CanSkip = m_CanSkip;
		cAbilityMove.SetMiscVar("ResumedFromAbility", new TValue(this));
		modifyAction?.Invoke(cAbilityMove);
		if (cAbilityMove.Strength > 0 || cAbilityMove.GetMiscVar("ResumeRegardessOfStrength").GetBool())
		{
			AbilityInterruptCopy = cAbilityMove;
			cAbilityMove.SetMiscVar("ResumeRegardessOfStrength", false);
			PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbilityMove }, TargetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true, interruptCurrent: false, addDirectlyToCurrentAbilities: false, forcedInterrupt: false, interruptRegardlessOfStrength: false, dontCreateCopies: true);
			foreach (CPhaseAction.CPhaseAbility item in from x in PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities.Concat(PhaseManager.CurrentPhaseAction.RemainingPhaseAbilities)
				where x.m_Ability.ParentAbility != null && x.m_Ability.ParentAbility.ID == ID
				select x)
			{
				item.m_Ability.ParentAbility = cAbilityMove;
			}
		}
		if ((m_State != EMoveState.MoveDone) & locoStop)
		{
			LocoStop(cancelAbilityOnStop: true, nextStep: true);
		}
	}

	public void ClearShadowNavLinks()
	{
		if (ShadowNavLinks.Count <= 0)
		{
			return;
		}
		foreach (CObjectProp shadowProp in ScenarioManager.CurrentScenarioState.ShadowProps)
		{
			ScenarioManager.PathFinder.GetNode(shadowProp.PropTile.m_ArrayIndex).NavLinks.RemoveRange(ShadowNavLinks);
			shadowProp.PropTile.m_Props.Find((CObjectProp p) => p.PrefabName == "ShadowPortal")?.DestroyProp(0f, sendMessageToClient: true, forceDestroy: true, this);
		}
		ShadowNavLinks.Clear();
	}

	public void RefreshShadowNavLinks()
	{
		ClearShadowNavLinks();
		if (CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.ShadowBuff).Count <= 0)
		{
			return;
		}
		ScenarioScript current = ScenarioScript.current;
		foreach (CObjectProp shadowProp in ScenarioManager.CurrentScenarioState.ShadowProps)
		{
			CNode node = ScenarioManager.PathFinder.GetNode(shadowProp.PropTile.m_ArrayIndex);
			CNavLink item = new CNavLink(1, node);
			foreach (CObjectProp shadowProp2 in ScenarioManager.CurrentScenarioState.ShadowProps)
			{
				if ((shadowProp.PropTile.m_HexMap == shadowProp2.PropTile.m_HexMap || shadowProp2.PropTile.m_HexMap.CanTeleportToFromAnotherMap) && (current == null || ((current.NoTeleportMapGuids.Contains(shadowProp.PropTile.m_HexMap.MapGuid) || !current.NoTeleportMapGuids.Contains(shadowProp2.PropTile.m_HexMap.MapGuid)) && (current.NoTeleportMapGuids.Contains(shadowProp2.PropTile.m_HexMap.MapGuid) || !current.NoTeleportMapGuids.Contains(shadowProp.PropTile.m_HexMap.MapGuid)))) && shadowProp2 != shadowProp)
				{
					CNode node2 = ScenarioManager.PathFinder.GetNode(shadowProp2.PropTile.m_ArrayIndex);
					CNavLink item2 = new CNavLink(1, node2);
					if (!node.NavLinks.Contains(item2))
					{
						node.NavLinks.Add(item2);
						ShadowNavLinks.Add(item2);
					}
					if (!node2.NavLinks.Contains(item))
					{
						node2.NavLinks.Add(item);
						ShadowNavLinks.Add(item);
					}
				}
			}
			CObjectProp prop = new CObjectProp("ShadowPortal", ScenarioManager.ObjectImportType.VFXProp, shadowProp.ArrayIndex, null, null, null, shadowProp.PropTile.m_HexMap.MapGuid);
			shadowProp.PropTile.SpawnProp(prop, notifyClient: true, 0f, this);
		}
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		LogEvent(ESESubTypeAbility.AbilityPerform);
		if (ShowCancelledHelpText(passing: true))
		{
			ClearShadowNavLinks();
			return true;
		}
		if (CAbility.ImmuneToAbility(TargetingActor, this))
		{
			if (TargetingActor.Type == CActor.EType.Player)
			{
				CPlayerCannotUseMoveAbilities_MessageData message = new CPlayerCannotUseMoveAbilities_MessageData(TargetingActor);
				ScenarioRuleClient.MessageHandler(message);
				return false;
			}
			PhaseManager.NextStep();
			return false;
		}
		if (m_CurrentTarget != null)
		{
			if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize))
			{
				AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
				if ((miscAbilityData == null || miscAbilityData.IgnoreImmobilize != true) && ActiveBonusData != null && ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
				{
					goto IL_0121;
				}
			}
			if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				AbilityData.MiscAbilityData miscAbilityData2 = MiscAbilityData;
				if (miscAbilityData2 == null || miscAbilityData2.IgnoreStun != true)
				{
					goto IL_0121;
				}
			}
			if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				goto IL_0121;
			}
		}
		s_LastMover = TargetingActor;
		if (WasInterrupted && (WasForcedInterrupted || m_PositionBeforeInterrupt != default(Point)))
		{
			if (WasForcedInterrupted || TargetingActor.ArrayIndex != m_PositionBeforeInterrupt)
			{
				CreateResumeCopy();
				return true;
			}
			m_PositionBeforeInterrupt = default;
		}
		if (WasInterrupted && !HasResumedAfterInterrupt && TargetingActor.Type == CActor.EType.Player)
		{
			HasResumedAfterInterrupt = true;
			List<Point> list = GetMiscVar("WaypointsSoFar").GetObject<List<Point>>();
			if (list != null)
			{
				s_AllPlayerArrayIndexOnPathIncludingRepeats.Clear();
				s_AllPlayerArrayIndexOnPathIncludingRepeats.AddRange(list);
			}
			CRecreateWaypoints_MessageData message2 = new CRecreateWaypoints_MessageData(this);
			ScenarioRuleClient.MessageHandler(message2);
			if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
			{
				List<CAbility> subAbilitiesToReQueue = new List<CAbility>();
				foreach (CAbility subAbility in SubAbilities)
				{
					subAbility.ParentAbility = this;
					subAbilitiesToReQueue.Add(subAbility);
				}
				PhaseManager.CurrentPhaseAction.RemainingPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility x) => subAbilitiesToReQueue.Any((CAbility y) => y.ID == x.m_Ability.ID));
				PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities.RemoveAll((CPhaseAction.CPhaseAbility x) => subAbilitiesToReQueue.Any((CAbility y) => y.ID == x.m_Ability.ID));
				cPhaseAction.StackNextAbilities(subAbilitiesToReQueue, TargetingActor, killAfter: false, stackToNextCurrent: true);
			}
			return true;
		}
		Point arrayIndex;
		switch (m_State)
		{
		case EMoveState.PreMoveBuffTargeting:
		{
			if (TargetingActor.Type != CActor.EType.Player || (MiscAbilityData.AutotriggerAbility.HasValue && MiscAbilityData.AutotriggerAbility.Value))
			{
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingTargetingFocus_MessageData cActorIsSelectingTargetingFocus_MessageData = new CActorIsSelectingTargetingFocus_MessageData(TargetingActor);
			cActorIsSelectingTargetingFocus_MessageData.m_TargetingAbility = this;
			cActorIsSelectingTargetingFocus_MessageData.m_IsPositive = true;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingTargetingFocus_MessageData);
			break;
		}
		case EMoveState.ApplyMoveBuff:
		{
			ScenarioRuleClient.FirstAbilityStarted();
			AbilityHasHappened = true;
			foreach (CActor item in m_ActorsToTarget)
			{
				ApplyToActor(item);
			}
			CMoveBuff_MessageData message5 = new CMoveBuff_MessageData(AnimOverload, TargetingActor)
			{
				m_MoveAbility = this
			};
			ScenarioRuleClient.MessageHandler(message5);
			if (AnimOverload.Contains("None") || SkipAnim)
			{
				PhaseManager.NextStep();
			}
			return true;
		}
		case EMoveState.CheckForAIToggleBonuses:
		{
			bool flag = false;
			List<CActiveBonus> list4 = CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Move);
			if (list4 != null)
			{
				foreach (CActiveBonus item2 in list4)
				{
					flag = item2.Ability.ActiveBonusData.IsToggleBonus && item2.CanToggleActiveBonus(TargetingActor);
				}
			}
			if (flag)
			{
				ShowToggleBonusesForAI();
				break;
			}
			m_State = EMoveState.ActorIsSelectingMoveTile;
			Perform();
			break;
		}
		case EMoveState.ActorIsSelectingMoveTile:
		{
			if (TargetingActor.Type == CActor.EType.Player && m_PlayerDestination != null)
			{
				if (m_PlayerArrayIndexPath.Count == 0)
				{
					if (MoveRestrictionType == EMoveRestrictionType.StraightLineOnly)
					{
						for (int num = 1; num < 7; num++)
						{
							List<CTile> tilesInLine = ScenarioManager.GetTilesInLine(m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y, RemainingMovesAfterLimit, (ScenarioManager.EAdjacentPosition)num);
							if (!tilesInLine.Contains(m_PlayerWaypoints.Last()))
							{
								continue;
							}
							m_PlayerArrayIndexPath.Clear();
							for (int num2 = 0; num2 < tilesInLine.Count; num2++)
							{
								m_PlayerArrayIndexPath.Add(tilesInLine[num2].m_ArrayIndex);
								if (tilesInLine[num2] == m_PlayerWaypoints.Last())
								{
									m_PlayerDestination = tilesInLine[num2];
									break;
								}
							}
							break;
						}
					}
					else
					{
						bool jump = m_Jump;
						bool fly = m_Fly;
						if (CarryOtherActorsOnHex && m_ActorsToCarry.Count == 0)
						{
							CTile tile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
							m_ActorsToCarry = GameState.GetActorsOnTile(tile, FilterActor, CAbilityFilterContainer.CreateDefaultFilter(), ActorsToIgnore, isTargetedAbility: false, false);
							m_ActorsToCarry.Remove(m_CurrentTarget);
							if (m_ActorsToCarry.Count > 0 && m_StartingPoint != m_ActorsToCarry[0].ArrayIndex && !HasMoved)
							{
								m_NewActorCarried = true;
							}
						}
						CActor.EType carryType = CActor.EType.Unknown;
						if (ActorsToCarry.Count > 0)
						{
							int strength = 0;
							carryType = ActorsToCarry[0].Type;
							GetMoveBonuses(ActorsToCarry[0], out jump, out fly, out var _, out var _, ref strength);
						}
						Point nextPathingPosition = GetNextPathingPosition(m_CurrentTarget, m_PlayerDestination, jump, fly, RemainingMovesAfterLimit, out m_PlayerWaypoints, out m_PlayerArrayIndexPath, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, m_IgnoreBlockedTileMoveCost, RemainingWaterTilesThatCostZero, CarryOtherActorsOnHex, carryType);
						foreach (CActor item3 in m_ActorsToCarry)
						{
							item3.ArrayIndex = nextPathingPosition;
						}
						ActorsToCarry = ActorsToCarry.Distinct().ToList();
					}
					foreach (Point item4 in m_PlayerArrayIndexPath)
					{
						if (!s_AllPlayerArrayIndexOnPath.Contains(item4))
						{
							s_AllPlayerArrayIndexOnPath.Add(item4);
							s_AllPlayerArrayIndexOnPathIncludingStart.Add(item4);
						}
						s_AllPlayerArrayIndexOnPathIncludingRepeats.Add(item4);
					}
				}
				if (m_PlayerArrayIndexPath.Count > 0)
				{
					m_MoveFromPoint = m_CurrentTarget.ArrayIndex;
					m_MoveToPoint = m_PlayerArrayIndexPath[0];
					m_CurrentTarget.ArrayIndex = m_MoveToPoint;
					m_PlayerArrayIndexPath.RemoveAt(0);
					CObjectProp cObjectProp2 = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y].FindProp(ScenarioManager.ObjectImportType.Door);
					if (cObjectProp2 != null && cObjectProp2 is CObjectDoor cObjectDoor2)
					{
						if (!cObjectDoor2.Activated)
						{
							DoorsOpened++;
						}
						cObjectDoor2.SetDoorOpenedByMovingActor(TargetingActor);
						if (cObjectDoor2.RoomsRevealedInLastOpening != null)
						{
							m_RoomRevealsPendingChecks.AddRange(cObjectDoor2.RoomsRevealedInLastOpening);
						}
					}
					if (AbilityInterruptCopy != null && AbilityHasBeenCancelled)
					{
						SetCancelAbilityFlag(cancel: false);
					}
				}
				m_State = EMoveState.ActorIsMoving;
				Perform();
				break;
			}
			RefreshShadowNavLinks();
			if (m_IsMergedAbility && HasMoved && m_AllSelectedPlayerWaypoints.Count <= 0 && m_MergedAbilityMoved)
			{
				m_State = EMoveState.ActorHasMoved;
				m_MergedAbilityMoved = false;
				PhaseManager.StepComplete();
				break;
			}
			CActorIsSelectingMoveTile_MessageData cActorIsSelectingMoveTile_MessageData = new CActorIsSelectingMoveTile_MessageData(TargetingActor);
			cActorIsSelectingMoveTile_MessageData.m_MoveAbility = this;
			ScenarioRuleClient.MessageHandler(cActorIsSelectingMoveTile_MessageData);
			Log.Info("About to highlight items usable during the movement phase");
			TargetingActor.Inventory.HighlightUsableItems(this, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
			if (TargetingActor.Type != CActor.EType.Player)
			{
				m_State = EMoveState.ActorIsMoving;
				Perform();
			}
			break;
		}
		case EMoveState.ActorIsMoving:
			if (m_CurrentTarget.Equals(TargetingActor) && m_CurrentTarget.Type != CActor.EType.Player && m_CurrentTarget is CHeroSummonActor cHeroSummonActor2 && !m_CurrentTarget.IsDead)
			{
				m_CurrentTarget.Move(0, m_Jump, m_Fly, Range, allowMove: false, m_IgnoreDifficultTerrain, ignoreHazardousTerrain: false, 0, m_FreeTileAwayFromCaster, null, firstMove: false, moveTest: true, carryOtherActors: false);
				if (cHeroSummonActor2.AIMoveFocusActors.Count == 0)
				{
					cHeroSummonActor2.ReturnToSummoner = null;
					CReturnToSummoner_MessageData message6 = new CReturnToSummoner_MessageData(cHeroSummonActor2, cHeroSummonActor2.Summoner);
					ScenarioRuleClient.MessageHandler(message6);
					return false;
				}
			}
			m_State = EMoveState.ActorIsMovingB;
			Perform();
			break;
		case EMoveState.ActorIsMovingB:
		{
			if (TargetingActor.m_OnMovingListeners != null)
			{
				TargetingActor.m_OnMovingListeners?.Invoke(this);
			}
			if (m_CurrentTarget.Equals(TargetingActor) && m_CurrentTarget.Type != CActor.EType.Player)
			{
				arrayIndex = m_CurrentTarget.ArrayIndex;
				List<Point> fixedPath = FixedPath;
				if (fixedPath != null)
				{
					if (fixedPath.Count > 0)
					{
						m_CurrentTarget.AIMoveFocusWaypoints = new List<CTile>();
						Point lastPoint = fixedPath[0];
						Point point = new Point(lastPoint.X - m_CurrentTarget.ArrayIndex.X, lastPoint.Y - m_CurrentTarget.ArrayIndex.Y);
						for (int num7 = 1; num7 < fixedPath.Count; num7++)
						{
							Point point2 = fixedPath[num7];
							Point point3 = new Point(point2.X - lastPoint.X, point2.Y - lastPoint.Y);
							if (point3 != point)
							{
								m_CurrentTarget.AIMoveFocusWaypoints.Add(ScenarioManager.GetTile(lastPoint));
							}
							lastPoint = point2;
							point = point3;
						}
						if (!m_CurrentTarget.AIMoveFocusWaypoints.Any((CTile x) => x.m_ArrayIndex == lastPoint))
						{
							m_CurrentTarget.AIMoveFocusWaypoints.Add(ScenarioManager.GetTile(lastPoint));
						}
						Point arrayIndex2 = fixedPath[0];
						m_CurrentTarget.ArrayIndex = arrayIndex2;
						fixedPath.RemoveAt(0);
					}
				}
				else
				{
					m_CurrentTarget.Move(m_MoveCount, m_Jump, m_Fly, Range, allowMove: true, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, m_RemainingWaterTilesThatCostZero, m_FreeTileAwayFromCaster, null, firstMove: false, moveTest: false, CarryOtherActorsOnHex);
				}
				CActor currentTarget = m_CurrentTarget;
				CHeroSummonActor summons = currentTarget as CHeroSummonActor;
				if (summons != null && summons.AIMoveFocusActors.Count == 0 && summons.ReturnToSummoner == true)
				{
					ActorState actorState = ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == summons.ActorGuid);
					if (actorState != null)
					{
						if (actorState.AIFocusOverride != null)
						{
							CAIFocusOverrideDetails aIFocusOverride = actorState.AIFocusOverride;
							if ((aIFocusOverride == null || aIFocusOverride.OverrideType != CAIFocusOverrideDetails.EOverrideType.None) && actorState.AIFocusOverride != summons.SummonData.AIFocusOverrideDetails)
							{
								goto IL_0e73;
							}
						}
						CAIFocusOverrideDetails cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
						cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
						cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor;
						CActor cActor2 = summons.Summoner;
						CMirrorDamageActiveBonus_MirrorAbilities mirrorAbilityBehaviour = CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour(cActor2);
						if (mirrorAbilityBehaviour != null && mirrorAbilityBehaviour.IsOnMirroredSide(summons.Tile, cActor2.Tile))
						{
							cActor2 = mirrorAbilityBehaviour.MirrorTarget;
						}
						cAIFocusOverrideDetails.TargetGUID = cActor2.ActorGuid;
						cAIFocusOverrideDetails.IsTemporary = true;
						cAIFocusOverrideDetails.IsSummonsReturn = true;
						cAIFocusOverrideDetails.FocusBenign = true;
						actorState.SetAIFocusOverride(cAIFocusOverrideDetails);
						m_CurrentTarget.Move(m_MoveCount, m_Jump, m_Fly, Range, allowMove: true, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, m_RemainingWaterTilesThatCostZero, m_FreeTileAwayFromCaster, null, firstMove: false, moveTest: false, CarryOtherActorsOnHex);
					}
				}
				goto IL_0e73;
			}
			if (m_CurrentTarget.Type == CActor.EType.Player)
			{
				foreach (CActiveBonus item5 in CharacterClassManager.FindAllActiveBonusAuras(m_CurrentTarget))
				{
					CPauseAura_MessageData message7 = new CPauseAura_MessageData(m_CurrentTarget)
					{
						m_AuraAbilityID = item5.BaseCard.ID
					};
					ScenarioRuleClient.MessageHandler(message7);
				}
			}
			if (m_CurrentTarget.Type == CActor.EType.Player)
			{
				m_CurrentTarget.Inventory.LockInSelectedItemsAndResetUnselected();
				if (!HasMoved)
				{
					ScenarioRuleClient.FirstAbilityStarted();
				}
			}
			if (ScenarioScript.current != null)
			{
				ScenarioScript.current.OnActorMoving(m_CurrentTarget, m_CurrentTarget.ArrayIndex, m_CurrentTarget.ArrayIndex);
			}
			if (AbilityHasBeenCancelled && m_InterruptedByTeleport)
			{
				return false;
			}
			CActorHasMoved_MessageData cActorHasMoved_MessageData2 = new CActorHasMoved_MessageData(m_CurrentTarget);
			cActorHasMoved_MessageData2.m_Ability = this;
			cActorHasMoved_MessageData2.m_Waypoints = m_PlayerWaypoints;
			cActorHasMoved_MessageData2.m_MovingActor = m_CurrentTarget;
			cActorHasMoved_MessageData2.m_ActorsToCarry = m_ActorsToCarry.ToList();
			cActorHasMoved_MessageData2.m_Jump = m_Jump;
			cActorHasMoved_MessageData2.m_Slide = false;
			cActorHasMoved_MessageData2.m_UseActorRunSpeedAsSlideSpeed = true;
			ScenarioRuleClient.MessageHandler(cActorHasMoved_MessageData2);
			break;
		}
		case EMoveState.ActorHasMoved:
		{
			if (AbilityInterruptCopy is CAbilityMove destAbility && !AbilityHasBeenCancelled)
			{
				CopyInterruptedProperties(destAbility, RemainingMovesAfterLimit, initialCopy: false, forced: false);
				LocoStop(cancelAbilityOnStop: true);
				Perform();
				return false;
			}
			DontMoveAbilityState = false;
			if (m_CurrentTarget is CHeroSummonActor cHeroSummonActor)
			{
				cHeroSummonActor.ReturnToSummoner = null;
				ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == m_CurrentTarget.ActorGuid)?.ResetAIFocusOverride();
			}
			int num3 = CalculateMoveUsed(out var _);
			int num4 = 0;
			for (int num5 = 0; num5 < m_TilesMoved.Count; num5++)
			{
				num4 += (ScenarioManager.GetTile(m_TilesMoved[num5]).IsWater ? 1 : 0);
			}
			AbilityHasHappened |= num3 > 0;
			RemainingMoves = RemainingMovesAfterLimit;
			m_MoveCount -= num3;
			m_RemainingWaterTilesThatCostZero = Math.Max(m_RemainingWaterTilesThatCostZero - num4, 0);
			PhaseManager.CurrentPhaseAction?.AutoConsume();
			CTile propTile2 = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
			CTile cTile = ScenarioManager.Tiles[m_StartingPoint.X, m_StartingPoint.Y];
			int thisMoveHexes = 0;
			if (propTile2 != m_LastTileChecked && (HasMoved || m_StartingPoint != m_CurrentTarget.ArrayIndex))
			{
				ClearPreviousTilePressurePlates();
				thisMoveHexes = 1;
				foreach (CActor item6 in m_ActorsToCarry)
				{
					item6.ArrayIndex = m_CurrentTarget.ArrayIndex;
					GameState.LostAdjacency(item6, (m_LastTileChecked == null) ? cTile : m_LastTileChecked, propTile2, this);
				}
				GameState.LostAdjacency(m_CurrentTarget, (m_LastTileChecked == null) ? cTile : m_LastTileChecked, propTile2, this);
				m_LastTileChecked = propTile2;
				for (int num6 = propTile2.m_Props.Count - 1; num6 >= 0; num6--)
				{
					CObjectProp prop = propTile2.m_Props[num6];
					foreach (CActor item7 in m_ActorsToCarry)
					{
						int strength2 = 0;
						GetMoveBonuses(item7, out var jump2, out var fly2, out var _, out var _, ref strength2);
						TryActivateProp(item7, jump2, fly2, prop);
					}
					TryActivateProp(m_CurrentTarget, m_Jump, m_Fly, prop);
				}
			}
			List<CObjectObstacle> list3 = new List<CObjectObstacle>();
			if (m_Jump || m_Fly)
			{
				CObjectObstacle cObjectObstacle2 = propTile2.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
				if (cObjectObstacle2 == null)
				{
					cObjectObstacle2 = CObjectProp.FindPropWithPathingBlocker(propTile2.m_ArrayIndex, ref propTile2);
				}
				if (cObjectObstacle2 != null)
				{
					list3.Add(cObjectObstacle2);
				}
			}
			CUpdatePropTransparency_MessageData message4 = new CUpdatePropTransparency_MessageData
			{
				m_PropList = list3,
				m_ActorSpawningMessage = m_CurrentTarget
			};
			ScenarioRuleClient.MessageHandler(message4);
			if (AbortIfMoverKilled())
			{
				return false;
			}
			CActor cActor = ScenarioManager.Scenario.FindActorAt(m_CurrentTarget.ArrayIndex, m_CurrentTarget);
			if (cActor != null && !s_AllTargetActorsOnPath.Contains(cActor))
			{
				s_AllTargetActorsOnPath.Add(cActor);
			}
			CActiveBonus.RefreshAllAuraActiveBonuses();
			TargetingActor.CalculateAttackStrengthForUI();
			if (m_CurrentTarget.Type == CActor.EType.Player)
			{
				ScenarioStatRepository.Instance.TotalTilesMoved.Increment();
			}
			if (m_CurrentTarget.Type == CActor.EType.Player || !m_AIFinishedMove)
			{
				if (m_ModifiedStrength > 0)
				{
					m_AlreadyMoved += num3;
					m_AlreadyMovedWaterTiles += (ScenarioManager.GetTile(propTile2.m_ArrayIndex).IsWater ? 1 : 0);
					m_TilesMoved.Add(propTile2.m_ArrayIndex);
					s_AllArrayIndicesAlongPath.Add(propTile2.m_ArrayIndex);
				}
				if (IsMergedAbility)
				{
					m_MergedAbilityMoved = true;
				}
			}
			if (HasMoved)
			{
				m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, m_ActorsToCarry.ToList(), m_NewActorCarried, 1, finalMovement: false, propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), thisMoveHexes, m_CurrentTarget.PrevArrayIndex);
				List<CActor> actorsToCarry = m_ActorsToCarry;
				if (actorsToCarry != null && actorsToCarry.Count > 0)
				{
					m_ActorsToCarry[0].m_OnCarriedListeners?.Invoke(this, m_ActorsToCarry[0], 1, finalMovement: false, propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), thisMoveHexes);
				}
				if (AbortIfMoverKilled())
				{
					return false;
				}
			}
			m_NewActorCarried = false;
			bool canSkip = true;
			if (MoveRestrictionType.Equals(EMoveRestrictionType.MustEndNextToEnemy) && m_MoveCount >= 1)
			{
				canSkip = false;
				foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
				{
					CActor livingActor = allAdjacentTile.LivingActor;
					if (livingActor != null && CActor.AreActorsEnemies(livingActor.Type, TargetingActor.Type, livingActor, TargetingActor))
					{
						canSkip = true;
						break;
					}
				}
			}
			if (MoveRestrictionType.Equals(EMoveRestrictionType.MustEndNextToObstacle) && m_MoveCount >= 1)
			{
				canSkip = false;
				foreach (CTile allAdjacentTile2 in ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
				{
					CTile propTile3 = allAdjacentTile2;
					CObjectProp cObjectProp3 = allAdjacentTile2.FindProp(ScenarioManager.ObjectImportType.Obstacle);
					if (cObjectProp3 == null)
					{
						cObjectProp3 = CObjectProp.FindPropWithPathingBlocker(allAdjacentTile2.m_ArrayIndex, ref propTile3);
					}
					if (cObjectProp3 != null)
					{
						canSkip = true;
						break;
					}
				}
			}
			if (MoveRestrictionType.Equals(EMoveRestrictionType.MustEndOnShadow) && m_MoveCount >= 1)
			{
				canSkip = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y].m_Props.Any((CObjectProp cObjectProp4) => cObjectProp4.ObjectType == ScenarioManager.ObjectImportType.Shadow);
			}
			else if (MoveRestrictionType.Equals(EMoveRestrictionType.MustEndOnSpecificTiles) && m_MoveCount >= 1)
			{
				canSkip = CheckMustEndOnSpecificTiles(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
			}
			SetCanSkip(canSkip);
			if (AbilityHasHappened && (TilesMoved?.Count ?? 0) > 0)
			{
				foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
				{
					GameState.ShowItemSelection(playerActor, TargetingActor, this, CItem.EItemTrigger.OnActorMoved);
				}
			}
			PerformStateVars("FinishedActorHasMoved");
			if (FinishedMove())
			{
				if (HasMoved)
				{
					m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, m_ActorsToCarry.ToList(), newActorCarried: false, 0, finalMovement: true, propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 0, m_CurrentTarget.PrevArrayIndex);
					List<CActor> actorsToCarry2 = m_ActorsToCarry;
					if (actorsToCarry2 != null && actorsToCarry2.Count > 0)
					{
						m_ActorsToCarry[0].m_OnCarriedListeners?.Invoke(this, m_ActorsToCarry[0], 0, finalMovement: true, propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile2.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 0);
					}
					if (AbortIfMoverKilled())
					{
						return false;
					}
				}
				CAbility cAbility = ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility?.m_Ability;
				if (cAbility == this || (cAbility is CAbilityMerged cAbilityMerged && cAbilityMerged.ActiveAbility == this))
				{
					if (m_IsMergedAbility)
					{
						PhaseManager.StepComplete();
					}
					else
					{
						ClearShadowNavLinks();
						m_State = EMoveState.CheckForPlayerIdle;
						Perform();
					}
				}
				else
				{
					m_State = EMoveState.MoveDone;
				}
				CheckPendingMapRevealsAndCurrentPhaseAbility();
				return true;
			}
			if (m_PlayerDestination != null && m_CurrentTarget.ArrayIndex == m_PlayerDestination.m_ArrayIndex && m_PlayerArrayIndexPath.Count <= 0)
			{
				m_AllSelectedPlayerWaypoints?.Remove(m_PlayerDestination);
				m_PlayerDestination = null;
			}
			m_State = EMoveState.ActorIsSelectingMoveTile;
			if (AbilityInterruptCopy != null)
			{
				CopyInterruptedProperties(AbilityInterruptCopy, AbilityInterruptCopy.Strength, initialCopy: false, AbilityInterruptCopy.WasForcedInterrupted);
			}
			if (CheckPendingMapRevealsAndCurrentPhaseAbility())
			{
				Perform();
			}
			break;
		}
		case EMoveState.UnableToMove:
			if (IsMergedAbility && m_MoveCount > 0)
			{
				m_State = EMoveState.ActorIsSelectingMoveTile;
			}
			break;
		case EMoveState.CheckForPlayerIdle:
		{
			CPlayerWaitForIdle_MessageData cPlayerWaitForIdle_MessageData = new CPlayerWaitForIdle_MessageData(TargetingActor);
			cPlayerWaitForIdle_MessageData.m_Actor = TargetingActor;
			ScenarioRuleClient.MessageHandler(cPlayerWaitForIdle_MessageData);
			return true;
		}
		case EMoveState.MoveDone:
			{
				PhaseManager.NextStep();
				return true;
			}
			IL_0e73:
			if (m_CurrentTarget.ArrayIndex != arrayIndex)
			{
				CTile propTile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
				List<CObjectObstacle> list2 = new List<CObjectObstacle>();
				if (m_Jump || m_Fly)
				{
					CObjectObstacle cObjectObstacle = propTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
					if (cObjectObstacle == null)
					{
						cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(propTile.m_ArrayIndex, ref propTile);
					}
					if (cObjectObstacle != null)
					{
						list2.Add(cObjectObstacle);
					}
				}
				CUpdatePropTransparency_MessageData message3 = new CUpdatePropTransparency_MessageData
				{
					m_PropList = list2,
					m_ActorSpawningMessage = m_CurrentTarget
				};
				ScenarioRuleClient.MessageHandler(message3);
				CObjectProp cObjectProp = propTile.FindProp(ScenarioManager.ObjectImportType.Door);
				if (cObjectProp != null && cObjectProp is CObjectDoor cObjectDoor)
				{
					if (!cObjectDoor.Activated)
					{
						DoorsOpened++;
					}
					cObjectDoor.SetDoorOpenedByMovingActor(TargetingActor);
					if (cObjectDoor.RoomsRevealedInLastOpening != null)
					{
						m_RoomRevealsPendingChecks.AddRange(cObjectDoor.RoomsRevealedInLastOpening);
					}
				}
				if (AbilityHasBeenCancelled && m_InterruptedByTeleport)
				{
					return false;
				}
				if (ScenarioScript.current != null)
				{
					ScenarioScript.current.OnActorMoving(m_CurrentTarget, arrayIndex, m_CurrentTarget.ArrayIndex);
				}
				CActorHasMoved_MessageData cActorHasMoved_MessageData = new CActorHasMoved_MessageData(m_CurrentTarget);
				cActorHasMoved_MessageData.m_Ability = this;
				cActorHasMoved_MessageData.m_Waypoints = m_CurrentTarget.AIMoveFocusWaypoints;
				cActorHasMoved_MessageData.m_MovingActor = m_CurrentTarget;
				cActorHasMoved_MessageData.m_ActorsToCarry = m_ActorsToCarry.ToList();
				cActorHasMoved_MessageData.m_Jump = m_Jump;
				ScenarioRuleClient.MessageHandler(cActorHasMoved_MessageData);
			}
			else
			{
				m_AIFinishedMove = true;
				ScenarioRuleClient.MessageHandler(new CStopLoco_MessageData(m_CurrentTarget));
				ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
			}
			break;
		}
		return false;
		IL_0121:
		if (m_CurrentTarget.Type == CActor.EType.Player)
		{
			if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				if (HasMoved)
				{
					LocoStop(cancelAbilityOnStop: false, nextStep: true);
				}
				else
				{
					CPlayerIsStunned_MessageData message8 = new CPlayerIsStunned_MessageData(m_CurrentTarget);
					ScenarioRuleClient.MessageHandler(message8);
				}
			}
			else if (m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				if (HasMoved)
				{
					LocoStop(cancelAbilityOnStop: false, nextStep: true);
				}
				else
				{
					CPlayerIsSleeping_MessageData message9 = new CPlayerIsSleeping_MessageData(m_CurrentTarget);
					ScenarioRuleClient.MessageHandler(message9);
				}
			}
			else if (HasMoved)
			{
				LocoStop(cancelAbilityOnStop: false, nextStep: true);
			}
			else
			{
				CPlayerIsImmobilized_MessageData message10 = new CPlayerIsImmobilized_MessageData(m_CurrentTarget);
				ScenarioRuleClient.MessageHandler(message10);
			}
		}
		else
		{
			LocoStop(cancelAbilityOnStop: false, nextStep: true);
		}
		m_State = EMoveState.UnableToMove;
		return true;
	}

	private CVector3 GetRotation(Point fromPoint, Point toPoint)
	{
		int num = Point.AxialDirectionToRotation(Point.OddRToAxial(toPoint) - Point.OddRToAxial(fromPoint));
		return new CVector3(0f, num, 0f);
	}

	public bool FinishedMove()
	{
		if (m_MoveCount > 0)
		{
			return m_AIFinishedMove;
		}
		return true;
	}

	private void ClearPreviousTilePressurePlates()
	{
		Point point = m_StartingPoint;
		if (TilesMoved.Count > 0)
		{
			point = TilesMoved[TilesMoved.Count - 1];
		}
		CTile cTile = ScenarioManager.Tiles[point.X, point.Y];
		for (int num = cTile.m_Props.Count - 1; num >= 0; num--)
		{
			CObjectProp cObjectProp = cTile.m_Props[num];
			if (cObjectProp is CObjectPressurePlate { ActivatesOnEndTurn: false })
			{
				List<CActor> list = ScenarioManager.Scenario.FindActorsAt(cTile.m_ArrayIndex);
				list.Remove(m_CurrentTarget);
				if (list.Count == 0 || !list[0].CanActivatePressurePlates())
				{
					cObjectProp.Deactivate();
				}
			}
		}
	}

	public override void TileSelected(CTile selectedTile, List<CTile> optionalTileList)
	{
		Log.Trace("[MOVE ABILITY] - Tile Selected");
		bool flag = false;
		if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize))
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData == null || miscAbilityData.IgnoreImmobilize != true)
			{
				goto IL_0288;
			}
		}
		if (!TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) && !TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep) && m_State == EMoveState.ActorIsSelectingMoveTile)
		{
			m_PlayerDestination = selectedTile;
			if (!m_TilesSelected.Contains(selectedTile))
			{
				m_TilesSelected.Add(selectedTile);
			}
			m_PlayerWaypoints = optionalTileList;
			m_AllSelectedPlayerWaypoints = optionalTileList;
			if (m_IsMergedAbility)
			{
				List<Point> list = new List<Point>();
				int num = RemainingMovesAfterLimit;
				Point arrayIndex = m_CurrentTarget.ArrayIndex;
				foreach (CTile playerWaypoint in m_PlayerWaypoints)
				{
					List<Point> list2 = CActor.FindCharacterPath(m_CurrentTarget, arrayIndex, playerWaypoint.m_ArrayIndex, m_Jump || m_Fly, ignoreMoveCost: false, out var foundPath, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, WaterTilesThatCostZero, CarryOtherActorsOnHex, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, Jump, Fly);
					if (foundPath && list2.Count <= num)
					{
						list.AddRange(list2);
						num -= list2.Count;
					}
					else
					{
						list2 = CActor.FindCharacterPath(m_CurrentTarget, arrayIndex, playerWaypoint.m_ArrayIndex, m_Jump || m_Fly, ignoreMoveCost: false, out foundPath, m_IgnoreDifficultTerrain, m_IgnoreHazardousTerrain, WaterTilesThatCostZero, CarryOtherActorsOnHex, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, Jump, Fly);
						if (!foundPath)
						{
							break;
						}
						list.AddRange(list2);
						num -= list2.Count;
					}
					arrayIndex = playerWaypoint.m_ArrayIndex;
				}
				foreach (Point item in list)
				{
					CActor cActor = ScenarioManager.Scenario.FindActorAt(item, m_CurrentTarget);
					if (cActor != null && !s_AllTargetActorsOnPath.Contains(cActor))
					{
						s_AllTargetActorsOnPath.Add(cActor);
					}
				}
			}
			m_PlayerArrayIndexPath.Clear();
			m_CanUndo = false;
			if (!m_IsMergedAbility)
			{
				flag = true;
			}
		}
		goto IL_0288;
		IL_0288:
		if (flag)
		{
			Perform();
		}
		base.TileSelected(selectedTile, optionalTileList);
		LogEvent(ESESubTypeAbility.AbilityTileSelected);
	}

	public override bool CanClearTargets()
	{
		if (m_State != EMoveState.PreMoveBuffTargeting)
		{
			return m_State == EMoveState.ActorIsSelectingMoveTile;
		}
		return true;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EMoveState.PreMoveBuffTargeting)
			{
				return m_State == EMoveState.ActorIsSelectingMoveTile;
			}
			return true;
		}
		return false;
	}

	public override bool RequiresWaypointSelection()
	{
		if (m_State == EMoveState.ActorIsSelectingMoveTile)
		{
			return ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA;
		}
		return false;
	}

	public override bool ShouldRestartAbilityWhenApplyingOverride(CAbilityOverride abilityOverride)
	{
		if (!base.ShouldRestartAbilityWhenApplyingOverride(abilityOverride) && !abilityOverride.Strength.HasValue && !abilityOverride.IgnoreDifficultTerrain.HasValue && !abilityOverride.IgnoreHazardousTerrain.HasValue && !abilityOverride.CarryOtherActorsOnHex.HasValue && !abilityOverride.Jump.HasValue)
		{
			return abilityOverride.Fly.HasValue;
		}
		return true;
	}

	private bool TryActivateProp(CActor actor, bool actorIsJumping, bool actorIsFlying, CObjectProp prop)
	{
		int num;
		if ((prop.ObjectType != ScenarioManager.ObjectImportType.PressurePlate || (actor.CanActivatePressurePlates() && !CObjectPressurePlate.IsEndTurnPressurePlate(prop))) && prop.ObjectType != ScenarioManager.ObjectImportType.Portal && (prop.WillActivate(actor, actorIsJumping, actorIsFlying) || (prop != null && prop.ObjectType == ScenarioManager.ObjectImportType.Door)))
		{
			if (CObjectProp.TryChooseToSkipTrap(actor, prop, this, actorIsJumping))
			{
				if (!m_PropsTriedToActivate.Contains(prop))
				{
					m_PropsTriedToActivate.Add(prop);
				}
				return false;
			}
			if (m_IgnoreHazardousTerrain)
			{
				if (prop.ObjectType != ScenarioManager.ObjectImportType.TerrainHotCoals)
				{
					num = ((prop.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns) ? 1 : 0);
					if (num == 0)
					{
						goto IL_00a1;
					}
				}
				else
				{
					num = 1;
				}
				goto IL_011b;
			}
			num = 0;
			goto IL_00a1;
		}
		return false;
		IL_011b:
		List<CActiveBonus> list = actor.FindPreventDamageBonuses();
		bool flag = list != null && list.Count > 0;
		if (num == 0 && (actor.OriginalType == CActor.EType.Player || (actor is CHeroSummonActor cHeroSummonActor && (flag || cHeroSummonActor.IsCompanionSummon))) && !prop.Activated && prop.WillActivationDamageActor(actor))
		{
			bool flag2 = prop.WillActivationKillActor(actor) && !flag;
			if (prop is CObjectTrap cObjectTrap && cObjectTrap.Conditions.Count > 0 && (cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Stun) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Immobilize) || cObjectTrap.Conditions.Contains(CCondition.ENegativeCondition.Sleep)))
			{
				flag2 = true;
			}
			if (flag2)
			{
				LocoStop();
			}
			else
			{
				LocoPause(actor, pause: true);
			}
		}
		if (!prop.Activated && prop.ObjectType == ScenarioManager.ObjectImportType.Door)
		{
			DoorsOpened++;
		}
		if (!m_PropsTriedToActivate.Contains(prop))
		{
			m_PropsTriedToActivate.Add(prop);
		}
		if (num == 0)
		{
			prop.AutomaticActivate(actor, actorIsJumping, actorIsFlying);
		}
		if (ControllingActor != null && prop.ObjectType == ScenarioManager.ObjectImportType.Trap)
		{
			SEventLogMessageHandler.AddEventLogMessage(new SEventActorForcedIntoTrap(GetAbilityBaseCard().ID, ControllingActor.Class.ID));
		}
		return true;
		IL_00a1:
		if (prop is CObjectTrap { Damage: not false } cObjectTrap2 && GetMiscVar("TreatFirstDamageTrapAsHeal").GetBool() && m_PropsTriedToActivate.Where((CObjectProp pr) => pr is CObjectTrap cObjectTrap3 && cObjectTrap3.Damage).Count() == 0)
		{
			int trapDamage = cObjectTrap2.GetTrapDamage();
			cObjectTrap2.SetCustomDamageValue(0);
			cObjectTrap2.SetHealValue(cObjectTrap2.HealValue + trapDamage);
		}
		goto IL_011b;
	}

	public bool CheckPendingMapRevealsAndCurrentPhaseAbility()
	{
		if (m_RoomRevealsPendingChecks.Count > 0)
		{
			foreach (CScenarioModifier item in ScenarioManager.CurrentScenarioState.ScenarioModifiers.Where((CScenarioModifier m) => (m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartRound || m.ScenarioModifierTriggerPhase == EScenarioModifierTriggerPhase.StartScenario) && m_RoomRevealsPendingChecks.Any((CMap x) => m.ShouldTriggerWhenOpeningRoom(x.MapGuid))))
			{
				item.PerformScenarioModifierInRound(ScenarioManager.CurrentScenarioState.RoundNumber);
			}
			m_RoomRevealsPendingChecks.Clear();
		}
		CAbility cAbility = (PhaseManager.Phase as CPhaseAction)?.CurrentPhaseAbility?.m_Ability;
		if (cAbility != this)
		{
			if (cAbility is CAbilityMerged cAbilityMerged)
			{
				return cAbilityMerged.ActiveAbility == this;
			}
			return false;
		}
		return true;
	}

	public void LocoStop(bool cancelAbilityOnStop = false, bool nextStep = false)
	{
		CStopLoco_MessageData cStopLoco_MessageData = new CStopLoco_MessageData(TargetingActor);
		cStopLoco_MessageData.m_Pause = true;
		ScenarioRuleClient.MessageHandler(cStopLoco_MessageData);
		if (cancelAbilityOnStop)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		if (nextStep)
		{
			PhaseManager.NextStep();
		}
	}

	public void LocoPause(CActor actor, bool pause)
	{
		CPauseLoco_MessageData cPauseLoco_MessageData = new CPauseLoco_MessageData(actor);
		cPauseLoco_MessageData.m_Pause = pause;
		ScenarioRuleClient.MessageHandler(cPauseLoco_MessageData);
	}

	private Point GetNextPathingPosition(CActor actor, CTile destination, bool jump, bool fly, int remainingMoves, out List<CTile> waypoints, out List<Point> arrayIndexPath, bool ignoreDifficultTerrain, bool ignoreNegativeTiles, bool ignoreBlockedTileMoveCost, int waterTilesThatCostZero, bool carryOtherActors, CActor.EType carryType)
	{
		arrayIndexPath = CActor.FindCharacterPath(actor, actor.ArrayIndex, destination.m_ArrayIndex, jump | fly, ignoreMoveCost: false, out var foundPath, ignoreDifficultTerrain, ignoreNegativeTiles, waterTilesThatCostZero, carryOtherActors, carryType, logFailure: false, remainingMoves, ignoreCannotStop: false, jump, fly, IgnoreDoorLocks);
		if (!foundPath || CalculateMoveCost(actor, actor.ArrayIndex, arrayIndexPath, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, waterTilesThatCostZero, m_FreeTileAwayFromCaster) > remainingMoves)
		{
			arrayIndexPath.Clear();
			arrayIndexPath = CActor.FindCharacterPath(actor, actor.ArrayIndex, destination.m_ArrayIndex, jump | fly, ignoreMoveCost: false, out foundPath, ignoreDifficultTerrain, ignoreNegativeTiles: true, waterTilesThatCostZero, carryOtherActors, carryType, logFailure: false, remainingMoves, ignoreCannotStop: false, jump, fly, IgnoreDoorLocks);
		}
		waypoints = new List<CTile>();
		int num = 0;
		if (arrayIndexPath.Count > 1)
		{
			ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.GetAdjacentPosition(actor.ArrayIndex.X, actor.ArrayIndex.Y, arrayIndexPath[0].X, arrayIndexPath[0].Y);
			for (int i = 1; i < arrayIndexPath.Count; i++)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(arrayIndexPath[i - 1].X, arrayIndexPath[i - 1].Y, arrayIndexPath[i].X, arrayIndexPath[i].Y);
				num += CalculateMoveUsed(actor, actor.ArrayIndex, arrayIndexPath, i - 1, fly | jump, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, m_FreeTileAwayFromCaster);
				if (num > remainingMoves)
				{
					break;
				}
				if (adjacentPosition != eAdjacentPosition)
				{
					waypoints.Add(ScenarioManager.Tiles[arrayIndexPath[i - 1].X, arrayIndexPath[i - 1].Y]);
					eAdjacentPosition = adjacentPosition;
				}
			}
		}
		if (num < remainingMoves)
		{
			waypoints.Add(destination);
		}
		if (arrayIndexPath.Count > 0)
		{
			return arrayIndexPath[0];
		}
		return destination.m_ArrayIndex;
	}

	public static int CalculateMoveCost(CActor movingActor, Point origin, List<Point> path, bool ignoreBlocked, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreBlockedTileMoveCost, int waterTilesThatCostZero, Point? freeTileAwayFromCaster)
	{
		int num = 0;
		for (int i = 0; i < path.Count; i++)
		{
			num += CalculateMoveUsed(movingActor, origin, path, i, ignoreBlocked, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost, waterTilesThatCostZero, freeTileAwayFromCaster);
		}
		return num;
	}

	public int CalculateMoveUsed(out List<Point> path)
	{
		path = PathArrayIndices();
		Point origin = m_StartingPoint;
		CTile cTile = GetMiscVar("ResumedAtPosition").GetObject<CTile>();
		if (cTile != null)
		{
			origin = cTile.m_ArrayIndex;
		}
		return CalculateMoveUsed(TargetingActor, origin, path, m_TilesMoved.Count, m_Fly || m_Jump, ignoreMoveCost: false, m_IgnoreDifficultTerrain, m_IgnoreBlockedTileMoveCost, WaterTilesThatCostZero, m_FreeTileAwayFromCaster);
	}

	public static int CalculateMoveUsed(CActor movingActor, Point origin, List<Point> path, int index, bool ignoreBlocked, bool ignoreMoveCost, bool ignoreDifficultTerrain, bool ignoreBlockedTileMoveCost, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool hasMoved = false)
	{
		if (path == null || path.Count == 0 || index < 0 || index >= path.Count)
		{
			return 0;
		}
		Point point = path[index];
		Point point2 = ((index > 0) ? path[index - 1] : origin);
		CNode node = ScenarioManager.PathFinder.GetNode(point2);
		CNode node2 = ScenarioManager.PathFinder.GetNode(point);
		CNavLink cNavLink = node.NavLinks.FirstOrDefault((CNavLink x) => x.DestinationNode.Equals(node2));
		int num = 0;
		if (freeTileAwayFromCaster.HasValue && Point.HexDist(freeTileAwayFromCaster.Value, point2) == 1 && Point.HexDist(freeTileAwayFromCaster.Value, point) > 1)
		{
			num = 1;
		}
		int num2 = 1;
		if (cNavLink != null)
		{
			num2 = cNavLink.Weight;
		}
		if (ignoreBlockedTileMoveCost && node2.Blocked)
		{
			return 0;
		}
		if (ignoreMoveCost && node2.IsIceTerrain && movingActor != null && movingActor.IgnoreIceSlideEffect)
		{
			return num2 - num;
		}
		if (node2.IsIceTerrain)
		{
			if (ignoreBlocked)
			{
				if (movingActor != null && movingActor.IgnoreIceSlideEffect)
				{
					return 0;
				}
				return num2 - num;
			}
			if (movingActor != null && movingActor.IgnoreIceSlideEffect)
			{
				return 0;
			}
			if (index == path.Count - 1)
			{
				return num2 - num;
			}
			Point position = path[index + 1];
			ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(point2.X, point2.Y, point.X, point.Y);
			ScenarioManager.EAdjacentPosition adjacentPosition2 = ScenarioManager.GetAdjacentPosition(point.X, point.Y, position.X, position.Y);
			bool flag = adjacentPosition != adjacentPosition2;
			if ((ScenarioManager.Scenario.FindActorAt(position, movingActor) != null) | flag)
			{
				return num2 - num;
			}
			return 0;
		}
		if (node2.IsWaterTerrain && waterTilesThatCostZero != 0)
		{
			int num3 = 0;
			for (int num4 = 0; num4 <= index; num4++)
			{
				if (ScenarioManager.GetTile(path[num4]).IsWater)
				{
					num3++;
				}
			}
			if (num3 <= waterTilesThatCostZero)
			{
				return 0;
			}
		}
		if (node2.IsDifficultTerrain && !ignoreMoveCost)
		{
			if (ignoreDifficultTerrain | ignoreBlocked)
			{
				return num2 - num;
			}
			if ((index > 0) | hasMoved)
			{
				if (!node.IsIceTerrain)
				{
					return 2;
				}
				return 1;
			}
			return num2 + 1 - num;
		}
		return num2 - num;
	}

	public static List<Point> FindPathAndWaypoints(CActor actor, Point origin, CTile destination, out List<CTile> waypoints, int moveCount, bool jump, bool fly, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero, Point? freeTileAwayFromCaster, bool excludeDestination, bool ignoreMoveCost, out bool foundPath, bool shouldPathThroughDoors, List<Point> transientBlockedList = null, List<Point> transientSuperBlockedList = null)
	{
		List<Point> list = ScenarioManager.PathFinder.FindPath(actor, origin, destination.m_ArrayIndex, jump | fly, ignoreMoveCost, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, out foundPath, ignoreDoorLocks: false, transientBlockedList, transientSuperBlockedList);
		if (excludeDestination && list.Count > 1)
		{
			destination = ScenarioManager.Tiles[list[list.Count - 2].X, list[list.Count - 2].Y];
		}
		waypoints = new List<CTile>();
		if (list.Count > 1)
		{
			ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.GetAdjacentPosition(origin.X, origin.Y, list[0].X, list[0].Y);
			int num = CalculateMoveUsed(actor, origin, list, 0, fly | jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
			for (int i = 1; i < list.Count; i++)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(list[i - 1].X, list[i - 1].Y, list[i].X, list[i].Y);
				num += CalculateMoveUsed(actor, origin, list, i, fly | jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
				if (num > moveCount)
				{
					break;
				}
				if (adjacentPosition != eAdjacentPosition)
				{
					waypoints.Add(ScenarioManager.Tiles[list[i - 1].X, list[i - 1].Y]);
					eAdjacentPosition = adjacentPosition;
				}
			}
		}
		waypoints.Add(destination);
		if (excludeDestination && list.Count > 0)
		{
			list.Remove(list[list.Count - 1]);
		}
		int num2 = CalculateMoveCost(actor, origin, list, fly | jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
		if (moveCount < num2)
		{
			int num3 = CalculateMoveUsed(actor, origin, list, 0, fly | jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
			for (int j = 1; j < list.Count; j++)
			{
				num3 += CalculateMoveUsed(actor, origin, list, j, fly | jump, ignoreMoveCost, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, freeTileAwayFromCaster);
				if (num3 > moveCount)
				{
					waypoints[waypoints.Count - 1] = ScenarioManager.Tiles[list[j - 1].X, list[j - 1].Y];
					break;
				}
			}
		}
		return list;
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

	public override bool AbilityComplete(bool dontMoveState, out bool fullAbilityRestart)
	{
		LogEvent(ESESubTypeAbility.AbilityComplete);
		fullAbilityRestart = false;
		if (m_State == EMoveState.ActorIsSelectingMoveTile && TargetingActor.Type == CActor.EType.Player && m_PlayerDestination == null)
		{
			return false;
		}
		if (m_State == EMoveState.ApplyMoveBuff)
		{
			return true;
		}
		if (!dontMoveState)
		{
			Log.Trace("[MOVE ABILITY] - Ability complete moving state from: " + m_State.ToString() + " to: " + (m_State + 1));
			m_State++;
		}
		return m_State == EMoveState.MoveDone;
	}

	public override void Update()
	{
		if (m_State == EMoveState.ActorIsMoving && !ScenarioManager.Scenario.HasActor(m_CurrentTarget))
		{
			PhaseManager.NextStep();
		}
	}

	public List<CTile> GetMustEndNextToObstacleTiles(CActor movingActor, int remainingMoves, Point startPoint, bool ignoreBlocked, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero)
	{
		List<CTile> tiles = new List<CTile>();
		List<CTile> list = new List<CTile>();
		HeroSummonYMLData heroSummonYMLData = (movingActor as CHeroSummonActor)?.SummonData;
		CMonsterClass cMonsterClass = (movingActor as CEnemyActor)?.MonsterClass;
		foreach (CMap map in ScenarioManager.CurrentScenarioState.Maps)
		{
			if (!map.Revealed)
			{
				continue;
			}
			foreach (CObjectObstacle item in map.Props.OfType<CObjectObstacle>())
			{
				foreach (TileIndex pathingBlocker in item.PathingBlockers)
				{
					foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y]))
					{
						if (!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked)
						{
							CActor cActor = ScenarioManager.Scenario.FindActorAt(allAdjacentTile.m_ArrayIndex);
							if (cActor == null || ((heroSummonYMLData == null || heroSummonYMLData.CanMoveOnActorClassID == null || heroSummonYMLData.CanMoveOnActorClassID != cActor.Class.ID) && (cMonsterClass?.MonsterYML.CanMoveOnActorClassID == null || cActor.Class?.ID == null || cMonsterClass.MonsterYML.CanMoveOnActorClassID.Contains(cActor.Class.ID))))
							{
								list.Add(allAdjacentTile);
							}
						}
					}
				}
			}
		}
		list = list.Distinct().ToList();
		List<CTile> list2 = new List<CTile>();
		foreach (CTile item2 in list)
		{
			List<Point> list3 = CActor.FindCharacterPath(movingActor, startPoint, item2.m_ArrayIndex, ignoreBlocked, ignoreMoveCost: true, out var foundPath, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, Jump, Fly);
			if (foundPath && list3.Count != 0 && CalculateMoveCost(movingActor, startPoint, list3, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, m_FreeTileAwayFromCaster) <= remainingMoves)
			{
				tiles.Clear();
				GetMustEndNextToObstacleTilesRecursive(movingActor, ref tiles, ScenarioManager.Tiles[startPoint.X, startPoint.Y], item2, remainingMoves, ignoreBlocked, ignoreDifficultTerrain, startPoint, m_FreeTileAwayFromCaster);
				list2.AddRange(tiles);
			}
		}
		return list2.Distinct().ToList();
	}

	public List<CTile> GetMustEndNextToEnemyTiles(CActor movingActor, int remainingMoves, Point startPoint, bool ignoreBlocked, bool ignoreDifficultTerrain, bool ignoreHazardousTerrain, int waterTilesThatCostZero)
	{
		List<CTile> tiles = new List<CTile>();
		List<CTile> list = new List<CTile>();
		HeroSummonYMLData heroSummonYMLData = (movingActor as CHeroSummonActor)?.SummonData;
		CMonsterClass cMonsterClass = (movingActor as CEnemyActor)?.MonsterClass;
		foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => CActor.AreActorsEnemies(movingActor.Type, x.Type, movingActor, x)))
		{
			foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(item.Tile))
			{
				if (!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked)
				{
					CActor cActor = ScenarioManager.Scenario.FindActorAt(allAdjacentTile.m_ArrayIndex);
					if (cActor == null || ((heroSummonYMLData == null || heroSummonYMLData.CanMoveOnActorClassID == null || heroSummonYMLData.CanMoveOnActorClassID != cActor.Class.ID) && (cMonsterClass?.MonsterYML.CanMoveOnActorClassID == null || cActor.Class?.ID == null || cMonsterClass.MonsterYML.CanMoveOnActorClassID.Contains(cActor.Class.ID))))
					{
						list.Add(allAdjacentTile);
					}
				}
			}
		}
		list = list.Distinct().ToList();
		List<CTile> list2 = new List<CTile>();
		foreach (CTile item2 in list)
		{
			List<Point> list3 = CActor.FindCharacterPath(movingActor, startPoint, item2.m_ArrayIndex, ignoreBlocked, ignoreMoveCost: false, out var foundPath, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, Jump, Fly);
			if (!foundPath || list3.Count == 0 || CalculateMoveCost(movingActor, startPoint, list3, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, m_FreeTileAwayFromCaster) > remainingMoves)
			{
				list3 = CActor.FindCharacterPath(movingActor, startPoint, item2.m_ArrayIndex, ignoreBlocked, ignoreMoveCost: true, out foundPath, ignoreDifficultTerrain, ignoreHazardousTerrain, waterTilesThatCostZero);
				if (!foundPath || list3.Count == 0 || CalculateMoveCost(movingActor, startPoint, list3, ignoreBlocked, ignoreMoveCost: true, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, waterTilesThatCostZero, m_FreeTileAwayFromCaster) > remainingMoves)
				{
					continue;
				}
			}
			tiles.Clear();
			GetMustEndNextToObstacleTilesRecursive(movingActor, ref tiles, ScenarioManager.Tiles[startPoint.X, startPoint.Y], item2, remainingMoves, ignoreBlocked, ignoreDifficultTerrain, startPoint, m_FreeTileAwayFromCaster);
			list2.AddRange(tiles);
		}
		return list2.Distinct().ToList();
	}

	public static void GetMustEndNextToObstacleTilesRecursive(CActor movingActor, ref List<CTile> tiles, CTile currentTile, CTile destinationTile, int remainingMoves, bool ignoreBlocked, bool ignoreDifficultTerrain, Point startPoint, Point? freeTileAwayFromCaster, int count = 0)
	{
		List<CTile> list = new List<CTile>();
		count++;
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			if (tiles.Contains(allAdjacentTile) || (allAdjacentTile.m_HexMap != null && allAdjacentTile.m_Hex2Map != null && (!allAdjacentTile.m_HexMap.Revealed || !allAdjacentTile.m_Hex2Map.Revealed)))
			{
				continue;
			}
			if (!ignoreBlocked)
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(allAdjacentTile.m_ArrayIndex);
				if ((cActor != null && cActor != movingActor && !CActor.AreActorsAllied(cActor.Type, movingActor.Type, cActor, movingActor)) || allAdjacentTile.FindProps(ScenarioManager.ObjectImportType.Obstacle).Count > 0)
				{
					continue;
				}
			}
			List<Point> path = ScenarioManager.PathFinder.FindPath(movingActor, allAdjacentTile.m_ArrayIndex, destinationTile.m_ArrayIndex, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreHazardousTerrain: false, 0, out var _, ignoreDoorLocks: false);
			if (CalculateMoveCost(movingActor, allAdjacentTile.m_ArrayIndex, path, ignoreBlocked, ignoreMoveCost: false, ignoreDifficultTerrain, ignoreBlockedTileMoveCost: false, 0, freeTileAwayFromCaster) <= remainingMoves - 1)
			{
				list.Add(allAdjacentTile);
				tiles.Add(allAdjacentTile);
			}
		}
		if (remainingMoves <= 0)
		{
			return;
		}
		foreach (CTile item in list)
		{
			GetMustEndNextToObstacleTilesRecursive(movingActor, ref tiles, item, destinationTile, remainingMoves - count, ignoreBlocked, ignoreDifficultTerrain, startPoint, freeTileAwayFromCaster, count);
		}
	}

	public bool CheckMustEndOnSpecificTiles(CTile endTile)
	{
		IList list = GetVar("MustEndOnSpecificTiles").GetObject<IList>();
		if (list == null)
		{
			list = GetMiscVar("MustEndOnSpecificTiles").GetObject<IList>();
		}
		return list?.Contains(endTile) ?? false;
	}

	public override void AbilityEnded()
	{
		Log.Trace("[MOVE ABILITY] - Ability Ended");
		m_State = EMoveState.MoveDone;
		CacheTilesMovedThisInstance(this);
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		if (HasMoved || (m_CurrentTarget != null && m_StartingPoint != m_CurrentTarget.ArrayIndex))
		{
			CTile cTile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
			for (int num = cTile.m_Props.Count - 1; num >= 0; num--)
			{
				CObjectProp cObjectProp = cTile.m_Props[num];
				if (!m_PropsTriedToActivate.Contains(cObjectProp))
				{
					switch (cObjectProp.ObjectType)
					{
					case ScenarioManager.ObjectImportType.PressurePlate:
						if (!CObjectPressurePlate.IsEndTurnPressurePlate(cObjectProp))
						{
							TryActivateProp(m_CurrentTarget, actorIsJumping: false, m_Fly, cObjectProp);
						}
						break;
					case ScenarioManager.ObjectImportType.Portal:
						cObjectProp.AutomaticActivate(m_CurrentTarget);
						break;
					default:
						if (Jump && AbilityInterruptCopy == null)
						{
							TryActivateProp(m_CurrentTarget, actorIsJumping: false, m_Fly, cObjectProp);
						}
						break;
					}
				}
			}
			GetMiscVar("MoveEnded").Calc(new CVarsContext(this));
		}
		m_PropsTriedToActivate.Clear();
		base.AbilityEnded();
		LocoPause(TargetingActor, pause: false);
		ClearShadowNavLinks();
		if (!WasForcedInterrupted && (AllTargetsOnMovePath || (AllTargetsOnMovePathSameStartAndEnd && m_StartingPoint == m_CurrentTarget.ArrayIndex)))
		{
			for (int num2 = s_AllTargetActorsOnPath.Count - 1; num2 >= 0; num2--)
			{
				CActor actorOnPath = s_AllTargetActorsOnPath[num2];
				if (!ProcessTargetOnMovePath(actorOnPath))
				{
					s_AllTargetActorsOnPath.RemoveAt(num2);
				}
			}
		}
		if (m_CurrentTarget != null && ScenarioManager.Scenario.HasActor(m_CurrentTarget) && m_CurrentTarget.Type == CActor.EType.Player && HasMoved)
		{
			foreach (CActiveBonus item in CharacterClassManager.FindAllActiveBonusAuras(m_CurrentTarget))
			{
				CUnpauseAura_MessageData message = new CUnpauseAura_MessageData(m_CurrentTarget)
				{
					m_AuraBaseCardID = item.BaseCard.ID,
					m_AuraBaseCardName = item.BaseCard.Name
				};
				ScenarioRuleClient.MessageHandler(message);
			}
		}
		GameState.UpdateHexesMovedThisTurn(m_CurrentTarget, m_TilesMoved);
		CPhaseAction.UpdateHexesMovedThisAction(m_TilesMoved.Count);
		AllArrayIndexOnPathIncludingRepeatsCopy = new List<Point>();
		AllArrayIndexOnPathIncludingRepeatsCopy.AddRange(AllPlayerArrayIndexOnPathIncludingRepeats);
	}

	public void UpdateCarryActors(List<CTile> optionalTileList, bool removeWaypoint)
	{
		if ((!removeWaypoint && (removeWaypoint || ActorsToCarry.Count != 0)) || !TargetsOnPath(this, optionalTileList, out var targetActors))
		{
			return;
		}
		ActorsToCarry.Clear();
		m_NewActorCarried = false;
		foreach (CActor item in targetActors)
		{
			if (!CAbility.ImmuneToAbility(item, this))
			{
				ActorsToCarry.Add(item);
			}
		}
		ActorsToCarry.Remove(m_CurrentTarget);
		ActorsToCarry = ActorsToCarry.Distinct().ToList();
		if (m_ActorsToCarry.Count > 0 && m_StartingPoint != m_ActorsToCarry[0].ArrayIndex)
		{
			m_NewActorCarried = true;
		}
	}

	public void CarryOtherActorsOnHexUpdated()
	{
		if (CarryOtherActorsOnHex)
		{
			CTile tile = ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y];
			m_ActorsToCarry = GameState.GetActorsOnTile(tile, FilterActor, CAbilityFilterContainer.CreateDefaultFilter(), ActorsToIgnore, isTargetedAbility: false, false);
			m_ActorsToCarry.Remove(m_CurrentTarget);
			if (m_ActorsToCarry.Count > 0 && m_StartingPoint != m_ActorsToCarry[0].ArrayIndex && !HasMoved)
			{
				m_NewActorCarried = true;
			}
		}
		else
		{
			m_ActorsToCarry.Clear();
		}
		ActorsToCarry = ActorsToCarry.Distinct().ToList();
		CAbilityRestarted_MessageData message = new CAbilityRestarted_MessageData(TargetingActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public static bool TargetsOnPath(CAbility attackAbility, List<CTile> optionalTileList, out List<CActor> targetActors)
	{
		targetActors = new List<CActor>();
		if (optionalTileList != null)
		{
			for (int i = 0; i < optionalTileList.Count; i++)
			{
				bool foundPath = false;
				List<Point> list = new List<Point>();
				if (i == 0)
				{
					list = ScenarioManager.PathFinder.FindPath(attackAbility.TargetingActor, attackAbility.TargetingActor.ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false);
					list.Insert(0, attackAbility.TargetingActor.ArrayIndex);
				}
				else
				{
					list = ScenarioManager.PathFinder.FindPath(attackAbility.TargetingActor, optionalTileList[i - 1].m_ArrayIndex, optionalTileList[i].m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out foundPath, ignoreDoorLocks: false);
					list.Insert(0, optionalTileList[i - 1].m_ArrayIndex);
				}
				if (foundPath)
				{
					foreach (Point item in list)
					{
						List<CActor> actorsOnTile = GameState.GetActorsOnTile(ScenarioManager.Tiles[item.X, item.Y], attackAbility.TargetingActor, CAbilityFilterContainer.CreateDefaultFilter(), attackAbility.ActorsToIgnore, isTargetedAbility: false, false);
						targetActors.AddRange(actorsOnTile);
					}
					continue;
				}
				return false;
			}
			if (optionalTileList.Count == 0)
			{
				List<CActor> actorsOnTile2 = GameState.GetActorsOnTile(ScenarioManager.Tiles[attackAbility.TargetingActor.ArrayIndex.X, attackAbility.TargetingActor.ArrayIndex.Y], attackAbility.TargetingActor, CAbilityFilterContainer.CreateDefaultFilter(), attackAbility.ActorsToIgnore, isTargetedAbility: false, false);
				targetActors.AddRange(actorsOnTile2);
			}
			targetActors = targetActors.Distinct().ToList();
			return true;
		}
		return false;
	}

	private bool ProcessTargetOnMovePath(CActor actorOnPath)
	{
		bool result = true;
		if (m_PositiveConditions.Count > 0)
		{
			if (actorOnPath != null)
			{
				actorOnPath = (CActor.AreActorsAllied(m_CurrentTarget.Type, actorOnPath.Type, m_CurrentTarget, actorOnPath) ? actorOnPath : null);
			}
			if (actorOnPath != null)
			{
				ProcessPositiveStatusEffects(actorOnPath);
				result = true;
			}
			else
			{
				result = false;
			}
		}
		if (m_NegativeConditions.Count > 0)
		{
			if (actorOnPath != null)
			{
				actorOnPath = ((!CActor.AreActorsAllied(m_CurrentTarget.Type, actorOnPath.Type, m_CurrentTarget, actorOnPath)) ? actorOnPath : null);
			}
			if (actorOnPath != null)
			{
				ProcessNegativeStatusEffects(actorOnPath);
				result = true;
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	private int CalcModifiedStrength()
	{
		List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffMove);
		int num = Strength;
		foreach (CActiveBonus item in list)
		{
			num += item.ReferenceStrength(this, null);
		}
		foreach (CActiveBonus item2 in list)
		{
			int num2 = item2.Ability.GetMiscVar("MaxMovement").GetInt(null, -1);
			if (num2 != -1)
			{
				num = Math.Min(num, num2);
			}
		}
		return num;
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		if (subTypeAbility == ESESubTypeAbility.AbilityPerform)
		{
			Log.Trace("[MOVE ABILITY] - Performing state: " + m_State);
		}
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityMove(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this, strength: CalcModifiedStrength(), defaultAction: IsDefaultMove, isBuff: ActiveBonusData.HasBehaviour()), m_State, m_TilesMoved.Distinct().Count(), m_AlreadyMoved - m_AlreadyMovedAtStart, new TileIndex(m_StartingPoint), new TileIndex(TargetingActor.ArrayIndex), m_Jump));
	}

	public override string GetDescription()
	{
		return "Move(" + m_InitialMoveCount + ")";
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

	public bool HasPassedState(EMoveState moveState)
	{
		return m_State > moveState;
	}

	public override void Reset()
	{
		base.Reset();
		s_LastMover = null;
		s_AllTargetActorsOnPath.Clear();
		s_AllPlayerArrayIndexOnPath.Clear();
		s_AllPlayerArrayIndexOnPathIncludingRepeats.Clear();
		s_AllPlayerArrayIndexOnPathIncludingStart.Clear();
		s_AllArrayIndicesAlongPath.Clear();
	}

	public override bool IsPositive()
	{
		return true;
	}

	public void OnSkippedByPlayer()
	{
		if (HasMoved)
		{
			SetCancelAbilityFlag(cancel: true);
			m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, m_ActorsToCarry.ToList(), newActorCarried: false, 0, finalMovement: true, m_CurrentTarget.Tile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), m_CurrentTarget.Tile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 0, m_CurrentTarget.PrevArrayIndex);
			CRefreshWorldSpaceStarHexDisplay_MessageData message = new CRefreshWorldSpaceStarHexDisplay_MessageData();
			ScenarioRuleClient.MessageHandler(message);
		}
	}
}
