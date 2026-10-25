using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class SharedAbilityTargeting
{
	private static void AddSelectedTile(CTile selectedTile, CAbility ability, List<CTile> selectedTiles)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (!selectedTiles.Contains(selectedTile))
			{
				selectedTiles.Add(selectedTile);
			}
			foreach (CTile linkedTile in ability.GetLinkedTiles(selectedTile))
			{
				if (!selectedTiles.Contains(linkedTile))
				{
					selectedTiles.Add(linkedTile);
				}
			}
		}
	}

	private static void RemoveSelectedTile(CTile selectedTile, CAbility ability, List<CTile> selectedTiles)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			selectedTiles.Remove(selectedTile);
			foreach (CTile linkedTile in ability.GetLinkedTiles(selectedTile))
			{
				selectedTiles.Remove(linkedTile);
			}
		}
	}

	public static bool TileSelected(CAbility ability, CTile selectedTile, CAreaEffect areaEffect, CActor targetActor, CActor filterActor, int range, ref List<CActor> validActorsInRange, List<CActor> actorsToIgnore, CAbilityFilterContainer abilityFilter, int maxNumberTargets, ref int numberTargetsRemaining, List<CTile> validTilesInAreaAffectedIncludingBlocked, ref List<CActor> actorsToTarget, ref List<CTile> selectedTiles, bool isTargetedAbility)
	{
		if (areaEffect != null)
		{
			if (ability.MiscAbilityData?.IgnorePreviousAbilityTargets != null)
			{
				AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
				if (miscAbilityData != null && miscAbilityData.IgnorePreviousAbilityTargets.Count > 0)
				{
					range = 99;
				}
			}
			List<CTile> tilesInRange = GameState.GetTilesInRange(targetActor, range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true);
			if (validTilesInAreaAffectedIncludingBlocked.Count > 0)
			{
				List<CActor> list = (from x in ability.TargetFromAnywhereActors()
					where validTilesInAreaAffectedIncludingBlocked.Contains(x.Tile)
					select x).ToList();
				if (list.Count == 0)
				{
					list = null;
				}
				else
				{
					tilesInRange.AddRange(list.Select((CActor x) => x.Tile));
				}
				foreach (CTile item in validTilesInAreaAffectedIncludingBlocked)
				{
					AddSelectedTile(item, ability, selectedTiles);
				}
				if (tilesInRange.Any((CTile x) => validTilesInAreaAffectedIncludingBlocked.Any((CTile y) => y == x)))
				{
					validActorsInRange = GameState.GetActorsInRange(targetActor, filterActor, range, actorsToIgnore, abilityFilter, areaEffect, validTilesInAreaAffectedIncludingBlocked, isTargetedAbility, null, ability.MiscAbilityData?.CanTargetInvisible);
					if (list != null)
					{
						foreach (CActor item2 in list)
						{
							if (!validActorsInRange.Contains(item2))
							{
								validActorsInRange.Add(item2);
							}
						}
					}
					if (ability.AbilityType == CAbility.EAbilityType.Attack && ability.MiscAbilityData != null && ability.MiscAbilityData.MinRange.HasValue)
					{
						CAbilityAttack attackAbility = ability as CAbilityAttack;
						List<CActor> second = validActorsInRange.Where((CActor actor) => attackAbility.IsActorWithinRangeAndLoS(actor, ability.MiscAbilityData.MinRange.Value - 1) && !attackAbility.IsActorWithinRangeAndLoS(actor, ability.MiscAbilityData.MinRange.Value)).ToList();
						List<CActor> second2 = validActorsInRange.Where((CActor actor) => !attackAbility.IsActorWithinRangeAndLoS(actor, range)).ToList();
						validActorsInRange = validActorsInRange.Except(second).Except(second2).ToList();
					}
					if (validActorsInRange.Count > 0)
					{
						targetActor.Inventory.HighlightUsableItems(ability, validActorsInRange, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction);
						CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
						cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
						cShowSingleTargetActiveBonus_MessageData.m_Ability = ability;
						ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
						return true;
					}
				}
			}
			validActorsInRange.Clear();
			return false;
		}
		foreach (CActor item3 in validActorsInRange)
		{
			if (ability.IsMirroredHealthCloneTargeted(item3))
			{
				continue;
			}
			CObjectObstacle cObjectObstacle = CObjectProp.FindPropWithPathingBlocker(selectedTile.m_ArrayIndex, ref selectedTile, returnPathingBlockerTile: true);
			if (!(item3.RealArrayIndex == selectedTile.m_ArrayIndex) && (item3.MultiHexArrayIndexes == null || !item3.MultiHexArrayIndexes.Contains(selectedTile.m_ArrayIndex)))
			{
				if (cObjectObstacle == null)
				{
					continue;
				}
				Point realArrayIndex = item3.RealArrayIndex;
				Point? point = cObjectObstacle.RuntimeAttachedActor?.ArrayIndex;
				if (!(realArrayIndex == point))
				{
					continue;
				}
			}
			bool flag = actorsToTarget.Contains(item3);
			if (!flag)
			{
				AbilityData.MiscAbilityData miscAbilityData2 = ability.MiscAbilityData;
				if (miscAbilityData2 != null && miscAbilityData2.TargetOneEnemyWithAllAttacks.HasValue)
				{
					AbilityData.MiscAbilityData miscAbilityData3 = ability.MiscAbilityData;
					if (miscAbilityData3 != null && miscAbilityData3.TargetOneEnemyWithAllAttacks.Value)
					{
						AddSelectedTile(item3.Tile, ability, selectedTiles);
						actorsToTarget = new List<CActor>(maxNumberTargets);
						for (int num = 0; num < maxNumberTargets; num++)
						{
							actorsToTarget.Add(item3);
						}
						goto IL_0462;
					}
				}
				if (actorsToTarget.Count < maxNumberTargets)
				{
					AddSelectedTile(item3.Tile, ability, selectedTiles);
					actorsToTarget.Add(item3);
				}
			}
			goto IL_0462;
			IL_0462:
			numberTargetsRemaining = maxNumberTargets - actorsToTarget.Count;
			if (actorsToTarget.Count > 0)
			{
				targetActor.Inventory.HighlightUsableItems(ability, actorsToTarget, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction, CItem.EItemTrigger.SingleAbility);
				CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData2 = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
				cShowSingleTargetActiveBonus_MessageData2.m_ShowSingleTargetActiveBonus = true;
				cShowSingleTargetActiveBonus_MessageData2.m_Ability = ability;
				ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData2);
			}
			CActorSelectedAttackFocus_MessageData message = new CActorSelectedAttackFocus_MessageData(targetActor)
			{
				m_AttackingActor = targetActor,
				m_Ability = ability,
				m_AttackFocus = item3,
				m_Adding = !flag
			};
			ScenarioRuleClient.MessageHandler(message);
			if (actorsToTarget.Count == maxNumberTargets)
			{
				return true;
			}
		}
		return false;
	}

	public static bool TileDeselected(CAbility ability, CTile selectedTile, int maxNumberTargets, ref int numberTargetsRemaining, ref List<CActor> actorsToTarget, ref List<CTile> selectedTiles)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (ability.AreaEffect != null || ability.AreaEffectBackup != null)
			{
				if (ability.MiscAbilityData != null && ability.MiscAbilityData.ExactRange.HasValue && ability.MiscAbilityData.ExactRange.Value)
				{
					return false;
				}
				ability.TilesSelected.Clear();
				actorsToTarget.Clear();
				for (int num = ability.ActiveSingleTargetItems.Count - 1; num >= 0; num--)
				{
					CItem cItem = ability.ActiveSingleTargetItems[num];
					if (cItem.SingleTarget != null)
					{
						ability.TargetingActor.Inventory.DeselectItem(cItem);
						ability.ActiveSingleTargetItems.Remove(cItem);
					}
				}
				for (int num2 = ability.ActiveSingleTargetActiveBonuses.Count - 1; num2 >= 0; num2--)
				{
					CActiveBonus cActiveBonus = ability.ActiveSingleTargetActiveBonuses[num2];
					if (cActiveBonus.CostAbilityExecuted)
					{
						cActiveBonus.ToggleLocked = false;
					}
					if (cActiveBonus.SingleTarget != null && cActiveBonus.ToggledBonus)
					{
						cActiveBonus.ToggleActiveBonus(null, ability.TargetingActor);
					}
				}
				ability.ActiveSingleTargetActiveBonuses.Clear();
				return true;
			}
			if (selectedTiles.Contains(selectedTile))
			{
				bool flag = ability.TargetingOverrideArrayIndex.X != 0 && ability.TargetingOverrideArrayIndex.Y != 0;
				CActor cActor = ScenarioManager.Scenario.FindActorAt(selectedTile.m_ArrayIndex, flag ? ability.TargetingActor : null);
				if (cActor != null)
				{
					if (actorsToTarget.Contains(cActor))
					{
						RemoveSelectedTile(selectedTile, ability, selectedTiles);
						actorsToTarget.RemoveAll((CActor x) => x.Tile == selectedTile || ability.GetLinkedTiles(selectedTile).Contains(x.Tile));
						numberTargetsRemaining++;
					}
					for (int num3 = ability.ActiveSingleTargetItems.Count - 1; num3 >= 0; num3--)
					{
						CItem cItem2 = ability.ActiveSingleTargetItems[num3];
						if (cItem2.SingleTarget != null && cItem2.SingleTarget == cActor)
						{
							ability.TargetingActor.Inventory.DeselectItem(cItem2);
							ability.ActiveSingleTargetItems.Remove(cItem2);
						}
					}
					for (int num4 = ability.ActiveSingleTargetActiveBonuses.Count - 1; num4 >= 0; num4--)
					{
						CActiveBonus cActiveBonus2 = ability.ActiveSingleTargetActiveBonuses[num4];
						if (cActiveBonus2.SingleTarget != null && cActiveBonus2.SingleTarget == cActor)
						{
							cActiveBonus2.ToggleActiveBonus(null, ability.TargetingActor);
						}
					}
					if (actorsToTarget.Count > 0)
					{
						ability.TargetingActor.Inventory.HighlightUsableItems(ability, actorsToTarget, CItem.EItemTrigger.SingleTarget, CItem.EItemTrigger.EntireAction);
						CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
						cShowSingleTargetActiveBonus_MessageData.m_ShowSingleTargetActiveBonus = true;
						cShowSingleTargetActiveBonus_MessageData.m_Ability = ability;
						ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData);
					}
					else
					{
						CShowSingleTargetActiveBonus_MessageData cShowSingleTargetActiveBonus_MessageData2 = new CShowSingleTargetActiveBonus_MessageData(ability.TargetingActor);
						cShowSingleTargetActiveBonus_MessageData2.m_ShowSingleTargetActiveBonus = false;
						cShowSingleTargetActiveBonus_MessageData2.m_Ability = ability;
						ScenarioRuleClient.MessageHandler(cShowSingleTargetActiveBonus_MessageData2);
					}
					return true;
				}
			}
			return false;
		}
	}

	public static bool GetValidActorsInRangeAlongPath(CAbility ability, List<Point> path, ref List<CActor> actors)
	{
		actors.Clear();
		foreach (Point item in path)
		{
			foreach (CActor item2 in GameState.GetActorsInRange(item, ability.TargetingActor, ability.Range, null, ability.AbilityFilter, null, null, isTargetedAbility: false, null, false))
			{
				if (!item2.Tokens.HasKey(CCondition.EPositiveCondition.Invisible) && !item2.Untargetable && !actors.Contains(item2))
				{
					actors.Add(item2);
				}
			}
		}
		return actors.Count > 0;
	}

	public static void GetValidActorsInRange(CAbility ability)
	{
		CAbilityFilter.CurrentVarsContext = ability.VarsContext;
		ability.ValidActorsInRange.Clear();
		CTile cTile = null;
		CAbility cAbility = ability.ParentAbility;
		bool flag = ability.IncludeCasterTile();
		if (ability != null && ability.ParentAbility != null && ability.MiscAbilityData != null && ability.MiscAbilityData.UseParentTiles.HasValue && ability.MiscAbilityData.UseParentTiles.Value)
		{
			CAbility cAbility2 = PhaseManager.CurrentPhaseAction?.CurrentPhaseAbility?.m_Ability;
			if (cAbility2 != null && cAbility2 != cAbility && cAbility2.ID == cAbility.ID)
			{
				cAbility = cAbility2;
			}
		}
		if (cAbility != null && (ability.MiscAbilityData == null || (ability.MiscAbilityData.UseParentTiles.HasValue && ability.MiscAbilityData.UseParentTiles.Value)))
		{
			cTile = ((cAbility.TilesSelected != null && cAbility.TilesSelected.Count > 0) ? cAbility.TilesSelected[0] : null);
			if (cTile == null)
			{
				cTile = ((cAbility.ActorsTargeted != null && cAbility.ActorsTargeted.Count > 0) ? ScenarioManager.Tiles[cAbility.ActorsTargeted[0].ArrayIndex.X, cAbility.ActorsTargeted[0].ArrayIndex.Y] : null);
			}
		}
		else if (ability.TargetThisActorAutomatically != null)
		{
			cTile = ScenarioManager.Tiles[ability.TargetThisActorAutomatically.ArrayIndex.X, ability.TargetThisActorAutomatically.ArrayIndex.Y];
		}
		if (ability.TargetingActor == null)
		{
			Log.ErrorWithReport($"GetValidActorsInRange: ability has no TargetingActor: {ability}");
		}
		CTile cTile2 = ((cTile != null) ? cTile : ScenarioManager.Tiles[ability.TargetingActor.ArrayIndex.X, ability.TargetingActor.ArrayIndex.Y]);
		if (ability.Targeting == CAbility.EAbilityTargeting.Range)
		{
			ability.TilesInRange = GameState.GetTilesInRange(cTile2.m_ArrayIndex, ability.Range, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ability.Targeting != CAbility.EAbilityTargeting.Range, ignoreBlockedWithActor: false, ability.MiscAbilityData?.IgnoreLOS ?? false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: true, ignoreHazardousTerrain: false, 0, allowClosedDoorTiles: false, includeTargetPosition: false, noActorsAllowed: false, skipPathingCheck: false, ability.IncludeCasterTile());
			if (ability.MiscAbilityData != null && ability.MiscAbilityData.MinRange.HasValue)
			{
				List<CTile> tilesUnderRange = GameState.GetTilesInRange(cTile2.m_ArrayIndex, ability.MiscAbilityData.MinRange.Value - 1, ability.Targeting, emptyTilesOnly: false, ignoreBlocked: true, null, ability.Targeting != CAbility.EAbilityTargeting.Range, ignoreBlockedWithActor: false, ability.MiscAbilityData?.IgnoreLOS ?? false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: true);
				ability.TilesInRange.RemoveAll((CTile tile) => tilesUnderRange.Contains(tile));
			}
			if (!flag)
			{
				ability.TilesInRange.Remove(cTile2);
			}
			List<CTile> immuneToDamagePropsTiles = new List<CTile>();
			foreach (CTile item3 in ability.TilesInRange)
			{
				CActor actorOnTile = GameState.GetActorOnTile(item3, ability.FilterActor, ability.AbilityFilter, ability.ActorsToIgnore, ability.IsTargetedAbility, ability.MiscAbilityData?.CanTargetInvisible, originalTargetType: false, ability.CanSkipUntergetableCheck());
				if (actorOnTile != null && actorOnTile is CObjectActor { AttachedProp: not null } cObjectActor && cObjectActor.AttachedProp.IsImmuneToDamage)
				{
					immuneToDamagePropsTiles.Add(item3);
				}
			}
			ability.TilesInRange.RemoveAll((CTile tile) => immuneToDamagePropsTiles.Contains(tile));
			if (ability.AreaEffect != null)
			{
				List<CTile> validTilesIncludingBlockedOut = new List<CTile>();
				Dictionary<CTile, CAreaEffect.EHexFlag> flagsOfValidTiles = new Dictionary<CTile, CAreaEffect.EHexFlag>();
				CAreaEffect.GetAllPossibleTiles(ability.TargetingActor, ability.AreaEffect, ability.Range, ability.TilesInRange, ref validTilesIncludingBlockedOut, ref flagsOfValidTiles, ability.IgnoreAreaEffectLos, ability.IgnoreDoorLocks);
				ability.AllPossibleTilesInAreaEffect = validTilesIncludingBlockedOut;
				List<CActor> list = new List<CActor>();
				foreach (CTile item4 in ability.TilesInRange)
				{
					CActor actorOnTile2 = GameState.GetActorOnTile(item4, ability.FilterActor, ability.AbilityFilter, ability.ActorsToIgnore, ability.IsTargetedAbility, ability.MiscAbilityData?.CanTargetInvisible);
					if (actorOnTile2 != null)
					{
						list.Add(actorOnTile2);
					}
				}
				ability.ValidActorsInRange = list;
				if (ability.UseSubAbilityTargeting && cAbility?.TilesSelected != null && cAbility.TilesSelected.Count > 1)
				{
					foreach (CTile item5 in cAbility.TilesSelected.Skip(1))
					{
						CActor cActor = ScenarioManager.Scenario.FindActorAt(item5.m_ArrayIndex);
						if (cActor != null && ability.AbilityFilter.IsValidTarget(cActor, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
						{
							ability.ValidActorsInRange.Add(cActor);
						}
					}
				}
			}
			List<CActor> list2 = GetAliveActorsInRange(cTile2.m_ArrayIndex, ability.Range, ability).Distinct().ToList();
			if (ability.MiscAbilityData != null && ability.MiscAbilityData.MinRange.HasValue)
			{
				for (int num = list2.Count - 1; num >= 0; num--)
				{
					CActor cActor2 = list2[num];
					bool flag2 = false;
					foreach (CTile item6 in cActor2.GetTilesCanAttackFrom())
					{
						if (CActor.GetDistanceTo(item6.m_ArrayIndex, cTile2.m_ArrayIndex, viaPathfinding: true) >= ability.MiscAbilityData.MinRange)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						list2.RemoveAt(num);
					}
				}
			}
			ability.ValidActorsInRange = list2;
			if (ability.UseSubAbilityTargeting && cAbility?.TilesSelected != null && cAbility.TilesSelected.Count > 1)
			{
				foreach (CTile item7 in cAbility.TilesSelected.Skip(1))
				{
					CActor cActor3 = ScenarioManager.Scenario.FindActorAt(item7.m_ArrayIndex);
					if (cActor3 != null && ability.AbilityFilter.IsValidTarget(cActor3, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
					{
						ability.ValidActorsInRange.Add(cActor3);
					}
				}
			}
		}
		else if (ability.AllTargetsOnMovePath)
		{
			foreach (CActor item8 in CAbilityMove.AllTargetActorsOnPath)
			{
				if (item8 != null)
				{
					CActor cActor4 = (CActor.AreActorsAllied(ability.TargetingActor.Type, item8.Type, ability.TargetingActor, item8) ? null : item8);
					if (cActor4 != null && !cActor4.PhasedOut && ability.AbilityFilter.IsValidTarget(cActor4, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
					{
						ability.ValidActorsInRange.Add(cActor4);
					}
				}
			}
		}
		else
		{
			ability.TilesInRange = new List<CTile>();
			if (ability.Targeting == CAbility.EAbilityTargeting.Attacker)
			{
				if (CAbilityAttack.s_LastAttacker != null && (CAbilityAttack.s_LastAttacker.Type != CActor.EType.Player || ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self)))
				{
					ability.ValidActorsInRange.Add(CAbilityAttack.s_LastAttacker);
				}
			}
			else if (ability.Targeting == CAbility.EAbilityTargeting.Room || ability.Targeting == CAbility.EAbilityTargeting.All || ability.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms)
			{
				ability.ValidActorsInRange = new List<CActor>();
				foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
				{
					if (allAliveActor.PhasedOut || !ability.AbilityFilter.IsValidTarget(allAliveActor, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible))
					{
						continue;
					}
					bool flag3 = false;
					if (ability.Targeting == CAbility.EAbilityTargeting.All)
					{
						flag3 = true;
					}
					else if (ability.Targeting == CAbility.EAbilityTargeting.Room && IsArrayIndexInSameRoom(cTile2.m_ArrayIndex, allAliveActor.ArrayIndex))
					{
						flag3 = true;
					}
					else if (ability.Targeting == CAbility.EAbilityTargeting.AllConnectedRooms)
					{
						ScenarioManager.PathFinder.FindPath(allAliveActor, allAliveActor.ArrayIndex, cTile2.m_ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ability.IgnoreDoorLocks);
						if (foundPath)
						{
							flag3 = true;
						}
					}
					if (flag3 && (!ability.GetMiscVar("CheckLOS").GetBool() || CActor.HaveLOS(ability.TargetingActor.Tile, allAliveActor.Tile)))
					{
						ability.ValidActorsInRange.Add(allAliveActor);
					}
				}
			}
			else if (ability.Targeting == CAbility.EAbilityTargeting.Custom && ability.TargetingVars != null)
			{
				if (!ability.AbilityHasBeenCancelled)
				{
					CVars cVars = new CVars();
					cVars.Set("obj", new TValue(ability));
					CVarsContext context = new CVarsContext(cVars, ability.VarsContext);
					foreach (var targetingVar in ability.TargetingVars)
					{
						string item = targetingVar.Item1;
						TValue item2 = targetingVar.Item2;
						TValue tValue = item2.Calc(context);
						cVars.Set(item, tValue);
						if (tValue.isValid)
						{
							ProcessTargetingValue(ability, item, item2, tValue);
						}
					}
				}
			}
			else
			{
				Log.ErrorWithReport("Invalid targeting type.  Unable to target.");
			}
		}
		if (ability.TargetThisActorAutomatically != null && ability.AbilityFilter.IsValidTarget(ability.TargetThisActorAutomatically, ability.TargetingActor, isTargetedAbility: true, useTargetOriginalType: false, false))
		{
			ability.ValidActorsInRange.Add(ability.TargetThisActorAutomatically);
		}
		if (ability.ValidActorsInRange.Count > 0)
		{
			for (int num2 = ability.ValidActorsInRange.Count - 1; num2 >= 0; num2--)
			{
				CActor cActor5 = ability.ValidActorsInRange[num2];
				if (CAbility.ImmuneToAbility(cActor5, ability))
				{
					AbilityData.MiscAbilityData miscAbilityData = ability.MiscAbilityData;
					if (miscAbilityData == null || miscAbilityData.AlsoTargetSelf != true)
					{
						ability.ImmuneActorsInRange.Add(cActor5);
					}
				}
			}
		}
		ability.ValidActorsInRange.RemoveAll((CActor a) => a.IsUsingOnDeathAbility);
		ability.ValidActorsInRange = ability.ValidActorsInRange.Distinct().ToList();
		ScenarioScript.current?.ValidateAbilityTargeting(ability);
		CAbilityFilter.CurrentVarsContext = null;
		if (ability is CAbilityAttack cAbilityAttack)
		{
			cAbilityAttack.RecalcCanTargetFromAnywhere();
		}
	}

	public static void ProcessTargetingValue(CAbility ability, string key, TValue val, TValue res)
	{
		if (key.StartsWith("AddValidActorInRange", StringComparison.Ordinal))
		{
			CActor cActor = res.GetObject<CActor>();
			if (cActor != null)
			{
				if (ability.AbilityFilter.IsValidTarget(cActor, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible) && !ability.ValidActorsInRange.Contains(cActor))
				{
					ability.ValidActorsInRange.Add(cActor);
				}
			}
			else
			{
				Log.Error($"Non-actor value of ValidActorInRange: {val} -> {res}");
			}
		}
		if (key.StartsWith("AddValidActorsInRange", StringComparison.Ordinal))
		{
			IList list = res.GetObject<IList>();
			if (list != null)
			{
				foreach (object item3 in list)
				{
					if (!(item3 is CActor cActor2))
					{
						Log.Error($"Non-actor value in ValidActorsInRange: {res} -> {TValue.ToString(item3)}");
					}
					else if (ability.AbilityFilter.IsValidTarget(cActor2, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible) && !ability.ValidActorsInRange.Contains(cActor2))
					{
						ability.ValidActorsInRange.Add(cActor2);
					}
				}
				return;
			}
			Log.Error($"Non-list value of ValidActorsInRange: {val} -> {res}");
		}
		else if (key.StartsWith("AddActorToTarget", StringComparison.Ordinal))
		{
			CActor cActor3 = res.GetObject<CActor>();
			if (cActor3 != null)
			{
				if (ability.AbilityFilter.IsValidTarget(cActor3, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible) && ability.ActorsToTarget != null && !ability.ActorsToTarget.Contains(cActor3))
				{
					ability.ActorsToTarget.Add(cActor3);
				}
			}
			else
			{
				Log.Error($"Non-actor value of ActorToTarget: {val} -> {res}");
			}
		}
		else if (key.StartsWith("AddActorsToTarget", StringComparison.Ordinal))
		{
			IList list2 = res.GetObject<IList>();
			if (list2 != null)
			{
				foreach (object item4 in list2)
				{
					if (!(item4 is CActor cActor4))
					{
						Log.Error($"Non-actor value in ActorsToTarget: {res} -> {TValue.ToString(item4)}");
					}
					else if (ability.AbilityFilter.IsValidTarget(cActor4, ability.TargetingActor, ability.IsTargetedAbility, useTargetOriginalType: false, ability.MiscAbilityData?.CanTargetInvisible) && ability.ActorsToTarget != null && !ability.ActorsToTarget.Contains(item4))
					{
						ability.ActorsToTarget.Add(cActor4);
					}
				}
				return;
			}
			Log.Error($"Non-list value of ActorsToTarget: {val} -> {res}");
		}
		else if (key.StartsWith("AddTileInRange", StringComparison.Ordinal))
		{
			CTile cTile = res.GetObject<CTile>();
			if (cTile != null)
			{
				if (!ability.TilesInRange.Contains(cTile))
				{
					ability.TilesInRange.Add(cTile);
				}
			}
			else
			{
				Log.Error($"Non-tile value of TileInRange: {val} -> {res}");
			}
		}
		else if (key.StartsWith("AddTilesInRange", StringComparison.Ordinal))
		{
			IList list3 = res.GetObject<IList>();
			if (list3 != null)
			{
				foreach (object item5 in list3)
				{
					if (!(item5 is CTile item))
					{
						Log.Error($"Non-tile value in TilesInRange: {res} -> {TValue.ToString(item5)}");
					}
					else if (!ability.TilesInRange.Contains(item))
					{
						ability.TilesInRange.Add(item);
					}
				}
				return;
			}
			Log.Error($"Non-list value of TilesInRange: {val} -> {res}");
		}
		else if (key.StartsWith("AddSelectedTiles", StringComparison.Ordinal))
		{
			IList list4 = res.GetObject<IList>();
			if (list4 != null)
			{
				foreach (object item6 in list4)
				{
					if (!(item6 is CTile item2))
					{
						Log.Error($"Non-tile value in AddSelectedTiles: {res} -> {TValue.ToString(item6)}");
					}
					else if (!ability.TilesSelected.Contains(item2))
					{
						ability.TilesSelected.Add(item2);
					}
				}
				return;
			}
			Log.Error($"Non-list value of AddSelectedTiles: {val} -> {res}");
		}
		else
		{
			if (!key.StartsWith("AddSelectedTile", StringComparison.Ordinal))
			{
				return;
			}
			CTile cTile2 = res.GetObject<CTile>();
			if (cTile2 != null)
			{
				if (!ability.TilesSelected.Contains(cTile2))
				{
					ability.TilesSelected.Add(cTile2);
				}
			}
			else
			{
				Log.Error($"Non-tile value of AddSelectedTile: {val} -> {res}");
			}
		}
	}

	public static bool IsActorInSameRoom(CActor target, CActor targetingActor)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y];
		CNode cNode2 = ScenarioManager.PathFinder.Nodes[target.ArrayIndex.X, target.ArrayIndex.Y];
		if (!cNode.IsBridge && !cNode2.IsBridge)
		{
			if (ScenarioManager.Tiles[target.ArrayIndex.X, target.ArrayIndex.Y].m_HexMap != ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y].m_HexMap)
			{
				return ScenarioManager.Tiles[target.ArrayIndex.X, target.ArrayIndex.Y].m_Hex2Map == ScenarioManager.Tiles[targetingActor.ArrayIndex.X, targetingActor.ArrayIndex.Y].m_HexMap;
			}
			return true;
		}
		return false;
	}

	public static bool IsArrayIndexInSameRoom(Point point1, Point point2)
	{
		CNode cNode = ScenarioManager.PathFinder.Nodes[point1.X, point1.Y];
		CNode cNode2 = ScenarioManager.PathFinder.Nodes[point2.X, point2.Y];
		if (!cNode.IsBridge && !cNode2.IsBridge)
		{
			if (ScenarioManager.Tiles[point1.X, point1.Y].m_HexMap != ScenarioManager.Tiles[point2.X, point2.Y].m_HexMap)
			{
				return ScenarioManager.Tiles[point1.X, point1.Y].m_Hex2Map == ScenarioManager.Tiles[point2.X, point2.Y].m_HexMap;
			}
			return true;
		}
		return false;
	}

	public static int GetDistanceBetweenActorsInHexes(CActor target, CActor targetingActor)
	{
		int result = -1;
		List<Point> list = ScenarioManager.PathFinder.FindPath(targetingActor, targetingActor.ArrayIndex, target.ArrayIndex, ignoreBlocked: true, ignoreMoveCost: true, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, 0, out var foundPath, ignoreDoorLocks: false);
		if (foundPath)
		{
			result = list.Count;
		}
		return result;
	}

	public static IEnumerable<CActor> GetActorsInRange(Point point, int range, CAbility ability)
	{
		if (ability.AbilityFilter.HasTargetTypeFlag(CAbilityFilter.EFilterTargetType.Self, exclusive: true) && !ability.AbilityFilter.HasNonTargetTypeFilters())
		{
			List<CActor> list = new List<CActor>();
			if (ability.ActorsToIgnore == null || !ability.ActorsToIgnore.Contains(ability.FilterActor))
			{
				list.Add(ability.FilterActor);
			}
			return list;
		}
		List<CTile> tilesInRange = GameState.GetTilesInRange(point, range, CAbility.EAbilityTargeting.Range, emptyTilesOnly: false, ignoreBlocked: true, null, ignorePathLength: false, ignoreBlockedWithActor: true, ability.MiscAbilityData?.IgnoreLOS ?? false, emptyOpenDoorTiles: false, ignoreMoveCost: true, ignoreDifficultTerrain: true);
		CTile tile = ScenarioManager.GetTile(point);
		if (tile != null)
		{
			tilesInRange.Add(tile);
		}
		return from x in tilesInRange
			select GameState.GetActorOnTile(x, ability.FilterActor, isTargetedAbility: ability.IsTargetedAbility, abilityFilter: ability.AbilityFilter, actorsToIgnore: ability.ActorsToIgnore, canTargetInvisible: ability.MiscAbilityData?.CanTargetInvisible == true, originalTargetType: false, skipUntargetableCheck: ability.CanSkipUntergetableCheck()) into x
			where x != null
			select x;
	}

	public static IEnumerable<CActor> GetAliveActorsInRange(Point point, int range, CAbility ability)
	{
		return from x in GetActorsInRange(point, range, ability)
			where !x.IsDead
			select x;
	}

	public static void RemoveInvalidActorsForAbilityMirrored(List<CActor> actors, CAbility ability)
	{
		if (ability != null)
		{
			CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour()?.RemoveInvalidActorsForAbility(ability, actors);
		}
	}

	public static void RemoveInvalidTilesForAbilityMirrored(List<CTile> tiles, CAbility ability)
	{
		if (ability != null)
		{
			ScenarioScript.current?.RemoveInvalidTilesForAbility(ability, tiles);
		}
	}

	public static bool IsMoveTileValidForAbilityMirrored(Point point, CAbilityMove ability)
	{
		if (ability?.TargetingActor == null)
		{
			return true;
		}
		if (!ability.TargetingActor.HasMirrorActor())
		{
			return true;
		}
		return CMirrorDamageActiveBonus_MirrorAbilities.GetMirrorAbilityBehaviour()?.IsMoveTileValidForAbilityMirrored(point, ability.TargetingActor) ?? true;
	}
}
