using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CAbilityPush : CAbility
{
	public enum EAdditionalPushEffect
	{
		None,
		IntoObstacles,
		TrackBlocked,
		DoubleHazardousDamageAndReplaceHazardousWithOneHexObstacle
	}

	public enum EPushState
	{
		None,
		SelectPushTarget,
		ActorIsSelectingPushTile,
		ActorIsPushing,
		ActorHasPushed,
		ActorFinalizePush,
		CheckForNextTarget,
		PushDone
	}

	[Serializable]
	public class PushedActorStats
	{
		public string Name;

		public string ID;

		public int DistanceMoved;

		public TileIndex MovedFromPoint;

		public TileIndex MovedToPoint;

		public PushedActorStats(string name, string id, int distanceMoved, TileIndex movedFromPoint, TileIndex movedToPoint)
		{
			Name = name;
			ID = id;
			DistanceMoved = distanceMoved;
			MovedFromPoint = movedFromPoint;
			MovedToPoint = movedToPoint;
		}
	}

	public static readonly EAdditionalPushEffect[] AdditionalPushEffects = (EAdditionalPushEffect[])Enum.GetValues(typeof(EAdditionalPushEffect));

	public static readonly EPushState[] PushStates = (EPushState[])Enum.GetValues(typeof(EPushState));

	public static bool s_LastPushAbilityWasExecuted;

	private EPushState m_State;

	private CTile m_PlayerDestination;

	private List<CTile> m_PlayerWaypoints;

	public List<Point> m_AbilityPath;

	public List<Point> m_CachedAbilityPath;

	private CActor m_CurrentTarget;

	private bool m_TileSelected;

	private Dictionary<CActor, PushedActorStats> m_ActorDistanceMovedDictionary;

	private int m_PushesBlocked;

	private CAttackSummary m_AdditionalPushDamageSummary;

	private bool m_TargetSet;

	private bool m_DoThisOnce = true;

	public Point m_StartingPoint;

	private int m_AbilityPathProgressIndex;

	[JsonProperty]
	public EAdditionalPushEffect AdditionalPushEffect { get; set; }

	[JsonProperty]
	public int AdditionalPushEffectDamage { get; set; }

	[JsonProperty]
	public int AdditionalPushEffectXP { get; set; }

	public CAttackSummary AdditionalPushDamageSummary => m_AdditionalPushDamageSummary;

	public bool CanOpenDoors => GetMiscVar("CanOpenDoors").GetBool();

	public int RemainingPushes { get; set; }

	public int DistanceMoved { get; set; }

	public bool MustMove { get; set; }

	public CActor CurrentTarget => m_CurrentTarget;

	public Point PushFromPoint { get; private set; }

	public CTile TryPushToThisTileAutomatically { get; set; }

	public List<CObjectObstacle> DestroyedObstacles { get; private set; }

	public List<CObjectObstacle> DestroyedObstaclesCurrentPath { get; private set; }

	public EPushState State => m_State;

	public List<Point> GetAbilityPath()
	{
		if (m_CachedAbilityPath != null)
		{
			return m_CachedAbilityPath;
		}
		return m_AbilityPath;
	}

	public CAbilityPush(EAdditionalPushEffect additionalPushEffect, int additionalPushEffectDamage, int additionalPushEffectXP)
	{
		AdditionalPushEffect = additionalPushEffect;
		AdditionalPushEffectDamage = additionalPushEffectDamage;
		AdditionalPushEffectXP = additionalPushEffectXP;
	}

	public override void CopyInterruptedProperties(CAbility destAbility, int strength, bool initialCopy, bool forced)
	{
		base.CopyInterruptedProperties(destAbility, strength, initialCopy, forced);
		CAbilityPush cAbilityPush = (CAbilityPush)destAbility;
		cAbilityPush.m_PotentiallyValidActorsInRange = m_PotentiallyValidActorsInRange?.ToList();
		cAbilityPush.RemainingPushes = RemainingPushes;
		cAbilityPush.DistanceMoved = DistanceMoved;
		cAbilityPush.MustMove = MustMove;
		cAbilityPush.PushFromPoint = PushFromPoint;
		cAbilityPush.m_State = m_State;
		cAbilityPush.m_PlayerDestination = m_PlayerDestination;
		cAbilityPush.m_PlayerWaypoints = m_PlayerWaypoints?.ToList();
		cAbilityPush.m_AbilityPath = m_AbilityPath?.ToList();
		cAbilityPush.m_CachedAbilityPath = m_CachedAbilityPath?.ToList();
		cAbilityPush.m_CurrentTarget = m_CurrentTarget;
		cAbilityPush.m_TileSelected = m_TileSelected;
		cAbilityPush.m_ActorDistanceMovedDictionary = new Dictionary<CActor, PushedActorStats>(m_ActorDistanceMovedDictionary);
		cAbilityPush.m_PushesBlocked = m_PushesBlocked;
		cAbilityPush.m_AdditionalPushDamageSummary = m_AdditionalPushDamageSummary;
		cAbilityPush.m_TargetSet = m_TargetSet;
		cAbilityPush.m_DoThisOnce = m_DoThisOnce;
		cAbilityPush.m_StartingPoint = m_StartingPoint;
		cAbilityPush.m_AbilityPathProgressIndex = m_AbilityPathProgressIndex;
		cAbilityPush.DestroyedObstacles = DestroyedObstacles?.ToList();
		cAbilityPush.DestroyedObstaclesCurrentPath = DestroyedObstaclesCurrentPath?.ToList();
		if (State == EPushState.ActorFinalizePush)
		{
			cAbilityPush.PrepForReselectingTiles();
		}
	}

	public static CAbility CreateDefaultPush(int strength, int range, int numberOfTargets, bool isMonster, string animationOverload = "", CAbilityFilterContainer abilityfilter = null)
	{
		return CAbility.CreateAbility(EAbilityType.Push, strength, useSpecialBaseStat: false, range, numberOfTargets, 0, EAbilityFlag.None, abilityfilter ?? new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Enemy), animationOverload, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, "", "", "", "DefaultPush", new List<CEnhancement>(), null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, string.Empty, textOnly: false, noLayoutSpace: false, showRange: true, showTarget: true, showArea: true, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, EAbilityTargeting.Range, null, null, null, null, null, isMonster, string.Empty, null, isSubAbility: false, isInlineSubAbility: false, 0, 1, isTargetedAbility: true, 0f, CAbilityPull.EPullType.None, EAdditionalPushEffect.None, 0, 0, null, new List<CConditionalOverride>(), new CAbilityRequirements(), 0, string.Empty, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, new List<AbilityData.StatIsBasedOnXData>(), 0, string.Empty, new List<CItem.EItemSlot>(), new List<CItem.EItemSlotState>(), null, EAttackType.None, null, new List<EAbilityType>(), ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, immunityToAbilityTypes: new List<EAbilityType>(), immuneToAttackTypes: new List<EAttackType>(), consumes: new List<AbilityConsume>(), teleportData: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: true, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue);
	}

	private void RefreshNumberOfTargets()
	{
		if (!AbilityHasHappened)
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
	}

	private void CheckTargetsWithPotentialStrength()
	{
		m_PotentiallyValidActorsInRange.Clear();
		SetTargets(TargetingActor, RemainingPushes);
		if (ValidActorsInRange.Count != 0)
		{
			return;
		}
		int num = RemainingPushes;
		foreach (CActionAugmentation currentActionValidAugmentation in GameState.CurrentActionValidAugmentations)
		{
			foreach (CActionAugmentationOp augmentationOp in currentActionValidAugmentation.AugmentationOps)
			{
				if (augmentationOp.AbilityOverride != null && augmentationOp.ParentAbilityName == Name && augmentationOp.AbilityOverride.Strength.HasValue)
				{
					if (augmentationOp.AbilityOverride.StrengthIsBase.HasValue && augmentationOp.AbilityOverride.StrengthIsBase.Value)
					{
						num = augmentationOp.AbilityOverride.Strength.Value;
						break;
					}
					num += augmentationOp.AbilityOverride.Strength.Value;
				}
			}
			SetTargets(TargetingActor, num);
			m_PotentiallyValidActorsInRange.Clear();
			m_PotentiallyValidActorsInRange.AddRange(ValidActorsInRange);
			ValidActorsInRange.Clear();
		}
	}

	public override void Start(CActor targetingActor, CActor filterActor, CActor controllingActor = null)
	{
		Log.SimpleInfo("[PUSH ABILITY] - Ability Started");
		m_CurrentTarget = null;
		s_LastPushAbilityWasExecuted = false;
		base.Start(targetingActor, filterActor, controllingActor);
		if (TargetingActor.IsMirrorActor() && !IsMirrorAbility)
		{
			SetCancelAbilityFlag(cancel: true);
		}
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(TargetingActor, EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffForcedMovement))
		{
			m_ModifiedStrength += item.ReferenceStrength(this, TargetingActor);
		}
		DestroyedObstacles = new List<CObjectObstacle>();
		DestroyedObstaclesCurrentPath = new List<CObjectObstacle>();
		RemainingPushes = m_ModifiedStrength;
		m_CanUndo = false;
		m_CanSkip = false;
		m_State = EPushState.SelectPushTarget;
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || TargetingActor.Type != CActor.EType.Player)
		{
			m_CanUndo = false;
		}
		CheckTargetsWithPotentialStrength();
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (m_CurrentTarget == null || TargetingActor.Type != CActor.EType.Player)
		{
			if (Strength <= 0)
			{
				m_State = EPushState.ActorHasPushed;
			}
			else
			{
				m_State = EPushState.SelectPushTarget;
			}
		}
		m_PlayerWaypoints = null;
		m_PlayerDestination = null;
		m_ActorsToTarget.Clear();
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, PushedActorStats>();
		m_PushesBlocked = 0;
		if (AbilityHasBeenCancelled)
		{
			CAbilityAttack.ApplyRetaliateToParentAttack(this, TargetThisActorAutomatically);
		}
		LogEvent(ESESubTypeAbility.AbilityStart);
		SetHasStarted(val: true);
	}

	public void SetTargets(CActor targetingActor, int strength)
	{
		if (targetingActor == null)
		{
			return;
		}
		PushFromPoint = targetingActor.ArrayIndex;
		CTile cTile = GetMiscVar("PushFromPoint").GetObject<CTile>();
		if (cTile != null)
		{
			PushFromPoint = cTile.m_ArrayIndex;
		}
		if (AreaEffect != null)
		{
			m_ValidTilesInAreaEffect = CAreaEffect.GetValidTiles(TargetingActor, ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y], AreaEffect, 0f, flipped: false, AreaEffectFlipAxis, GameState.GetTilesInRange(targetingActor, Range, Targeting, emptyTilesOnly: false, ignoreBlocked: true), ref m_ValidTilesInAreaEffectIncludingBlocked, ref m_FlagsOfValidTiles, IgnoreAreaEffectLos, IgnoreDoorLocks);
		}
		if (TargetThisActorAutomatically != null)
		{
			if (!CAbility.ImmuneToAbility(TargetThisActorAutomatically, this))
			{
				ValidActorsInRange = new List<CActor> { TargetThisActorAutomatically };
				m_CurrentTarget = TargetThisActorAutomatically;
				if (TargetingActor.Type == CActor.EType.Player && TryPushToThisTileAutomatically == null)
				{
					m_State = EPushState.ActorIsSelectingPushTile;
				}
			}
			else
			{
				SetCancelAbilityFlag(cancel: true);
			}
		}
		else if (UseSubAbilityTargeting)
		{
			if (ParentAbility != null && AdditionalPushEffect == EAdditionalPushEffect.None && ParentAbility.SubAbilities != null)
			{
				foreach (CAbility subAbility in ParentAbility.SubAbilities)
				{
					if (subAbility is CAbilityPush { AdditionalPushEffect: not EAdditionalPushEffect.None } cAbilityPush)
					{
						AdditionalPushEffect = cAbilityPush.AdditionalPushEffect;
						AdditionalPushEffectDamage = cAbilityPush.AdditionalPushEffectDamage;
						AdditionalPushEffectXP = cAbilityPush.AdditionalPushEffectXP;
					}
				}
			}
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
				if (AdditionalPushEffect != EAdditionalPushEffect.TrackBlocked && (ScenarioScript.current == null || !ScenarioScript.current.AllowPushWhenNoPushTiles(cActor2)) && GetPushTiles(cActor2, cActor2.Type, cActor2.ArrayIndex, PushFromPoint, strength, AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles), this, allowAlliesToMoveThroughAllies: false).Count == 0)
				{
					flag = true;
				}
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
					m_State = EPushState.ActorIsSelectingPushTile;
				}
			}
		}
		if (TargetingActor.Type != CActor.EType.Player)
		{
			if (ValidActorsInRange.Count <= 0)
			{
				SetCancelAbilityFlag(cancel: true);
			}
			else
			{
				List<CActor> list = m_ValidActorsInRange.ToList();
				list.Sort((CActor x, CActor y) => (SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(x, TargetingActor) < SharedAbilityTargeting.GetDistanceBetweenActorsInHexes(y, TargetingActor)) ? 1 : GameState.s_ActorInitiativeComparer.Compare(x, y));
				ValidActorsInRange = list;
			}
		}
		if (UseSubAbilityTargeting || IsModifierAbility)
		{
			NumberTargets = ValidActorsInRange.Count;
		}
		RefreshNumberOfTargets();
		m_TargetSet = true;
	}

	public override bool Perform()
	{
		base.Perform();
		if (GameState.WaitingForMercenarySpecialMechanicSlotChoice)
		{
			return true;
		}
		Log.SimpleInfo("[PUSH ABILITY] - Performing state: " + m_State);
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
		case EPushState.SelectPushTarget:
			m_CanUndo = !AbilityHasHappened;
			if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || TargetingActor.Type != CActor.EType.Player)
			{
				m_CanUndo = false;
			}
			m_CanSkip = true;
			if (m_DoThisOnce)
			{
				CheckTargetsWithPotentialStrength();
				m_DoThisOnce = false;
			}
			if (TargetingActor.Type != CActor.EType.Player || TryPushToThisTileAutomatically != null)
			{
				if (AreaEffect == null)
				{
					int num3 = ValidActorsInRange.Count - 1;
					while (num3 >= 0 && ValidActorsInRange.Count > m_NumberTargetsRemaining)
					{
						ValidActorsInRange.RemoveAt(num3);
						num3--;
					}
				}
				if (ValidActorsInRange.Count > 0)
				{
					m_CurrentTarget = GetNextTarget();
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPushState.ActorIsPushing;
					Perform();
				}
				else if (m_PotentiallyValidActorsInRange.Count == 0)
				{
					SetCancelAbilityFlag(cancel: true);
					PhaseManager.NextStep();
				}
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
		case EPushState.ActorIsSelectingPushTile:
			m_CanUndo = true;
			if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || TargetingActor.Type != CActor.EType.Player)
			{
				m_CanUndo = false;
			}
			m_CanSkip = !MustMove;
			if (m_CurrentTarget == null || !ScenarioManager.Scenario.HasActor(m_CurrentTarget) || (GetPushTiles(m_CurrentTarget, m_CurrentTarget.Type, m_CurrentTarget.ArrayIndex, PushFromPoint, RemainingPushes, AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles), this, allowAlliesToMoveThroughAllies: false).Count == 0 && AdditionalPushEffect != EAdditionalPushEffect.TrackBlocked))
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
				bool flag2 = false;
				if (m_CurrentTarget != null)
				{
					int strength = 1;
					CAbilityMove.GetMoveBonuses(m_CurrentTarget, out var _, out var fly, out var _, out var _, ref strength);
					flag2 = fly;
				}
				bool flag3 = AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles);
				Log.SimpleInfo("[PUSH ABILITY] - Player location: " + m_CurrentTarget.ArrayIndex.ToString());
				Log.SimpleInfo("[PUSH ABILITY] - Player destination: " + m_PlayerDestination.m_ArrayIndex.ToString());
				Log.SimpleInfo("[PUSH ABILITY] - Flying: " + flag2);
				Log.SimpleInfo("[PUSH ABILITY] - IntoBlocked: " + flag3);
				Log.SimpleInfo("[PUSH ABILITY] - Generating path using passed in waypoints");
				string text = "[PUSH ABILITY] - In waypoints: ";
				foreach (CTile playerWaypoint in m_PlayerWaypoints)
				{
					text = text + "\n" + playerWaypoint.m_ArrayIndex.ToString();
				}
				Log.SimpleInfo(text);
				GetNextPathingPositionUsingExistingWaypoints(m_CurrentTarget, m_PlayerDestination, jump: false, flag2 | flag3, m_PlayerWaypoints, out m_PlayerWaypoints, out m_AbilityPath, ignoreDifficultTerrain: true, ignoreNegativeTiles: false, 0, ignoreMoveCost: true, logFailure: true);
				string text2 = "[PUSH ABILITY] - Out waypoints: ";
				foreach (CTile playerWaypoint2 in m_PlayerWaypoints)
				{
					text2 = text2 + "\n" + playerWaypoint2.m_ArrayIndex.ToString();
				}
				Log.SimpleInfo(text2);
				string text3 = "[PUSH ABILITY] - Out ability path: ";
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
					bool flag4 = false;
					for (int num5 = cTile2.m_Props.Count - 1; num5 >= 0; num5--)
					{
						CObjectProp cObjectProp2 = cTile2.m_Props[num5];
						if (cObjectProp2.ObjectType != ScenarioManager.ObjectImportType.Door && cObjectProp2.WillActivationKillActor(m_CurrentTarget))
						{
							flag4 = true;
							break;
						}
					}
					if (PathShouldEndDueToClosingDoor(num4) || GameState.WillMovingOnTileEndMovement(cTile2, CurrentTarget, this))
					{
						flag4 = true;
					}
					if (flag4)
					{
						Log.SimpleInfo("[PUSH ABILITY] - Truncating push path as prop activation will kill target along the way");
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
				Log.SimpleInfo("[PUSH ABILITY] - Updating Current Push Target (" + m_CurrentTarget.GetPrefabName() + " " + m_CurrentTarget.ID + ") array index to: " + m_PlayerDestination.m_ArrayIndex.ToString());
				m_CurrentTarget.ArrayIndex = m_PlayerDestination.m_ArrayIndex;
				m_State = EPushState.ActorIsPushing;
				if (AdditionalPushEffect.Equals(EAdditionalPushEffect.TrackBlocked) && m_AbilityPath.Count <= 0)
				{
					m_State = EPushState.ActorFinalizePush;
				}
				Perform();
			}
			else
			{
				m_TileSelected = false;
				if (!AbilityHasHappened)
				{
					CRefreshFirstAbility_MessageData message = new CRefreshFirstAbility_MessageData(TargetingActor)
					{
						m_IsFirstAbility = CPhaseAction.IsFirstAbility(this)
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				CActorIsSelectingPushTile_MessageData message2 = new CActorIsSelectingPushTile_MessageData(TargetingActor)
				{
					m_PushAbility = this
				};
				ScenarioRuleClient.MessageHandler(message2);
			}
			break;
		case EPushState.ActorIsPushing:
			m_CanUndo = false;
			m_CanSkip = false;
			if (TargetingActor.Type == CActor.EType.Player)
			{
				TargetingActor.Inventory.LockInSelectedItemsAndResetUnselected();
				ScenarioRuleClient.FirstAbilityStarted();
			}
			if (TargetingActor.Type == CActor.EType.Player && TryPushToThisTileAutomatically == null)
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
			if (TargetingActor.Type == CActor.EType.Player && !m_TileSelected && TryPushToThisTileAutomatically == null)
			{
				CheckNextTarget();
			}
			else if (TargetingActor.Type != CActor.EType.Player || TryPushToThisTileAutomatically != null)
			{
				m_CurrentTarget.Push(PushFromPoint, RemainingPushes, out m_AbilityPath, AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles), TryPushToThisTileAutomatically, this);
				if (m_CurrentTarget.ArrayIndex != m_StartingPoint)
				{
					CActorIsPushing_MessageData cActorIsPushing_MessageData = new CActorIsPushing_MessageData(TargetingActor);
					cActorIsPushing_MessageData.m_PushAbility = this;
					cActorIsPushing_MessageData.m_Waypoints = m_CurrentTarget.AIMoveFocusWaypoints;
					cActorIsPushing_MessageData.AnimOverload = AnimOverload;
					cActorIsPushing_MessageData.m_SkipPushAnim = ActorsTargeted.Count > 0;
					ScenarioRuleClient.MessageHandler(cActorIsPushing_MessageData);
				}
				else
				{
					m_State = EPushState.ActorHasPushed;
					ScenarioRuleClient.StepComplete(processImmediately: false, fromSRL: true);
				}
			}
			else
			{
				CActorIsPushing_MessageData cActorIsPushing_MessageData2 = new CActorIsPushing_MessageData(TargetingActor);
				cActorIsPushing_MessageData2.m_PushAbility = this;
				cActorIsPushing_MessageData2.m_Waypoints = m_PlayerWaypoints;
				cActorIsPushing_MessageData2.AnimOverload = AnimOverload;
				ScenarioRuleClient.MessageHandler(cActorIsPushing_MessageData2);
			}
			break;
		case EPushState.ActorHasPushed:
		{
			s_LastPushAbilityWasExecuted = true;
			LogEvent(ESESubTypeAbility.ApplyToActor);
			CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(TargetingActor);
			cActorHasPushed_MessageData.m_PushAbility = this;
			cActorHasPushed_MessageData.m_ActorBeingPushed = m_CurrentTarget;
			ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
			break;
		}
		case EPushState.ActorFinalizePush:
		{
			AbilityHasHappened = s_LastPushAbilityWasExecuted;
			for (int i = 0; i < m_AbilityPath.Count; i++)
			{
				Point firstPoint = ((i == 0) ? m_StartingPoint : m_AbilityPath[i - 1]);
				GameState.LostAdjacency(m_CurrentTarget, firstPoint, m_AbilityPath[i], this);
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
				CTile propTile = ScenarioManager.Tiles[point.X, point.Y];
				for (int num = propTile.m_Props.Count - 1; num >= 0; num--)
				{
					CObjectProp cObjectProp = propTile.m_Props[num];
					if (!CObjectProp.TryChooseToSkipTrap(m_CurrentTarget, cObjectProp) && !GetMiscVar("IgnorePropActivation").GetBool() && (cObjectProp.ObjectType != ScenarioManager.ObjectImportType.Door || CanOpenDoors))
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
				if (AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles))
				{
					CObjectObstacle cObjectObstacle = propTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
					if (cObjectObstacle == null)
					{
						cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(propTile.m_ArrayIndex, ref propTile);
					}
					if (cObjectObstacle != null && !cObjectObstacle.PropActorHasBeenAssigned)
					{
						cObjectObstacle.DestroyProp(SpawnDelay);
						DestroyedObstaclesCurrentPath.Add(cObjectObstacle);
						if (AdditionalPushEffectDamage > 0)
						{
							int health = m_CurrentTarget.Health;
							bool actorWasAsleep = m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
							GameState.ActorBeenDamaged(m_CurrentTarget, AdditionalPushEffectDamage, checkIfPlayerCanAvoidDamage: true, TargetingActor, this, AbilityType);
							if (GameState.ActorHealthCheck(TargetingActor, m_CurrentTarget, isTrap: false, isTerrain: false, actorWasAsleep))
							{
								CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(TargetingActor);
								cActorBeenDamaged_MessageData.m_ActorBeingDamaged = m_CurrentTarget;
								cActorBeenDamaged_MessageData.m_DamageAbility = null;
								cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
								cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
								ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
								m_ActorsToIgnore.Add(m_CurrentTarget);
							}
						}
					}
				}
				m_CurrentTarget.ArrayIndex = point;
				int distanceMoved = DistanceMoved + 1;
				DistanceMoved = distanceMoved;
				distanceMoved = RemainingPushes - 1;
				RemainingPushes = distanceMoved;
				UpdateCurrentTargetStat(DistanceMoved, m_StartingPoint, point);
				CActiveBonus.RefreshAllAuraActiveBonuses();
				m_CurrentTarget.CalculateAttackStrengthForUI();
				m_CurrentTarget.m_OnMovedListeners?.Invoke(this, m_CurrentTarget, new List<CActor>(), newActorCarried: false, 1, m_AbilityPathProgressIndex >= m_AbilityPath.Count, propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), propTile.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1, prevPosition);
				prevPosition = m_CurrentTarget.ArrayIndex;
				if (m_CurrentTarget.Health <= 0)
				{
					break;
				}
				if (AbilityHasBeenCancelled && AbilityInterruptCopy != null)
				{
					PhaseManager.StepComplete();
					return false;
				}
				if (m_State != EPushState.ActorFinalizePush)
				{
					return false;
				}
			}
			if (AdditionalPushEffect.Equals(EAdditionalPushEffect.TrackBlocked))
			{
				if (m_AbilityPath.Count > 0)
				{
					CTile cTile = ScenarioManager.Tiles[m_AbilityPath.Last().X, m_AbilityPath.Last().Y];
					int num2 = ScenarioManager.GetTileDistance(cTile.m_ArrayIndex.X, cTile.m_ArrayIndex.Y, PushFromPoint.X, PushFromPoint.Y) - 1;
					m_PushesBlocked = Math.Max(0, Strength - num2);
				}
				else
				{
					m_PushesBlocked = Strength;
				}
				if (m_PushesBlocked > 0 && AdditionalPushEffectDamage > 0)
				{
					int health2 = m_CurrentTarget.Health;
					bool actorWasAsleep2 = m_CurrentTarget.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
					GameState.ActorBeenDamaged(m_CurrentTarget, AdditionalPushEffectDamage * m_PushesBlocked, checkIfPlayerCanAvoidDamage: true, TargetingActor, this, AbilityType);
					if (GameState.ActorHealthCheck(TargetingActor, m_CurrentTarget, isTrap: false, isTerrain: false, actorWasAsleep2))
					{
						CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData2 = new CActorBeenDamaged_MessageData(TargetingActor);
						cActorBeenDamaged_MessageData2.m_ActorBeingDamaged = m_CurrentTarget;
						cActorBeenDamaged_MessageData2.m_DamageAbility = null;
						cActorBeenDamaged_MessageData2.m_ActorOriginalHealth = health2;
						cActorBeenDamaged_MessageData2.m_ActorWasAsleep = actorWasAsleep2;
						ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData2);
						m_ActorsToIgnore.Add(m_CurrentTarget);
					}
				}
			}
			if (m_NegativeConditions.Count > 0)
			{
				ProcessNegativeStatusEffects(m_CurrentTarget);
			}
			if (m_PositiveConditions.Count > 0)
			{
				ProcessPositiveStatusEffects(m_CurrentTarget);
			}
			if (StackInlineSubAbilities(m_CurrentTarget))
			{
				m_State = EPushState.CheckForNextTarget;
			}
			PhaseManager.StepComplete();
			return true;
		}
		case EPushState.CheckForNextTarget:
			CheckNextTarget();
			return true;
		}
		return false;
		IL_00aa:
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

	private void UpdateCurrentTargetStat(int movedCount, Point movedFrom, Point movedTo)
	{
		if (m_ActorDistanceMovedDictionary.TryGetValue(m_CurrentTarget, out var value))
		{
			value.DistanceMoved = movedCount;
		}
		else
		{
			value = new PushedActorStats(m_CurrentTarget.ActorLocKey(), (m_CurrentTarget is CEnemyActor cEnemyActor) ? (" " + cEnemyActor.ID) : "", movedCount, new TileIndex(movedFrom), new TileIndex(movedTo));
		}
		m_ActorDistanceMovedDictionary[m_CurrentTarget] = value;
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
		Log.SimpleInfo("[PUSH ABILITY] - Tile selected");
		bool flag = false;
		if (m_State == EPushState.SelectPushTarget)
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
					CPlayerSelectedTile_MessageData cPlayerSelectedTile_MessageData = new CPlayerSelectedTile_MessageData(TargetingActor);
					cPlayerSelectedTile_MessageData.m_Ability = this;
					ScenarioRuleClient.MessageHandler(cPlayerSelectedTile_MessageData);
					if (IsCurrentlyTargetingActors())
					{
						ScenarioRuleClient.MessageHandler(new CActorIsSelectingTargetingFocus_MessageData(TargetingActor)
						{
							m_IsPositive = false,
							m_TargetingAbility = this
						});
					}
				}
				if (TargetingActor.Type != CActor.EType.Player)
				{
					if (!m_TilesSelected.Contains(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]))
					{
						m_TilesSelected.Add(ScenarioManager.Tiles[m_CurrentTarget.ArrayIndex.X, m_CurrentTarget.ArrayIndex.Y]);
					}
					m_State = EPushState.ActorIsPushing;
					flag = true;
				}
				else if (TargetingActor.Type == CActor.EType.Player && UseSubAbilityTargeting)
				{
					m_State = EPushState.ActorIsSelectingPushTile;
					flag = true;
				}
			}
			else if (UseSubAbilityTargeting)
			{
				if (ValidActorsInRange.Count > 0)
				{
					ValidActorsInRange.RemoveAt(0);
				}
				if (ValidActorsInRange.Count > 0)
				{
					flag = true;
				}
				else
				{
					PhaseManager.NextStep();
				}
			}
		}
		else if (m_State == EPushState.ActorIsSelectingPushTile)
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
		Log.SimpleInfo("[PUSH ABILITY] - Tile deselected");
		bool flag = false;
		if (m_State == EPushState.SelectPushTarget && m_TilesSelected.Contains(selectedTile))
		{
			m_TilesSelected.Remove(selectedTile);
			ValidActorsInRange.Add(m_CurrentTarget);
			m_ActorsToTarget.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			flag = true;
		}
		if (flag && TargetingActor.Type == CActor.EType.Player)
		{
			CPlayerSelectedTile_MessageData message = new CPlayerSelectedTile_MessageData(TargetingActor)
			{
				m_Ability = this
			};
			ScenarioRuleClient.MessageHandler(message);
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

	public override void ClearTargets()
	{
		int undoNumberTargetsRemaining = m_UndoNumberTargetsRemaining;
		base.ClearTargets();
		m_CurrentTarget = null;
		m_ActorsToTarget.Clear();
		if (m_State == EPushState.SelectPushTarget)
		{
			m_NumberTargetsRemaining = undoNumberTargetsRemaining;
			m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
			Perform();
		}
		else if (m_State == EPushState.ActorIsSelectingPushTile)
		{
			m_State = EPushState.SelectPushTarget;
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
		if (m_State == EPushState.SelectPushTarget || m_State == EPushState.ActorIsSelectingPushTile)
		{
			return m_ActorsToTarget.Count > 0;
		}
		return false;
	}

	public override bool CanReceiveTileSelection()
	{
		if (base.CanReceiveTileSelection())
		{
			if (m_State != EPushState.SelectPushTarget)
			{
				return m_State == EPushState.ActorIsSelectingPushTile;
			}
			return true;
		}
		return false;
	}

	public override bool EnoughTargetsSelected()
	{
		if (m_State != EPushState.SelectPushTarget)
		{
			return true;
		}
		return m_ActorsToTarget.Count > 0;
	}

	public override bool RequiresWaypointSelection()
	{
		return m_State == EPushState.ActorIsSelectingPushTile;
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
		if (m_State == EPushState.ActorIsPushing && !ScenarioManager.Scenario.HasActor(m_CurrentTarget))
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
		if (!dontMoveState && m_State != EPushState.PushDone)
		{
			Log.SimpleInfo("[PUSH ABILITY] - Ability complete moving state from: " + m_State.ToString() + " to: " + (m_State + 1));
			m_State++;
		}
		else
		{
			Log.SimpleInfo("[PUSH ABILITY] - Ability complete check not moving state");
		}
		return m_State == EPushState.PushDone;
	}

	public override void AbilityPassStep()
	{
		if (TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
		{
			AbilityData.MiscAbilityData miscAbilityData = MiscAbilityData;
			if (miscAbilityData == null || miscAbilityData.IgnoreStun != true)
			{
				m_State = EPushState.PushDone;
				return;
			}
		}
		if (m_State == EPushState.SelectPushTarget)
		{
			m_State = EPushState.ActorFinalizePush;
		}
		if (m_State != EPushState.ActorIsSelectingPushTile)
		{
			return;
		}
		if (ParentAbility != null && ParentAbility is CAbilityAttack { MultiPassAttack: not false } cAbilityAttack && IsInlineSubAbility)
		{
			CActor actor = ((cAbilityAttack.OriginalTargetingActor != null) ? cAbilityAttack.OriginalTargetingActor : cAbilityAttack.TargetingActor);
			if (ScenarioManager.Scenario.HasActor(actor) && !m_CurrentTarget.IsDead)
			{
				cAbilityAttack.TryPerformRetaliate(cAbilityAttack.AttackSummary.FindTarget(m_CurrentTarget), checkWaitForAbilities: false);
			}
		}
		ProcessPositiveStatusEffects(m_CurrentTarget);
		ProcessNegativeStatusEffects(m_CurrentTarget);
	}

	public override string GetDescription()
	{
		return "Pull(" + Strength + ")";
	}

	public static List<CTile> GetPushTiles(CActor actorToPush, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked = false, CAbility ability = null, bool allowAlliesToMoveThroughAllies = true)
	{
		List<CTile> pushTiles_New = GetPushTiles_New(actorToPush, pushTarget, pushSource, remainingPushes, intoBlocked, allowAlliesToMoveThroughAllies, ability is CAbilityPush cAbilityPush && cAbilityPush.CanOpenDoors);
		SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(pushTiles_New, ability);
		return pushTiles_New;
	}

	private static List<CTile> GetPushTiles_New(CActor actorToPush, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked = false, bool allowAlliesToMoveThroughAllies = true, bool allowOpenDoors = false)
	{
		List<CTile> tiles = new List<CTile>();
		if (remainingPushes == 0)
		{
			return tiles;
		}
		List<Point> pathSoFar = new List<Point> { pushTarget };
		GetPushTilesRecursive_New(actorToPush, pushTarget, pushSource, remainingPushes, ref pathSoFar, ref tiles, intoBlocked, allowAlliesToMoveThroughAllies, allowOpenDoors);
		return tiles;
	}

	private static void GetPushTilesRecursive_New(CActor actorToPush, Point pushTarget, Point pushSource, int remainingPushes, ref List<Point> pathSoFar, ref List<CTile> tiles, bool intoBlocked, bool allowAlliesToMoveThroughAllies = true, bool allowOpenDoors = false)
	{
		int strength = 1;
		CAbilityMove.GetMoveBonuses(actorToPush, out var jump, out var fly, out var _, out var _, ref strength);
		bool flag = false;
		List<Point> list = new List<Point>();
		if (pathSoFar.Count > 1 && ScenarioManager.PathFinder.GetNode(pathSoFar[pathSoFar.Count - 1]).IsIceTerrain && !fly && !jump)
		{
			Point point = pathSoFar[pathSoFar.Count - 2];
			Point ptNext = pathSoFar[pathSoFar.Count - 1];
			CPathFinderContext cPathFinderContext = ScenarioManager.PathFinder.FloodFill_ToRange(actorToPush, point, 1, fly, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, ignoreDoorLocks: false, (CPathFinderContext context) =>
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
			ScenarioManager.PathFinder.FloodFill_ToRange(actorToPush, pushTarget, 1, intoBlocked | fly, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, allowOpenDoors, (CPathFinderContext context) =>
			{
				CAIMoveNew.AddDoorsToPathFindingContext(context, disallowDoorways: false, allowOpenDoors);
			}, isCalculatingDistance: false, ignoreCannotStop: true, EAbilityType.Push);
		}
		int num = Point.AxialDistance(Point.OddRToAxial(pushSource), Point.OddRToAxial(pushTarget));
		foreach (Point item in list)
		{
			CObjectProp cObjectProp = ScenarioManager.Tiles[item.X, item.Y].FindProp(ScenarioManager.ObjectImportType.Door);
			if ((cObjectProp != null && !cObjectProp.Activated && !allowOpenDoors) || Point.AxialDistance(Point.OddRToAxial(pushSource), Point.OddRToAxial(item)) <= num)
			{
				continue;
			}
			bool flag2 = true;
			int num2 = num;
			List<Point> list2 = CActor.FindCharacterPath(actorToPush, pushTarget, item, intoBlocked | fly, ignoreMoveCost: true, out var foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: true, jump, fly);
			if (!foundPath)
			{
				continue;
			}
			pathSoFar.Add(item);
			foreach (Point item2 in list2)
			{
				int num3 = Point.AxialDistance(Point.OddRToAxial(pushSource), Point.OddRToAxial(item2));
				if (num3 > num2)
				{
					num2 = num3;
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
			if (!tiles.Contains(ScenarioManager.Tiles[item.X, item.Y]) && (remainingPushes > 0 || ((remainingPushes == 0) & flag)))
			{
				CActor cActor = ScenarioManager.Scenario.FindActorAt(item);
				if (cActor == null || cActor.DoesNotBlock || (allowAlliesToMoveThroughAllies && CustomAbility.IsAlly(cActor, actorToPush)))
				{
					tiles.Add(ScenarioManager.Tiles[item.X, item.Y]);
				}
			}
			if (remainingPushes > 0)
			{
				int remainingPushes2 = ((ScenarioManager.PathFinder.GetNode(item).IsIceTerrain && !fly && !jump) ? remainingPushes : (remainingPushes - 1));
				GetPushTilesRecursive_New(actorToPush, item, pushSource, remainingPushes2, ref pathSoFar, ref tiles, intoBlocked, allowAlliesToMoveThroughAllies, allowOpenDoors);
			}
			pathSoFar.RemoveAt(pathSoFar.Count - 1);
		}
	}

	private static List<CTile> GetPushTiles_Old(CActor pushingActor, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked)
	{
		List<CTile> tiles = new List<CTile>();
		List<Point> list = CActor.FindCharacterPath(pushingActor, pushTarget, pushSource, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, pushingActor?.FlyingAfterConditions ?? false);
		if (!foundPath || list.Count == 0 || remainingPushes == 0)
		{
			return tiles;
		}
		GetPushTilesRecursive_Old(pushingActor, ref tiles, ScenarioManager.Tiles[pushTarget.X, pushTarget.Y], list.Count, targetType, pushSource, remainingPushes, 0, intoBlocked);
		return tiles;
	}

	private static void GetPushTilesRecursive_Old(CActor pushingActor, ref List<CTile> tiles, CTile currentTile, int distance, CActor.EType targetType, Point pushSource, int remainingPushes, int count = 0, bool intoBlocked = false)
	{
		List<CTile> list = new List<CTile>();
		count++;
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			List<Point> list2 = ScenarioManager.PathFinder.FindPath(pushingActor, currentTile.m_ArrayIndex, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
			if (!foundPath || list2.Count != 1)
			{
				continue;
			}
			List<Point> list3 = CActor.FindCharacterPath(pushingActor, pushSource, allAdjacentTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath2, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, pushingActor?.FlyingAfterConditions ?? false);
			bool flag = IsClosedDoor(allAdjacentTile);
			bool flag2 = false;
			if (pushingActor != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(pushingActor, out var _, out var fly, out var _, out var _, ref strength);
				flag2 = fly;
			}
			if (!(!flag & foundPath2) || list3.Count <= distance || tiles.Contains(allAdjacentTile) || !(!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked | flag2 | intoBlocked) || !((allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) == null || !((CObjectObstacle)allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump) | intoBlocked))
			{
				continue;
			}
			List<CActor> list4 = ScenarioManager.Scenario.FindActorsAt(allAdjacentTile.m_ArrayIndex);
			bool flag3 = false;
			bool flag4 = false;
			foreach (CActor item in list4)
			{
				if (item != null && !item.DoesNotBlock)
				{
					if (!CActor.AreActorsAllied(item.Type, targetType, item, null) || item.IsDowned)
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
		if (count >= remainingPushes)
		{
			return;
		}
		foreach (CTile item2 in list)
		{
			GetPushTilesRecursive_Old(pushingActor, ref tiles, item2, distance + 1, targetType, pushSource, remainingPushes, count, intoBlocked);
		}
	}

	public static List<CTile> GetPushTilesIncludingWallTiles(CActor pushingActor, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked = false)
	{
		List<CTile> tiles = new List<CTile>();
		List<Point> list = CActor.FindCharacterPath(pushingActor, pushTarget, pushSource, ignoreBlocked: true, ignoreMoveCost: true, out var foundPath, ignoreDifficultTerrain: false, ignoreNegativeTiles: false, 0, carryOtherActors: false, CActor.EType.Unknown, logFailure: false, int.MaxValue, ignoreCannotStop: false, jump: false, pushingActor?.FlyingAfterConditions ?? false);
		if (!foundPath || list.Count == 0 || remainingPushes == 0)
		{
			return tiles;
		}
		GetPushTilesIncludingWallsRecursive(pushingActor, ref tiles, ScenarioManager.Tiles[pushTarget.X, pushTarget.Y], list.Count, targetType, pushSource, remainingPushes, 0, intoBlocked);
		return tiles;
	}

	private static void GetPushTilesIncludingWallsRecursive(CActor pushingActor, ref List<CTile> tiles, CTile currentTile, int distance, CActor.EType targetType, Point pushSource, int remainingPushes, int count = 0, bool intoBlocked = false)
	{
		List<CTile> list = new List<CTile>();
		count++;
		foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(currentTile))
		{
			if (ScenarioManager.GetTileDistance(currentTile.m_ArrayIndex.X, currentTile.m_ArrayIndex.Y, allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y) != 1)
			{
				continue;
			}
			int tileDistance = ScenarioManager.GetTileDistance(pushSource.X, pushSource.Y, allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y);
			bool flag = IsClosedDoor(allAdjacentTile);
			bool flag2 = false;
			CNode cNode = ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y];
			if (cNode != null && !cNode.IsBridge && (allAdjacentTile.m_HexMap != currentTile.m_HexMap || (allAdjacentTile.m_Hex2Map != null && allAdjacentTile.m_Hex2Map != currentTile.m_HexMap)))
			{
				flag2 = true;
			}
			bool flag3 = false;
			if (pushingActor != null)
			{
				int strength = 1;
				CAbilityMove.GetMoveBonuses(pushingActor, out var _, out var fly, out var _, out var _, ref strength);
				flag3 = fly;
			}
			if (tileDistance <= distance || tiles.Contains(allAdjacentTile) || !(!ScenarioManager.PathFinder.Nodes[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y].Blocked | flag3 | intoBlocked) || !((allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle) == null || !((CObjectObstacle)allAdjacentTile.FindProp(ScenarioManager.ObjectImportType.Obstacle)).IgnoresFlyAndJump) | intoBlocked))
			{
				continue;
			}
			if (flag | flag2)
			{
				tiles.Add(allAdjacentTile);
				continue;
			}
			CActor cActor = ScenarioManager.Scenario.FindActorAt(allAdjacentTile.m_ArrayIndex);
			if (cActor == null)
			{
				list.Add(allAdjacentTile);
				tiles.Add(allAdjacentTile);
			}
			else if (CActor.AreActorsAllied(cActor.Type, targetType, cActor, null))
			{
				list.Add(allAdjacentTile);
			}
		}
		if (count >= remainingPushes)
		{
			return;
		}
		foreach (CTile item in list)
		{
			GetPushTilesRecursive_Old(pushingActor, ref tiles, item, distance + 1, targetType, pushSource, remainingPushes, count, intoBlocked);
		}
	}

	public static Point GetNextPathingPositionUsingExistingWaypoints(CActor actor, CTile destination, bool jump, bool fly, List<CTile> inWaypoints, out List<CTile> waypoints, out List<Point> arrayIndexPath, bool ignoreDifficultTerrain = false, bool ignoreNegativeTiles = false, int waterTilesThatCostZero = 0, bool ignoreMoveCost = false, bool logFailure = false)
	{
		arrayIndexPath = new List<Point>();
		waypoints = new List<CTile>();
		if (inWaypoints == null)
		{
			return destination.m_ArrayIndex;
		}
		arrayIndexPath.AddRange(CActor.FindCharacterPath(actor, actor.ArrayIndex, inWaypoints[0].m_ArrayIndex, jump | fly, ignoreMoveCost, out var foundPath, ignoreDifficultTerrain, ignoreNegativeTiles, waterTilesThatCostZero, carryOtherActors: false, CActor.EType.Unknown, logFailure, int.MaxValue, ignoreCannotStop: true, jump, fly));
		if (!foundPath)
		{
			Log.SimpleInfo("[GetNextPathingPosition] - Failed to find path to waypoint, from:" + actor.ArrayIndex.ToString() + " to:" + inWaypoints[0].m_ArrayIndex.ToString());
		}
		else
		{
			Log.SimpleInfo("[GetNextPathingPosition] - Found path to waypoint, from:" + actor.ArrayIndex.ToString() + " to:" + inWaypoints[0].m_ArrayIndex.ToString());
		}
		for (int i = 0; i < inWaypoints.Count - 1; i++)
		{
			arrayIndexPath.AddRange(CActor.FindCharacterPath(actor, inWaypoints[i].m_ArrayIndex, inWaypoints[i + 1].m_ArrayIndex, jump | fly, ignoreMoveCost, out foundPath, ignoreDifficultTerrain, ignoreNegativeTiles, waterTilesThatCostZero, carryOtherActors: false, CActor.EType.Unknown, logFailure, int.MaxValue, ignoreCannotStop: true, jump, fly));
		}
		if (arrayIndexPath.Count > 1)
		{
			ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.GetAdjacentPosition(actor.ArrayIndex.X, actor.ArrayIndex.Y, arrayIndexPath[0].X, arrayIndexPath[0].Y);
			for (int j = 1; j < arrayIndexPath.Count; j++)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(arrayIndexPath[j - 1].X, arrayIndexPath[j - 1].Y, arrayIndexPath[j].X, arrayIndexPath[j].Y);
				if (adjacentPosition != eAdjacentPosition)
				{
					waypoints.Add(ScenarioManager.Tiles[arrayIndexPath[j - 1].X, arrayIndexPath[j - 1].Y]);
					eAdjacentPosition = adjacentPosition;
				}
			}
		}
		waypoints.Add(destination);
		if (arrayIndexPath.Count > 0)
		{
			return arrayIndexPath[0];
		}
		return destination.m_ArrayIndex;
	}

	public static bool IsPushTileAdjacentToBlockedPushTile(CTile checkTile, CActor pushingActor, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked = false)
	{
		if (checkTile.m_ArrayIndex.X == 1 || checkTile.m_ArrayIndex.Y == 1 || checkTile.m_ArrayIndex.X == ScenarioManager.Width || checkTile.m_ArrayIndex.Y == ScenarioManager.Height)
		{
			return true;
		}
		List<CTile> pushTilesIncludingWallTiles = GetPushTilesIncludingWallTiles(pushingActor, targetType, pushTarget, pushSource, remainingPushes, intoBlocked);
		CNode cNode = ScenarioManager.PathFinder.Nodes[checkTile.m_ArrayIndex.X, checkTile.m_ArrayIndex.Y];
		foreach (CTile item in pushTilesIncludingWallTiles)
		{
			if (ScenarioManager.IsTileAdjacent(checkTile.m_ArrayIndex.X, checkTile.m_ArrayIndex.Y, item.m_ArrayIndex.X, item.m_ArrayIndex.Y, ignoreWalls: true))
			{
				CNode cNode2 = ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y];
				if (IsClosedDoor(item) || ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Blocked || !ScenarioManager.PathFinder.Nodes[item.m_ArrayIndex.X, item.m_ArrayIndex.Y].Walkable || (checkTile.m_HexMap != item.m_HexMap && !cNode.IsBridge && !cNode2.IsBridge))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void CheckNextTarget()
	{
		Log.SimpleInfo("[PUSH ABILITY] - Checking for next target");
		DestroyedObstacles.AddRange(DestroyedObstaclesCurrentPath);
		DestroyedObstaclesCurrentPath.Clear();
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
		if (m_NumberTargetsRemaining > 0 && m_ValidActorsInRange.Count > 0)
		{
			m_State = EPushState.SelectPushTarget;
			m_ValidActorsInRange.Remove(m_CurrentTarget);
			m_CurrentTarget = null;
			m_PlayerDestination = null;
			RemainingPushes = m_ModifiedStrength;
			DistanceMoved = 0;
			CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
			ScenarioRuleClient.MessageHandler(message);
			Perform();
		}
		else
		{
			m_State = EPushState.PushDone;
			if (((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility != null && ((CPhaseAction)PhaseManager.Phase).CurrentPhaseAbility.m_Ability.AbilityType == EAbilityType.Push)
			{
				PhaseManager.NextStep(!AbilityHasHappened);
			}
		}
	}

	public override void AbilityEnded()
	{
		Log.SimpleInfo("[PUSH ABILITY] - Ability Ended");
		foreach (CActor item in ActorsTargeted)
		{
			CAbilityAttack.ApplyRetaliateToParentAttack(this, item);
		}
		if (AdditionalPushEffectXP > 0)
		{
			if (AdditionalPushEffect.Equals(EAdditionalPushEffect.IntoObstacles) && DestroyedObstacles != null && DestroyedObstacles.Count > 0)
			{
				TargetingActor.GainXP(AdditionalPushEffectXP * DestroyedObstacles.Count, XPSourceKey);
			}
			else if (AdditionalPushEffect.Equals(EAdditionalPushEffect.TrackBlocked))
			{
				TargetingActor.GainXP(AdditionalPushEffectXP * m_PushesBlocked, XPSourceKey);
			}
			else
			{
				TargetingActor.GainXP(AdditionalPushEffectXP, XPSourceKey);
			}
		}
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		CheckForAdjacentSleepingActorsToAwaken();
		LogEvent(ESESubTypeAbility.AbilityEnded);
		base.AbilityEnded();
	}

	public override void Restart()
	{
		base.Restart();
		s_LastPushAbilityWasExecuted = false;
		if (m_ActorsToTarget != null)
		{
			m_ActorsToTarget.Clear();
		}
		m_CurrentTarget = null;
		RemainingPushes = m_ModifiedStrength;
		m_State = EPushState.SelectPushTarget;
		SetCancelAbilityFlag(cancel: false);
		if (AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) || TargetingActor.Type != CActor.EType.Player || NumberTargets == -1 || AreaEffect != null)
		{
			m_CanUndo = false;
		}
		CheckTargetsWithPotentialStrength();
		if (m_XpPerTargetData != null)
		{
			m_XpPerTargetData.Init();
		}
		if (Strength <= 0)
		{
			m_State = EPushState.ActorHasPushed;
		}
		m_PlayerWaypoints = null;
		m_PlayerDestination = null;
		m_ActorDistanceMovedDictionary = new Dictionary<CActor, PushedActorStats>();
		DestroyedObstacles = new List<CObjectObstacle>();
		DestroyedObstaclesCurrentPath = new List<CObjectObstacle>();
		m_PushesBlocked = 0;
		LogEvent(ESESubTypeAbility.AbilityRestart);
	}

	public void InterruptAndAddExtraPush(int extraPush)
	{
		CClearWaypointsAndTargets_MessageData message = new CClearWaypointsAndTargets_MessageData();
		ScenarioRuleClient.MessageHandler(message);
		PrepForReselectingTiles();
		RemainingPushes += extraPush;
		m_ModifiedStrength += extraPush;
		if (AbilityInterruptCopy != null)
		{
			bool hasEnded = AbilityInterruptCopy.HasEnded;
			CopyInterruptedProperties(AbilityInterruptCopy, 0, initialCopy: false, forced: false);
			AbilityInterruptCopy.HasEnded = hasEnded;
		}
	}

	public void PrepForReselectingTiles()
	{
		if (CurrentTarget != null && !CurrentTarget.IsDead)
		{
			m_PlayerWaypoints = null;
			m_PlayerDestination = null;
			m_State = EPushState.ActorIsSelectingPushTile;
			if (m_CachedAbilityPath != null && m_CachedAbilityPath.Count > 0)
			{
				int abilityPathProgressIndex = m_AbilityPathProgressIndex;
				RemainingPushes = m_CachedAbilityPath.Count - abilityPathProgressIndex;
				m_ModifiedStrength = RemainingPushes;
			}
		}
	}

	public override void LogEvent(ESESubTypeAbility subTypeAbility)
	{
		List<PushedActorStats> actorsDistanceMoved = m_ActorDistanceMovedDictionary.Values.ToList();
		SEventLogMessageHandler.AddEventLogMessage(new SEventAbilityPush(new SEventAbility.SEventAbilityBaseData(TargetingActor, subTypeAbility, this), m_State, actorsDistanceMoved));
	}

	private void InitAdditionalPushDamageSummary()
	{
		if (!ValidActorsInRange.Contains(CurrentTarget))
		{
			ValidActorsInRange.Add(CurrentTarget);
		}
		m_AdditionalPushDamageSummary = new CAttackSummary(this);
		m_NumberTargetsRemaining = NumberTargets + m_AdditionalPushDamageSummary.ActiveBonusAddTargetBuff;
		m_UndoNumberTargetsRemaining = m_NumberTargetsRemaining;
		Range += m_AdditionalPushDamageSummary.ActiveBonusAddRangeBuff;
	}

	public override bool IsPositive()
	{
		return false;
	}

	public override CAbilityOverride OverrideAbilityValues(CAbilityOverride abilityOverride, bool perform, CItem item = null, CAbilityFilterContainer filter = null, CActor targetActor = null, bool fromModifier = false)
	{
		int remainingPushes = RemainingPushes;
		int originalRange = m_OriginalRange;
		CAbilityOverride result = base.OverrideAbilityValues(abilityOverride, perform, item, filter, targetActor, fromModifier);
		if (RemainingPushes != remainingPushes || m_OriginalRange != originalRange)
		{
			CheckTargetsWithPotentialStrength();
		}
		RefreshNumberOfTargets();
		return result;
	}

	public override void UndoOverride(CAbilityOverride abilityOverride, bool perform, CItem item = null)
	{
		int remainingPushes = RemainingPushes;
		int originalRange = m_OriginalRange;
		base.UndoOverride(abilityOverride, perform, item);
		if (RemainingPushes != remainingPushes || m_OriginalRange != originalRange)
		{
			CheckTargetsWithPotentialStrength();
		}
	}

	public override void RecalcStatIsBasedOnX()
	{
		base.RecalcStatIsBasedOnX();
		SetStatBasedOnX(TargetingActor, StatIsBasedOnXEntries);
		RemainingPushes = Strength;
	}

	public override bool IsCurrentlyTargetingActors()
	{
		if (State == EPushState.SelectPushTarget)
		{
			return true;
		}
		return base.IsCurrentlyTargetingActors();
	}
}
