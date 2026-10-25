using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.Utils;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public static class CustomAbility
{
	public class MoveActorInDirectionResult
	{
		public int ActualMovedTiles;

		public CActor BlockingActor;

		public CObjectDoor BlockingDoor;

		public void Reset()
		{
			ActualMovedTiles = 0;
			BlockingActor = null;
			BlockingDoor = null;
		}
	}

	private static List<CActor> _alreadyProcessedAlliedActors;

	private static bool _inRecalcValidActorsForSelfAndXOthers;

	public static void Test(CAbility ability)
	{
		CExpression.Globals.breakpoint();
	}

	public static int GetAdjacentProps(CActor target, ScenarioManager.ObjectImportType type, int range = 1, bool excludeHealth = false)
	{
		List<CTile> list = target.TilesInRange(range);
		HashSet<CObjectObstacle> seenObstacles = new HashSet<CObjectObstacle>();
		return list.FindAll((CTile tile) =>
		{
			CObjectProp cObjectProp = tile.FindProp(type);
			if (cObjectProp != null && (!excludeHealth || cObjectProp.RuntimeAttachedActor == null || cObjectProp.RuntimeAttachedActor.TreatAsObstacle))
			{
				if (cObjectProp is CObjectObstacle item)
				{
					seenObstacles.Add(item);
				}
				return true;
			}
			if (type == ScenarioManager.ObjectImportType.Obstacle)
			{
				CTile propTile = null;
				CObjectObstacle cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(tile.m_ArrayIndex, ref propTile);
				if (cObjectObstacle != null && seenObstacles.Add(cObjectObstacle) && (!excludeHealth || cObjectObstacle.RuntimeAttachedActor == null || cObjectObstacle.RuntimeAttachedActor.TreatAsObstacle))
				{
					return true;
				}
			}
			return false;
		}).Count;
	}

	public static int GetAdjacentVFXProps(CActor target, string prefabName, int range = 1)
	{
		return target.TilesInRange(range).FindAll((CTile tile) => (from x in tile.FindProps(ScenarioManager.ObjectImportType.VFXProp)
			where x.PrefabName == prefabName
			select x).Any()).Count;
	}

	public static int GetAdjacentSpecificPropsCount(CActor target, ESpecificPropType type, int range = 1)
	{
		return target.TilesInRange(range).FindAll((CTile tile) => tile.FindProp(type) != null).Count;
	}

	public static int GetAdjacentShadows(CActor target, int range = 1)
	{
		return GetAdjacentProps(target, ScenarioManager.ObjectImportType.Shadow, range);
	}

	public static List<CTile> GetShadowTilesInRange(CActor target, int range = 1, bool excludeUsed = false)
	{
		List<CTile> list = target.TilesInRange(range);
		list.Add(target.Tile);
		list = list.FindAll((CTile tile) => tile.FindProp(ScenarioManager.ObjectImportType.Shadow) != null);
		if (excludeUsed)
		{
			CPlayerActor deathWalker = ScenarioManager.Scenario.PlayerActors.Find((CPlayerActor actor) => actor.Class.ID == "DeathwalkerID");
			list = list.Where((CTile tile) => tile.m_ArrayIndex != deathWalker.OverrideArrayIndex).ToList();
			CAbilityPerformFromShadow.RemoveTilesReserevdByPerformFromShadow(list);
		}
		return list;
	}

	public static List<CTile> GetResourceTilesInRange(CActor target, string resourceKey, int range = 1)
	{
		List<CTile> list = target.TilesInRange(range);
		list.Add(target.Tile);
		return list.FindAll((CTile tile) => tile.m_Props.Any((CObjectProp x) => x is CObjectResource cObjectResource && cObjectResource.ResourceID.StartsWith(resourceKey) && cObjectResource.HasRemainingLoot));
	}

	public static List<CActor> GetActorsWithAdjacentShadows(CAbility ability, int n)
	{
		return ScenarioManager.Scenario.AllAliveActors.FindAll((CActor actor) => GetAdjacentProps(actor, ScenarioManager.ObjectImportType.Shadow) >= n && ability.AbilityFilter.IsValidTarget(actor, ability.TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false) && CActor.HaveLOS(ability.TargetingActor.Tile, actor.Tile)).ToList();
	}

	public static void UpdateShadowTokenBonuses(CActor actor, CAbility ability)
	{
		if (!(actor.Class is CCharacterClass cCharacterClass))
		{
			return;
		}
		foreach (CBaseCard item in cCharacterClass.ActivatedCards.Where((CBaseCard card) => card.ActiveBonuses.Any((CActiveBonus bonus) => bonus.Ability != ability && bonus.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.PlacesShadowToken))).ToList())
		{
			foreach (CActiveBonus activeBonuse in item.ActiveBonuses)
			{
				activeBonuse.Actor.IsShadowMarked = false;
				CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(activeBonuse.Actor);
				ScenarioRuleClient.MessageHandler(message);
				activeBonuse.Finish();
			}
		}
		foreach (CActor item2 in ability.ActorsToTarget)
		{
			item2.IsShadowMarked = true;
			CUpdateWorldspaceConditionsUI_MessageData message2 = new CUpdateWorldspaceConditionsUI_MessageData(item2);
			ScenarioRuleClient.MessageHandler(message2);
		}
	}

	public static bool CanPlaceShadow(CTile tile)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y];
		CObjectProp cObjectProp = tile.m_Props.Find((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow);
		if (tile.m_HexMap.Revealed && cObjectProp == null && cNode.Walkable)
		{
			return true;
		}
		return false;
	}

	public static List<CTile> GetAdjacentTilesForShadows()
	{
		List<CTile> list = new List<CTile>();
		List<CActor> actorsKilledThisAction = CPhaseAction.ActorsKilledThisAction;
		if (actorsKilledThisAction.Count == 0)
		{
			return list;
		}
		foreach (CActor item in actorsKilledThisAction)
		{
			list.AddRange(item.ValidTilesInRange(1));
			list.Remove(item.Tile);
		}
		return list;
	}

	public static CActor GetActorOnShadow(CAbility ability, CTile tile, bool useAbilityFilter = true)
	{
		CAbilityFilterContainer cAbilityFilterContainer = new CAbilityFilterContainer();
		CAbilityFilter item = new CAbilityFilter(CAbilityFilter.EFilterTargetType.All);
		cAbilityFilterContainer.AbilityFilters.Add(item);
		return GameState.GetActorOnTile(tile, ability.TargetingActor, useAbilityFilter ? ability.AbilityFilter : cAbilityFilterContainer, null, isTargetedAbility: true, false);
	}

	public static List<CTile> GetTilesFromOrigin(Point origin, int range = 1, int minRange = 0)
	{
		List<CTile> tilesAtRange = GameState.GetTilesAtRange(origin, range, range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ignoreLOS: false, emptyOpenDoorTiles: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: true);
		List<CTile> list = new List<CTile>();
		foreach (CTile item in tilesAtRange)
		{
			if (item.m_HexMap.Revealed && !item.m_Props.Any((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<CActor> GetTargetsFromOrigin(CActor actor, Point origin, int range = 1)
	{
		return GameState.GetActorsInRange(origin, actor, range, null, CAbilityFilterContainer.CreateDefaultFilter(), null, null, isTargetedAbility: true, null, false);
	}

	public static void ApplyAbilityToActor(CAbility ability, CActor caster, CActor target, CVarsContext context = null)
	{
		CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: true, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, context);
		cAbility.Start(caster, caster);
		cAbility.ApplyToActor(target);
	}

	public static void PerformActiveBonusAbility(CActiveBonus activeBonus, CVarsContext context = null)
	{
		PerformActiveBonusAbility(activeBonus.Ability.ActiveBonusData, activeBonus.Ability.TargetingActor, context);
	}

	public static void PerformActiveBonusAbility(AbilityData.ActiveBonusData data, CActor targetingActor, CVarsContext context = null, bool instant = false)
	{
		if (instant)
		{
			ApplyAbilityToActor(data.AbilityData, targetingActor, targetingActor, context);
			return;
		}
		CAbility cAbility = CAbility.CopyAbility(data.AbilityData, generateNewID: false, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, context);
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, targetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true, interruptCurrent: true);
		}
		else
		{
			cAbility.IsScenarioModifierAbility = true;
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, targetingActor));
		}
	}

	public static void InjectActiveBonusAbilityXTimes(int x, AbilityData.ActiveBonusData data, CActor targetingActor, CVarsContext context = null)
	{
		List<CAbility> list = new List<CAbility>();
		for (int i = 0; i < x; i++)
		{
			CAbility cAbility = CAbility.CopyAbility(data.AbilityData, generateNewID: false, fullCopy: true, copyCurrentOverrides: false, applyCurrentStatIsBasedOnX: false, context);
			if (context != null)
			{
				CAbility cAbility2 = context.GetVar("obj").GetObject<CAbility>();
				if (cAbility2 != null)
				{
					cAbility.ParentAbility = cAbility2;
				}
			}
			list.Add(cAbility);
			if (cAbility.SubAbilities == null)
			{
				continue;
			}
			foreach (CAbility subAbility in cAbility.SubAbilities)
			{
				if (!subAbility.IsInlineSubAbility)
				{
					subAbility.ParentAbility = cAbility;
					list.Add(subAbility);
				}
			}
		}
		PhaseManager.CurrentPhaseAction.InjectAbilities(list, targetingActor);
	}

	public static void StackXPAbility(int strength, CActor targetingActor, CVarsContext context = null)
	{
		CAbility cAbility = CAbilityXP.CreateDefaultXP(strength, 1, 1, isMonster: false, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
		cAbility.MiscAbilityData.AutotriggerAbility = true;
		cAbility.MiscAbilityData.IgnoreStun = true;
		if (PhaseManager.CurrentPhase is CPhaseAction cPhaseAction)
		{
			cPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, targetingActor, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: true, interruptCurrent: true);
		}
		else
		{
			cAbility.IsScenarioModifierAbility = true;
			GameState.PendingScenarioModifierAbilities.Add(new Tuple<CAbility, CActor>(cAbility, targetingActor));
		}
	}

	public static CTile GetLastMovedShadowTile()
	{
		CPhaseAction.CPhaseAbility cPhaseAbility = PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities.Find((CPhaseAction.CPhaseAbility phaseAbility) => phaseAbility.m_Ability is CAbilityMoveShadow);
		if (cPhaseAbility != null)
		{
			return (cPhaseAbility.m_Ability as CAbilityMoveShadow).TileToMoveTo;
		}
		return null;
	}

	public static bool SteppedOnShadow(CTile tile)
	{
		return tile.m_Props.Any((CObjectProp prop) => prop.ObjectType == ScenarioManager.ObjectImportType.Shadow);
	}

	public static void RemoveAdvantageBonusOnMove(CActor actor)
	{
		foreach (CActiveBonus item in CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.AttackersGainDisadvantage))
		{
			item.Finish();
		}
	}

	public static bool HasFreeShadows(CActor actor)
	{
		if (ScenarioManager.CurrentScenarioState.ShadowProps.Count > 0)
		{
			return ScenarioManager.CurrentScenarioState.ShadowProps.Select((CObjectProp prop) => prop.PropTile).Any((CTile tile) =>
			{
				List<CActor> actorsOnTile = GameState.GetActorsOnTile(tile, actor, CAbilityFilterContainer.CreateDefaultFilter(), null, isTargetedAbility: false, false);
				return (actorsOnTile.Count() == 0 || actorsOnTile.First() == actor) && !tile.IsBlocked;
			});
		}
		return false;
	}

	public static List<CActor> GetValidActorsOnShadows(CActor actor)
	{
		IEnumerable<CTile> enumerable = ScenarioManager.CurrentScenarioState.ShadowProps.Select((CObjectProp prop) => prop.PropTile);
		List<CActor> list = new List<CActor>();
		CAbilityFilterContainer cAbilityFilterContainer = new CAbilityFilterContainer();
		CAbilityFilter item = new CAbilityFilter(CAbilityFilter.EFilterTargetType.All);
		cAbilityFilterContainer.AbilityFilters.Add(item);
		foreach (CTile item2 in enumerable)
		{
			CActor actorOnTile = GameState.GetActorOnTile(item2, actor, cAbilityFilterContainer, null, isTargetedAbility: true, false);
			if (actorOnTile != null && CActor.HaveLOS(item2, ScenarioManager.Tiles[actor.ArrayIndex.X, actor.ArrayIndex.Y]))
			{
				list.Add(actorOnTile);
			}
		}
		return list;
	}

	public static bool ValidateForcedMove(CAbility ability, CActor caster)
	{
		if (ability is CAbilityMove cAbilityMove)
		{
			if (cAbilityMove.IsControlAbility && cAbilityMove.ControllingActor == caster)
			{
				return cAbilityMove.HasMoved;
			}
			return false;
		}
		if (ability is CAbilityPull cAbilityPull)
		{
			return cAbilityPull.TargetingActor == caster;
		}
		if (ability is CAbilityPush cAbilityPush)
		{
			return cAbilityPush.TargetingActor == caster;
		}
		return false;
	}

	public static List<CActor> GetForcedMovementTargets(CAbility ability)
	{
		List<CActor> list = new List<CActor>();
		if (ability is CAbilityMove cAbilityMove)
		{
			list.Add(cAbilityMove.TargetingActor);
		}
		else if (ability is CAbilityPull cAbilityPull)
		{
			list.Add(cAbilityPull.CurrentTarget);
		}
		else if (ability is CAbilityPush cAbilityPush)
		{
			list.Add(cAbilityPush.CurrentTarget);
		}
		return list;
	}

	public static bool IsAlly(CActor targetingActor, CActor actor, bool checkOriginalType = false, bool includeObjects = false)
	{
		if (checkOriginalType)
		{
			if (targetingActor != actor && CActor.AreActorsAllied(targetingActor.OriginalType, actor.OriginalType, targetingActor, actor))
			{
				if (!includeObjects)
				{
					return !(actor is CObjectActor);
				}
				return true;
			}
			return false;
		}
		if (targetingActor != actor && CActor.AreActorsAllied(targetingActor.Type, actor.Type, targetingActor, actor))
		{
			if (!includeObjects)
			{
				return !(actor is CObjectActor);
			}
			return true;
		}
		return false;
	}

	public static bool IsEnemy(CActor targetingActor, CActor actor, bool checkOriginalType = false, bool includeObjects = false)
	{
		if (checkOriginalType)
		{
			if (targetingActor != actor && CActor.AreActorsEnemies(targetingActor.OriginalType, actor.OriginalType, targetingActor, actor))
			{
				if (!includeObjects)
				{
					return !(actor is CObjectActor);
				}
				return true;
			}
			return false;
		}
		if (targetingActor != actor && CActor.AreActorsEnemies(targetingActor.Type, actor.Type, targetingActor, actor))
		{
			return !(actor is CObjectActor);
		}
		return false;
	}

	public static List<CTile> GetAdjacentTiles(List<CTile> tiles, bool includeSelf = false)
	{
		List<CTile> list = new List<CTile>();
		foreach (CTile tile in tiles)
		{
			GameState.GetAdjacentTiles(tile.m_ArrayIndex, list, clear: false);
			if (includeSelf)
			{
				list.Add(tile);
			}
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetAdjacentTiles(List<CActor> actors, bool includeSelf = false)
	{
		List<CTile> list = new List<CTile>();
		foreach (CActor actor in actors)
		{
			list.AddRange(GameState.GetTilesInRange(actor, 1));
			if (includeSelf)
			{
				list.Add(actor.Tile);
			}
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetTilesInRange(List<CTile> tiles, int range, bool includeOrigin)
	{
		List<CTile> list = new List<CTile>();
		foreach (CTile tile in tiles)
		{
			list.AddRange(GameState.GetTilesInRange(tile.m_ArrayIndex, range));
			if (!includeOrigin)
			{
				list.Remove(tile);
			}
		}
		return list.Distinct().ToList();
	}

	public static List<CTile> GetAdjacentTiles(CTile tile, bool includeStartTile = false)
	{
		return GetAdjacentTiles(tile.m_ArrayIndex.X, tile.m_ArrayIndex.Y, includeStartTile);
	}

	public static List<CTile> GetAdjacentTiles(int x, int y, bool includeStartTile = false)
	{
		List<CTile> list = new List<CTile>();
		if (includeStartTile)
		{
			list.Add(ScenarioManager.Tiles[x, y]);
		}
		GameState.GetAdjacentTiles(new Point(x, y), list, clear: false);
		return list;
	}

	public static List<CActor> GetActorsOnTiles(List<CTile> tiles)
	{
		List<CActor> list = new List<CActor>();
		foreach (CTile tile in tiles)
		{
			CActor livingActor = tile.LivingActor;
			if (livingActor != null && !list.Contains(livingActor))
			{
				list.Add(livingActor);
			}
		}
		return list;
	}

	public static CAbility GetParentAbility(CAbility ability)
	{
		if (ability.ParentAbility != null)
		{
			return ability.ParentAbility;
		}
		if (ability.ParentName != null)
		{
			CAbility cAbility = ((CPhaseAction)PhaseManager.Phase)?.PreviousPhaseAbilities.Where((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == ability.ParentName)?.LastOrDefault()?.m_Ability;
			if (cAbility != null)
			{
				return cAbility;
			}
			CAbility cAbility2 = ((CPhaseAction)PhaseManager.Phase)?.CurrentPhaseAbilities.Where((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == ability.ParentName)?.LastOrDefault()?.m_Ability;
			if (cAbility2 != null)
			{
				return cAbility2;
			}
		}
		return null;
	}

	public static CAbility FindAbilityInAction(CAbility ability, string name)
	{
		return ability.GetAction().Abilities.SingleOrDefault((CAbility x) => x.Name.Equals(name));
	}

	public static T PreviousAbilityInThisAction<T>(CAbility ability, bool ignoreBuffs = false, bool checkHappened = true) where T : CAbility
	{
		if (ability == null)
		{
			return null;
		}
		return ((CPhaseAction)PhaseManager.Phase)?.PreviousPhaseAbilities?.Where((CPhaseAction.CPhaseAbility x) => x.m_Ability is T && x.m_Ability.GetAction() == ability.GetAction() && (!checkHappened || x.m_Ability.AbilityHasHappened) && (!ignoreBuffs || x.m_Ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA))?.LastOrDefault()?.m_Ability as T;
	}

	public static bool HasPerformedAction(CAbility ability, bool ignoreBuffs = false)
	{
		if (ability == null)
		{
			return false;
		}
		return ((CPhaseAction)PhaseManager.Phase)?.PreviousPhaseAbilities?.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.GetAction() == ability.GetAction() && x.m_Ability.AbilityHasHappened && (!ignoreBuffs || x.m_Ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)) == true;
	}

	public static bool HasPerformedAttackAction(CActor actor)
	{
		CAbility lastAbilityPerformed = actor.LastAbilityPerformed;
		if (lastAbilityPerformed == null)
		{
			return false;
		}
		CAbilityAttack cAbilityAttack = PreviousAbilityInThisAction<CAbilityAttack>(lastAbilityPerformed, ignoreBuffs: true, checkHappened: false);
		if (cAbilityAttack != null && !(cAbilityAttack.GetAbilityBaseCard() is CAbilityCard))
		{
			return false;
		}
		return HasPerformedAction(cAbilityAttack, ignoreBuffs: true);
	}

	public static CAbility LastPhaseAbility()
	{
		if (PhaseManager.CurrentPhaseAction == null)
		{
			return null;
		}
		return PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities?.LastOrDefault()?.m_Ability;
	}

	public static bool GetMiscVarBool(CAbility ability, string param)
	{
		return ability?.GetMiscVar(param).GetBool() ?? false;
	}

	public static CAbility GetCurrentContextAbility()
	{
		for (CVarsContext cVarsContext = CAbilityFilter.CurrentVarsContext; cVarsContext != null; cVarsContext = cVarsContext.parentContext)
		{
			if (cVarsContext.obj is CAbility result)
			{
				return result;
			}
		}
		return null;
	}

	public static CAbility FindAbilityInPhaseByName(string name, CActor actor = null, bool hasHappened = true)
	{
		return ((CPhaseAction)PhaseManager.Phase)?.PreviousPhaseAbilities?.Where((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name == name && (!hasHappened || x.m_Ability.AbilityHasHappened) && (actor == null || x.TargetingActor == actor))?.LastOrDefault()?.m_Ability;
	}

	public static bool IsAbilityQueued(string name)
	{
		if (GameState.PendingScenarioModifierAbilities.Any((Tuple<CAbility, CActor> x) => x.Item1.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
		{
			return true;
		}
		if (PhaseManager.CurrentPhaseAction != null)
		{
			return PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities?.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) ?? false;
		}
		return false;
	}

	public static List<CTile> TilesMovedInPreviousAbility(CAbility ability)
	{
		return PreviousAbilityInThisAction<CAbilityMove>(ability).TilesMoved.Select((Point x) => ScenarioManager.GetTile(x)).ToList();
	}

	public static int TilesLootedInPreviousAbility(CAbility ability)
	{
		return PreviousAbilityInThisAction<CAbilityLoot>(ability)?.LootCount ?? 0;
	}

	public static List<CTile> GetTilesAdjacentToArea(CAbility ability)
	{
		List<CTile> list = new List<CTile>();
		Dictionary<CTile, CAreaEffect.EHexFlag>.KeyCollection keys = ability.FlagsOfValidTiles.Keys;
		foreach (CTile item in keys)
		{
			GameState.GetAdjacentTiles(item.m_ArrayIndex, list, clear: false);
		}
		return list.Except(keys).Distinct().ToList();
	}

	public static List<CActor> GetActorsAdjacentToArea(CAbility ability)
	{
		return GetTilesAdjacentToArea(ability).SelectMany((CTile tile) => tile.LivingActors).ToList();
	}

	public static List<CTile> GetTilesOnDirectPath(CTile startTile, CTile targetTile, bool includeStartTile = false, bool includeTargetTile = false)
	{
		if (targetTile == null)
		{
			return null;
		}
		List<Point> list = ScenarioManager.PathFinder.FindPath(null, startTile.m_ArrayIndex, targetTile.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, out var foundPath, ignoreDoorLocks: false);
		if (!foundPath || list == null || list.Count == 0)
		{
			return null;
		}
		if (!includeTargetTile)
		{
			list.RemoveAt(list.Count - 1);
		}
		List<CTile> list2 = list.Select((Point x) => ScenarioManager.GetTile(x)).ToList();
		if (includeStartTile)
		{
			list2.Insert(0, startTile);
		}
		return list2;
	}

	public static List<CTile> GetLastEmptyTilesOnDirectPaths(CTile startTile, CTile targetTile)
	{
		if (Point.HexDist(startTile.m_ArrayIndex, targetTile.m_ArrayIndex) <= 1)
		{
			return null;
		}
		CPathFinderContext pfContext = ScenarioManager.PathFinder.InitQuery(null, startTile.m_ArrayIndex, targetTile.m_ArrayIndex, hasTarget: false, int.MaxValue, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, null, ignoreDoorLocks: false, null, isCalculatingDistance: false);
		pfContext.ignoreNavLinks = true;
		pfContext.fillEntireLevel = true;
		pfContext.FloodFill();
		List<Point> adjacentLocations = new List<Point>();
		List<CTile> prevTiles = new List<CTile>();
		if (!AddPrevTiles(targetTile))
		{
			return null;
		}
		for (int i = 0; i < prevTiles.Count; i++)
		{
			CTile cTile = prevTiles[i];
			if (!cTile.IsEmpty)
			{
				prevTiles.RemoveAt(i);
				i--;
				AddPrevTiles(cTile);
			}
		}
		return prevTiles;
		bool AddPrevTiles(CTile tile)
		{
			CPathFinderContext.TNode node = pfContext.GetNode(tile.m_ArrayIndex, init: false);
			if (!node.HasFlag(CPathFinderContext.TNode.Flags.Reachable))
			{
				return false;
			}
			CPathFinder.GetAdjacentLocations(tile.m_ArrayIndex, adjacentLocations);
			foreach (Point item in adjacentLocations)
			{
				CPathFinderContext.TNode node2 = pfContext.GetNode(item, init: false);
				if (node2.HasFlag(CPathFinderContext.TNode.Flags.Reachable) && node2.cost.numHexes <= node.cost.numHexes - 1)
				{
					CTile tile2 = ScenarioManager.GetTile(item);
					if (!prevTiles.Contains(tile2))
					{
						prevTiles.Add(tile2);
					}
				}
			}
			return true;
		}
	}

	public static void RepeatAbility(CAbility ability, CActor targetingActor = null)
	{
		if (targetingActor == null)
		{
			targetingActor = ability.TargetingActor;
		}
		PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { ability }, targetingActor, killAfter: false, stackToNextCurrent: true);
	}

	public static int GetDebuffRange(CAbility ability, int debuff)
	{
		if (ability.Range == 1)
		{
			return 0;
		}
		if (ability.Range - debuff >= 1)
		{
			return -debuff;
		}
		return -debuff + 1;
	}

	public static List<CActor> GetActorsNextToTerrain(CAbility ability)
	{
		List<CActor> list = new List<CActor>();
		List<CActor> actorsInRange = GameState.GetActorsInRange(ability.TargetingActor, ability.TargetingActor, ability.Range, null, ability.AbilityFilter, null, null, isTargetedAbility: true, null, false);
		foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp prop) => prop is CObjectHazardousTerrain).ToList())
		{
			List<CTile> list2 = new List<CTile>();
			GameState.GetAdjacentTiles(new Point(item.ArrayIndex.X, item.ArrayIndex.Y), list2, clear: false);
			list2.Add(item.PropTile);
			List<CActor> list3 = list2.SelectMany((CTile tile) => tile.LivingActors).ToList();
			if (list3.Count < 2)
			{
				continue;
			}
			foreach (CActor item2 in list3)
			{
				if (ability.AbilityFilter.IsValidTarget(item2, ability.TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false) && actorsInRange.Contains(item2) && (ability.TilesSelected.Count == 0 || list3.Any((CActor otherActor) => ability.TilesSelected.Contains(otherActor.Tile))))
				{
					list.Add(item2);
				}
			}
		}
		return list.Distinct().ToList();
	}

	public static void PlayVFX(string VFXPropType, Point position)
	{
		ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
		{
			m_VFXPropType = VFXPropType,
			m_ArrayIndex = position
		});
	}

	public static void PlayVFX(string VFXPropType, List<CTile> positions)
	{
		foreach (CTile position in positions)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = VFXPropType,
				m_ArrayIndex = position.m_ArrayIndex
			});
		}
	}

	public static void ImproveTrap(CObjectTrap trap, int addValue = 1, bool sendVfx = false)
	{
		Log.Info($"ImproveTrap({trap}, {addValue})");
		if (trap.HealValue > 0)
		{
			trap.HealValue += addValue;
		}
		if (trap.Damage)
		{
			int trapDamage = trap.GetTrapDamage();
			if (trapDamage > 0)
			{
				trap.SetCustomDamageValue(trapDamage + addValue);
			}
		}
		if (sendVfx)
		{
			ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
			{
				m_VFXPropType = "TrapperEnhanceTrap",
				m_ArrayIndex = trap.ArrayIndex.ToPoint()
			});
		}
		trap.LogActor();
	}

	public static void PlayTrapperEnhanceVFXOnAllTraps()
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		int upperBound = tiles.GetUpperBound(0);
		int upperBound2 = tiles.GetUpperBound(1);
		for (int i = tiles.GetLowerBound(0); i <= upperBound; i++)
		{
			for (int j = tiles.GetLowerBound(1); j <= upperBound2; j++)
			{
				if (tiles[i, j]?.FindProp(ScenarioManager.ObjectImportType.Trap) is CObjectTrap { Activated: false } cObjectTrap)
				{
					ScenarioRuleClient.MessageHandler(new CTryPlayVfxOnTile_MessageData(null)
					{
						m_VFXPropType = "TrapperEnhanceTrap",
						m_ArrayIndex = cObjectTrap.ArrayIndex.ToPoint()
					});
				}
			}
		}
	}

	public static void AddTrapHeal(CObjectTrap trap, int addValue = 1)
	{
		Log.Info($"AddTrapHeal({trap}, {addValue})");
		trap.HealValue += addValue;
		trap.UpdateVisualState();
		trap.LogActor();
	}

	public static void AddTrapDamage(CObjectTrap trap, int addValue = 1)
	{
		Log.Info($"AddTrapDamage({trap}, {addValue})");
		trap.Damage = true;
		int trapDamage = trap.GetTrapDamage();
		trap.SetCustomDamageValue(trapDamage + addValue);
		trap.LogActor();
	}

	public static void AddTrapEffects(CObjectTrap trap, CObjectTrap effectsTrap)
	{
		Log.Info($"AddTrapEffects({trap}, {effectsTrap})");
		trap.HealValue += effectsTrap.HealValue;
		if (effectsTrap.Damage)
		{
			trap.Damage = true;
			trap.SetCustomDamageValue(trap.GetTrapDamage() + effectsTrap.GetTrapDamage());
		}
		List<CCondition.ENegativeCondition> conditions = effectsTrap.Conditions;
		if (conditions != null && conditions.Count > 0)
		{
			if (trap.Conditions == null)
			{
				trap.Conditions = new List<CCondition.ENegativeCondition>();
			}
			foreach (CCondition.ENegativeCondition condition in effectsTrap.Conditions)
			{
				if (!trap.Conditions.Contains(condition))
				{
					trap.Conditions.Add(condition);
				}
			}
		}
		List<CCondition.EPositiveCondition> positiveConditions = effectsTrap.PositiveConditions;
		if (positiveConditions != null && positiveConditions.Count > 0)
		{
			if (trap.PositiveConditions == null)
			{
				trap.PositiveConditions = new List<CCondition.EPositiveCondition>();
			}
			foreach (CCondition.EPositiveCondition positiveCondition in effectsTrap.PositiveConditions)
			{
				if (!trap.PositiveConditions.Contains(positiveCondition))
				{
					trap.PositiveConditions.Add(positiveCondition);
				}
			}
		}
		trap.UpdateVisualState();
		trap.LogActor();
	}

	public static void ApplySpringLoadedBuff(CAbilityMove moveAbility)
	{
		Log.Info($"ApplySpringLoadedBuff({moveAbility})");
		moveAbility.RemainingMoves += 2;
		moveAbility.Jump = true;
		if (moveAbility.AbilityInterruptCopy != null && moveAbility.AbilityInterruptCopy is CAbilityMove cAbilityMove)
		{
			cAbilityMove.RemainingMoves = moveAbility.RemainingMoves;
			cAbilityMove.Jump = moveAbility.Jump;
			cAbilityMove.WasForcedInterrupted = true;
			if (cAbilityMove.m_State == CAbilityMove.EMoveState.MoveDone)
			{
				cAbilityMove.m_State = CAbilityMove.EMoveState.ActorIsSelectingMoveTile;
			}
		}
		else
		{
			moveAbility.CreateResumeCopy();
		}
	}

	public static int GetAlliesMovedThrough(int index, CAbilityMove moveAbility)
	{
		if (index == 0)
		{
			if (_alreadyProcessedAlliedActors == null)
			{
				_alreadyProcessedAlliedActors = new List<CActor>();
			}
			else
			{
				_alreadyProcessedAlliedActors.Clear();
			}
		}
		List<CTile> list = new List<CTile>();
		for (int i = index; i < moveAbility.TilesMoved.Count - 1; i++)
		{
			list.Add(ScenarioManager.GetTile(moveAbility.TilesMoved[i]));
		}
		List<CActor> list2 = (from a in GetActorsOnTiles(list)
			where a != moveAbility.TargetingActor && !_alreadyProcessedAlliedActors.Contains(a) && CActor.AreActorsAllied(a.Type, moveAbility.TargetingActor.Type, a, moveAbility.TargetingActor)
			select a).Distinct().ToList();
		if (list2.Count > 0)
		{
			_alreadyProcessedAlliedActors.Add(list2[0]);
		}
		return list2.Count;
	}

	public static List<CAbilityCard> GetActivatedModeCards(CPlayerActor actor)
	{
		return actor?.CharacterClass.ActivatedAbilityCards.Where((CAbilityCard card) => card.TopAction.IsModeActive(card) || card.BottomAction.IsModeActive(card)).ToList();
	}

	public static List<CActor> GetActorsAdjacentToActors(List<CActor> actors)
	{
		List<CActor> list = new List<CActor>();
		foreach (CActor actor in actors)
		{
			foreach (CActor allAdjacentActor in ScenarioManager.GetAllAdjacentActors(actor))
			{
				if (!list.Contains(allAdjacentActor))
				{
					list.Add(allAdjacentActor);
				}
			}
		}
		return list;
	}

	public static void OverrideArrayIndex(CActor actor, Point position)
	{
		if (position == actor.RealArrayIndex)
		{
			actor.OverrideArrayIndex = default;
		}
		else
		{
			actor.OverrideArrayIndex = position;
		}
	}

	public static List<CActor> ActorsKilledByThisAbility(CAbility currentAbility)
	{
		List<CActor> list = new List<CActor>();
		list.AddRange(GameState.ActorsKilledThisTurn.FindAll((CActor actor) => actor.LastAbilityDamagedBy == currentAbility));
		return list;
	}

	public static List<CActor> ActorsOnBlueHexes(CAbility currentAbility, bool includeSelf = false)
	{
		List<CActor> list = new List<CActor>();
		if (currentAbility == null)
		{
			return list;
		}
		if (currentAbility.AreaEffect != null || currentAbility.HadAreaEffect)
		{
			List<CTile> source = (from pair in currentAbility.FlagsOfValidTiles
				where (pair.Value & CAreaEffect.EHexFlag.Ally) != 0
				select pair.Key).ToList();
			list.AddRange((from tile in source
				where tile.LivingActor != null
				select tile.LivingActor).ToList());
			if (includeSelf)
			{
				list.Add(currentAbility.TargetingActor);
			}
		}
		return list;
	}

	public static List<CActor> ActorsOnRedHexes(CAbility currentAbility, bool includeSelf = false)
	{
		List<CActor> list = new List<CActor>();
		if (currentAbility == null)
		{
			return list;
		}
		if (currentAbility.AreaEffect != null)
		{
			List<CTile> source = (from pair in currentAbility.FlagsOfValidTiles
				where (pair.Value & CAreaEffect.EHexFlag.Enemy) != 0
				select pair.Key).ToList();
			list.AddRange((from tile in source
				where tile.LivingActor != null
				select tile.LivingActor).ToList());
			if (includeSelf)
			{
				list.Add(currentAbility.TargetingActor);
			}
		}
		return list;
	}

	public static List<object> GetDrawnLootCards()
	{
		List<byte> lootDeck = ScenarioManager.CurrentScenarioState.LootDeck;
		List<object> list = new List<object>();
		for (int i = CLootDeck.GetDelimiterID(lootDeck) + 1; i < lootDeck.Count; i++)
		{
			byte b = lootDeck[i];
			LootCard lootCard = CLootDeck.AllCards[b];
			if (lootCard.AvailableCampaignSectionIDs == null || !lootCard.AvailableCampaignSectionIDs.Any())
			{
				list.Add(lootCard);
			}
		}
		return list;
	}

	public static bool DiscardLootCard(CActor caster)
	{
		List<object> drawnLootCards = GetDrawnLootCards();
		return DiscardLootCard(caster, drawnLootCards);
	}

	public static bool DiscardLootCard(CActor caster, List<object> lootCards)
	{
		List<LootCard> cards = lootCards.Select((object x) => (LootCard)x).ToList();
		if (GameState.ShowLootCardSelection(caster, cards, out var selectedLootCard, "FH/GUI_DISCARD_LOOT_CARD"))
		{
			List<byte> lootDeck = ScenarioManager.CurrentScenarioState.LootDeck;
			int num = CLootDeck.AllCards.FindIndex((LootCard x) => x == selectedLootCard);
			lootDeck.Remove((byte)num);
			CLootDeck.AddCardBeforeDelimiter(lootDeck, selectedLootCard);
			caster.StartLooting();
			int num2 = CLootDeck.CalcLootAmount(selectedLootCard, ScenarioManager.CurrentScenarioState);
			switch (selectedLootCard.Type)
			{
			case LootCard.EType.Lumber:
			case LootCard.EType.Metal:
			case LootCard.EType.Hide:
			case LootCard.EType.Arrowvine:
			case LootCard.EType.Axenut:
			case LootCard.EType.Corpsecap:
			case LootCard.EType.Flamefruit:
			case LootCard.EType.Rockroot:
			case LootCard.EType.Snowthistle:
				caster.GainResource(selectedLootCard.Type, -num2);
				break;
			case LootCard.EType.Money:
				caster.GainLootMoney(-num2);
				break;
			}
			caster.LootFromLootCards = new LootState();
			caster.LootFromLootCards.AccumulateLootFrom(caster.TotalLoot);
			return true;
		}
		return false;
	}

	public static List<CAbilityCard> GetCardPileList(CPlayerActor actor, CBaseCard.ECardPile cardPile)
	{
		return actor.CharacterClass.GetCardPileList(cardPile).ToList();
	}

	public static int GetCardInitiativeAdjusted(CPlayerActor actor, CAbilityCard card)
	{
		if (actor == null)
		{
			return card.Initiative;
		}
		if (actor.Agility != CPlayerActor.EAgility.Slow)
		{
			return card.Initiative;
		}
		return card.Initiative + 30;
	}

	public static void RedrawMonsterCards()
	{
		MonsterClassManager.SelectRoundAbilityCards(reset: true);
		PhaseManager.SetNextPhase(CPhase.PhaseType.MonsterClassesSelectAbilityCards);
		if (PhaseManager.CurrentPhase is CPhaseSelectMonsterClassAbilityCards cPhaseSelectMonsterClassAbilityCards)
		{
			cPhaseSelectMonsterClassAbilityCards.HasPromptedItems = false;
		}
	}

	public static List<CAbilityCard> GetEqualOrLowerLevelCards(int level, List<CAbilityCard> cards)
	{
		return cards.Where((CAbilityCard x) => x.Level <= level).ToList();
	}

	public static bool IsDoorAvailableForOpen(CTile tile)
	{
		CObjectDoor cObjectDoor = tile.FindDoor();
		if (cObjectDoor == null || cObjectDoor.Activated || cObjectDoor.DoorIsLocked)
		{
			return false;
		}
		return true;
	}

	public static void TryOpenDoor(CActor actor, CObjectDoor door)
	{
		door.ForceActivate(actor);
	}

	public static void ShowTopMonsterCards()
	{
		List<CMonsterClass> list = new List<CMonsterClass>();
		foreach (CMonsterClass monsterClass in MonsterClassManager.Classes)
		{
			if (ScenarioManager.Scenario.AllAliveMonsters.Find((CEnemyActor x) => x.MonsterClass == monsterClass) != null)
			{
				list.Add(monsterClass);
			}
		}
		foreach (CObjectClass objectClass in MonsterClassManager.ObjectClasses)
		{
			if (ScenarioManager.Scenario.Objects.Find((CObjectActor x) => x.MonsterClass == objectClass) != null)
			{
				list.Add(objectClass);
			}
		}
		ShowTopMonsterCards(list);
	}

	public static void PutMonsterCardAtBottomOfDeck(CBaseCard card, CActor actor)
	{
		PutMonsterCardAtBottomOfDeck(card, actor?.MonsterClass);
	}

	public static void PutMonsterCardAtBottomOfDeck(CBaseCard card, CMonsterClass monsterClass)
	{
		if (monsterClass == null || !(card is CMonsterAbilityCard item))
		{
			return;
		}
		monsterClass.AbilityCards.Remove(item);
		monsterClass.AbilityCards.Add(item);
		foreach (CActor item2 in from x in ScenarioManager.CurrentScenarioState.Monsters
			where x.ClassID == monsterClass.ID
			select x.Actor into x
			where x != null
			select x)
		{
			foreach (CActiveBonus item3 in (from x in item2.FindApplicableActiveBonuses()
				where x.Ability.GetMiscVar("ShowNextCard").GetBool()
				select x).ToList())
			{
				item3.Finish();
			}
		}
	}

	public static void ShowTopMonsterCards(CActor actor, bool offerPutBottomOfDeck = false, string altHelpKey = null)
	{
		CMonsterClass monsterClass = actor.MonsterClass;
		if (monsterClass != null)
		{
			ShowTopMonsterCards(new List<CMonsterClass> { monsterClass }, offerPutBottomOfDeck, altHelpKey);
		}
	}

	public static void ShowTopMonsterCards(List<CActor> actors)
	{
		ShowTopMonsterCards((from x in actors
			select x.MonsterClass into x
			where x != null
			select x).Distinct().ToList());
	}

	public static void CreateShowNextCardBonus(CEnemyActor actor)
	{
		CAbility cAbility = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: true, "", new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, "AShowNextMonsterCardBonus");
		cAbility.SetMiscVar("ShowNextCard", true);
		cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
		cAbility.ActiveBonusData.Hidden = true;
		cAbility.ActiveBonusData.Behaviour = CActiveBonus.EActiveBonusBehaviourType.GenericBeforeRevealMonsterCards;
		cAbility.ActiveBonusData.Tracker = new List<int> { 0 };
		cAbility.Start(actor, actor);
		cAbility.ApplyToActor(actor);
	}

	public static void ShowTopMonsterCards(List<CMonsterClass> monsterClassesChosen, bool offerPutBottomOfDeck = false, string altHelpKey = null)
	{
		bool flag = false;
		foreach (CMonsterClass monsterClass in monsterClassesChosen)
		{
			if (!monsterClass.HasCards)
			{
				continue;
			}
			flag = true;
			if (monsterClass.NextAbilityCard == null)
			{
				continue;
			}
			IEnumerable<CEnemyActor> enumerable = ScenarioManager.Scenario.AllAliveMonsters.Where((CEnemyActor x) => x.Class == monsterClass);
			CEnemyActor cEnemyActor = enumerable.FirstOrDefault();
			foreach (CEnemyActor item in enumerable)
			{
				CreateShowNextCardBonus(item);
			}
			if (cEnemyActor != null)
			{
				if (monsterClass.NextAbilityCard != null)
				{
					CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
					{
						m_MessageKey = "GUI_MONSTER_NEXT_CARD",
						m_Actors = new List<CActor> { cEnemyActor },
						m_Params = new List<string>
						{
							"Actor_0",
							monsterClass.NextAbilityCard.Name
						}
					};
					ScenarioRuleClient.MessageHandler(message);
				}
				else
				{
					CCombatLogMessage_MessageData message2 = new CCombatLogMessage_MessageData
					{
						m_MessageKey = "GUI_MONSTER_NO_NEXT_CARD",
						m_Actors = new List<CActor> { cEnemyActor },
						m_Params = new List<string> { "Actor_0" }
					};
					ScenarioRuleClient.MessageHandler(message2);
				}
			}
		}
		if (flag)
		{
			CShowNextMonsterClassAbilityCards_MessageData cShowNextMonsterClassAbilityCards_MessageData = new CShowNextMonsterClassAbilityCards_MessageData(null);
			cShowNextMonsterClassAbilityCards_MessageData.m_MonsterClasses = monsterClassesChosen;
			cShowNextMonsterClassAbilityCards_MessageData.m_OfferPutBottomOfDeck = offerPutBottomOfDeck;
			cShowNextMonsterClassAbilityCards_MessageData.m_AltHelpKey = altHelpKey;
			ScenarioRuleClient.MessageHandler(cShowNextMonsterClassAbilityCards_MessageData);
			GameState.WaitForConfirmation("ShowingNextMonsterCards");
		}
	}

	public static AttackModifierYMLData CreateRollingAttackModifierCopy(AttackModifierYMLData attackModifierCard)
	{
		AttackModifierYMLData attackModifierYMLData = attackModifierCard.Copy(createCard: true);
		attackModifierYMLData.Rolling = true;
		if (attackModifierCard.ModifiedFromModifier != null)
		{
			attackModifierYMLData.ModifiedFromModifier = attackModifierCard.ModifiedFromModifier;
		}
		return attackModifierYMLData;
	}

	public static void PushActorTo(CActor currentActor, CTile tileToMoveTo, CTile tileToMoveFrom, int newAngle = 0, bool choreographerWait = true, bool ignoreObjectActor = false)
	{
		CObjectActor cObjectActor = currentActor as CObjectActor;
		GameState.LostAdjacency(currentActor, currentActor.ArrayIndex, tileToMoveTo.m_ArrayIndex, null, wasForced: true);
		if (cObjectActor != null)
		{
			tileToMoveFrom.DelProp(cObjectActor.AttachedProp);
			tileToMoveTo.AddProp(cObjectActor.AttachedProp);
			CVector3 rotation = new CVector3(0f, newAngle, 0f);
			CObjectObstacle cObjectObstacle = cObjectActor.AttachedProp as CObjectObstacle;
			if (cObjectObstacle != null)
			{
				foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
				{
					ScenarioManager.PathFinder.Nodes[pathingBlocker.X, pathingBlocker.Y].Blocked = false;
				}
			}
			cObjectActor.AttachedProp.SetLocation(new TileIndex(tileToMoveTo.m_ArrayIndex), null, rotation);
			if (cObjectObstacle != null)
			{
				foreach (TileIndex pathingBlocker2 in cObjectObstacle.PathingBlockers)
				{
					ScenarioManager.PathFinder.Nodes[pathingBlocker2.X, pathingBlocker2.Y].Blocked = true;
				}
			}
			if (tileToMoveTo.m_HexMap.MapGuid != tileToMoveFrom.m_HexMap.MapGuid)
			{
				cObjectActor.AttachedProp.StartingMapGuid = tileToMoveTo.m_HexMap.MapGuid;
			}
		}
		currentActor.ArrayIndex = tileToMoveTo.m_ArrayIndex;
		if ((cObjectActor == null) | ignoreObjectActor)
		{
			CActorHasPushed_MessageData cActorHasPushed_MessageData = new CActorHasPushed_MessageData(currentActor);
			cActorHasPushed_MessageData.m_PushAbility = null;
			cActorHasPushed_MessageData.m_ActorBeingPushed = currentActor;
			cActorHasPushed_MessageData.m_ChoreographerWait = choreographerWait;
			ScenarioRuleClient.MessageHandler(cActorHasPushed_MessageData);
		}
		for (int num = tileToMoveTo.m_Props.Count - 1; num >= 0; num--)
		{
			if (tileToMoveTo.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.Door && (tileToMoveTo.m_Props[num].ObjectType != ScenarioManager.ObjectImportType.PressurePlate || currentActor.CanActivatePressurePlates()))
			{
				tileToMoveTo.m_Props[num].AutomaticActivate(currentActor);
			}
		}
		CAbility cAbility = CAbilityMove.CreateDefaultMove(1, currentActor.IsMonsterType);
		cAbility.TargetingActor = currentActor;
		currentActor.m_OnMovedListeners?.Invoke(cAbility, currentActor, new List<CActor>(), newActorCarried: false, 1, finalMovement: false, tileToMoveTo.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainWater || p.ObjectType == ScenarioManager.ObjectImportType.TerrainRubble), tileToMoveTo.m_Props.Count((CObjectProp p) => p.ObjectType == ScenarioManager.ObjectImportType.TerrainHotCoals || p.ObjectType == ScenarioManager.ObjectImportType.TerrainThorns), 1, currentActor.PrevArrayIndex);
	}

	public static void MoveActorInDirection(CActor currentActor, ScenarioManager.EAdjacentPosition moveDirection, List<CActor> actorList, int numTilesToMove, MoveActorInDirectionResult result = null, bool slideWithoutAnim = false, float slideSpeed = -1f, bool shouldWait = false, float forcedSpeed = -1f, bool ignoreObjectActor = false)
	{
		result?.Reset();
		if (!actorList.Contains(currentActor))
		{
			return;
		}
		int dirAngle = ScenarioManager.GetDirAngle(moveDirection);
		CObjectActor cObjectActor = currentActor as CObjectActor;
		CTile tile = currentActor.Tile;
		actorList.Remove(currentActor);
		for (int i = 0; i < numTilesToMove; i++)
		{
			CTile tile2 = currentActor.Tile;
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(currentActor.ArrayIndex.X, currentActor.ArrayIndex.Y, moveDirection);
			if (adjacentTile == null)
			{
				break;
			}
			CObjectDoor cObjectDoor = null;
			CActor cActor = null;
			cObjectDoor = (CObjectDoor)adjacentTile.FindProp(ScenarioManager.ObjectImportType.Door);
			cActor = ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex);
			if (cActor != null && cActor.DoesNotBlock)
			{
				cActor = null;
			}
			if (cActor != null)
			{
				MoveActorInDirection(cActor, moveDirection, actorList, numTilesToMove, null, slideWithoutAnim: false, -1f, shouldWait: false, -1f, ignoreObjectActor);
				cActor = ScenarioManager.Scenario.FindActorAt(adjacentTile.m_ArrayIndex);
			}
			CNode node = ScenarioManager.PathFinder.GetNode(tile2.m_ArrayIndex);
			CNode node2 = ScenarioManager.PathFinder.GetNode(adjacentTile.m_ArrayIndex);
			bool flag = true;
			if (!node2.Walkable)
			{
				flag = false;
			}
			if (node2.Blocked)
			{
				flag = false;
			}
			if (cObjectDoor != null && !cObjectDoor.DoorIsOpen)
			{
				flag = false;
			}
			if (cActor != null && !actorList.Contains(cActor))
			{
				flag = false;
			}
			if (node2.IslandID != node.IslandID && !node2.IsBridgeOpen && !node.IsBridgeOpen && !node2.IsBridge && !node.IsBridge)
			{
				flag = false;
			}
			if (flag)
			{
				bool ignoreObjectActor2 = ignoreObjectActor;
				PushActorTo(currentActor, adjacentTile, tile2, dirAngle, shouldWait, ignoreObjectActor2);
				if (result != null)
				{
					result.ActualMovedTiles++;
				}
				continue;
			}
			if (result != null)
			{
				result.BlockingActor = cActor;
				result.BlockingDoor = cObjectDoor;
			}
			break;
		}
		if (cObjectActor != null && !ignoreObjectActor)
		{
			CActorHasMoved_MessageData cActorHasMoved_MessageData = new CActorHasMoved_MessageData(cObjectActor);
			cActorHasMoved_MessageData.m_Ability = null;
			cActorHasMoved_MessageData.m_Waypoints = new List<CTile> { tile, cObjectActor.Tile };
			cActorHasMoved_MessageData.m_MovingActor = cObjectActor;
			cActorHasMoved_MessageData.m_ActorsToCarry = new List<CActor>();
			cActorHasMoved_MessageData.m_Jump = false;
			cActorHasMoved_MessageData.m_ForceMoveViaLoco = true;
			cActorHasMoved_MessageData.m_DontTeleport = true;
			cActorHasMoved_MessageData.m_ShouldWait = shouldWait;
			cActorHasMoved_MessageData.m_Slide = slideWithoutAnim;
			cActorHasMoved_MessageData.m_SlideSpeed = slideSpeed;
			cActorHasMoved_MessageData.m_ForcedSpeed = forcedSpeed;
			ScenarioRuleClient.MessageHandler(cActorHasMoved_MessageData);
		}
	}

	public static void MoveActorInDirection(CActor actor, CTile tileToMoveTowardsTo, int tilesToMove = 1, bool ignoreObjectActor = false)
	{
		ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(actor.ArrayIndex.X, actor.ArrayIndex.Y, tileToMoveTowardsTo.m_ArrayIndex.X, tileToMoveTowardsTo.m_ArrayIndex.Y);
		MoveActorInDirection(actor, adjacentPosition, new List<CActor> { actor }, tilesToMove, null, slideWithoutAnim: false, -1f, shouldWait: false, -1f, ignoreObjectActor);
	}

	public static void MoveActorsInDirection(CActor actor, CTile tileToMoveTowardsTo, int range, int tilesToMove, bool ignoreObjectActor = false)
	{
		ScenarioManager.EAdjacentPosition adjacentPosition = ScenarioManager.GetAdjacentPosition(actor.ArrayIndex.X, actor.ArrayIndex.Y, tileToMoveTowardsTo.m_ArrayIndex.X, tileToMoveTowardsTo.m_ArrayIndex.Y);
		MoveActorsInDirection(actor, adjacentPosition, range, tilesToMove, ignoreObjectActor);
	}

	public static void MoveActorsInDirection(CActor actor, ScenarioManager.EAdjacentPosition moveDirection, int range, int tilesToMove, bool ignoreObjectActor = false)
	{
		List<CActor> list = ((actor == null) ? ScenarioManager.Scenario.AllAliveActors.ToList() : (from x in ScenarioManager.Scenario.AllAliveActors
			where actor.GetDistanceTo(x) <= range
			orderby x.GetDistanceTo(actor) descending
			select x).ToList());
		while (list.Count > 0)
		{
			MoveActorInDirection(list[0], moveDirection, list, tilesToMove, null, slideWithoutAnim: false, -1f, shouldWait: false, -1f, ignoreObjectActor);
		}
	}

	public static List<CActor> GetActorsInRangeForAbility(CActor actor, CAbility ability)
	{
		List<CActor> result;
		if (ability.GetAbilityBaseCard(actor) is CAbilityCard)
		{
			CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
			cAbility.DontMoveAbilityState = true;
			cAbility.Start(actor, actor);
			result = cAbility.ValidActorsInRange?.ToList();
		}
		else
		{
			CActor filterActor = ability.FilterActor;
			CActor targetingActor = ability.TargetingActor;
			ability.FilterActor = actor;
			ability.TargetingActor = actor;
			result = SharedAbilityTargeting.GetActorsInRange(actor.ArrayIndex, ability.Range, ability).ToList();
			ability.FilterActor = filterActor;
			ability.TargetingActor = targetingActor;
		}
		return result;
	}

	public static CAIFocusOverrideDetails GetCurrentFocusOverride(CActor focusActor)
	{
		return ScenarioManager.CurrentScenarioState.Monsters.Find((EnemyState actor) => actor.ActorGuid == focusActor.ActorGuid)?.AIFocusOverride;
	}

	public static void SetFocusOverride(CActor focusActor, CActor focusTarget, bool ignoreHazards = true)
	{
		if (focusTarget != null)
		{
			CAIFocusOverrideDetails cAIFocusOverrideDetails = new CAIFocusOverrideDetails();
			cAIFocusOverrideDetails.OverrideTargetType = CAIFocusOverrideDetails.EOverrideTargetType.Actor;
			cAIFocusOverrideDetails.OverrideType = CAIFocusOverrideDetails.EOverrideType.OverrideFocus;
			cAIFocusOverrideDetails.TargetGUID = focusTarget.ActorGuid;
			cAIFocusOverrideDetails.IgnoreHazards = ignoreHazards;
			SetFocusOverride(focusActor, cAIFocusOverrideDetails);
		}
		else
		{
			SetFocusOverride(focusActor, (CAIFocusOverrideDetails)null);
		}
	}

	public static void SetFocusOverride(CActor focusActor, CAIFocusOverrideDetails focus)
	{
		(ScenarioManager.CurrentScenarioState.ActorStates.FirstOrDefault((ActorState a) => a.ActorGuid == focusActor.ActorGuid) as EnemyState).SetAIFocusOverride(focus);
	}

	public static List<int> AllTwoHerbPotionIDs()
	{
		return new List<int>
		{
			1031, 1035, 1037, 1039, 1041, 1043, 1045, 1047, 1051, 1049,
			1057, 1055, 1059, 1053, 1061
		};
	}

	public static List<CTile> GetTilesAffectedByExplosive_Scenario51(List<CTile> tiles)
	{
		List<CTile> list = new List<CTile>();
		foreach (CTile tile in tiles)
		{
			if (tile.IsWalkable && (tile.IsBlocked || (tile.LivingActor != null && CActor.AreActorsEnemies(CActor.EType.Player, tile.LivingActor.Type, null, tile.LivingActor))))
			{
				list.Add(tile);
			}
		}
		return list;
	}

	public static List<CActiveBonus> GetTideActiveBonuses(CActor actor)
	{
		return (from ab in CharacterClassManager.FindAllActiveBonuses()
			where ab.Actor == actor && ab.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.Tide)
			select ab).ToList();
	}

	public static List<CActiveBonus> GetCancellableTideActiveBonuses(CActor actor)
	{
		return (from bonus in GetTideActiveBonuses(actor)
			where !bonus.Ability.GetMiscVar("DontCancelTideOnRest").GetBool()
			select bonus).ToList();
	}

	public static List<CActiveBonus> GetTideActiveBonusesPlayedThisRound(CActor actor)
	{
		List<CActiveBonus> tideActiveBonuses = GetTideActiveBonuses(actor);
		tideActiveBonuses.RemoveAll((CActiveBonus x) => x.ActiveBonusStartRound != ScenarioManager.CurrentScenarioState.RoundNumber);
		return tideActiveBonuses;
	}

	public static CObjectProp FindProp(string propName)
	{
		return ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.PrefabName == propName);
	}

	public static CObjectProp GetPropOnHex(int x, int y)
	{
		return ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.ArrayIndex.X == x && p.ArrayIndex.Y == y);
	}

	public static bool IsActorNextToProp(CActor actor, CObjectProp prop)
	{
		if (prop == null)
		{
			return false;
		}
		List<CTile> adjacentTiles = GetAdjacentTiles(actor.Tile);
		if (adjacentTiles.Contains(prop.PropTile))
		{
			return true;
		}
		if (prop is CObjectObstacle cObjectObstacle)
		{
			foreach (TileIndex pathingBlocker in cObjectObstacle.PathingBlockers)
			{
				if (adjacentTiles.Contains(ScenarioManager.Tiles[pathingBlocker.X, pathingBlocker.Y]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AnyForgoneAction()
	{
		bool flag = GameState.IsActionFlagSet(GameState.EActionFlag.TopForgone);
		bool flag2 = GameState.IsActionFlagSet(GameState.EActionFlag.BottomForgone);
		return flag | flag2;
	}

	public static void ApplyConditionToActors(CActor targetingActor, List<CActor> actors, CCondition.ENegativeCondition negativeCondition)
	{
		foreach (CActor actor in actors)
		{
			actor.ApplyCondition(targetingActor, negativeCondition);
		}
	}

	public static bool HasPlayedInfusion(CActor actor)
	{
		if (!GameState.PlayedInfusion)
		{
			return GameState.PendingInfusionPhase != null;
		}
		return true;
	}

	public static List<(CActor actor, int health)> GetActorsAndHealth(List<CActor> actors)
	{
		return actors?.Select((CActor x) => (x: x, Health: x.Health)).ToList();
	}

	public static List<CActor> GetActorsWhoWereHealed(List<(CActor actor, int health)> actors)
	{
		return (from x in actors
			where x.health != x.actor.Health
			select x.actor).ToList();
	}

	public static List<CActor> GetActorsWithAdjacentObstacles(CAbility ability, int n, List<CTile> tiles)
	{
		return (from tile in GetAdjacentTiles(tiles).FindAll((CTile tile) => tile.LivingActor != null)
			select tile.LivingActor).ToList().FindAll((CActor actor) => GetAdjacentTiles(actor.Tile).Intersect(tiles).Count() >= n && ability.AbilityFilter.IsValidTarget(actor, ability.TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false)).ToList();
	}

	public static int GetNumberOfActiveInfusionPairs(CActor actor)
	{
		return GetNumberOfActiveInfusions(actor) / 2;
	}

	public static int GetNumberOfActiveInfusions(CActor actor)
	{
		return actor.FindApplicableActiveBonuses().FindAll((CActiveBonus bonus) => bonus.Ability.ActiveBonusData.GetMiscVar("InfusionBonus").GetBool() || bonus.Ability.GetMiscVar("TreatAsInfusedPair").GetBool()).Count;
	}

	public static int GetDamageDealt(CAbility ability)
	{
		if (ability is CAbilityAttack cAbilityAttack)
		{
			if (cAbilityAttack?.TotalTargetSummaries == null)
			{
				return 0;
			}
			return cAbilityAttack.TotalTargetSummaries.Sum((CAttackSummary.TargetSummary targetSummary) => targetSummary.FinalAttackStrength);
		}
		if (ability is CAbilityDamage cAbilityDamage)
		{
			if (cAbilityDamage?.TargetsDamagedSummaries == null)
			{
				return 0;
			}
			return cAbilityDamage.TargetsDamagedSummaries.Sum((CAttackSummary.TargetSummary targetSummary) => targetSummary.FinalAttackStrength);
		}
		return 0;
	}

	public static List<(CClass actorClass, CActor actor)> GetClassesFromActors(List<object> actors)
	{
		List<(CClass, CActor)> list = new List<(CClass, CActor)>();
		foreach (CActor actor in actors)
		{
			CClass classToAdd = null;
			CActor cActor2 = null;
			cActor2 = ((!(actor is CHeroSummonActor { Summoner: not null } cHeroSummonActor)) ? actor : cHeroSummonActor.Summoner);
			classToAdd = cActor2.Class;
			if (!list.Any(((CClass actorClass, CActor actor) x) => x.actorClass == classToAdd))
			{
				list.Add((classToAdd, cActor2));
			}
		}
		return list;
	}

	public static int RemoveCurseModifiers(List<(CClass actorClass, CActor actor)> classesAndActors)
	{
		int num = 0;
		foreach (var classesAndActor in classesAndActors)
		{
			num += classesAndActor.actorClass.RemoveCurses(classesAndActor.actor);
		}
		return num;
	}

	public static void AddCurseModifiersToEnemyMonsterDecks(int cnt)
	{
		if (cnt == 0)
		{
			return;
		}
		for (int i = 0; i < cnt; i++)
		{
			MonsterClassManager.AddCurseCard(MonsterClassManager.EnemyMonsterAttackModifierDeck);
		}
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			CCurse_MessageData message = new CCurse_MessageData(null, null)
			{
				m_ActorToCurse = enemy,
				m_DifficultyModCurse = false,
				m_ConditionAlreadyApplied = false
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static void AdjacentEnemiesTakeTrapDamage(CActor actor)
	{
		foreach (CActor item in GameState.GetActorsInRange(actor, actor, 1, new List<CActor> { actor }, CAbilityFilterContainer.CreateDefaultEnemyFilter(), null, null, isTargetedAbility: false, null, true, originalTargetType: false, skipUntargetableCheck: false, ignoreLOS: false, useRealPositions: true, freeIce: false))
		{
			List<CAbility.EAbilityType> immunities = item.Immunities;
			if (immunities == null || !immunities.Contains(CAbility.EAbilityType.Damage))
			{
				_ = item.Health;
				bool actorWasAsleep = item.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
				GameState.ActorBeenDamaged(item, ScenarioManager.TrapDamage, checkIfPlayerCanAvoidDamage: true, actor);
				if (ScenarioManager.Scenario.HasActor(item))
				{
					GameState.ActorHealthCheck(actor, item, out var _, isTrap: false, isTerrain: false, actorWasAsleep);
				}
			}
		}
	}

	public static bool AbilityHasFlag(CAbility ability, CAbility.EAbilityFlag flag)
	{
		if (ability == null)
		{
			return false;
		}
		return ability.Flags.HasFlag(flag);
	}

	public static bool ActiveBonusDataHasFlag(AbilityData.ActiveBonusData data, AbilityData.EActiveBonusFlag flag)
	{
		return data.Flags.HasFlag(flag);
	}

	public static bool Damage(CActor actor, int damageValue, CActor damageSource = null)
	{
		bool actorWasAsleep = actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep);
		int health = actor.Health;
		GameState.ActorBeenDamaged(actor, damageValue, checkIfPlayerCanAvoidDamage: true, damageSource);
		CActorBeenDamaged_MessageData cActorBeenDamaged_MessageData = new CActorBeenDamaged_MessageData(actor);
		cActorBeenDamaged_MessageData.m_ActorBeingDamaged = actor;
		cActorBeenDamaged_MessageData.m_ActorOriginalHealth = health;
		cActorBeenDamaged_MessageData.m_ActorWasAsleep = actorWasAsleep;
		ScenarioRuleClient.MessageHandler(cActorBeenDamaged_MessageData);
		if (ScenarioManager.Scenario.HasActor(actor))
		{
			GameState.ActorHealthCheck(actor, actor, out var _, isTrap: false, isTerrain: false, actorWasAsleep);
		}
		return true;
	}

	public static bool DamageAnimated(CActor actor, int damageValue, CActor damageSource = null, string animOverload = null)
	{
		CAbility cAbility = CAbilityDamage.CreateDefaultDamage(damageValue, 1, 1, isMonster: false, animOverload);
		cAbility.TargetingActor = damageSource;
		cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
		cAbility.MiscAbilityData.AutotriggerAbility = true;
		cAbility.AnimOverload = animOverload;
		cAbility.Targeting = CAbility.EAbilityTargeting.Custom;
		cAbility.TargetingVars = new List<(string, TValue)>();
		cAbility.TargetingVars.Add(("AddValidActorInRange", new TValue(actor)));
		PhaseManager.CurrentPhaseAction.StackNextAbilities(new List<CAbility> { cAbility }, damageSource, killAfter: false, stackToNextCurrent: true, copyCurrentActionID: false, interruptCurrent: true, addDirectlyToCurrentAbilities: false, forcedInterrupt: true, interruptRegardlessOfStrength: false, dontCreateCopies: true);
		return true;
	}

	public static bool AbilityHasKilledEnemy(CAbility ability)
	{
		foreach (CActor allDeadActor in ScenarioManager.Scenario.AllDeadActors)
		{
			if (allDeadActor.CauseOfDeathAbility != null && allDeadActor.CauseOfDeathAbility.ID == ability.ID)
			{
				return true;
			}
		}
		return false;
	}

	public static void ResetInfusionTurnRestriction()
	{
		foreach (CActiveBonus item in (IEnumerable<CActiveBonus>)(from bonus in CharacterClassManager.FindAllActiveBonuses().Distinct()
			where bonus.Ability.GetMiscVar("Infusion").GetBool()
			select bonus).ToList())
		{
			item.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerTurn);
			item.ApplyToAction = false;
		}
		ScenarioManager.Scenario.AllAliveActors.Find((CActor actor) => actor.Class.ID == "InfuserID").TriggerInfusionsWithSummon = false;
	}

	public static void SetTriggerInfusionsWithSummon(CActor infuser, bool value)
	{
		infuser.TriggerInfusionsWithSummon = value;
	}

	public static void PayMovementPoints(int points, CActor actor, CActiveBonus activeBonus = null)
	{
		CAbilityMove cAbilityMove = PhaseManager.LastMoveAbility(actor);
		if (cAbilityMove == null)
		{
			return;
		}
		int remainingStrength = cAbilityMove.RemainingMoves - points;
		if (remainingStrength > 0)
		{
			cAbilityMove.CreateResumeCopy((CAbilityMove resumeCopy) =>
			{
				int strength = (resumeCopy.RemainingMoves = remainingStrength);
				resumeCopy.Strength = strength;
			}, locoStop: false);
		}
		cAbilityMove.SetCancelAbilityFlag(cancel: true);
		if (activeBonus == null)
		{
			return;
		}
		CPhaseAction.CPhaseAbility currentPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbility;
		currentPhaseAbility.m_PostEndAction = (Action)Delegate.Combine(currentPhaseAbility.m_PostEndAction, (Action)(() =>
		{
			activeBonus.CostAbilityExecuted = false;
		}));
		foreach (CActiveBonus item in from x in actor.FindApplicableActiveBonuses(CAbility.EAbilityType.Move, CActiveBonus.EActiveBonusBehaviourType.BuffMove)
			where x.ToggledBonus
			select x)
		{
			if (item.TryGetBehaviour(out CMoveActiveBonus_BuffMove behaviour))
			{
				behaviour.m_PayedMovementPoints = true;
				behaviour.OnMoving(cAbilityMove);
			}
		}
		PhaseManager.CurrentPhaseAction.AutoConsume();
	}

	public static void UnrestrictOnAbilityFinish(CAbility ability, CActiveBonus bonus)
	{
		CPhaseAction.CPhaseAbility cPhaseAbility = PhaseManager.CurrentPhaseAction.CurrentPhaseAbilities.SingleOrDefault((CPhaseAction.CPhaseAbility x) => x.m_Ability == ability);
		if (cPhaseAbility != null)
		{
			cPhaseAbility.m_PostEndAction = (Action)Delegate.Combine(cPhaseAbility.m_PostEndAction, (Action)(() =>
			{
				bonus.ResetRestriction(CActiveBonus.EActiveBonusRestrictionType.OncePerRound);
			}));
		}
	}

	public static bool IsElementInfused(ElementInfusionBoardManager.EElement element)
	{
		return ElementInfusionBoardManager.GetAvailableElements().Contains(element);
	}

	public static bool IsElementInCreation(ElementInfusionBoardManager.EElement element)
	{
		return ElementInfusionBoardManager.GetElementsToInfuseCopy().Any((KeyValuePair<string, List<ElementInfusionBoardManager.EElement>> x) => x.Value.Contains(element));
	}

	public static void OverrideRestrictionOfTideActiveBonuses(CActor actor)
	{
		foreach (CActiveBonus item in actor.ActiveBonuses.Where((CActiveBonus x) => x.Ability.ActiveBonusData.GetMiscVar("RestrictionAffectedByOtherTides").GetBool()).ToList())
		{
			item.Ability.ActiveBonusData.SetMiscVar("NoRestriction", new TValue(val: true));
		}
	}

	public static void PressureChange(CPlayerActor actor, int value, bool instantChange = false)
	{
		IEnumerable<CActiveBonus> source = CharacterClassManager.FindAllActiveBonuses().Distinct();
		CActiveBonus cActiveBonus = source.FirstOrDefault((CActiveBonus bonus) => bonus.Ability.GetMiscVar("DoubleGainPressure").GetBool());
		CActiveBonus cActiveBonus2 = source.FirstOrDefault((CActiveBonus bonus) => bonus.Ability.GetMiscVar("DoubleLosePressure").GetBool());
		if (value > 0 && cActiveBonus != null)
		{
			value++;
			cActiveBonus.UpdateXPTracker();
			cActiveBonus.FinishIfNoRemaining();
		}
		if (value < 0 && cActiveBonus2 != null)
		{
			value--;
			cActiveBonus2.UpdateXPTracker();
			cActiveBonus2.FinishIfNoRemaining();
		}
		actor.IsChangingPressure = true;
		actor.PressureChangeValue += value;
		if (instantChange)
		{
			CPlayerActor.FinishChangePressure(endTurn: false);
		}
	}

	public static CActiveBonus FindActiveBonusOnCard(CBaseCard card, string bonusName)
	{
		return card.ActiveBonuses.SingleOrDefault((CActiveBonus x) => x.Ability.Name == bonusName);
	}

	public static void SetPressure(int val, CActor actor)
	{
		if (actor is CPlayerActor cPlayerActor)
		{
			cPlayerActor.Pressure = (CPlayerActor.EPressure)val;
		}
	}

	public static CActiveBonus FindActiveBonusOnActor(CActor actor, string miscVarName)
	{
		return actor.ActiveBonuses.SingleOrDefault((CActiveBonus x) => x.Ability.GetMiscVar(miscVarName).GetBool());
	}

	public static bool HasActiveBonus(CBaseCard card, string bonusName)
	{
		return FindActiveBonusOnCard(card, bonusName) != null;
	}

	public static bool HasActiveBonus(CPlayerActor actor, string bonusName)
	{
		foreach (CBaseCard activatedCard in actor.CharacterClass.ActivatedCards)
		{
			if (FindActiveBonusOnCard(activatedCard, bonusName) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static void PerformActiveBonusAbilityOnCard(CBaseCard card, string bonusName)
	{
		CActiveBonus cActiveBonus = FindActiveBonusOnCard(card, bonusName);
		if (cActiveBonus != null)
		{
			CVars cVars = new CVars();
			cVars.Set("CalledFromCard", new TValue(card));
			CVarsContext context = new CVarsContext(cVars);
			PerformActiveBonusAbility(cActiveBonus, context);
		}
	}

	public static void RemoveCardBonuses(CBaseCard card)
	{
		foreach (CActiveBonus item in card.ActiveBonuses.ToList())
		{
			if (!item.Finishing())
			{
				item.Finish();
			}
		}
	}

	public static void RemoveBonusesWithVar(CActor actor, string key)
	{
		foreach (CActiveBonus item in (from x in actor.FindApplicableActiveBonuses()
			where x.Ability.ActiveBonusData.GetMiscVar(key).GetBool()
			select x).ToList())
		{
			if (!item.Finishing())
			{
				item.Finish();
			}
		}
	}

	public static List<CBaseCard> GetActivatedInfusionCards(CActor actor)
	{
		if (!(actor is CPlayerActor cPlayerActor))
		{
			return new List<CBaseCard>();
		}
		return cPlayerActor.CharacterClass.ActivatedCards.Where((CBaseCard card) => card.ActiveBonuses.Any((CActiveBonus activeBonus) => activeBonus.Ability.GetMiscVar("Infusion").GetBool())).ToList();
	}

	public static void GainTrophyTokens(int Tokens, CPlayerActor pa)
	{
		if (Tokens != 0)
		{
			pa?.SetTrophyTokens(pa.GetTrophyTokens() + Tokens);
		}
	}

	public static void DestroyPropsOfType(EPropType type, List<CTile> tiles, CAbility destroyedByAbility = null)
	{
		foreach (CTile tile in tiles)
		{
			tile.FindPropOfTypes(type, forDestructionFromPlayer: true)?.DestroyProp(0f, sendMessageToClient: true, forceDestroy: false, destroyedByAbility);
		}
	}

	public static List<CTile> GetPushTilesForPreview(CAbility ability, CTile mouseOverTile, IEnumerable<Point> superBlocked = null)
	{
		CTile startTile = ability.TargetingActor.Tile;
		if (ability.TilesSelected.Count > 0)
		{
			startTile = ability.TilesSelected.Last();
		}
		return GetTilesForPreviewFromTile(ability, startTile, mouseOverTile, superBlocked);
	}

	public static List<CTile> GetTilesForPreviewFromTile(CAbility ability, CTile startTile, CTile mouseOverTile, IEnumerable<Point> superBlocked = null)
	{
		int count = ability.TilesSelected.Count;
		int numberTargets = ability.NumberTargets;
		List<CTile> list = new List<CTile>();
		if (count >= numberTargets)
		{
			return list;
		}
		List<Point> list2 = new List<Point>();
		if (superBlocked != null)
		{
			list2.AddRange(superBlocked);
		}
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && !CAbilityFilter.IsValidTile(cTile, ability.TileFilter, includeInitial: false, null, parentContext: ability.VarsContext, vars: ability.MiscVars))
			{
				list2.Add(cTile.m_ArrayIndex);
			}
		}
		bool foundPath;
		foreach (Point item in CActor.FindStraightPath(ability.TargetingActor, startTile.m_ArrayIndex, mouseOverTile.m_ArrayIndex, ignoreBlocked: true, out foundPath, list2))
		{
			CTile tile = ScenarioManager.GetTile(item);
			if (tile != null)
			{
				list.Add(tile);
				if (count + list.Count >= numberTargets)
				{
					break;
				}
			}
		}
		return list;
	}

	public static IEnumerable<Point> TilesWithFeatures()
	{
		CTile[,] tiles = ScenarioManager.Tiles;
		foreach (CTile cTile in tiles)
		{
			if (cTile != null && !CAbilityFilter.IsTileFeatureless(cTile))
			{
				yield return cTile.m_ArrayIndex;
			}
		}
	}

	public static CTile TryGetFirstTile(List<CTile> res)
	{
		if (res == null || res.Count == 0)
		{
			return null;
		}
		return res[0];
	}

	public static CTile TryGetLastTile(List<CTile> res)
	{
		if (res == null || res.Count == 0)
		{
			return null;
		}
		return res[res.Count - 1];
	}

	public static void RemoveRemovedTilesToFormPath(CAbility ability)
	{
		CTile cTile = ability.GetVar("From", ability.VarsContext).GetObject(ability.VarsContext, ability.TargetingActor.Tile);
		List<CTile> tilesSelected = ability.TilesSelected;
		for (int i = 0; i < tilesSelected.Count; i++)
		{
			CTile cTile2 = cTile;
			if (i > 0)
			{
				cTile2 = tilesSelected[i - 1];
			}
			CTile cTile3 = tilesSelected[i];
			if (Point.HexDist(cTile2.m_ArrayIndex, cTile3.m_ArrayIndex) > 1)
			{
				tilesSelected.RemoveRange(i, tilesSelected.Count - i);
				break;
			}
		}
	}

	public static void RecalcValidTilesToFormPath(CAbility ability, bool asPush = false, IEnumerable<Point> superBlocked = null)
	{
		List<CTile> tilesSelected = ability.TilesSelected;
		int numberTargets = ability.NumberTargets;
		CTile cTile = ability.GetVar("From", ability.VarsContext).GetObject(ability.VarsContext, ability.TargetingActor.Tile);
		CTile cTile2 = cTile;
		if (tilesSelected.Count < numberTargets && tilesSelected.Count > 0)
		{
			CTile cTile3 = tilesSelected[tilesSelected.Count - 1];
			if (tilesSelected.Count > 1)
			{
				cTile = tilesSelected[tilesSelected.Count - 2];
			}
			tilesSelected.Remove(cTile3);
			List<CTile> tilesForPreviewFromTile = GetTilesForPreviewFromTile(ability, cTile, cTile3, superBlocked);
			tilesForPreviewFromTile.Remove(cTile);
			tilesSelected.AddRange(tilesForPreviewFromTile);
			cTile2 = cTile3;
		}
		ability.NumberTargetsRemaining = numberTargets - tilesSelected.Count;
		ability.TilesInRange.Clear();
		if (!asPush)
		{
			List<Point> list = new List<Point>();
			if (superBlocked != null)
			{
				list.AddRange(superBlocked);
			}
			CTile[,] tiles = ScenarioManager.Tiles;
			foreach (CTile cTile4 in tiles)
			{
				if (cTile4 != null && (!cTile4.IsWalkable || !CAbilityFilter.IsValidTile(cTile4, ability.TileFilter, includeInitial: false, null, parentContext: ability.VarsContext, vars: ability.MiscVars)))
				{
					list.Add(cTile4.m_ArrayIndex);
				}
			}
			List<Point> list2 = new List<Point>();
			ScenarioManager.PathFinder.FloodFill_ToRange(ability.TargetingActor, cTile2.m_ArrayIndex, ability.NumberTargetsRemaining, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, list, list2, null, ignoreDoorLocks: false, null, isCalculatingDistance: false, ignoreCannotStop: true);
			ability.TilesInRange.AddRange(list2.Select((Point x) => ScenarioManager.GetTile(x)).Concat(tilesSelected).Distinct());
			if (tilesSelected.Count == 0)
			{
				ability.TilesInRange.Remove(cTile);
			}
		}
		else
		{
			List<CTile> pushTiles = GetPushTiles(ability.TargetingActor, CActor.EType.Enemy, cTile2.m_ArrayIndex, ability.TargetingActor.ArrayIndex, ability.NumberTargetsRemaining, intoBlocked: true, ability);
			ability.TilesInRange.AddRange(pushTiles.Concat(tilesSelected).Distinct());
		}
	}

	public static List<CTile> GetPushTiles(CActor actorToPush, CActor.EType targetType, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked, CAbility ability)
	{
		List<CTile> pushTiles_New = GetPushTiles_New(actorToPush, pushTarget, pushSource, remainingPushes, intoBlocked, ability);
		SharedAbilityTargeting.RemoveInvalidTilesForAbilityMirrored(pushTiles_New, ability);
		return pushTiles_New;
	}

	private static List<CTile> GetPushTiles_New(CActor actorToPush, Point pushTarget, Point pushSource, int remainingPushes, bool intoBlocked, CAbility ability)
	{
		List<CTile> tiles = new List<CTile>();
		if (remainingPushes == 0)
		{
			return tiles;
		}
		List<Point> pathSoFar = new List<Point> { pushTarget };
		GetPushTilesRecursive_New(actorToPush, pushTarget, pushSource, remainingPushes, ref pathSoFar, ref tiles, intoBlocked, ability);
		return tiles;
	}

	private static void GetPushTilesRecursive_New(CActor actorToPush, Point pushTarget, Point pushSource, int remainingPushes, ref List<Point> pathSoFar, ref List<CTile> tiles, bool intoBlocked, CAbility ability)
	{
		List<Point> list = new List<Point>();
		ScenarioManager.PathFinder.FloodFill_ToRange(actorToPush, pushTarget, 1, intoBlocked, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: true, 0, null, null, list, null, ignoreDoorLocks: false, (CPathFinderContext context) =>
		{
			CAIMoveNew.AddDoorsToPathFindingContext(context, disallowDoorways: false, shouldPathThroughDoors: false);
		}, isCalculatingDistance: false, ignoreCannotStop: false, CAbility.EAbilityType.Push);
		int num = Point.AxialDistance(Point.OddRToAxial(pushSource), Point.OddRToAxial(pushTarget));
		foreach (Point item in list)
		{
			CTile cTile = ScenarioManager.Tiles[item.X, item.Y];
			CObjectProp cObjectProp = cTile.FindProp(ScenarioManager.ObjectImportType.Door);
			if ((cObjectProp != null && !cObjectProp.Activated) || !CAbilityFilter.IsValidTile(cTile, ability.TileFilter) || Point.AxialDistance(Point.OddRToAxial(pushSource), Point.OddRToAxial(item)) <= num)
			{
				continue;
			}
			bool flag = true;
			int num2 = num;
			List<Point> list2 = CActor.FindCharacterPath(actorToPush, pushTarget, item, intoBlocked, ignoreMoveCost: true, out var foundPath);
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
				flag = false;
				break;
			}
			if (!flag)
			{
				pathSoFar.RemoveAt(pathSoFar.Count - 1);
				continue;
			}
			if (!tiles.Contains(ScenarioManager.Tiles[item.X, item.Y]) && remainingPushes > 0)
			{
				tiles.Add(ScenarioManager.Tiles[item.X, item.Y]);
			}
			if (remainingPushes > 0)
			{
				int remainingPushes2 = remainingPushes - 1;
				GetPushTilesRecursive_New(actorToPush, item, pushSource, remainingPushes2, ref pathSoFar, ref tiles, intoBlocked, ability);
			}
			pathSoFar.RemoveAt(pathSoFar.Count - 1);
		}
	}

	public static void MoveHazardousTerrain(CObjectHazardousTerrain prop, CTile tile, CAbility fromAbility = null)
	{
		prop.DestroyProp();
		tile.SpawnProp(new CObjectHazardousTerrain(prop.PrefabName, prop.ObjectType, new TileIndex(tile.m_ArrayIndex), null, prop.Rotation, prop.Owner, tile.m_HexMap.MapGuid), notifyClient: true, 0f, fromAbility);
		if (fromAbility?.TargetingActor != null)
		{
			CCombatLogMessage_MessageData message = new CCombatLogMessage_MessageData
			{
				m_MessageKey = "COMBAT_LOG_MOVE_PROP",
				m_Actors = new List<CActor> { fromAbility.TargetingActor },
				m_Params = new List<string> { "Actor_0", "<sprite name=HazardHex>" }
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	public static void StackNonInlineSubabilities(CAbility ability, bool inject = false)
	{
		List<CAbility> list = new List<CAbility>();
		foreach (CAbility subAbility in ability.SubAbilities)
		{
			if (!subAbility.IsInlineSubAbility && !PhaseManager.CurrentPhaseAction.RemainingPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.ID == subAbility.ID && x.TargetingActor == ability.TargetingActor))
			{
				CAbility cAbility = CAbility.CopyAbility(subAbility, generateNewID: false);
				cAbility.ParentAbility = ability;
				cAbility.ProcessIfDead = ability.ProcessIfDead;
				list.Add(cAbility);
			}
		}
		if (inject)
		{
			PhaseManager.CurrentPhaseAction.InjectAbilities(list, ability.TargetingActor);
		}
		else
		{
			PhaseManager.CurrentPhaseAction.StackNextAbilities(list, ability.TargetingActor, killAfter: false, stackToNextCurrent: true);
		}
	}

	public static void ToggleLootAmuletsFromAliveMonsters(CAbility currentAbility, CActiveBonus activeBonus, bool isOn)
	{
		currentAbility.NumberTargets = (isOn ? 1 : currentAbility.Original.NumberTargets);
		currentAbility.ClearTargets();
		currentAbility.SetMiscVar("LootAmuletsFromAliveMonsters", isOn);
		currentAbility.Start(currentAbility.TargetingActor, currentAbility.FilterActor, currentAbility.ControllingActor);
		currentAbility.Perform();
	}

	public static bool IsItemPartiallySpent(CItem item)
	{
		if (item.SlotState == CItem.EItemSlotState.Spent)
		{
			return true;
		}
		return item.ActiveBonuses?.Any((CActiveBonus x) => x.HasTracker && x.TrackerIndex > 0) ?? false;
	}

	public static List<object> GetListFromActor(CActor TargetingActor, string listName)
	{
		List<object> list = TargetingActor.GetVar(listName).GetObject<List<object>>();
		if (list == null)
		{
			list = new List<object>();
			TargetingActor.SetTempVar(listName, new TValue(list));
		}
		return list;
	}

	public static void AddObjectToList(CActor TargetingActor, string listName, object obj)
	{
		GetListFromActor(TargetingActor, listName).Add(obj);
	}

	public static bool ExclusiveSelf(CAbility ability)
	{
		return ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true);
	}

	public static bool IsFromAbilityCard(CAbility ability)
	{
		if (ability.GetAbilityBaseCard() != null && ability.GetAbilityBaseCard().CardType == CBaseCard.ECardType.CharacterAbility)
		{
			return true;
		}
		if (ability.ParentAbility != null && IsFromAbilityCard(ability.ParentAbility))
		{
			return true;
		}
		if (ability is CAbilityAttack { IsDefaultAttack: not false })
		{
			return true;
		}
		if (ability is CAbilityMove { IsDefaultMove: not false })
		{
			return true;
		}
		return false;
	}

	public static bool IsFromShattersongCard(CAbility ability)
	{
		if (ability.GetAbilityBaseCard() != null && ability.GetAbilityBaseCard() is CAbilityCard { ClassID: "ShattersongID" })
		{
			return true;
		}
		if (ability.ParentAbility != null && IsFromShattersongCard(ability.ParentAbility))
		{
			return true;
		}
		if (ability is CAbilityAttack { IsDefaultAttack: not false } && ability.TargetingActor?.Class?.ID == "ShattersongID")
		{
			return true;
		}
		if (ability is CAbilityMove { IsDefaultMove: not false } && ability.TargetingActor?.Class?.ID == "ShattersongID")
		{
			return true;
		}
		return false;
	}

	public static bool IsSelectingTargets(CAbility ability)
	{
		if (ability is CAbilityAttack cAbilityAttack)
		{
			if (cAbilityAttack.State != CAbilityAttack.EAttackState.SelectActiveBonusBuffTarget)
			{
				return cAbilityAttack.State == CAbilityAttack.EAttackState.SelectAttackFocus;
			}
			return true;
		}
		if (ability is CAbilityPush cAbilityPush)
		{
			if (cAbilityPush.AbilityHasHappened)
			{
				return false;
			}
			return cAbilityPush.State == CAbilityPush.EPushState.SelectPushTarget;
		}
		if (ability is CAbilityTargeting cAbilityTargeting)
		{
			if (cAbilityTargeting.AbilityHasHappened)
			{
				return false;
			}
			if (cAbilityTargeting.State != CAbilityTargeting.TargetingState.ActorIsSelectingTargetingFocus)
			{
				return cAbilityTargeting.State == CAbilityTargeting.TargetingState.ActorIsSelectingAoE;
			}
			return true;
		}
		return true;
	}

	public static bool CostChooseAbilityHasTargets(CActiveBonus bonus)
	{
		if (!(bonus.Ability.ActiveBonusData.CostAbility is CAbilityChooseAbility cAbilityChooseAbility))
		{
			return false;
		}
		return cAbilityChooseAbility.HasValidChooseAbilities(bonus.Caster);
	}

	public static void UntoggleSingleTargetItems(CAbility ability)
	{
		if (!(ability is CAbilityAttack cAbilityAttack))
		{
			return;
		}
		foreach (CItem item in cAbilityAttack.ActiveSingleTargetItems.ToList())
		{
			cAbilityAttack.ToggleSingleTargetItem(item);
			item.SlotState = CItem.EItemSlotState.Equipped;
		}
	}

	public static void UntoggleAugments(bool isOn)
	{
		foreach (CActionAugmentation item in PhaseManager.CurrentPhaseAction.ActionAugmentationsToConsume.ToList())
		{
			PhaseManager.CurrentPhaseAction.ToggleActionAugmentation(item);
		}
		PhaseManager.CurrentPhaseAction.RefreshConsumeBar();
	}

	public static void BuffAttackBonus(CAttackActiveBonus bonus, int strength, string condition)
	{
		if (strength != 0)
		{
			AbilityData.StatIsBasedOnXData item = new AbilityData.StatIsBasedOnXData
			{
				AbilityStatType = Enum.Parse<CAbility.EAbilityStatType>("Strength"),
				AddTo = false,
				Multiplier = bonus.Ability.Strength + strength,
				BasedOn = CAbility.EStatIsBasedOnXType.X
			};
			bonus.Ability.StatIsBasedOnXEntries.Add(item);
		}
		CCondition.ENegativeCondition eNegativeCondition = CCondition.NegativeConditions.SingleOrDefault((CCondition.ENegativeCondition x) => x.ToString() == condition);
		if (eNegativeCondition != CCondition.ENegativeCondition.NA)
		{
			bonus.Ability.AddNegativeCondition(eNegativeCondition);
		}
	}

	public static CAbility FindAbilityWithPrintedProperty(string property, CAbility ability)
	{
		if (IsPropertyPrintedOnAbility(ability, property))
		{
			return ability;
		}
		if (ability is CAbilityControlActor cAbilityControlActor)
		{
			foreach (CAbility controlAbility in cAbilityControlActor.ControlActorData.ControlAbilities)
			{
				CAbility cAbility = FindAbilityWithPrintedProperty(property, controlAbility);
				if (cAbility != null)
				{
					return cAbility;
				}
			}
		}
		return null;
	}

	public static void ModifyAbilityValue(CAbility ability, string valueType, int value)
	{
		CAbility arg = ability;
		ability = FindAbilityWithPrintedProperty(valueType, ability);
		if (ability == null)
		{
			Log.Error($"Trying to modify {valueType} on {arg}, but can't find that property on it");
			return;
		}
		CAbilityAttack cAbilityAttack = ability as CAbilityAttack;
		switch (valueType)
		{
		case "Range":
			if (cAbilityAttack != null)
			{
				bool flag = cAbilityAttack.Range > 1;
				AbilityData.MiscAbilityData miscAbilityData = cAbilityAttack.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.TreatAsMelee.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData2 = cAbilityAttack.MiscAbilityData;
					if (miscAbilityData2 != null && miscAbilityData2.TreatAsMelee.Value)
					{
						flag = false;
					}
				}
				if (!flag)
				{
					if (cAbilityAttack.MiscAbilityData == null)
					{
						cAbilityAttack.MiscAbilityData = new AbilityData.MiscAbilityData();
					}
					cAbilityAttack.MiscAbilityData.TreatAsMelee = true;
				}
			}
			if (ability.OriginalTargetCount == -1)
			{
				ability.NumberTargets = -1;
				ability.AllTargets = false;
			}
			ability.m_HasAutoSelected = false;
			break;
		case "Attack":
		case "Damage":
		case "Move":
		case "Heal":
		case "Push":
		case "Pull":
		case "Shield":
		case "Retaliate":
		case "RecoverDiscardedCards":
			valueType = "Strength";
			break;
		case "Loot":
			valueType = "Range";
			break;
		case "NumberOfTargetsAsRange":
			valueType = "NumberOfTargets";
			break;
		}
		ability.ResetStatBasedOnXAddedStats(ability.StatIsBasedOnXEntries);
		CAbility.EAbilityStatType type = Enum.Parse<CAbility.EAbilityStatType>(valueType);
		CAbilityOverride.AddStatIsBasedOnX(ability, value, type);
		UntoggleSingleTargetItems(ability);
		ScenarioRuleClient.MessageHandler(new CRefreshWorldSpaceStarHexDisplay_MessageData(isRestart: true));
	}

	public static CObjectProp FindTrapOrObstacle(CTile tile)
	{
		CObjectTrap cObjectTrap = tile.FindTrapProp(forDestructionFromPlayer: true);
		if (cObjectTrap != null)
		{
			return cObjectTrap;
		}
		return tile.FindObstacleProp(forDestructionFromPlayer: true, checkMultiHexProps: true);
	}

	public static List<AttackModifierYMLData> GetAttackModifierDeck(CActor actor)
	{
		string deckID;
		return GetAttackModifierDeck(actor, out deckID);
	}

	public static List<AttackModifierYMLData> GetAttackModifierDeck(CActor actor, out string deckID)
	{
		if (actor is CEnemyActor enemyActor)
		{
			return MonsterClassManager.GetAttackModifiersDeck(enemyActor, out deckID).AttackModifierCards;
		}
		if (actor is CPlayerActor cPlayerActor)
		{
			deckID = cPlayerActor.CharacterClass.LocKey;
			return cPlayerActor.CharacterClass.AttackModifierCards;
		}
		if (actor is CHeroSummonActor cHeroSummonActor)
		{
			deckID = cHeroSummonActor.Summoner?.CharacterClass.LocKey;
			return cHeroSummonActor.Summoner?.CharacterClass?.AttackModifierCards;
		}
		deckID = null;
		return null;
	}

	public static AttackModifierCard NextAttackModifier(CActor actor)
	{
		return GetAttackModifierDeck(actor)?.FirstOrDefault()?.Card;
	}

	public static void MoveNextModifierToBackOfDeck(CActor actor, bool refreshDecks = true)
	{
		List<AttackModifierYMLData> attackModifierDeck = GetAttackModifierDeck(actor);
		AttackModifierYMLData attackModifierYMLData = attackModifierDeck?.FirstOrDefault();
		if (attackModifierYMLData != null)
		{
			attackModifierDeck.Remove(attackModifierYMLData);
			attackModifierDeck.Add(attackModifierYMLData);
			if (refreshDecks)
			{
				CShowNextAttackModifierBehaviour.ValidateAllShowNextModifierBonuses(actor);
			}
		}
	}

	public static void ProgressCardActionFlags(CPlayerActor playerActor, CAbilitySelectAndPlayCard selectAndPlay)
	{
		int count = playerActor.CharacterClass.RoundAbilityCardsSelectedInCardSelection.Count;
		CAbilityCard cAbilityCard = selectAndPlay.selectedAbilityCardsOutOfPhase.FirstOrDefault();
		if (cAbilityCard.LastPlayedActionType != CBaseCard.ActionType.NA)
		{
			CAction action = cAbilityCard.GetAction(cAbilityCard.LastPlayedActionType);
			GameState.SetActionFlagRound((action?.ID == cAbilityCard.TopAction?.ID || action?.ID == cAbilityCard.DefaultAttackAction?.ID) ? GameState.EActionFlag.TopPlayed : GameState.EActionFlag.BottomPlayed, count);
		}
	}

	public static bool SameModifierDeck(CActor A, CActor B)
	{
		List<AttackModifierYMLData> attackModifierDeck = GetAttackModifierDeck(A);
		List<AttackModifierYMLData> attackModifierDeck2 = GetAttackModifierDeck(B);
		return attackModifierDeck == attackModifierDeck2;
	}

	public static void LoseAllCards(CActor actor, CAbility ability)
	{
		CPlayerActor cPlayerActor = (CPlayerActor)actor;
		int cards = 0;
		for (int num = cPlayerActor.CharacterClass.ActivatedAbilityCards.Count - 1; num >= 0; num--)
		{
			RemoveCardBonuses(cPlayerActor.CharacterClass.ActivatedAbilityCards[num]);
		}
		cPlayerActor.CharacterClass.CheckForFinishedActiveBonuses(cPlayerActor);
		for (int num2 = cPlayerActor.CharacterClass.HandAbilityCards.Count - 1; num2 >= 0; num2--)
		{
			LoseCard(cPlayerActor, cPlayerActor.CharacterClass.HandAbilityCards[num2], ref cards);
		}
		for (int num3 = cPlayerActor.CharacterClass.DiscardedAbilityCards.Count - 1; num3 >= 0; num3--)
		{
			LoseCard(cPlayerActor, cPlayerActor.CharacterClass.DiscardedAbilityCards[num3], ref cards);
		}
		for (int num4 = cPlayerActor.CharacterClass.RoundAbilityCards.Count - 1; num4 >= 0; num4--)
		{
			LoseCard(cPlayerActor, cPlayerActor.CharacterClass.RoundAbilityCards[num4], ref cards);
		}
		for (int i = 0; i < GameState.GetMaxRoundActionsCount(); i++)
		{
			if (GameState.GetActionFlagRound(i) == GameState.EActionFlag.None)
			{
				GameState.SetActionFlagRound(GameState.EActionFlag.TopForgone | GameState.EActionFlag.BottomForgone, i);
			}
		}
		cPlayerActor.SetTempVar("LostAllCards", true);
		CCombatLogMessage_MessageData cCombatLogMessage_MessageData = new CCombatLogMessage_MessageData();
		cCombatLogMessage_MessageData.m_MessageKey = "$FH/COMBAT_LOG_LOSE_CARDS_ACTOR$";
		cCombatLogMessage_MessageData.m_Params = new List<string>();
		cCombatLogMessage_MessageData.m_Actors = new List<CActor>();
		cCombatLogMessage_MessageData.m_Params.Add("Actor_0");
		cCombatLogMessage_MessageData.m_Actors.Add(actor);
		cCombatLogMessage_MessageData.m_Params.Add(cards.ToString());
		ScenarioRuleClient.MessageHandler(cCombatLogMessage_MessageData);
	}

	private static void LoseCard(CPlayerActor playerActor, CAbilityCard card, ref int cards)
	{
		if (card.SelectedAction != null && card.SelectedAction.CardPile == CBaseCard.ECardPile.PermanentlyLost)
		{
			playerActor.CharacterClass.MoveAbilityCard(card, playerActor.CharacterClass.GetCardPileList(card.CurrentCardPile), playerActor.CharacterClass.PermanentlyLostAbilityCards, CCharacterClass.GetCardPileListKey(card.CurrentCardPile), CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.PermanentlyLost));
		}
		else
		{
			playerActor.CharacterClass.MoveAbilityCard(card, playerActor.CharacterClass.GetCardPileList(card.CurrentCardPile), playerActor.CharacterClass.LostAbilityCards, CCharacterClass.GetCardPileListKey(card.CurrentCardPile), CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.Lost));
		}
		cards++;
	}

	public static void ClearDontRefreshTargetingUIFlag()
	{
		List<CAbility> list = ((CPhaseAction)PhaseManager.Phase)?.CurrentPhaseAbilities?.Select((CPhaseAction.CPhaseAbility a) => a.m_Ability).ToList();
		if (list == null)
		{
			return;
		}
		foreach (CAbility item in list.Where((CAbility a) => a.GetMiscVar("ClearDontRefreshTargetingUIFlag").GetBool()))
		{
			item.Flags &= ~CAbility.EAbilityFlag.DontRefreshTargetingUI;
		}
	}

	public static void SpawnObstaclesOnTiles(List<CTile> tiles, CAbility ability, string propName)
	{
		foreach (CTile tile in tiles)
		{
			TileIndex tileIndex = new TileIndex(tile.m_ArrayIndex);
			CObjectObstacle prop = new CObjectObstacle(propName, ScenarioManager.ObjectImportType.Obstacle, tileIndex, null, new CVector3(0f, 0f, 0f), new List<TileIndex> { tileIndex }, ability.TargetingActor, tile.m_HexMap.MapGuid, ignoresFlyAndJump: false, setPositionToCenterOfAllPathBlockers: true);
			tile.SpawnProp(prop, notifyClient: true, 0f, ability);
		}
	}

	public static void SpawnVFXProp(CTile tile, string propName)
	{
		CObjectProp prop = new CObjectProp(propName, ScenarioManager.ObjectImportType.VFXProp, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
		tile.SpawnProp(prop);
	}

	public static void DestroyPropByName(string propName)
	{
		FindPropByName(propName)?.DestroyProp();
	}

	public static CObjectProp FindPropByName(string propName)
	{
		return ScenarioManager.CurrentScenarioState.Props.Find((CObjectProp p) => p.PrefabName == propName);
	}

	public static List<CCondition.EPositiveCondition> GetPositiveConditionsThatCanBeTransferred(CActor actor)
	{
		List<CCondition.EPositiveCondition> allPositiveConditions = actor.Tokens.GetAllPositiveConditions();
		allPositiveConditions.Remove(CCondition.EPositiveCondition.Advantage);
		allPositiveConditions.Remove(CCondition.EPositiveCondition.Bless);
		allPositiveConditions.Remove(CCondition.EPositiveCondition.Immovable);
		allPositiveConditions.Remove(CCondition.EPositiveCondition.Mutate);
		return allPositiveConditions;
	}

	public static List<CTile> GetTargetedOrPotentialTiles(CAbility ability)
	{
		if (ability.AbilityType == CAbility.EAbilityType.CreateLimitedHazardousTerrain && (ability as CAbilityCreateLimitedHazardousTerrain).m_State > CAbilityCreateLimitedHazardousTerrain.CAbilityCreateLimitedHazardousTerrainState.PreCreateProp)
		{
			return (ability as CAbilityCreateLimitedHazardousTerrain).CachedSpawnTiles;
		}
		return ability.TilesSelected;
	}

	public static bool IsObjectActor(CActor target)
	{
		return target is CObjectActor;
	}

	public static CAbilityCard GetCardFromBonusVars(string varKey, CActiveBonus bonus)
	{
		CVarsContext context = new CVarsContext(bonus);
		TValue var = bonus.Ability.ActiveBonusData.GetVar(varKey);
		if (!var.isUnknown)
		{
			int id = var.Calc(context).GetInt();
			return bonus.Actor.CharacterClass.FindCardWithID(id);
		}
		return null;
	}

	public static void DiscardVarIfHasntHappened(string varKey, CActiveBonus bonus)
	{
		CAbility cAbility = (bonus.Ability as CAbilityAddActiveBonus)?.AddAbility;
		CAbility cAbility2 = ((cAbility == null) ? null : PhaseManager.CurrentPhaseAction?.FindAbilityInPhase(cAbility));
		if (bonus.Remaining > 0 || (cAbility2 != null && !cAbility2.AbilityHasHappened))
		{
			CAbilityCard cardFromBonusVars = GetCardFromBonusVars(varKey, bonus);
			if (cardFromBonusVars != null)
			{
				CCharacterClass characterClass = bonus.Actor.CharacterClass;
				characterClass.MoveAbilityCard(cardFromBonusVars, characterClass.GetCardPileList(cardFromBonusVars.CurrentCardPile), characterClass.GetCardPileList(CBaseCard.ECardPile.Discarded), CCharacterClass.GetCardPileListKey(cardFromBonusVars.CurrentCardPile), CCharacterClass.GetCardPileListKey(CBaseCard.ECardPile.Discarded));
			}
		}
	}

	public static bool ValidateTargetAllyOrSelf(CAbility previouslyTargetingAbility, CActor self, CActor newTarget)
	{
		if (previouslyTargetingAbility == null || !previouslyTargetingAbility.AbilityHasHappened)
		{
			return false;
		}
		if (previouslyTargetingAbility.ActorsTargeted.Contains(self))
		{
			if (newTarget != self)
			{
				return CActor.AreActorsAllied(newTarget.Type, self.Type, newTarget, self);
			}
			return false;
		}
		return newTarget == self;
	}

	public static bool IsDeathwalkerShadowMarked(CAbility ability)
	{
		CActor targetingActor = ability.TargetingActor;
		if (targetingActor.Class.ID == "DeathwalkerID" && targetingActor.FindApplicableActiveBonuses(CAbility.EAbilityType.AddActiveBonus, CActiveBonus.EActiveBonusBehaviourType.ShadowBuffAttack).Any())
		{
			return true;
		}
		return false;
	}

	public static void RefreshInfuserDisarm(CHeroSummonActor animatedClaymore)
	{
		if (!(animatedClaymore?.Class?.ID != "AnimatedClaymoreID"))
		{
			CPlayerActor summoner = animatedClaymore.Summoner;
			if (summoner != null && animatedClaymore.IsDeadForObjectives)
			{
				summoner.Tokens.RemoveNegativeToken(CCondition.ENegativeCondition.Disarm, EConditionDecTrigger.WhenAnimatedClaymoreEnds);
				summoner.ApplyCondition(summoner, CCondition.ENegativeCondition.Disarm, 0, EConditionDecTrigger.Turns);
			}
		}
	}

	public static void RepeatOnSameTargets(CAbilityAttack attackAbility)
	{
		int numberTargetsRemaining = attackAbility.NumberTargetsRemaining;
		if (numberTargetsRemaining <= 0)
		{
			return;
		}
		List<CActor> list = new List<CActor>();
		foreach (CAttackSummary.TargetSummary totalTargetSummary in attackAbility.TotalTargetSummaries)
		{
			if (!totalTargetSummary.ActorToAttack.IsDead && attackAbility.IsActorWithinRangeAndLoS(totalTargetSummary.ActorToAttack, attackAbility.Range))
			{
				list.Add(totalTargetSummary.ActorToAttack);
			}
		}
		if (list.Count != 0)
		{
			CAbility cAbility = CAbility.CopyAbility(attackAbility, generateNewID: false);
			cAbility.Targeting = CAbility.EAbilityTargeting.Custom;
			cAbility.TargetingVars = new List<(string, TValue)>
			{
				("AddValidActorsInRange", new TValue(list)),
				("AddTilesInRange", new TValue(list.Select((CActor x) => x.Tile).ToList()))
			};
			cAbility.VarsContext.obj = null;
			int numberTargets = (cAbility.OriginalTargetCount = numberTargetsRemaining);
			cAbility.NumberTargets = numberTargets;
			cAbility.StatIsBasedOnXEntries.RemoveAll((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.NumberOfTargets);
			PhaseManager.StartAbilities(cAbility, attackAbility.TargetingActor, stackInsteadOfInject: true);
		}
	}

	public static void RecalcValidActorsForSelfAndXOthers(CAbility ability)
	{
		CActor self = ability.TargetingActor;
		int num = ability.ActorsToTarget.Where((CActor x) => x == self).Count();
		int num2 = ability.ActorsToTarget.Where((CActor x) => x != self).Count();
		int num3 = ability.NumberTargets - 1 - num2;
		int num4 = 1 - num;
		SharedAbilityTargeting.GetValidActorsInRange(ability);
		if (num3 == 0)
		{
			ability.ValidActorsInRange.RemoveAll((CActor x) => x != self);
		}
		if (num4 == 0)
		{
			ability.ValidActorsInRange.RemoveAll((CActor x) => x == self);
		}
		if (ability.TilesSelected.Count > 0)
		{
			foreach (CTile item in ability.TilesSelected)
			{
				CActor livingActor = item.LivingActor;
				if (livingActor != null)
				{
					ability.ValidActorsInRange.Add(livingActor);
				}
			}
		}
		ability.NumberTargetsRemaining = num3 + num4;
		ability.TilesInRange.Clear();
		if (!_inRecalcValidActorsForSelfAndXOthers)
		{
			_inRecalcValidActorsForSelfAndXOthers = true;
			if (!ability.CanSkip || (!ability.IsPositive() && ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self) && !ability.NegativeAbilityIsMandatory))
			{
				ability.TileSelected(self.Tile, null);
			}
			_inRecalcValidActorsForSelfAndXOthers = false;
		}
	}

	public static int GetValidAndToTargetActors(CAbility ability)
	{
		return ability.ValidActorsInRange.Concat(ability.ActorsToTarget).Distinct().Count();
	}

	public static int GetValidAndToTargetActorsClampedToMax(CAbility ability)
	{
		return Math.Min(GetValidAndToTargetActors(ability), ability.NumberTargets);
	}

	public static void AutoControlSelfIfOnlyValidTarget(CAbilityControlActor controlAbility)
	{
		CAbilityAddActiveBonus cAbilityAddActiveBonus = controlAbility.VarsContext.GetVar("ActiveBonus").GetObject<CActiveBonus>()?.Ability as CAbilityAddActiveBonus;
		cAbilityAddActiveBonus?.CanBeToggledDuringAbilities.Clear();
		if (controlAbility.ValidActorsInRange.Count == 1 && controlAbility.ValidActorsInRange.Contains(controlAbility.TargetingActor))
		{
			cAbilityAddActiveBonus?.CanBeToggledDuringAbilities.Add(controlAbility.ControlActorData.ControlAbilities.FirstOrDefault());
			if (controlAbility.MiscAbilityData == null)
			{
				controlAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			controlAbility.MiscAbilityData.AutotriggerAbility = true;
			controlAbility.TileSelected(controlAbility.TargetingActor.Tile, null);
		}
	}

	public static bool HasOrmiBeenSummoned()
	{
		return ScenarioManager.CurrentScenarioState.AllyMonsters.Any((EnemyState x) => x.ClassID.Equals("Ormi1ID", StringComparison.InvariantCultureIgnoreCase) || x.ClassID.Equals("Ormi2ID", StringComparison.InvariantCultureIgnoreCase));
	}

	public static List<CTile> GetValidOrmiTiles()
	{
		return (from x in GetAdjacentTiles(((IEnumerable<CPlayerActor>)ScenarioManager.Scenario.AllPlayers).Select((Func<CPlayerActor, CActor>)((CPlayerActor x) => x)).ToList())
			where CAbilityFilter.IsTileEmpty(x)
			select x).ToList();
	}

	public static bool AllBonusCopiesUnrestricted(CActiveBonus bonus)
	{
		return (from x in CharacterClassManager.FindAllActiveBonuses().Distinct()
			where x.Ability.ID == bonus.Ability.ID
			select x).All((CActiveBonus x) => !x.IsRestricted(x.Actor));
	}

	public static List<CTile> TilesMovedIncludingBacktracking(CAbilityMove MoveAbility, CActor TargetingActor, CVarsContext context = null)
	{
		CTile originalStartTile = MoveAbility.OriginalStartTile;
		CTile tile = TargetingActor.Tile;
		List<CTile> list = MoveAbility.TilesMoved.Select((Point obj) => ScenarioManager.GetTile(obj)).ToList();
		list.Remove(tile);
		if (originalStartTile != null)
		{
			list.Insert(0, originalStartTile);
		}
		return list;
	}

	public static CAbility GetAbilityFromBonusOnCard(CBaseCard card, string key)
	{
		return FindActiveBonusOnCard(card, key)?.Ability.ActiveBonusData.AbilityData;
	}

	public static void TranslocationRodValidateTargets(CAbility ability)
	{
		if (ability.ActorsToTarget.Count > 0)
		{
			CActor cActor = ability.ActorsToTarget[0];
			if (IsAlly(ability.TargetingActor, cActor))
			{
				ability.ValidActorsInRange.RemoveAll((CActor x) => !IsAlly(ability.TargetingActor, x));
			}
			else
			{
				ability.ValidActorsInRange.RemoveAll((CActor x) => IsAlly(ability.TargetingActor, x));
			}
			if (ScenarioScript.current != null && ScenarioScript.current.IsUnderwater())
			{
				return;
			}
			if (cActor is CEnemyActor cEnemyActor && cEnemyActor.MonsterClass.IsWaterMonster)
			{
				ability.ValidActorsInRange.RemoveAll((CActor x) => !x.IsInWaterTile);
			}
			else if (!cActor.IsInWaterTile)
			{
				ability.ValidActorsInRange.RemoveAll((CActor x) => x is CEnemyActor cEnemyActor2 && cEnemyActor2.MonsterClass.IsWaterMonster);
			}
		}
		else
		{
			SharedAbilityTargeting.GetValidActorsInRange(ability);
		}
	}

	public static int GetPropTileCount(CTile tile)
	{
		int num = 0;
		foreach (CObjectProp prop in tile.m_Props)
		{
			int num2 = prop.GetTiles().Count;
			List<CObjectProp> linkedPropsTotal = prop.GetLinkedPropsTotal();
			if (linkedPropsTotal != null)
			{
				num2 += linkedPropsTotal.Count;
			}
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public static int GetRemainingMoves()
	{
		return (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability as CAbilityMove)?.RemainingMovesAfterLimit ?? 0;
	}

	public static void IncrementBoneHordeSpecialAbility(CActor boneHorde)
	{
		CActiveBonus cActiveBonus = boneHorde.FindPreventDamageBonuses()?.Find((CActiveBonus b) => b.Ability.Name == "BoneHordeSpecial");
		cActiveBonus.TrackerIndex = Math.Max(0, cActiveBonus.TrackerIndex - 1);
	}

	public static int GetBoneHordeSpecialAbilityCount(CActor boneHorde)
	{
		CActiveBonus cActiveBonus = boneHorde.FindPreventDamageBonuses()?.Find((CActiveBonus b) => b.Ability.Name == "BoneHordeSpecial");
		if (cActiveBonus?.Ability?.ActiveBonusData == null)
		{
			return 0;
		}
		return cActiveBonus.Remaining;
	}

	public static bool HasRemainingStandees(CActor actor)
	{
		return actor?.MonsterClass?.HasRemainingStandees() ?? true;
	}

	public static List<CPlayerActor> PlayersSortedByInitiative()
	{
		List<CPlayerActor> list = ScenarioManager.Scenario.AllPlayers.Where((CPlayerActor a) => !a.IsDead).ToList();
		list.Sort((CPlayerActor x, CPlayerActor y) => GameState.s_ActorInitiativeComparer.Compare(x, y));
		return list;
	}

	public static bool IsBuffAbility(CAbility ability)
	{
		if (ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			return ability.SubAbilities.Any((CAbility x) => x.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA);
		}
		return true;
	}

	public static bool IsPersistentAbility(CAbility ability)
	{
		if (ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.Persistent && ability.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.Summon && !ability.SubAbilities.Any((CAbility x) => x.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent))
		{
			return ability.SubAbilities.Any((CAbility x) => x.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Summon);
		}
		return true;
	}

	public static List<CActor> ActorsHealedThisAction()
	{
		List<CActor> list = CPhaseAction.ActorsHealedThisAction?.ToList();
		if (list == null)
		{
			return list;
		}
		if (PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability is CAbilityHeal cAbilityHeal && cAbilityHeal.ModifiedStrength() > 0)
		{
			foreach (CActor item in cAbilityHeal.ActorsToTarget)
			{
				if (!item.Tokens.HasKey(CCondition.ENegativeCondition.Poison) && item.Health < item.MaxHealth && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public static int ActorsHealedThisActionCount()
	{
		return ActorsHealedThisAction()?.Count ?? 0;
	}

	public static bool IsPropertyPrintedOnAbility(CAbility ability, string property)
	{
		if (ability?.MiscVars == null)
		{
			return false;
		}
		foreach (var miscVar in ability.MiscVars)
		{
			if (miscVar.Item1.Equals("PrintedProperties", StringComparison.InvariantCultureIgnoreCase))
			{
				TValue item = miscVar.Item2;
				List<object> list = item.Calc().GetObject<List<object>>();
				if (list != null && list.Contains(property))
				{
					return true;
				}
			}
		}
		if (ability is CAbilityAttack { AttackSummary: var attackSummary } cAbilityAttack && attackSummary?.Targets != null)
		{
			foreach (CAttackSummary.TargetSummary target in cAbilityAttack.AttackSummary.Targets)
			{
				if (IsPropertyPrintedOnAbility(target.AttackAbilityWithOverrides, property))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasPerformedAbilityWithMiscVarThisAction(CAbility ability, string miscVar)
	{
		if (PhaseManager.CurrentPhaseAction?.PreviousPhaseAbilities == null)
		{
			return false;
		}
		CAction action = ability.GetAction();
		return PhaseManager.CurrentPhaseAction.PreviousPhaseAbilities.Any((CPhaseAction.CPhaseAbility x) => x.m_Ability.AbilityHasHappened && x.m_Ability.GetAction() == action && !x.m_Ability.GetMiscVar(miscVar).isUnknown);
	}

	public static bool HasDiscardedPersistentCards(CActor actor)
	{
		if (!(actor is CPlayerActor cPlayerActor))
		{
			return false;
		}
		return cPlayerActor.CharacterClass.DiscardedAbilityCards.Any((CAbilityCard card) => card.GetActions().Any((CAction action) => action.Abilities.Any((CAbility ability) => ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Persistent || ability.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.Summon)));
	}

	public static List<CTile> GetTilesAdjacentToThreeHexObstacles()
	{
		IEnumerable<CObjectProp> enumerable = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PropType == EPropType.ThreeHexObstacle);
		List<CTile> list = new List<CTile>();
		foreach (CObjectProp item in enumerable)
		{
			foreach (Point tile in item.GetTiles())
			{
				list.AddRange(GetAdjacentTiles(tile.X, tile.Y));
			}
		}
		return list.Distinct().ToList();
	}

	public static void HighlightThreeHexObstacles(bool active)
	{
		IEnumerable<CObjectProp> enumerable = ScenarioManager.CurrentScenarioState.Props.Where((CObjectProp x) => x.PropType == EPropType.ThreeHexObstacle);
		List<Point> list = new List<Point>();
		foreach (CObjectProp item in enumerable)
		{
			list.AddRange(item.GetTiles());
		}
		ScenarioManager.ToggleTileHighlights(list.Distinct(), active, new CVector4(1f, 1f, 0f, 1f));
	}

	private static bool CanMoveAfterConditions(CAbility ability)
	{
		if (ability.TargetingActor != null && ability.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize) && (ability.MiscAbilityData == null || !ability.MiscAbilityData.IgnoreImmobilize.HasValue || !ability.MiscAbilityData.IgnoreImmobilize.Value))
		{
			return false;
		}
		return true;
	}

	private static bool CanAttackAfterConditions(CAbility ability)
	{
		if (ability.TargetingActor != null && ability.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm))
		{
			return false;
		}
		return true;
	}

	public static bool CanBePerformedAfterConditions(CAbility ability, bool checkBase = true)
	{
		if (ability.TargetingActor != null && ability.TargetingActor.Tokens.HasKey(CCondition.ENegativeCondition.Stun) && (ability.MiscAbilityData == null || !ability.MiscAbilityData.IgnoreStun.HasValue || !ability.MiscAbilityData.IgnoreStun.Value))
		{
			return false;
		}
		if (((ability.TargetingActor != null) & checkBase) && !ability.CanBePerformed())
		{
			return false;
		}
		if (ability is CAbilityMove ability2)
		{
			return CanMoveAfterConditions(ability2);
		}
		if (ability is CAbilityAttack ability3)
		{
			return CanAttackAfterConditions(ability3);
		}
		return true;
	}

	public static bool IsNormalPush(CAbility ability)
	{
		if (ability is CAbilityPush cAbilityPush)
		{
			return !cAbilityPush.GetMiscVar("IgnoreForcedMoveBuffs").GetBool();
		}
		return false;
	}

	public static void CancelAbilityIfActorIsDead(CAbility ability, CActor actor)
	{
		if (actor != null && actor.IsDead)
		{
			ability.SetCancelAbilityFlag(cancel: true);
		}
	}

	public static void ConsumeResonanceActiveBonus(CActiveBonus ActiveBonus, CAbility currentAbility)
	{
		if (currentAbility.AbilityHasHappened && ActiveBonus.Actor is CPlayerActor cPlayerActor && ActiveBonus.IsActiveBonusToggledAndNotRestricted(cPlayerActor) && ActiveBonus.Ability.ActiveBonusData.IsToggleBonus)
		{
			int num = ActiveBonus.Ability.ActiveBonusData.GetMiscVar("ResonanceCost").GetInt();
			if (num != 0)
			{
				cPlayerActor.SetResonanceTokens(cPlayerActor.GetResonanceTokens() - num);
				ActiveBonus.RestrictActiveBonus(cPlayerActor);
			}
		}
	}

	public static void FinishBonusLock(CActiveBonus ActiveBonus, CBespokeBehaviour behaviour, CAbility currentAbility)
	{
		if (!currentAbility.AbilityHasHappened || !ActiveBonus.ToggledBonus)
		{
			return;
		}
		List<CActiveBonus> list = ActiveBonus.BaseCard.ActiveBonuses.ToList();
		behaviour.OnBehaviourTriggered();
		foreach (CActiveBonus item2 in list)
		{
			if (item2.HasTracker)
			{
				item2.UpdateXPTracker();
				item2.FinishIfNoRemaining();
			}
		}
		if (ActiveBonus.Finished() && ActiveBonus.BaseCard is CItem item)
		{
			ActiveBonus.Actor.Inventory.HandleUsedItem(item, ActiveBonus);
		}
	}

	public static void AddHalfDamageModifier(CAttackSummary.TargetSummary summary)
	{
		summary.PostShieldScalarModifiers.Add(("/2", "/2"));
	}

	public static int GetAddActiveStrength(CActiveBonus activeBonus)
	{
		return CalcAbilityStrength((activeBonus.Ability as CAbilityAddActiveBonus).AddAbility, activeBonus.Actor);
	}

	public static int CalcAbilityStrength(CAbility ability, CActor actor)
	{
		CAbility cAbility = CAbility.CopyAbility(ability, generateNewID: false);
		cAbility.AbilityStartListenersInvoked = true;
		cAbility.Start(actor, actor);
		return cAbility.Strength;
	}
}
