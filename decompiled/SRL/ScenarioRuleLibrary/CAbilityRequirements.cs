using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization.OptIn)]
public class CAbilityRequirements
{
	public enum EStartAbilityRequirementType
	{
		None,
		SubAbility,
		PreviousAbility,
		ThisAbility,
		ParentAbility
	}

	public static readonly EStartAbilityRequirementType[] StartAbilityRequirementTypes = (EStartAbilityRequirementType[])Enum.GetValues(typeof(EStartAbilityRequirementType));

	[JsonProperty]
	public EStartAbilityRequirementType StartAbilityRequirementType;

	[JsonProperty]
	public int XP;

	[JsonProperty]
	public int RequirementRange;

	[JsonProperty]
	public int RoundIsInterval;

	[JsonProperty]
	public int ActiveBonusInternalRoundInterval;

	[JsonProperty]
	public CAbilityFilterContainer RequirementActorFilter;

	[JsonProperty]
	public CPlayerActor.EAgility RequiredAgility;

	[JsonProperty]
	public CPlayerActor.EForm RequiredForm;

	[JsonProperty]
	public CPlayerActor.EPressure RequiredPressure;

	[JsonProperty]
	public bool? AbilityMovementWasStraightLine;

	[JsonProperty]
	public bool? OccupyingObstacleHex;

	[JsonProperty]
	public bool? EnterHazardousTerrain;

	[JsonProperty]
	public bool? EnterDifficultTerrain;

	[JsonProperty]
	public bool? ElementsConsumedThisAction;

	[JsonProperty]
	public bool? OnYourTurn;

	[JsonProperty]
	public int? ActiveBonusAtTrackerIndex;

	[JsonProperty]
	public int? ActiveBonusGreaterOrEqualToTrackerIndex;

	[JsonProperty]
	public bool? AbilityHasHappened;

	[JsonProperty]
	public bool? ActionHasHappened;

	[JsonProperty]
	public CEqualityFilter HP;

	[JsonProperty]
	public CEqualityFilter HPPercentOfMax;

	[JsonProperty]
	public CEqualityFilter HexesMovedThisTurn;

	[JsonProperty]
	public CEqualityFilter ActorsTargetedByAbility;

	[JsonProperty]
	public CEqualityFilter ActorsKilledByAbility;

	[JsonProperty]
	public CEqualityFilter ActorsKilledThisAction;

	[JsonProperty]
	public CEqualityFilter ActorsKilledThisTurn;

	[JsonProperty]
	public CEqualityFilter ActorsKilledThisRound;

	[JsonProperty]
	public CEqualityFilter AllActorsKilledThisRound;

	[JsonProperty]
	public CEqualityFilter TrophyTokensGainedThisRound;

	[JsonProperty]
	public CEqualityFilter ValidActorsInRangeOfAddedAbilityRange;

	[JsonProperty]
	public CEqualityFilter ValidActorsInRangeOfRequirementRange;

	[JsonProperty]
	public CEqualityFilter ActiveSpawnersInRange;

	[JsonProperty]
	public CEqualityFilter InactiveSpawnersInRange;

	[JsonProperty]
	public CEqualityFilter LootInRange;

	[JsonProperty]
	public CEqualityFilter DoorsOpenedByAbility;

	[JsonProperty]
	public CEqualityFilter WallsAdjacent;

	[JsonProperty]
	public CEqualityFilter ActorHasPerformedValidAbilityTypesThisTurn;

	[JsonProperty]
	public CEqualityFilter ActorHasPerformedValidAbilityTypesThisAction;

	[JsonProperty]
	public CEqualityFilter TimeTokens;

	[JsonProperty]
	public CEqualityFilter TrophyTokens;

	[JsonProperty]
	public CEqualityFilter InitiativeIs;

	[JsonProperty]
	public CEqualityFilter SingleUsePreventDamageCount;

	[JsonProperty]
	public CEqualityFilter DiscardedAbilityCardsCount;

	[JsonProperty]
	public CEqualityFilter ObstaclesDestroyedThisAction;

	[JsonProperty]
	public List<CCondition.ENegativeCondition> DoesntHaveNegativeCondition;

	[JsonProperty]
	public List<ScenarioManager.ObjectImportType> ObjectImportTypes;

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> InertElements;

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> WaningElements;

	[JsonProperty]
	public List<ElementInfusionBoardManager.EElement> StrongElements;

	[JsonProperty]
	public List<CAbility.EAbilityType> AbilityTypes;

	[JsonProperty]
	public List<string> ActorGuids;

	[JsonProperty]
	public List<(string, TValue)> Vars;

	[JsonProperty]
	public bool? IsElite;

	[JsonProperty]
	public bool? IsLongRest;

	[JsonProperty]
	public bool ValidateWhileSelected;

	[JsonProperty]
	public bool? ForgoTopAction { get; set; }

	[JsonProperty]
	public bool? ForgoBottomAction { get; set; }

	public CAbilityRequirements()
	{
		RequirementActorFilter = CAbilityFilterContainer.CreateDefaultFilter();
	}

	public CAbilityRequirements Copy()
	{
		return new CAbilityRequirements
		{
			StartAbilityRequirementType = StartAbilityRequirementType,
			XP = XP,
			RequirementRange = RequirementRange,
			RoundIsInterval = RoundIsInterval,
			ActiveBonusInternalRoundInterval = ActiveBonusInternalRoundInterval,
			RequirementActorFilter = RequirementActorFilter.Copy(),
			RequiredAgility = RequiredAgility,
			RequiredForm = RequiredForm,
			RequiredPressure = RequiredPressure,
			AbilityMovementWasStraightLine = AbilityMovementWasStraightLine,
			OccupyingObstacleHex = OccupyingObstacleHex,
			EnterHazardousTerrain = EnterHazardousTerrain,
			EnterDifficultTerrain = EnterDifficultTerrain,
			ElementsConsumedThisAction = ElementsConsumedThisAction,
			OnYourTurn = OnYourTurn,
			ActiveBonusAtTrackerIndex = ActiveBonusAtTrackerIndex,
			ActiveBonusGreaterOrEqualToTrackerIndex = ActiveBonusGreaterOrEqualToTrackerIndex,
			AbilityHasHappened = AbilityHasHappened,
			ActionHasHappened = ActionHasHappened,
			HP = HP,
			HPPercentOfMax = HPPercentOfMax,
			HexesMovedThisTurn = HexesMovedThisTurn,
			ActorsTargetedByAbility = ActorsTargetedByAbility,
			ActorsKilledByAbility = ActorsKilledByAbility,
			ActorsKilledThisAction = ActorsKilledThisAction,
			ActorsKilledThisTurn = ActorsKilledThisTurn,
			ActorsKilledThisRound = ActorsKilledThisRound,
			AllActorsKilledThisRound = AllActorsKilledThisRound,
			TrophyTokensGainedThisRound = TrophyTokensGainedThisRound,
			ValidActorsInRangeOfAddedAbilityRange = ValidActorsInRangeOfAddedAbilityRange,
			ValidActorsInRangeOfRequirementRange = ValidActorsInRangeOfRequirementRange,
			ActiveSpawnersInRange = ActiveSpawnersInRange,
			InactiveSpawnersInRange = InactiveSpawnersInRange,
			LootInRange = LootInRange,
			DoorsOpenedByAbility = DoorsOpenedByAbility,
			WallsAdjacent = WallsAdjacent,
			ActorHasPerformedValidAbilityTypesThisTurn = ActorHasPerformedValidAbilityTypesThisTurn,
			ActorHasPerformedValidAbilityTypesThisAction = ActorHasPerformedValidAbilityTypesThisAction,
			TimeTokens = TimeTokens,
			TrophyTokens = TrophyTokens,
			DoesntHaveNegativeCondition = DoesntHaveNegativeCondition,
			InitiativeIs = InitiativeIs,
			SingleUsePreventDamageCount = SingleUsePreventDamageCount,
			DiscardedAbilityCardsCount = DiscardedAbilityCardsCount,
			ObstaclesDestroyedThisAction = ObstaclesDestroyedThisAction,
			ObjectImportTypes = ObjectImportTypes?.ToList(),
			InertElements = InertElements?.ToList(),
			WaningElements = WaningElements?.ToList(),
			StrongElements = StrongElements?.ToList(),
			AbilityTypes = AbilityTypes?.ToList(),
			ActorGuids = ActorGuids?.ToList(),
			ForgoTopAction = ForgoTopAction,
			ForgoBottomAction = ForgoBottomAction,
			Vars = Vars,
			IsElite = IsElite,
			IsLongRest = IsLongRest,
			ValidateWhileSelected = ValidateWhileSelected
		};
	}

	public bool MeetsAbilityRequirements(CActor actor, CAbility ability, CVarsContext context = null, CItem item = null, CActiveBonus activeBonus = null)
	{
		if (ability == null && context != null)
		{
			return CheckVars(context, actor, item, activeBonus);
		}
		if (RoundIsInterval != 0 && ScenarioManager.CurrentScenarioState.RoundNumber % RoundIsInterval != 0)
		{
			return false;
		}
		bool num = ability != null && (ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.NotAffectedByIgnoreSlowEffects) != 0;
		bool flag = actor.ActiveBonuses.FirstOrDefault((CActiveBonus b) => (b.Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.IgnoreSlowEffects) != 0) != null;
		if ((!num & flag) && RequiredAgility == CPlayerActor.EAgility.Slow)
		{
			return false;
		}
		bool num2 = ability != null && (ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.NotAffectedByIgnoreFastEffects) != 0;
		bool flag2 = actor.ActiveBonuses.FirstOrDefault((CActiveBonus b) => (b.Ability.ActiveBonusData.Flags & AbilityData.EActiveBonusFlag.IgnoreFastEffects) != 0) != null;
		if ((!num2 & flag2) && RequiredAgility == CPlayerActor.EAgility.Fast)
		{
			return false;
		}
		CPlayerActor cPlayerActor = actor as CPlayerActor;
		if (RequiredAgility != CPlayerActor.EAgility.None)
		{
			if (cPlayerActor == null)
			{
				cPlayerActor = actor.GetMirrorActor(includeMirroredClone: true) as CPlayerActor;
			}
			if (cPlayerActor != null && cPlayerActor.CalcAgility() != RequiredAgility)
			{
				return false;
			}
		}
		if (RequiredPressure != CPlayerActor.EPressure.None && cPlayerActor != null && cPlayerActor.Pressure != RequiredPressure)
		{
			return false;
		}
		if (cPlayerActor != null && RequiredForm != CPlayerActor.EForm.None && RequiredForm != cPlayerActor.Form)
		{
			return false;
		}
		if (cPlayerActor != null && TimeTokens != null && !TimeTokens.Compare(cPlayerActor.GetTimeTokens()))
		{
			return false;
		}
		if (cPlayerActor != null && TrophyTokens != null && !TrophyTokens.Compare(cPlayerActor.GetTrophyTokens()))
		{
			return false;
		}
		if (cPlayerActor != null && DiscardedAbilityCardsCount != null && !DiscardedAbilityCardsCount.Compare(cPlayerActor.CharacterClass.DiscardedAbilityCards.Count))
		{
			return false;
		}
		if (actor != null && SingleUsePreventDamageCount != null && !SingleUsePreventDamageCount.Compare(actor.GetSingleUsePreventDamageCount()))
		{
			return false;
		}
		if (DoesntHaveNegativeCondition != null)
		{
			foreach (CCondition.ENegativeCondition item5 in DoesntHaveNegativeCondition)
			{
				if (item5 != CCondition.ENegativeCondition.NA && actor.Tokens.HasKey(item5))
				{
					return false;
				}
			}
		}
		if (AbilityMovementWasStraightLine.HasValue && AbilityMovementWasStraightLine.Value && ability != null && ability is CAbilityMove cAbilityMove)
		{
			List<Point> list = new List<Point>();
			list.Add(cAbilityMove.StartingPoint);
			list.AddRange(cAbilityMove.AllArrayIndexOnPathIncludingRepeatsCopy);
			if (list.Count > 2)
			{
				ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(list[0].X, list[0].Y, list[1].X, list[1].Y);
				for (int num3 = 1; num3 < list.Count - 1; num3++)
				{
					ScenarioManager.EAdjacentPosition adjacentPosition2 = ScenarioManager.GetAdjacentPosition(list[num3].X, list[num3].Y, list[num3 + 1].X, list[num3 + 1].Y);
					if (adjacentPosition != adjacentPosition2)
					{
						return false;
					}
				}
			}
			else if (cAbilityMove.AllArrayIndexOnPathIncludingRepeatsCopy.Count <= 0)
			{
				return false;
			}
		}
		if (actor != null && !MeetsAbilityRequirements(ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]))
		{
			return false;
		}
		if (ElementsConsumedThisAction.HasValue && ElementsConsumedThisAction.Value && PhaseManager.CurrentPhase.Type == CPhase.PhaseType.Action && !((CPhaseAction)PhaseManager.CurrentPhase).HasConsumedElements)
		{
			return false;
		}
		if (actor != null && OnYourTurn.HasValue && OnYourTurn.Value)
		{
			if (PhaseManager.CurrentPhase.Type < CPhase.PhaseType.StartTurn || PhaseManager.CurrentPhase.Type > CPhase.PhaseType.EndTurn)
			{
				return false;
			}
			if (GameState.TurnActor != actor)
			{
				return false;
			}
		}
		if (actor != null && actor is CEnemyActor cEnemyActor && IsElite.HasValue && cEnemyActor.MonsterClass.NonEliteVariant != null != IsElite.Value)
		{
			return false;
		}
		if (actor != null && actor is CPlayerActor cPlayerActor2 && IsLongRest.HasValue && cPlayerActor2.CharacterClass.LongRest != IsLongRest.Value)
		{
			return false;
		}
		if (AbilityHasHappened.HasValue && ability != null && AbilityHasHappened.Value != ability.AbilityHasHappened)
		{
			return false;
		}
		if (ActionHasHappened.HasValue && actor != null && ActionHasHappened.Value != actor.LastActionHasHappened)
		{
			return false;
		}
		if (ForgoTopAction.HasValue && ForgoTopAction.Value)
		{
			if (GameState.IsPerformingCardOutOfPhase)
			{
				return false;
			}
			if (actor != null && actor is CPlayerActor cPlayerActor3 && actor == GameState.InternalCurrentActor && (GameState.GetRemainingActions(cPlayerActor3, top: true) == 0 || GameState.IsActionFlagSet(GameState.EActionFlag.TopForgone) || cPlayerActor3.CharacterClass.LongRest || cPlayerActor3.CharacterClass.HasLongRested))
			{
				return false;
			}
		}
		if (ForgoBottomAction.HasValue && ForgoBottomAction.Value)
		{
			if (GameState.IsPerformingCardOutOfPhase)
			{
				return false;
			}
			if (actor != null && actor is CPlayerActor cPlayerActor4 && actor == GameState.InternalCurrentActor && (GameState.GetRemainingActions(cPlayerActor4, top: false) == 0 || GameState.IsActionFlagSet(GameState.EActionFlag.BottomForgone) || cPlayerActor4.CharacterClass.LongRest || cPlayerActor4.CharacterClass.HasLongRested))
			{
				return false;
			}
		}
		if (HP != null && !HP.Compare(actor.Health))
		{
			return false;
		}
		if (HPPercentOfMax != null && !HPPercentOfMax.Compare(actor.Health, actor.OriginalMaxHealth))
		{
			return false;
		}
		if (HexesMovedThisTurn != null && !HexesMovedThisTurn.Compare(GameState.HexesMovedThisTurn.Count))
		{
			return false;
		}
		if (ability != null && ActorsTargetedByAbility != null)
		{
			if (ability.Targeting == CAbility.EAbilityTargeting.Attacker)
			{
				SharedAbilityTargeting.GetValidActorsInRange(ability);
				if (!ActorsTargetedByAbility.Compare(ability.ValidActorsInRange.Count))
				{
					return false;
				}
			}
			else if (!ActorsTargetedByAbility.Compare(ability.ActorsTargeted.Count((CActor x) => RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
			{
				return false;
			}
		}
		if (ability != null && ActorsKilledByAbility != null && !ActorsKilledByAbility.Compare(ability.ActorsTargeted.Count((CActor x) => x.IsDead && RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (ActorsKilledThisAction != null && !ActorsKilledThisAction.Compare(CPhaseAction.ActorsKilledThisAction.Count((CActor x) => x.KilledByActor == actor && RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (ActorsKilledThisTurn != null && !ActorsKilledThisTurn.Compare(GameState.ActorsKilledThisTurn.Count((CActor x) => x.KilledByActor == actor && RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (ActorsKilledThisRound != null && !ActorsKilledThisRound.Compare(GameState.ActorsKilledThisRound.Count((CActor x) => x.KilledByActor == actor && RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (AllActorsKilledThisRound != null && !AllActorsKilledThisRound.Compare(GameState.ActorsKilledThisRound.Count((CActor x) => RequirementActorFilter.IsValidTarget(x, actor, isTargetedAbility: false, useTargetOriginalType: false, false))))
		{
			return false;
		}
		if (TrophyTokensGainedThisRound != null && !TrophyTokensGainedThisRound.Compare(GameState.TrophyTokensGainedThisRound))
		{
			return false;
		}
		if (ObstaclesDestroyedThisAction != null && !ObstaclesDestroyedThisAction.Compare(CPhaseAction.ObstaclesDestroyedThisAction))
		{
			return false;
		}
		if (ValidActorsInRangeOfAddedAbilityRange != null && ability != null && actor != null)
		{
			List<CActor> actorsInRange = GameState.GetActorsInRange(actor.ArrayIndex, actor, ability.Range, null, RequirementActorFilter, null, null, isTargetedAbility: false, null, false);
			if (!ValidActorsInRangeOfAddedAbilityRange.Compare(actorsInRange.Count))
			{
				return false;
			}
		}
		if (ValidActorsInRangeOfRequirementRange != null)
		{
			if (actor == null)
			{
				return false;
			}
			List<CActor> actorsInRange2 = GameState.GetActorsInRange(actor, actor, RequirementRange, null, RequirementActorFilter, null, null, isTargetedAbility: false, null, false);
			if (!ValidActorsInRangeOfRequirementRange.Compare(actorsInRange2.Count))
			{
				return false;
			}
		}
		if (ActiveSpawnersInRange != null && ability != null && actor != null)
		{
			List<CTile> tilesInRange = GameState.GetTilesInRange(actor, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			CTile item2 = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			if (!tilesInRange.Contains(item2))
			{
				tilesInRange.Add(item2);
			}
			tilesInRange = tilesInRange.Where((CTile x) => x.m_Spawners.Any((CSpawner y) => y is CInteractableSpawner && y.IsActive)).ToList();
			if (!ActiveSpawnersInRange.Compare(tilesInRange.Count))
			{
				return false;
			}
		}
		if (InactiveSpawnersInRange != null && ability != null && actor != null)
		{
			List<CTile> tilesInRange2 = GameState.GetTilesInRange(actor, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			CTile item3 = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			if (!tilesInRange2.Contains(item3))
			{
				tilesInRange2.Add(item3);
			}
			tilesInRange2 = tilesInRange2.Where((CTile x) => x.m_Spawners.Any((CSpawner y) => y is CInteractableSpawner && !y.IsActive)).ToList();
			if (!InactiveSpawnersInRange.Compare(tilesInRange2.Count))
			{
				return false;
			}
		}
		if (LootInRange != null && ability != null && actor != null)
		{
			List<CTile> tilesInRange3 = GameState.GetTilesInRange(actor, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			CTile item4 = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			if (!tilesInRange3.Contains(item4))
			{
				tilesInRange3.Add(item4);
			}
			List<CObjectProp> list2 = new List<CObjectProp>();
			foreach (CTile item6 in tilesInRange3)
			{
				list2.AddRange(item6.m_Props.FindAll((CObjectProp x) => x.ObjectType == ScenarioManager.ObjectImportType.Chest || x.ObjectType == ScenarioManager.ObjectImportType.GoalChest || x.ObjectType == ScenarioManager.ObjectImportType.MoneyToken || x.ObjectType == ScenarioManager.ObjectImportType.CarryableQuestItem || x.ObjectType == ScenarioManager.ObjectImportType.Resource));
			}
			if (ObjectImportTypes != null)
			{
				list2.RemoveAll((CObjectProp x) => !ObjectImportTypes.Contains(x.ObjectType));
			}
			if (!LootInRange.Compare(list2.Count))
			{
				return false;
			}
		}
		if (DoorsOpenedByAbility != null)
		{
			if (ability == null || !(ability is CAbilityMove cAbilityMove2))
			{
				return false;
			}
			if (!DoorsOpenedByAbility.Compare(cAbilityMove2.DoorsOpened))
			{
				return false;
			}
		}
		if (WallsAdjacent != null && actor != null)
		{
			CTile cTile = ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y];
			List<CTile> list3 = new List<CTile>();
			for (int num4 = 1; num4 < 7; num4++)
			{
				ScenarioManager.EAdjacentPosition eposition = (ScenarioManager.EAdjacentPosition)num4;
				CTile adjacentTile = ScenarioManager.GetAdjacentTile(actor.ArrayIndex.X, actor.ArrayIndex.Y, eposition);
				if (adjacentTile != null)
				{
					CNode cNode = ScenarioManager.PathFinder.Nodes[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
					if ((!cNode.Walkable && !cNode.IsBridge) || !cTile.IsMapShared(adjacentTile))
					{
						list3.Add(adjacentTile);
					}
				}
			}
			if (!WallsAdjacent.Compare(list3.Count))
			{
				return false;
			}
		}
		if (actor != null && InitiativeIs != null && !InitiativeIs.Compare(actor.Initiative()))
		{
			return false;
		}
		if (actor != null && ActorHasPerformedValidAbilityTypesThisTurn != null && AbilityTypes != null)
		{
			List<CAbility.EAbilityType> list4 = actor.AbilityTypesPerformedThisTurn.Intersect(AbilityTypes).ToList();
			if (!ActorHasPerformedValidAbilityTypesThisTurn.Compare(list4.Count))
			{
				return false;
			}
		}
		if (actor != null && ActorHasPerformedValidAbilityTypesThisAction != null && AbilityTypes != null)
		{
			List<CAbility.EAbilityType> abilityTypesPerformedThisAction = actor.AbilityTypesPerformedThisAction;
			if (abilityTypesPerformedThisAction == null)
			{
				return false;
			}
			List<CAbility.EAbilityType> list5 = abilityTypesPerformedThisAction.Intersect(AbilityTypes).ToList();
			if (!ActorHasPerformedValidAbilityTypesThisAction.Compare(list5.Count))
			{
				return false;
			}
		}
		if (actor != null && ActorGuids != null && !ActorGuids.Contains(actor.ActorGuid))
		{
			return false;
		}
		if (InertElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement inertElement in InertElements)
			{
				if (ElementInfusionBoardManager.ElementColumn(inertElement, actor) != ElementInfusionBoardManager.EColumn.Inert)
				{
					return false;
				}
			}
		}
		if (WaningElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement waningElement in WaningElements)
			{
				if (ElementInfusionBoardManager.ElementColumn(waningElement, actor) != ElementInfusionBoardManager.EColumn.Waning)
				{
					return false;
				}
			}
		}
		if (StrongElements != null)
		{
			foreach (ElementInfusionBoardManager.EElement strongElement in StrongElements)
			{
				if (ElementInfusionBoardManager.ElementColumn(strongElement, actor) != ElementInfusionBoardManager.EColumn.Strong)
				{
					return false;
				}
			}
		}
		if (!CheckVars(ability, actor, item, activeBonus, context))
		{
			return false;
		}
		return true;
	}

	public bool MeetsAbilityRequirements(CTile tile)
	{
		if (OccupyingObstacleHex.HasValue && OccupyingObstacleHex.Value)
		{
			CObjectObstacle cObjectObstacle = tile.FindProp(ScenarioManager.ObjectImportType.Obstacle) as CObjectObstacle;
			if (cObjectObstacle == null)
			{
				cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref tile);
			}
			if (cObjectObstacle == null)
			{
				CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
				if (cNode != null && cNode.Blocked)
				{
					List<CTile> allAdjacentTiles = ScenarioManager.GetAllAdjacentTiles(tile);
					for (int i = 0; i < allAdjacentTiles.Count; i++)
					{
						if (allAdjacentTiles[i].m_Props.Count > 0 && allAdjacentTiles[i].FindProp(ScenarioManager.ObjectImportType.Obstacle) != null)
						{
							return true;
						}
					}
				}
				return false;
			}
		}
		return true;
	}

	public bool MeetsActiveBonusRequirements(CActiveBonus bonus)
	{
		if (ActiveBonusInternalRoundInterval != 0 && (bonus.InternalRoundNumber - 1) % ActiveBonusInternalRoundInterval != 0)
		{
			return false;
		}
		if (ActiveBonusAtTrackerIndex.HasValue && bonus.HasTracker && bonus.TrackerIndex != ActiveBonusAtTrackerIndex.Value)
		{
			return false;
		}
		if (ActiveBonusGreaterOrEqualToTrackerIndex.HasValue && bonus.HasTracker && bonus.TrackerIndex < ActiveBonusGreaterOrEqualToTrackerIndex.Value)
		{
			return false;
		}
		return true;
	}

	public bool CheckVars(CAbility ability, CActor actor, CItem item = null, CActiveBonus activeBonus = null, CVarsContext context = null)
	{
		if (Vars == null)
		{
			return true;
		}
		CVars cVars = new CVars();
		if (context != null)
		{
			CVarsContext cVarsContext = ability?.VarsContext;
			while (cVarsContext?.parentContext != null)
			{
				cVarsContext = cVarsContext.parentContext;
			}
			if (context != cVarsContext)
			{
				context.parentContext = cVarsContext;
			}
		}
		CVarsContext ctx = new CVarsContext(cVars, context ?? ability?.VarsContext);
		cVars.Set("Ability", new TValue(ability));
		return CheckVars(ctx, actor, item, activeBonus);
	}

	public bool CheckVars(CVarsContext ctx, CActor actor, CItem item = null, CActiveBonus activeBonus = null)
	{
		if (Vars == null)
		{
			return true;
		}
		CVars cVars = ctx.obj as CVars;
		if (cVars == null)
		{
			cVars = new CVars();
			ctx = new CVarsContext(cVars, ctx);
		}
		cVars.Set("Item", new TValue(item));
		cVars.Set("Actor", new TValue(actor));
		cVars.Set("CheckingBonus", new TValue(activeBonus));
		foreach (var var in Vars)
		{
			string item2 = var.Item1;
			TValue item3 = var.Item2;
			TValue value = item3.Calc(ctx);
			if (item2.StartsWith("Validate", StringComparison.Ordinal) && !value.GetBool())
			{
				return false;
			}
			cVars.Set(item2, value);
		}
		return true;
	}

	public void PayRequirementCost(int roundCardsCount)
	{
		if (ForgoTopAction.HasValue && ForgoTopAction.Value)
		{
			ForegoAction(GameState.CurrentActor as CPlayerActor, top: true, roundCardsCount);
		}
		if (ForgoBottomAction.HasValue && ForgoBottomAction.Value)
		{
			ForegoAction(GameState.CurrentActor as CPlayerActor, top: false, roundCardsCount);
		}
	}

	private void ForegoAction(CPlayerActor player, bool top, int roundCardsCount)
	{
		if (player != null)
		{
			switch (player.CharacterClass.RoundAbilityCards.Count)
			{
			case 0:
				return;
			case 1:
			{
				CAbilityCard cardToForego = player.CharacterClass.RoundAbilityCards[0];
				ForegoAction(player, top, cardToForego, roundCardsCount);
				return;
			}
			}
			PhaseManager.StartAbilities(CAbility.CreateAbility(CAbility.EAbilityType.SelectCard, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, helpBoxTooltipLocKey: top ? "GUI_CHOOSE_FORGO_CARD_TOP" : "GUI_CHOOSE_FORGO_CARD_BOTTOM", miscVars: new List<(string, TValue)>
			{
				("MinRequiredCards", 1),
				("SelectAndMoveCardPilesToMoveFrom", new TValue(new List<CBaseCard.ECardPile> { CBaseCard.ECardPile.Round })),
				("TopAction", top),
				("RoundCardsCount", roundCardsCount),
				("ShowFullCardPicker", true)
			}, miscAbilityData: new AbilityData.MiscAbilityData
			{
				IgnoreStun = true,
				AutotriggerAbility = true,
				CanSkip = false
			}, activeBonusData: new AbilityData.ActiveBonusData
			{
				Duration = CActiveBonus.EActiveBonusDurationType.NA
			}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: null, abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: new List<(string, List<(string, TValue)>)> { (CAbilitySelectCard.SelectCardsState.AnimateCharacter.ToString(), new List<(string, TValue)> { ("ForegoAction", new TValue(CExpression.Parse("CAbilityRequirements.ForegoAction(obj)"))) }) }, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: true, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue), player, stackInsteadOfInject: true, player);
		}
	}

	public static void ForegoAction(CAbilitySelectCard selectCardAbility)
	{
		bool top = selectCardAbility.GetMiscVar("TopAction").GetBool();
		int roundCardsCount = selectCardAbility.GetMiscVar("RoundCardsCount").GetInt();
		CPlayerActor cPlayerActor = selectCardAbility.TargetingActor as CPlayerActor;
		CAbilityCard cAbilityCard = cPlayerActor.CharacterClass.SelectedAbilityCardsOutOfPhase.FirstOrDefault();
		if (cAbilityCard != null)
		{
			ForegoAction(cPlayerActor, top, cAbilityCard, roundCardsCount);
		}
	}

	public static void ForegoAction(CPlayerActor player, bool top, CAbilityCard cardToForego, int roundCardsCount)
	{
		player.SetTempVar("LastForegoneCard", new TValue(cardToForego));
		GameState.SetActionFlagRound(GameState.EActionFlag.TopForgone, roundCardsCount);
		GameState.AddPlayedCard(cardToForego.ID, top ? GameState.EActionFlag.TopForgone : GameState.EActionFlag.BottomForgone, CAbilityExtraTurn.EExtraTurnType.None, player.ActorGuid);
		CForgoActionForActiveBonus_MessageData message = new CForgoActionForActiveBonus_MessageData();
		ScenarioRuleClient.MessageHandler(message);
	}
}
