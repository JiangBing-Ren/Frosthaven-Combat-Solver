using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class CMirrorDamageActiveBonus_MirrorAbilities : CBespokeBehaviour
{
	public delegate Point FindMirrorTileFunc(Point target);

	public delegate bool IsOnMirroredSideFunc(CTile a, CTile b);

	public FindMirrorTileFunc FindMirrorPointOverride;

	public IsOnMirroredSideFunc IsOnMirroredSideOverride;

	private List<(CAbility, CActor)> PendingAbilities = new List<(CAbility, CActor)>();

	public override CActiveBonus.EActiveBonusBehaviourType BehaviourType => CActiveBonus.EActiveBonusBehaviourType.MirrorAbilities;

	public CActor MirrorTarget { get; private set; }

	public CMirrorDamageActiveBonus_MirrorAbilities(CActor actor, CActor caster, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
		MirrorTarget = caster;
	}

	private void PerformPendingAbilities()
	{
		PhaseManager.StartAbilities(PendingAbilities, stackInsteadOfInject: true);
		PendingAbilities.Clear();
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			allAliveActor.SetTempVar("LastTileBeforeForceMove", TValue.Unknown);
		}
	}

	private CActor CalcMirrorTarget(CAbility endedAbility)
	{
		CActor result = MirrorTarget;
		if (endedAbility.TargetingActor != ActiveBonus.Actor)
		{
			switch (endedAbility.AbilityType)
			{
			case CAbility.EAbilityType.Move:
			{
				CTile startTile = (endedAbility as CAbilityMove).StartTile;
				result = FindMirrorTile(startTile).LivingActor;
				break;
			}
			case CAbility.EAbilityType.Teleport2:
			{
				CTile sourceTile = (endedAbility as CAbilityTeleport2).SourceTile;
				result = FindMirrorTile(sourceTile).LivingActor;
				break;
			}
			default:
				result = FindMirrorActor(endedAbility.TargetingActor);
				break;
			}
		}
		return result;
	}

	private bool WasPerformedByMirrorSource(CAbility endedAbility)
	{
		return endedAbility.TargetingActor == ActiveBonus.Actor;
	}

	private bool WasGrantedFromSomeoneToMirror(CAbility endedAbility)
	{
		if (endedAbility.ControllingActor != null)
		{
			return endedAbility.TargetingActor == MirrorTarget;
		}
		return false;
	}

	private bool WasGrantedFromMirrorSourceToSomeone(CAbility endedAbility)
	{
		return endedAbility.TargetingActor == ActiveBonus.Actor;
	}

	private bool HasBonuses(CAbility endedAbility)
	{
		if (endedAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.NA)
		{
			return endedAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.Summon;
		}
		return false;
	}

	public override void OnAbilityEnded(CAbility endedAbility)
	{
		if (!endedAbility.AbilityHasHappened)
		{
			PerformPendingAbilities();
			return;
		}
		CActor cActor = CalcMirrorTarget(endedAbility);
		base.OnAbilityEnded(endedAbility);
		bool flag = WasPerformedByMirrorSource(endedAbility);
		bool flag2 = WasGrantedFromSomeoneToMirror(endedAbility);
		bool flag3 = WasGrantedFromMirrorSourceToSomeone(endedAbility);
		bool flag4 = HasBonuses(endedAbility);
		bool flag5 = (endedAbility is CAbilityShield || endedAbility is CAbilityRetaliate) && endedAbility.ActiveBonusData.Duration != CActiveBonus.EActiveBonusDurationType.Persistent;
		if (!(flag | flag2 | flag3) || !(!flag4 | flag5) || cActor == null)
		{
			CAbility.EAbilityType abilityType = endedAbility.AbilityType;
			if ((uint)(abilityType - 34) <= 1u)
			{
				List<CActor> onlyOnSpecificActors = ScenarioManager.Scenario.AllAliveActors.Where((CActor x) => x.HasMirrorActor()).ToList();
				MirrorPushPullAbility(endedAbility, endedAbility.TargetingActor, onlyOnSpecificActors);
			}
			PerformPendingAbilities();
		}
		else
		{
			if (endedAbility.IsScenarioModifierAbility || endedAbility.GetMiscVar("DontCopy").GetBool() || !endedAbility.AbilityHasHappened)
			{
				return;
			}
			switch (endedAbility.AbilityType)
			{
			case CAbility.EAbilityType.MoveTrap:
			{
				CAbilityMoveTrap cAbilityMoveTrap = endedAbility as CAbilityMoveTrap;
				List<CTile> list2 = cAbilityMoveTrap.InitialTrapPositions.ToList();
				List<CTile> list3 = cAbilityMoveTrap.FinalTrapPositions.ToList();
				RetargetMirrorTileList(list2);
				RetargetMirrorTileList(list3);
				if (!list2.Any((CTile x) => x.m_Props.Any((CObjectProp cObjectProp) => cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Trap)))
				{
					break;
				}
				CAbility.CopyAbility(endedAbility, generateNewID: false).TargetingActor = cActor;
				List<(CObjectTrap, CTile)> list4 = new List<(CObjectTrap, CTile)>();
				for (int num = 0; num < list2.Count; num++)
				{
					CTile cTile2 = list2[num];
					CTile item2 = list3[num];
					CObjectTrap cObjectTrap = cTile2.FindPropOfType<CObjectTrap>();
					if (cObjectTrap != null)
					{
						list4.Add((cObjectTrap, item2));
					}
				}
				CDisarmTrap_MessageData message3 = new CDisarmTrap_MessageData(cAbilityMoveTrap.AnimOverload, cActor)
				{
					m_ActorDisarmingTrap = cActor,
					m_Tiles = list2,
					m_DisarmedTraps = list4.Select(((CObjectTrap, CTile) x) => x.Item1).ToList(),
					m_DisarmTrapAbility = cAbilityMoveTrap
				};
				ScenarioRuleClient.MessageHandler(message3);
				foreach (var (cObjectTrap2, cTile3) in list4)
				{
					cObjectTrap2.DestroyProp();
					AbilityData.TrapData trapData = new AbilityData.TrapData
					{
						Damage = cObjectTrap2.DamageValue,
						Heal = cObjectTrap2.HealValue,
						Conditions = cObjectTrap2.Conditions?.ToList(),
						PositiveConditions = cObjectTrap2.PositiveConditions?.ToList(),
						AdjacentRange = cObjectTrap2.AdjacentRange,
						AdjacentDamage = cObjectTrap2.AdjacentDamageValue,
						AdjacentConditions = cObjectTrap2.AdjacentConditions?.ToList(),
						AdjacentFilter = cObjectTrap2.AdjacentFilter?.Copy(),
						TriggeredXP = cObjectTrap2.TriggeredXP
					};
					CObjectTrap prop = new CObjectTrap(cObjectTrap2.PrefabName, ScenarioManager.ObjectImportType.Trap, new TileIndex(cTile3.m_ArrayIndex), null, null, cActor, cTile3.m_HexMap.MapGuid, trapData);
					cTile3.SpawnProp(prop);
					CPlacingTrap_MessageData message4 = new CPlacingTrap_MessageData(cAbilityMoveTrap.AnimOverload, cActor)
					{
						m_ActorPlacingTrap = cActor,
						m_Tiles = new List<CTile> { cTile3 },
						m_TrapAbility = cAbilityMoveTrap
					};
					ScenarioRuleClient.MessageHandler(message4);
				}
				break;
			}
			case CAbility.EAbilityType.Teleport2:
			{
				CAbilityTeleport2 cAbilityTeleport = endedAbility as CAbilityTeleport2;
				List<CActor> actorsTargeted = cAbilityTeleport.ActorsTargeted;
				List<(CTile, CTile)> list = new List<(CTile, CTile)>();
				foreach (CActor item3 in actorsTargeted)
				{
					CTile cTile = item3.GetVar("LastTileBeforeForceMove").GetObject<CTile>();
					if (cTile != null)
					{
						list.Add((FindMirrorTile(cTile), FindMirrorTile(item3.Tile)));
					}
				}
				foreach (var item4 in list)
				{
					CActor livingActor = item4.Item1.LivingActor;
					if (livingActor != null)
					{
						CTile item = item4.Item2;
						livingActor.ArrayIndex = item.m_ArrayIndex;
						CActorIsTeleporting2_MessageData message = new CActorIsTeleporting2_MessageData(cActor)
						{
							m_Teleport2Ability = cAbilityTeleport,
							m_Waypoints = new List<CTile> { item },
							m_ActorToTeleport = livingActor
						};
						ScenarioRuleClient.MessageHandler(message);
						CActorHasTeleported2_MessageData message2 = new CActorHasTeleported2_MessageData(cActor)
						{
							m_Teleport2Ability = cAbilityTeleport,
							m_Waypoints = new List<CTile> { item },
							m_ActorToTeleport = livingActor,
							m_DestinationTile = item
						};
						ScenarioRuleClient.MessageHandler(message2);
					}
				}
				break;
			}
			case CAbility.EAbilityType.RemoveShadow:
			{
				CAbilityRemoveShadow cAbilityRemoveShadow = endedAbility as CAbilityRemoveShadow;
				List<CTile> list5 = cAbilityRemoveShadow.RemovedShadows.Select((CObjectProp x) => x.PropTile).ToList();
				RetargetMirrorTileList(list5);
				List<CObjectProp> list6 = (from x in ((IEnumerable<CTile>)list5).Select((Func<CTile, CObjectProp>)((CTile x) => x.FindPropOfType<CObjectShadow>()))
					where x != null
					select x).ToList();
				if (list6.Count > 0)
				{
					CAbilityCreateShadow.RemoveShadows(list6, cAbilityRemoveShadow, cActor);
				}
				break;
			}
			case CAbility.EAbilityType.CreateShadow:
			{
				CAbilityCreateShadow cAbilityCreateShadow = endedAbility as CAbilityCreateShadow;
				List<CTile> list10 = cAbilityCreateShadow.NewlyCreatedShadows.Select((CObjectProp x) => x.PropTile).ToList();
				List<CTile> list11 = cAbilityCreateShadow.NewlyRemovedShadows.Select((CObjectProp x) => x.PropTile).ToList();
				RetargetMirrorTileList(list10);
				RetargetMirrorTileList(list11);
				if (list10.Count > 0)
				{
					foreach (CTile item5 in list10)
					{
						CAbilityCreateShadow.SpawnShadow(item5, null, 0f, null, cActor);
					}
					CCreatingTerrain_MessageData message5 = new CCreatingTerrain_MessageData(cAbilityCreateShadow.AnimOverload, cActor)
					{
						m_Ability = cAbilityCreateShadow,
						m_Tiles = list10,
						m_ActorCreatingTerrain = cActor
					};
					ScenarioRuleClient.MessageHandler(message5);
				}
				List<CObjectProp> list12 = (from x in ((IEnumerable<CTile>)list11).Select((Func<CTile, CObjectProp>)((CTile x) => x.FindPropOfType<CObjectShadow>()))
					where x != null
					select x).ToList();
				if (list12.Count > 0)
				{
					CAbilityCreateShadow.RemoveShadows(list12, cAbilityCreateShadow, cActor);
				}
				break;
			}
			case CAbility.EAbilityType.MoveShadow:
			{
				CAbilityMoveShadow obj2 = endedAbility as CAbilityMoveShadow;
				List<CTile> list8 = obj2.InitialShadowPositions.ToList();
				List<CTile> list9 = obj2.FinalShadowPositions.ToList();
				RetargetMirrorTileList(list8);
				RetargetMirrorTileList(list9);
				if (!list8.Any((CTile x) => x.m_Props.Any((CObjectProp cObjectProp) => cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Shadow)))
				{
					break;
				}
				CAbility cAbility2 = CAbility.CopyAbility(endedAbility, generateNewID: false);
				cAbility2.TargetingActor = cActor;
				CPlayCharacterAnimation_MessageData cPlayCharacterAnimation_MessageData = new CPlayCharacterAnimation_MessageData("Shadow Manipulation", cActor);
				cPlayCharacterAnimation_MessageData.m_TileToLookAt = cActor.Tile;
				cPlayCharacterAnimation_MessageData.m_Ability = cAbility2;
				ScenarioRuleClient.MessageHandler(cPlayCharacterAnimation_MessageData);
				for (int num2 = 0; num2 < list8.Count; num2++)
				{
					CTile cTile4 = list8[num2];
					CTile newTile = list9[num2];
					if (cTile4.m_Props.SingleOrDefault((CObjectProp cObjectProp) => cObjectProp.ObjectType == ScenarioManager.ObjectImportType.Shadow) is CObjectShadow shadow)
					{
						CAbilityMoveShadow.MoveShadow(shadow, newTile);
					}
				}
				break;
			}
			case CAbility.EAbilityType.ModeTransfer:
			{
				CAbilityModeTransfer cAbilityModeTransfer = endedAbility as CAbilityModeTransfer;
				if (cAbilityModeTransfer.LastSummonKilled != null)
				{
					CActor cActor2 = FindMirrorActor(cAbilityModeTransfer.LastSummonKilled);
					if (cActor2 != null)
					{
						CAbilityCard lastActivatedModeCard = cAbilityModeTransfer.LastActivatedModeCard;
						if (lastActivatedModeCard != null)
						{
							CAction selectedAction = lastActivatedModeCard.SelectedAction;
							GameState.KillActor(cAbilityModeTransfer.TargetingActor, cActor2, CActor.ECauseOfDeath.SummonActiveBonusCancelled, out var _, endedAbility, actorWasAsleep: false, null, instantlyDeletePrefab: true);
							lastActivatedModeCard.SetSelectedAction(selectedAction);
						}
					}
				}
				if (cAbilityModeTransfer.LastSummonSummoned != null)
				{
					CAbilityModeTransfer modeAbility = CAbility.CopyAbility(cAbilityModeTransfer, generateNewID: false) as CAbilityModeTransfer;
					CAbilityCard cAbilityCard = cAbilityModeTransfer.LastSummonSummoned.FindSummonCard();
					CAction cAction = cAbilityCard.SelectedAction ?? cAbilityCard.LastSelectedAction;
					if (cAction != null)
					{
						cAbilityCard.SetSelectedAction(cAction);
						CAbilityModeTransfer.SummonModeActor(cAbilityModeTransfer.TargetingActor, cAction, cAbilityCard, modeAbility, FindMirrorTile(cAbilityModeTransfer.LastSummonSummoned.Tile));
					}
				}
				if (cAbilityModeTransfer.TargetingActor.Tile != FindMirrorTile(MirrorTarget.Tile))
				{
					GameState.TeleportActor(MirrorTarget, FindMirrorTile(cAbilityModeTransfer.TargetingActor.Tile));
				}
				break;
			}
			case CAbility.EAbilityType.Push:
			case CAbility.EAbilityType.Pull:
				if (!MirrorPushPullAbility(endedAbility, cActor))
				{
					return;
				}
				break;
			case CAbility.EAbilityType.Loot:
			{
				CAbilityLoot obj = endedAbility as CAbilityLoot;
				CAbility cAbility = CAbilityLoot.CreateDefaultLoot(99);
				List<CTile> list7 = obj.PropsLooted.Select((CObjectProp x) => x.PropTile).ToList();
				RetargetMirrorTileList(list7);
				cAbility.SetMiscVar("AutoLootTiles", new TValue(list7));
				PendingAbilities.Add((cAbility, cActor));
				break;
			}
			case CAbility.EAbilityType.Move:
			{
				CAbilityMove cAbilityMove = endedAbility as CAbilityMove;
				CAbilityMove cAbilityMove2 = CAbility.CopyAbility(endedAbility, generateNewID: false, fullCopy: true, copyCurrentOverrides: true, applyCurrentStatIsBasedOnX: false, endedAbility.VarsContext) as CAbilityMove;
				cAbilityMove2.FixedPath = cAbilityMove.TilesMovedThisInstance.ToList();
				if (cAbilityMove2.FixedPath.Count != 0)
				{
					int moveCount = (cAbilityMove2.Strength = cAbilityMove2.FixedPath.Count);
					cAbilityMove2.MoveCount = moveCount;
					if (cAbilityMove2.TilesSelected.Count == 0 && cAbilityMove2.FixedPath.Count > 0)
					{
						cAbilityMove2.TilesSelected.Add(ScenarioManager.GetTile(cAbilityMove2.FixedPath[cAbilityMove2.FixedPath.Count - 1]));
					}
					RetargetMirrorPointList(cAbilityMove2.FixedPath);
					GenericMirrorAbility(cAbilityMove, cAbilityMove2, cActor);
				}
				break;
			}
			default:
			{
				CAbility copy = CAbility.CopyAbility(endedAbility, generateNewID: false, fullCopy: true, copyCurrentOverrides: true, applyCurrentStatIsBasedOnX: false, endedAbility.VarsContext);
				GenericMirrorAbility(endedAbility, copy, cActor);
				break;
			}
			case CAbility.EAbilityType.Trap:
			case CAbility.EAbilityType.Create:
			case CAbility.EAbilityType.Consume:
			case CAbility.EAbilityType.Infuse:
			case CAbility.EAbilityType.DestroyObstacle:
			case CAbility.EAbilityType.ControlActor:
			case CAbility.EAbilityType.ConsumeElement:
			case CAbility.EAbilityType.ChooseAbility:
			case CAbility.EAbilityType.ExtraTurn:
			case CAbility.EAbilityType.Choose:
			case CAbility.EAbilityType.CreateTerrain:
			case CAbility.EAbilityType.MoveActiveBonusTracker:
			case CAbility.EAbilityType.PerformFromShadow:
			case CAbility.EAbilityType.DestroyTerrain:
			case CAbility.EAbilityType.SelectAndPlayCard:
			case CAbility.EAbilityType.SelectAndMoveCard:
			case CAbility.EAbilityType.CreateLimitedHazardousTerrain:
			case CAbility.EAbilityType.SelectCard:
				break;
			}
			PerformPendingAbilities();
		}
	}

	public static List<CActor> GetMiscVarsActors(CAbility ability, string key)
	{
		return ability.GetMiscVar(key).GetObject<List<string>>()?.Select((string x) => ScenarioManager.FindActor(x))?.ToList();
	}

	public static List<CTile> GetMiscVarsTiles(CAbility ability, string key)
	{
		return ability.GetMiscVar(key).GetObject<List<Point>>()?.Select((Point x) => ScenarioManager.GetTile(x))?.ToList();
	}

	private void GenericMirrorAbility(CAbility endedAbility, CAbility copy, CActor MirrorTarget)
	{
		CBaseCard abilityBaseCard = endedAbility.GetAbilityBaseCard();
		if (abilityBaseCard != null && (abilityBaseCard is CItem || (abilityBaseCard.CardType == CBaseCard.ECardType.CharacterAbility && endedAbility.AbilityType != CAbility.EAbilityType.Summon)))
		{
			copy.CachedAbilityBaseCard = CBaseCard.CreateDummy();
			copy.CardID = copy.CachedAbilityBaseCard.ID;
			copy.ActiveBonusData.Hidden = true;
		}
		copy.SetHasStarted(val: false);
		copy.SetCancelAbilityFlag(cancel: false);
		List<CActor> list = copy.ActorsTargeted;
		if (list == null || list.Count == 0)
		{
			list = copy.ActorsToTarget;
		}
		if (copy.MiscAbilityData != null)
		{
			copy.MiscAbilityData.UseParentTargets = false;
			copy.MiscAbilityData.AutotriggerAbility = true;
		}
		if (list.Contains(endedAbility.TargetingActor) && !WasPerformedByMirrorSource(endedAbility))
		{
			list.Remove(endedAbility.TargetingActor);
			copy.ValidActorsInRange.Remove(endedAbility.TargetingActor);
			copy.TilesInRange.Remove(endedAbility.TargetingActor.Tile);
			copy.TilesSelected.Remove(endedAbility.TargetingActor.Tile);
			copy.ValidTilesInAreaAffected.Remove(endedAbility.TargetingActor.Tile);
			copy.ValidTilesInAreaEffectedIncludingBlocked.Remove(endedAbility.TargetingActor.Tile);
		}
		RetargetMirrorActorList(list);
		RetargetMirrorActorList(copy.ValidActorsInRange);
		RetargetMirrorTileList(copy.TilesInRange);
		RetargetMirrorTileList(copy.TilesSelected);
		RetargetMirrorTileList(copy.ValidTilesInAreaAffected);
		RetargetMirrorTileList(copy.ValidTilesInAreaEffectedIncludingBlocked);
		copy.AddParentContext(endedAbility.VarsContext);
		if (endedAbility.TargetingOverrideArrayIndex != default(Point))
		{
			copy.TargetingOverrideArrayIndex = FindMirrorPoint(endedAbility.TargetingOverrideArrayIndex);
		}
		copy.Targeting = CAbility.EAbilityTargeting.Custom;
		copy.TargetingVars = new List<(string, TValue)>();
		if (list != null)
		{
			copy.TargetingVars.Add(("AddValidActorsInRange", new TValue(CExpression.Parse("CMirrorDamageActiveBonus_MirrorAbilities.GetMiscVarsActors(obj, \"CachedActorGuids\")"))));
			copy.TargetingVars.Add(("AddActorsToTarget", new TValue(CExpression.Parse("CMirrorDamageActiveBonus_MirrorAbilities.GetMiscVarsActors(obj, \"CachedActorGuids\")"))));
			copy.SetMiscVar("CachedActorGuids", new TValue(list.Select((CActor x) => x.ActorGuid).ToList()));
		}
		if (copy.TilesInRange != null)
		{
			copy.TargetingVars.Add(("AddTilesInRange", new TValue(CExpression.Parse("CMirrorDamageActiveBonus_MirrorAbilities.GetMiscVarsTiles(obj, \"CachedTilesInRange\")"))));
			copy.SetMiscVar("CachedTilesInRange", new TValue(copy.TilesInRange.Select((CTile x) => x.m_ArrayIndex).ToList()));
		}
		if (copy.TilesSelected != null)
		{
			copy.TargetingVars.Add(("AddSelectedTilesInRange", new TValue(CExpression.Parse("CMirrorDamageActiveBonus_MirrorAbilities.GetMiscVarsTiles(obj, \"CachedTilesSelected\")"))));
			copy.SetMiscVar("CachedTilesSelected", new TValue(copy.TilesSelected.Select((CTile x) => x.m_ArrayIndex).ToList()));
		}
		copy.SetMiscVar("IsMirrorAbility", true);
		copy.NumberTargets = list.Count;
		copy.ActorsToTarget.Clear();
		copy.TilesSelected?.Clear();
		copy.SubAbilities.Clear();
		PendingAbilities.Add((copy, MirrorTarget));
	}

	private bool MirrorPushPullAbility(CAbility endedAbility, CActor targetingActor, List<CActor> onlyOnSpecificActors = null)
	{
		List<CTile> list = new List<CTile>();
		List<List<Point>> list2 = new List<List<Point>>();
		foreach (CActor item2 in endedAbility.ActorsTargeted)
		{
			CTile cTile = item2.GetVar("LastTileBeforeForceMove").GetObject<CTile>();
			if ((onlyOnSpecificActors == null || onlyOnSpecificActors.Contains(item2)) && cTile != null)
			{
				list.Add(cTile);
				List<Point> item = item2.GetVar("LastForceMovePath").GetObject<List<Point>>();
				list2.Add(item);
			}
		}
		RetargetMirrorTileList(list);
		List<(CAbility, CActor)> list3 = new List<(CAbility, CActor)>();
		for (int i = 0; i < list.Count; i++)
		{
			CTile cTile2 = list[i];
			List<Point> list4 = list2[i];
			if (list4 == null || list4.Count == 0)
			{
				continue;
			}
			RetargetMirrorPointList(list4);
			CActor livingActor = cTile2.LivingActor;
			if (livingActor == null)
			{
				continue;
			}
			CAbility cAbility = CAbility.CreateAbility(endedAbility.AbilityType, endedAbility.Strength, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.TargetThisActorAutomatically = livingActor;
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.SetMiscVar("IsMirrorAbility", true);
			cAbility.SetMiscVar("ForcedPath", new TValue(list4));
			if (cAbility is CAbilityPull cAbilityPull)
			{
				cAbilityPull.TryPullToThisTileAutomatically = ScenarioManager.GetTile(list4.LastOrDefault());
				if (endedAbility.TargetingActor != ActiveBonus.Actor)
				{
					cAbilityPull.SetMiscVar("PullToPoint", new TValue(cAbilityPull.TryPullToThisTileAutomatically));
				}
			}
			else if (cAbility is CAbilityPush cAbilityPush)
			{
				cAbilityPush.TryPushToThisTileAutomatically = ScenarioManager.GetTile(list4.LastOrDefault());
				if (endedAbility.TargetingActor != ActiveBonus.Actor)
				{
					cAbilityPush.SetMiscVar("PushFromPoint", new TValue(FindMirrorTile(endedAbility.TargetingActor.Tile)));
				}
			}
			list3.Add((cAbility, targetingActor));
		}
		foreach (var item3 in list3)
		{
			PendingAbilities.Insert(0, item3);
		}
		if (endedAbility.IsInlineSubAbility && !PendingAbilities.Any(((CAbility, CActor) x) => x.Item1 == endedAbility))
		{
			return false;
		}
		return true;
	}

	public override void OnPropCreated(CActor actor, CAbility endedAbility, CObjectProp prop)
	{
		if (prop.ObjectType == ScenarioManager.ObjectImportType.Shadow)
		{
			return;
		}
		if (actor.HasMirrorActor())
		{
			CTile cTile = FindMirrorTile(prop.PropTile);
			CObjectProp cObjectProp = null;
			if (prop is CObjectTrap && endedAbility is CAbilityTrap cAbilityTrap)
			{
				CObjectTrap prop2 = new CObjectTrap(cAbilityTrap.TrapData.TrapName, ScenarioManager.ObjectImportType.Trap, new TileIndex(cTile.m_ArrayIndex), null, null, MirrorTarget, cTile.m_HexMap.MapGuid, cAbilityTrap.TrapData);
				cTile.SpawnProp(prop2, notifyClient: true, cAbilityTrap.SpawnDelay);
				CPlacingTrap_MessageData message = new CPlacingTrap_MessageData(cAbilityTrap.AnimOverload, MirrorTarget)
				{
					m_ActorPlacingTrap = MirrorTarget,
					m_Tiles = new List<CTile> { cTile },
					m_TrapAbility = cAbilityTrap
				};
				ScenarioRuleClient.MessageHandler(message);
			}
			else if (prop is CObjectObstacle cObjectObstacle)
			{
				List<TileIndex> list = cObjectObstacle.PathingBlockers.ToList();
				RetargetMirrorTileList(list);
				CAreaEffect cAreaEffect = endedAbility?.AreaEffect ?? CObjectProp.GetAreaEffect(prop.PrefabName, prop.PropType.ToString());
				int num = 0;
				for (num = 0; num < 360; num += 60)
				{
					List<CTile> rotatedTiles = CAreaEffect.GetRotatedHexTiles(cTile, cTile, melee: true, cAreaEffect.Hexes, num);
					if (list.All((TileIndex x) => rotatedTiles.Contains(ScenarioManager.GetTile(x.ToPoint()))))
					{
						break;
					}
				}
				num = 180 - num;
				cObjectProp = new CObjectObstacle(prop.PrefabName, ScenarioManager.ObjectImportType.Obstacle, new TileIndex(cTile.m_ArrayIndex), null, new CVector3(0f, num, 0f), list, MirrorTarget, cTile.m_HexMap.MapGuid, ignoresFlyAndJump: false, setPositionToCenterOfAllPathBlockers: true);
			}
			else if (endedAbility.AbilityType == CAbility.EAbilityType.CreateTerrain)
			{
				CAbilityCreateTerrain.SpawnTerrain(prop.ObjectType, cTile, prop.Rotation, MirrorTarget, 0f, prop.PrefabName);
			}
			else if (endedAbility.AbilityType == CAbility.EAbilityType.CreateLimitedHazardousTerrain)
			{
				CAbilityCreateLimitedHazardousTerrain.SpawnTerrain(cTile, prop.Rotation, 0f, MirrorTarget, null, prop.PrefabName);
			}
			if (cObjectProp != null)
			{
				cTile.SpawnProp(cObjectProp);
			}
		}
		base.OnPropCreated(actor, endedAbility, prop);
	}

	public override void OnPropDestroyed(CActor actor, CAbility endedAbility, CObjectProp prop)
	{
		if (prop.ObjectType != ScenarioManager.ObjectImportType.Shadow)
		{
			if (actor.HasMirrorActor())
			{
				FindMirrorTile(prop.PropTile).FindPropOfTypes(prop.PropType, forDestructionFromPlayer: true)?.DestroyProp();
			}
			base.OnPropDestroyed(actor, endedAbility, prop);
		}
	}

	private void RetargetMirrorActorList(List<CActor> actors)
	{
		if (actors == null)
		{
			return;
		}
		for (int i = 0; i < actors.Count; i++)
		{
			CActor target = actors[i];
			CActor value = FindMirrorActor(target);
			actors[i] = value;
		}
		for (int num = actors.Count - 1; num >= 0; num--)
		{
			if (actors[num] == null)
			{
				actors.RemoveAt(num);
			}
		}
	}

	private void RetargetMirrorPointList(List<Point> points)
	{
		if (points != null)
		{
			for (int i = 0; i < points.Count; i++)
			{
				Point target = points[i];
				Point value = FindMirrorPoint(target);
				points[i] = value;
			}
		}
	}

	private void RetargetMirrorTileList(List<CTile> tiles)
	{
		if (tiles == null)
		{
			return;
		}
		for (int i = 0; i < tiles.Count; i++)
		{
			CTile cTile = tiles[i];
			CTile tile = ScenarioManager.GetTile(FindMirrorPoint(cTile.m_ArrayIndex));
			tiles[i] = tile;
		}
		for (int num = tiles.Count - 1; num >= 0; num--)
		{
			if (tiles[num] == null)
			{
				tiles.RemoveAt(num);
			}
		}
	}

	private void RetargetMirrorTileList(List<TileIndex> tiles)
	{
		if (tiles == null)
		{
			return;
		}
		for (int i = 0; i < tiles.Count; i++)
		{
			TileIndex tileIndex = tiles[i];
			TileIndex value = new TileIndex(FindMirrorPoint(tileIndex.ToPoint()));
			tiles[i] = value;
		}
		for (int num = tiles.Count - 1; num >= 0; num--)
		{
			if (tiles[num] == null)
			{
				tiles.RemoveAt(num);
			}
		}
	}

	public CActor FindMirrorActor(CActor target)
	{
		Point arrayIndex = target.ArrayIndex;
		CTile cTile = target.GetVar("LastTileBeforeForceMove").GetObject<CTile>();
		if (cTile != null)
		{
			arrayIndex = cTile.m_ArrayIndex;
		}
		return ScenarioManager.GetTile(FindMirrorPoint(arrayIndex))?.LivingActor;
	}

	public Point FindMirrorPoint(Point target)
	{
		if (FindMirrorPointOverride != null)
		{
			return FindMirrorPointOverride(target);
		}
		Point point = target - m_Actor.ArrayIndex;
		return MirrorTarget.ArrayIndex + point;
	}

	public CTile FindMirrorTile(CTile target)
	{
		return ScenarioManager.GetTile(FindMirrorPoint(target.m_ArrayIndex));
	}

	public bool IsOnMirroredSide(CTile a, CTile b)
	{
		if (IsOnMirroredSideOverride != null)
		{
			return IsOnMirroredSideOverride(a, b);
		}
		return false;
	}

	public void RemoveInvalidActorsForAbility(CAbility ability, List<CActor> actors)
	{
		if (!ability.TargetingActor.HasMirrorActor())
		{
			return;
		}
		switch (ability.AbilityType)
		{
		case CAbility.EAbilityType.ModeTransfer:
		{
			if ((ability as CAbilityModeTransfer).m_State != CAbilityModeTransfer.EModeTransferState.SelectTargetSummon)
			{
				return;
			}
			for (int num = actors.Count - 1; num >= 0; num--)
			{
				CActor cActor = actors[num];
				if (FindMirrorActor(cActor) == null || IsOnMirroredSide(cActor.Tile, ability.TargetingActor.Tile))
				{
					actors.RemoveAt(num);
				}
			}
			return;
		}
		case CAbility.EAbilityType.Attack:
		case CAbility.EAbilityType.NullTargeting:
			return;
		}
		for (int num2 = actors.Count - 1; num2 >= 0; num2--)
		{
			CActor target = actors[num2];
			if (FindMirrorActor(target) == null)
			{
				actors.RemoveAt(num2);
			}
		}
	}

	public static CMirrorDamageActiveBonus_MirrorAbilities GetMirrorAbilityBehaviour(CActor actor = null)
	{
		return ((actor == null) ? ScenarioManager.Scenario.AllAliveActors.SelectMany((CActor x) => x.FindApplicableActiveBonuses(CAbility.EAbilityType.MirrorDamage, CActiveBonus.EActiveBonusBehaviourType.MirrorAbilities)).FirstOrDefault() : actor.FindApplicableActiveBonuses(CAbility.EAbilityType.MirrorDamage, CActiveBonus.EActiveBonusBehaviourType.MirrorAbilities).FirstOrDefault())?.GetBehaviour<CMirrorDamageActiveBonus_MirrorAbilities>();
	}

	public bool IsMoveTileValidForAbilityMirrored(Point point, CActor actorWithMirror)
	{
		CTile tile = ScenarioManager.GetTile(point);
		CTile cTile = FindMirrorTile(tile);
		if (cTile == null || cTile.IsBlocked || (cTile.LivingActor != null && !CustomAbility.IsAlly(cTile.LivingActor, actorWithMirror)))
		{
			return false;
		}
		return true;
	}

	public bool IsStopTileValidForAbilityMirrored(Point point, CActor actorWithMirror)
	{
		CTile tile = ScenarioManager.GetTile(point);
		CTile cTile = FindMirrorTile(tile);
		if (cTile == null || cTile.IsBlocked || (cTile.LivingActor != null && cTile.LivingActor != actorWithMirror && cTile.LivingActor != MirrorTarget))
		{
			return false;
		}
		return true;
	}
}
