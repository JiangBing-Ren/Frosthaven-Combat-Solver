using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Libs.GameConsole;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH111 : ScenarioScript
{
	private const int MiddlePoint = 8;

	private const string ShimmerClassID = "TheShimmerID";

	[ConsoleVariable(Alias = "fh111_simplified_player_mirror")]
	public static bool SimplifiedPlayerMirror = true;

	private CScenarioModifier _mirrorDamageModifier;

	private CScenarioModifier _mirrorFocusModifier;

	private CScenarioModifier _mirrorAbilitiesModifier;

	private CScenarioModifier _disadvantageModifier;

	private const string MirroredEntitySimplifiedClass = "MirroredCharacter111SimplifiedID";

	protected override bool Initialize()
	{
		_mirrorDamageModifier = GetScenarioModifier("MirrorDamage");
		_mirrorAbilitiesModifier = GetScenarioModifier("MirrorAbilities");
		_mirrorFocusModifier = GetScenarioModifier("MirrorFocus");
		_disadvantageModifier = GetScenarioModifier("Disadvantage");
		CObjectDoor cObjectDoor = ScenarioScript.FindDoor(16, 7);
		CObjectDoor cObjectDoor2 = ScenarioScript.FindDoor(16, 9);
		cObjectDoor.TwinDoors = new CObjectDoor[1] { cObjectDoor2 };
		cObjectDoor2.TwinDoors = new CObjectDoor[1] { cObjectDoor };
		return base.Initialize();
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		AssignMirrorFunctions();
	}

	private void AssignMirrorFunctions()
	{
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			CActiveBonus cActiveBonus = allAliveActor.FindApplicableActiveBonuses(CAbility.EAbilityType.MirrorDamage, CActiveBonus.EActiveBonusBehaviourType.MirrorAbilities).FirstOrDefault();
			if (cActiveBonus != null)
			{
				CMirrorDamageActiveBonus_MirrorAbilities behaviour = cActiveBonus.GetBehaviour<CMirrorDamageActiveBonus_MirrorAbilities>();
				if (behaviour != null)
				{
					behaviour.FindMirrorPointOverride = GetMirrorPoint;
					behaviour.IsOnMirroredSideOverride = IsOnMirroredSide;
					if (allAliveActor.Class.ID == "TheShimmerID" && allAliveActor.ArrayIndex.Y > 8)
					{
						allAliveActor.CustomLocKey = "TheShimmerMirrored";
						ApplyMirrorVFX(allAliveActor);
					}
				}
			}
			if (allAliveActor.Class.ID == "TheShimmerID")
			{
				allAliveActor.NoGoldDrop = true;
			}
		}
	}

	private bool IsOnMirroredSide(CTile a, CTile b)
	{
		int num = a.m_ArrayIndex.Y - 8;
		int num2 = b.m_ArrayIndex.Y - 8;
		return num * num2 < 0;
	}

	private Point GetMirrorPoint(Point p)
	{
		if (p.Y == 8)
		{
			return p;
		}
		int num = Math.Abs(8 - p.Y);
		if (p.Y < 8)
		{
			return new Point(p.X, 8 + num);
		}
		return new Point(p.X, 8 - num);
	}

	public CActor GetMirrorActor(CActor actor)
	{
		Point mirrorPoint = GetMirrorPoint(actor.ArrayIndex);
		return ScenarioScript.FindActor(mirrorPoint.X, mirrorPoint.Y);
	}

	protected override void TickPhaseInternal()
	{
		base.TickPhaseInternal();
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickPhaseEndTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurnLoot:
			TickPhaseEndTurnLoot();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickPhaseEndRound();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			TickActorRevealed();
			break;
		}
	}

	protected override void OnFinishedRevealingRoom()
	{
		base.OnFinishedRevealingRoom();
		if (_tickRoomRevealed.RoomName == "Room_2")
		{
			_mirrorDamageModifier.SetDeactivated(deactivate: false);
			_mirrorAbilitiesModifier.SetDeactivated(deactivate: false);
			_mirrorFocusModifier.SetDeactivated(deactivate: false);
		}
		else if (_tickRoomRevealed.RoomName == "Room_3")
		{
			if (!ScenarioScript.FindRoom("Room_4").Revealed)
			{
				ScenarioScript.FindDoor(9, 4).ForceActivate(_tickActor);
			}
		}
		else if (_tickRoomRevealed.RoomName == "Room_4" && !ScenarioScript.FindRoom("Room_3").Revealed)
		{
			ScenarioScript.FindDoor(9, 12).ForceActivate(_tickActor);
		}
	}

	private void TickActorRevealed()
	{
		if (_tickActor is CEnemyActor cEnemyActor && cEnemyActor.Class.ID == "TheShimmerID")
		{
			CActor mirrorActor = GetMirrorActor(cEnemyActor);
			if (mirrorActor != null)
			{
				ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, cEnemyActor, mirrorActor);
				ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, mirrorActor, cEnemyActor);
				ApplyScenarioActiveBonusToTickActor(_mirrorAbilitiesModifier, mirrorActor, cEnemyActor);
				AssignMirrorFunctions();
			}
		}
	}

	private void TickPhaseEndRound()
	{
		if (!OncePerRound())
		{
			return;
		}
		foreach (CActor allAliveActor in ScenarioManager.Scenario.AllAliveActors)
		{
			if (allAliveActor is CPlayerActor || allAliveActor is CEnemyActor)
			{
				CheckMirrorActorEndRoundConsequences(allAliveActor);
			}
		}
	}

	private void TickPhaseEndTurn()
	{
		CActor tickActor = _tickActor;
		CActor mirrorActor = tickActor.GetMirrorActor(includeMirroredClone: true);
		TryEscapeMap();
		if (tickActor.CauseOfDeath == CActor.ECauseOfDeath.ActorRemovedFromMap && mirrorActor != null)
		{
			GameState.KillActor(mirrorActor, mirrorActor, CActor.ECauseOfDeath.ActorRemovedFromMap, out var _);
		}
	}

	private void TickPhaseEndTurnLoot()
	{
		if (ScenarioManager.Scenario.AllActors.SingleOrDefault((CActor x) => x.Class.ID == "MirroredCharacterID") == null)
		{
			CObjectProp lootedChest = ScenarioManager.CurrentScenarioState.ActivatedProps.SingleOrDefault((CObjectProp x) => x.PropTile == _tickActor.Tile && x.PropType == EPropType.GoalChest);
			AfterLootGoalChest(lootedChest);
		}
	}

	private void CheckMirrorActorEndRoundConsequences(CActor actor)
	{
		if (actor.GetMirrorAbilitiesActor(includeMirroredClone: true) != null)
		{
			return;
		}
		if (SimplifiedPlayerMirror)
		{
			if (actor.Class.ID == "MirroredCharacter111SimplifiedID")
			{
				return;
			}
		}
		else if (actor.GetMirrorActor(includeMirroredClone: true, checkMirrorAbilities: false) != null)
		{
			return;
		}
		CActor mirrorActor = GetMirrorActor(actor);
		if (mirrorActor == null)
		{
			DealScenarioDamage(actor, 1);
		}
		else if (CustomAbility.IsAlly(actor, mirrorActor))
		{
			CAbility cAbility = CAbilityHeal.CreateDefaultHeal(1, 1, 1, actor.IsMonsterType, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self));
			if (cAbility.MiscAbilityData == null)
			{
				cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
			}
			cAbility.MiscAbilityData.AutotriggerAbility = true;
			cAbility.SkipAnim = true;
			cAbility.IsScenarioModifierAbility = true;
			PhaseManager.StartAbilities(new List<(CAbility, CActor)> { (cAbility, actor) });
		}
		else
		{
			DealScenarioDamage(actor, 2);
		}
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (_tickActor.Class.ID == "TheShimmerID")
		{
			CActor mirrorActor = _tickActor.GetMirrorActor(includeMirroredClone: true);
			if (mirrorActor != null)
			{
				Point mirrorPoint = GetMirrorPoint(mirrorActor.ArrayIndex);
				_tickActor.ArrayIndex = mirrorPoint;
			}
			CTile tile = _tickActor.Tile;
			CObjectChest prop = new CObjectChest(ScenarioManager.ObjectImportType.GoalChest.ToString(), ScenarioManager.ObjectImportType.GoalChest, new TileIndex(tile.m_ArrayIndex), null, null, null, tile.m_HexMap.MapGuid);
			tile.SpawnProp(prop);
			_mirrorDamageModifier.SetHiddenState(shouldSetHidden: true);
			_mirrorAbilitiesModifier.SetHiddenState(shouldSetHidden: true);
			_mirrorFocusModifier.SetHiddenState(shouldSetHidden: true);
		}
	}

	private void AfterLootGoalChest(CObjectProp lootedChest)
	{
		if (lootedChest == null)
		{
			return;
		}
		CObjectProp cObjectProp = ScenarioManager.GetTile(GetMirrorPoint(lootedChest.PropTile.m_ArrayIndex)).m_Props.SingleOrDefault((CObjectProp x) => x.PropType == EPropType.GoalChest);
		if (cObjectProp != null)
		{
			cObjectProp.DestroyProp();
			CActor cActor = _tickActor;
			if (cActor is CHeroSummonActor cHeroSummonActor)
			{
				cActor = cHeroSummonActor.Summoner;
			}
			CActor cActor2 = SpawnDamageRedirection(cActor, ScenarioManager.GetTile(GetMirrorPoint(cActor.ArrayIndex)));
			CFocusCamera_MessageData message = new CFocusCamera_MessageData(_tickActor)
			{
				m_Point = cActor2.ArrayIndex,
				m_Zoom = 0.3f
			};
			ScenarioRuleClient.MessageHandler(message);
			GameState.TriggerCampaignSection("FS_111_Ice_Cave_Special_1");
			ScenarioScript.FindDoor(9, 4)?.UnlockLockedDoorWithoutOpening();
			ScenarioScript.FindDoor(9, 12)?.UnlockLockedDoorWithoutOpening();
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		base.TickAbilityEndedInternal();
		if (_tickAbility is CAbilityLoot { PropsLooted: var propsLooted })
		{
			CObjectProp lootedChest = propsLooted.SingleOrDefault((CObjectProp x) => x.PropType == EPropType.GoalChest);
			AfterLootGoalChest(lootedChest);
		}
	}

	private void ApplyMirrorVFX(CActor actor)
	{
		if (!actor.ActiveBonuses.Any((CActiveBonus x) => x.Ability?.ActiveBonusData?.Vars != null && x.Ability.ActiveBonusData.Vars.Any(((string, TValue) y) => y.Item1 == "PersistentCasterVFX" && y.Item2.GetString() == "AmuletReanimationVFX")))
		{
			CAbility cAbility = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: false);
			cAbility.ActiveBonusData.Duration = CActiveBonus.EActiveBonusDurationType.Persistent;
			cAbility.ActiveBonusData.Hidden = true;
			cAbility.ActiveBonusData.Vars = new List<(string, TValue)> { ("PersistentCasterVFX", new TValue("AmuletReanimationVFX")) };
			cAbility.Start(actor, actor);
			cAbility.ApplyToActor(actor);
		}
	}

	private CActor SpawnDamageRedirection(CActor actor, CTile tile)
	{
		CActor livingActor = tile.LivingActor;
		if (SimplifiedPlayerMirror)
		{
			CTile closestEmptyTile = ScenarioScript.GetClosestEmptyTile(tile.m_ArrayIndex);
			CActor cActor = SpawnDamageRedirectionBase(actor, closestEmptyTile, copyBonuses: false, "MirroredCharacter111SimplifiedID", isSummon: false);
			cActor.CustomLocKey = "MirrorEntity";
			ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, actor, cActor);
			ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, cActor, actor);
			ApplyMirrorVFX(cActor);
			cActor.CheckForCachedValuesAfterActiveBonusesUpdate();
			return cActor;
		}
		CActor cActor2 = SpawnDamageRedirectionBase(actor, tile, copyBonuses: false);
		ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, actor, cActor2);
		ApplyScenarioActiveBonusToTickActor(_mirrorDamageModifier, cActor2, actor);
		_disadvantageModifier.SetDeactivated(deactivate: false);
		ApplyScenarioActiveBonusToTickActor(_disadvantageModifier, actor, cActor2);
		ApplyScenarioActiveBonusToTickActor(_disadvantageModifier, cActor2, actor);
		ApplyScenarioActiveBonusToTickActor(_mirrorAbilitiesModifier, cActor2, actor);
		AssignMirrorFunctions();
		if (IsDeathwalker(actor as CPlayerActor))
		{
			foreach (CObjectProp item in ScenarioManager.CurrentScenarioState.ShadowProps.ToList())
			{
				CTile tile2 = ScenarioManager.GetTile(GetMirrorPoint(item.ArrayIndex.ToPoint()));
				CObjectProp prop = CAbilityCreateShadow.SpawnShadow(tile2, null);
				tile2.SpawnProp(prop);
			}
		}
		cActor2.CheckForCachedValuesAfterActiveBonusesUpdate();
		if (livingActor != null)
		{
			CTile closestEmptyTile2 = ScenarioScript.GetClosestEmptyTile(livingActor);
			if (closestEmptyTile2 != null)
			{
				CustomAbility.PushActorTo(livingActor, closestEmptyTile2, tile, 0, choreographerWait: false);
			}
		}
		ApplyMirrorVFX(cActor2);
		return cActor2;
	}

	public override bool IsTeleportValid(CTile a, CTile b)
	{
		if (IsOnMirroredSide(a, b))
		{
			return false;
		}
		return base.IsTeleportValid(a, b);
	}

	private CTile FindMirrorTile(CTile tile)
	{
		if (tile == null)
		{
			return null;
		}
		return ScenarioManager.GetTile(GetMirrorPoint(tile.m_ArrayIndex));
	}

	public override void RemoveInvalidTilesForAbility(CAbility ability, List<CTile> tiles)
	{
		base.RemoveInvalidTilesForAbility(ability, tiles);
		switch (ability.AbilityType)
		{
		case CAbility.EAbilityType.Push:
		case CAbility.EAbilityType.Pull:
		{
			CActor cActor = null;
			if (ability is CAbilityPush cAbilityPush)
			{
				cActor = cAbilityPush.CurrentTarget;
			}
			else if (ability is CAbilityPush cAbilityPush2)
			{
				cActor = cAbilityPush2.CurrentTarget;
			}
			if (cActor == null || !cActor.HasMirrorActor())
			{
				return;
			}
			for (int num = tiles.Count - 1; num >= 0; num--)
			{
				CTile tile = tiles[num];
				CTile cTile = FindMirrorTile(tile);
				if (cTile == null || cTile.IsBlocked || (cTile.LivingActor != null && cTile.LivingActor != cActor.GetMirrorActor(includeMirroredClone: true)))
				{
					tiles.RemoveAt(num);
				}
			}
			return;
		}
		case CAbility.EAbilityType.Summon:
		case CAbility.EAbilityType.Teleport2:
		{
			for (int num3 = tiles.Count - 1; num3 >= 0; num3--)
			{
				CTile a = tiles[num3];
				if (IsOnMirroredSide(a, ability.TargetingActor.Tile))
				{
					tiles.RemoveAt(num3);
				}
			}
			if (!ability.TargetingActor.HasMirrorActor())
			{
				return;
			}
			for (int num4 = tiles.Count - 1; num4 >= 0; num4--)
			{
				CTile tile2 = tiles[num4];
				CTile cTile3 = FindMirrorTile(tile2);
				if (cTile3 == null || cTile3.IsBlocked || cTile3.LivingActor != null)
				{
					tiles.RemoveAt(num4);
				}
			}
			return;
		}
		case CAbility.EAbilityType.NullHex:
		{
			if (!ability.TargetingActor.HasMirrorActor())
			{
				return;
			}
			for (int num2 = tiles.Count - 1; num2 >= 0; num2--)
			{
				CTile cTile2 = tiles[num2];
				if (FindMirrorTile(cTile2) == null || IsOnMirroredSide(cTile2, ability.TargetingActor.Tile))
				{
					tiles.RemoveAt(num2);
				}
			}
			return;
		}
		}
		for (int num5 = tiles.Count - 1; num5 >= 0; num5--)
		{
			CTile a2 = tiles[num5];
			if (IsOnMirroredSide(a2, ability.TargetingActor.Tile))
			{
				tiles.RemoveAt(num5);
			}
		}
		if (!ability.TargetingActor.HasMirrorActor())
		{
			return;
		}
		for (int num6 = tiles.Count - 1; num6 >= 0; num6--)
		{
			CTile cTile4 = tiles[num6];
			CTile cTile5 = FindMirrorTile(cTile4);
			if (cTile5 == null || cTile5.IsBlocked || IsOnMirroredSide(cTile4, ability.TargetingActor.Tile))
			{
				tiles.RemoveAt(num6);
			}
		}
	}
}
