using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH118 : ScenarioScript
{
	[Serializable]
	private struct SerializationData
	{
		public int LootedAmuletsOfProtection;

		public int LootedAmuletsOfReanimation;

		public string LastTurnActorGUID;

		public bool LivingBonesResurrected;
	}

	private const string LIVING_BONES_ID = "LivingBonesID";

	private const string LIVING_BONES_ELITE_ID = "LivingBonesEliteID";

	private const string FROZEN_CORPSE_ID = "FrozenCorpseID";

	private const string FROZEN_CORPSE_ELITE_ID = "FrozenCorpseEliteID";

	private const string AMULET_OF_PROTECTION = "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_1";

	private const string AMULET_OF_PROTECTION_DESC = "FH/FH_118_Amulet_of_Protection_D";

	private const string AMULET_OF_REANIMATION = "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2";

	private const string AMULET_OF_REANIMATION_DESC = "FH/FH_118_Amulet_of_Reanimation_D";

	private readonly List<Point> _initialAmuletOfProtectionLocations = new List<Point>
	{
		new Point(2, 8),
		new Point(11, 10),
		new Point(11, 7),
		new Point(4, 4),
		new Point(10, 8),
		new Point(9, 11),
		new Point(2, 11)
	};

	private readonly List<Point> _initialAmuletOfReanimationLocations = new List<Point>
	{
		new Point(12, 9),
		new Point(1, 9),
		new Point(3, 11),
		new Point(2, 3),
		new Point(3, 1)
	};

	private readonly Dictionary<Point, CScenarioModifier> _amuletModifiersByLocation = new Dictionary<Point, CScenarioModifier>();

	private CObjective_Scripted _lootAmuletsObjective;

	private CScenarioModifier _amuletOfProtectionScenarioModifier;

	private CScenarioModifier _amuletOfReanimationScenarioModifier;

	private CScenarioModifier _lootAmuletScenarioModifier;

	[StateDump(Skip = true)]
	private readonly List<CObjectProp> _amuletsBuffer = new List<CObjectProp>();

	[StateDump(Skip = true)]
	private readonly HashSet<CObjectProp> _amuletsBeforeLoot = new HashSet<CObjectProp>();

	[StateDump(Skip = true)]
	private readonly HashSet<CObjectProp> _amuletsAfterLoot = new HashSet<CObjectProp>();

	private int _lootedAmuletsOfProtection;

	private int _lootedAmuletsOfReanimation;

	private CActor LastTurnActor;

	private bool LivingBonesResurrected;

	private int MaxAmuletsOfReanimationCount => ScenarioScript.PartySize + 1;

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	protected override bool Initialize()
	{
		_lootAmuletsObjective = GetObjective<CObjective_Scripted>("LootAmulets");
		_lootAmuletsObjective.Current = _lootedAmuletsOfReanimation;
		_lootAmuletsObjective.Total = MaxAmuletsOfReanimationCount;
		_amuletOfProtectionScenarioModifier = GetScenarioModifier("AmuletOfProtection");
		_amuletOfReanimationScenarioModifier = GetScenarioModifier("AmuletOfReanimation");
		_lootAmuletScenarioModifier = GetScenarioModifier("LootAmulet");
		foreach (Point initialAmuletOfProtectionLocation in _initialAmuletOfProtectionLocations)
		{
			_amuletModifiersByLocation.Add(initialAmuletOfProtectionLocation, _amuletOfProtectionScenarioModifier);
		}
		foreach (Point initialAmuletOfReanimationLocation in _initialAmuletOfReanimationLocations)
		{
			_amuletModifiersByLocation.Add(initialAmuletOfReanimationLocation, _amuletOfReanimationScenarioModifier);
		}
		return true;
	}

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(new SerializationData
		{
			LootedAmuletsOfProtection = _lootedAmuletsOfProtection,
			LootedAmuletsOfReanimation = _lootedAmuletsOfReanimation,
			LastTurnActorGUID = LastTurnActor?.ActorGuid,
			LivingBonesResurrected = LivingBonesResurrected
		});
	}

	protected override void DeserializeInternal(string storage)
	{
		foreach (CEnemyActor enemy in ScenarioManager.Scenario.Enemies)
		{
			if (enemy.Deactivated)
			{
				SetActorActive(enemy, active: false);
			}
		}
		if (!string.IsNullOrEmpty(storage))
		{
			SerializationData serializationData = JsonConvert.DeserializeObject<SerializationData>(storage);
			_lootedAmuletsOfProtection = serializationData.LootedAmuletsOfProtection;
			_lootedAmuletsOfReanimation = serializationData.LootedAmuletsOfReanimation;
			LivingBonesResurrected = serializationData.LivingBonesResurrected;
			LastTurnActor = (string.IsNullOrEmpty(serializationData.LastTurnActorGUID) ? null : ScenarioManager.FindActor(serializationData.LastTurnActorGUID));
			_lootAmuletsObjective.Current = _lootedAmuletsOfReanimation;
			_lootAmuletsObjective.Total = MaxAmuletsOfReanimationCount;
		}
	}

	protected override void TickPhaseInternal()
	{
		EScenarioModifierTriggerPhase scenarioModifierTriggerPhase = _scenarioModifier.ScenarioModifierTriggerPhase;
		switch (scenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.StartScenario:
			TickOnActorSpawnedOrRevealed();
			break;
		case EScenarioModifierTriggerPhase.OnActorSpawnedOrRevealed:
			TickOnActorSpawnedOrRevealed();
			break;
		case EScenarioModifierTriggerPhase.StartTurn:
			TickStartTurn();
			break;
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		case EScenarioModifierTriggerPhase.EndRound:
			TickEndRound();
			break;
		case EScenarioModifierTriggerPhase.AfterSelectMonsterClassAbilityCards:
			TickAfterSelectMonsterClassAbilityCards();
			break;
		default:
			Error($"Unknown phase {scenarioModifierTriggerPhase}", null, 148);
			break;
		}
	}

	protected override void TickDeathInternal()
	{
		CActor tickActor = _tickActor;
		if (!IsLivingBonesOrFrozenCorpseActor(tickActor) || tickActor.Deactivated)
		{
			return;
		}
		CActiveBonus amuletActiveBonus = GetAmuletActiveBonus(tickActor);
		if (amuletActiveBonus == null)
		{
			return;
		}
		string amulet = "";
		if (amuletActiveBonus.Ability.ActiveBonusData.GetMiscVar("Protection").GetBool())
		{
			Log.GameplayInfo($"ScenarioScript_FH118::  {tickActor} died with amulet of protection");
			amulet = "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_1";
		}
		else if (amuletActiveBonus.Ability.ActiveBonusData.GetMiscVar("Reanimation").GetBool())
		{
			Log.GameplayInfo($"ScenarioScript_FH118::  {tickActor} died with amulet of reanimation");
			amulet = "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2";
			if (SpawnActorAtTile(tickActor.ArrayIndex, "LivingBonesID", out var actor))
			{
				GetAmuletActiveBonus(actor)?.Finish();
				SetActorActive(actor, active: false);
				actor.MarkWontPlayThisRound(tickActor.PlayedThisRound || GameState.TurnActor == tickActor);
				GameState.SortIntoInitiativeAndIDOrder();
				Log.GameplayInfo($"ScenarioScript_FH118::  spawned {actor} due to amulet of reanimation after death of {tickActor}");
			}
		}
		SpawnAmulet(_tickActor.ArrayIndex, amulet);
	}

	protected override void TickAbilityStartedInternal()
	{
		if (_tickAbility is CAbilityLoot cAbilityLoot && cAbilityLoot.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			GetDroppedAmulets(_amuletsBeforeLoot);
		}
	}

	protected override void TickAbilityEndedInternal()
	{
		if (_tickAbility is CAbilityNullTargeting cAbilityNullTargeting && cAbilityNullTargeting.GetMiscVar("IsLootAmuletAbility").GetBool() && cAbilityNullTargeting.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA && cAbilityNullTargeting.ActorsTargeted.Count > 0)
		{
			CActiveBonus amuletActiveBonus = GetAmuletActiveBonus(cAbilityNullTargeting.ActorsTargeted[0]);
			if (amuletActiveBonus != null)
			{
				string lootedAmuletType = "";
				if (amuletActiveBonus.Ability.ActiveBonusData.GetMiscVar("Protection").GetBool())
				{
					lootedAmuletType = "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_1";
				}
				else if (amuletActiveBonus.Ability.ActiveBonusData.GetMiscVar("Reanimation").GetBool())
				{
					lootedAmuletType = "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2";
				}
				UpdateLootedAmulets(lootedAmuletType);
				amuletActiveBonus.Finish();
			}
		}
		if (_tickAbility is CAbilityLoot cAbilityLoot && cAbilityLoot.ActiveBonusData.Duration == CActiveBonus.EActiveBonusDurationType.NA)
		{
			GetDroppedAmulets(_amuletsAfterLoot);
			List<CObjectProp> lootedAmulets = FindLootedAmulets(_amuletsBeforeLoot, _amuletsAfterLoot);
			UpdateLootedAmulets(lootedAmulets);
		}
	}

	private void TickOnActorSpawnedOrRevealed()
	{
		TryDisableGoldDrop(_tickActor);
		TryApplyAmuletBonusToActor(_tickActor);
	}

	private void TickStartTurn()
	{
		CActor tickActor = _tickActor;
		if (IsLivingBones(tickActor) && !IsLivingBones(LastTurnActor))
		{
			RessurectAllLivingBones();
		}
		LastTurnActor = tickActor;
	}

	private void RessurectAllLivingBones()
	{
		if (LivingBonesResurrected)
		{
			return;
		}
		LivingBonesResurrected = true;
		foreach (CEnemyActor allAliveMonster in ScenarioManager.Scenario.AllAliveMonsters)
		{
			if (allAliveMonster.Deactivated && IsLivingBones(allAliveMonster))
			{
				RessurectLivingBones(allAliveMonster);
			}
		}
	}

	private void RessurectLivingBones(CActor actor)
	{
		Point? point = null;
		Point origin = actor.ArrayIndex;
		List<CActor> list = ScenarioManager.Scenario.FindActorsAt(origin);
		if (list.Count == 1 && list[0] == actor)
		{
			point = origin;
			GameState.TeleportActor(actor, ScenarioManager.GetTile(origin));
			SetActorActive(actor, active: true);
		}
		else
		{
			List<CTile> tilesInRange = GameState.GetTilesInRange(origin, 1);
			if (GetNearestFreeTile(origin.X, origin.Y, out var nearestTile, tilesInRange))
			{
				point = nearestTile.m_ArrayIndex;
			}
		}
		if (point.HasValue)
		{
			GameState.TeleportActor(actor, ScenarioManager.GetTile(point.Value));
			SetActorActive(actor, active: true);
		}
		GetDroppedAmulets(_amuletsBuffer, (CObjectProp amulet) => amulet.ArrayIndex.ToPoint() == origin);
		foreach (CObjectProp item in _amuletsBuffer)
		{
			if (item.PropCustomLocKey == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2")
			{
				item.DestroyProp();
				ApplyAmuletBonusToActor(actor, _amuletOfReanimationScenarioModifier);
				break;
			}
		}
	}

	private void TickEndTurn()
	{
		if (_tickActor.Type == CActor.EType.Player)
		{
			GetDroppedAmulets(_amuletsBuffer, (CObjectProp amulet) => amulet.ArrayIndex.ToPoint() == _tickActor.ArrayIndex);
			UpdateLootedAmulets(_amuletsBuffer);
		}
	}

	private void TickStartRound()
	{
		LastTurnActor = null;
		LivingBonesResurrected = false;
	}

	private void TickEndRound()
	{
	}

	private void TickAfterSelectMonsterClassAbilityCards()
	{
	}

	private void SetActorActive(CActor actor, bool active)
	{
		actor.Deactivated = !active;
		actor.SkipTurnWhenDeactivated = active;
		CSetActiveActorHealthBar_MessageData message = new CSetActiveActorHealthBar_MessageData(actor, active);
		ScenarioRuleClient.MessageHandler(message);
		CSetActiveActorVisuals_MessageData message2 = new CSetActiveActorVisuals_MessageData(actor, active);
		ScenarioRuleClient.MessageHandler(message2);
	}

	private void UpdateLootedAmulets(string lootedAmuletType)
	{
		if (lootedAmuletType == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_1")
		{
			_lootedAmuletsOfProtection++;
		}
		else if (lootedAmuletType == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2")
		{
			_lootedAmuletsOfReanimation++;
		}
		UpdateAmuletsObjective();
	}

	private void UpdateLootedAmulets(List<CObjectProp> lootedAmulets)
	{
		foreach (CObjectProp lootedAmulet in lootedAmulets)
		{
			if (lootedAmulet.PropCustomLocKey == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_1")
			{
				_lootedAmuletsOfProtection++;
			}
			else
			{
				if (!(lootedAmulet.PropCustomLocKey == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2"))
				{
					continue;
				}
				_lootedAmuletsOfReanimation++;
				foreach (CEnemyActor item in GetDeactivatedLivingBonesActorsAt(lootedAmulet.ArrayIndex.ToPoint()))
				{
					GameState.KillActor(item, item, CActor.ECauseOfDeath.Undetermined, out var _);
				}
			}
		}
		UpdateAmuletsObjective();
	}

	private void UpdateAmuletsObjective()
	{
		_lootAmuletsObjective.Current = _lootedAmuletsOfReanimation;
		_lootAmuletsObjective.Total = MaxAmuletsOfReanimationCount;
		_lootAmuletsObjective.ScriptedIsComplete = _lootAmuletsObjective.Current >= _lootAmuletsObjective.Total;
		UpdateObjectiveProgress();
	}

	private bool IsPropAmulet(CObjectProp prop)
	{
		if (!(prop.PropCustomLocKey == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_1"))
		{
			return prop.PropCustomLocKey == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2";
		}
		return true;
	}

	private void GetDroppedAmulets(ICollection<CObjectProp> outAmulets, Func<CObjectProp, bool> predicate = null)
	{
		outAmulets.Clear();
		foreach (CObjectProp prop in ScenarioManager.CurrentScenarioState.Props)
		{
			if ((predicate == null || predicate(prop)) && IsPropAmulet(prop))
			{
				outAmulets.Add(prop);
			}
		}
	}

	private List<CObjectProp> FindLootedAmulets(ICollection<CObjectProp> amuletsBeforeLoot, ICollection<CObjectProp> amuletsAfterLoot)
	{
		List<CObjectProp> list = new List<CObjectProp>();
		foreach (CObjectProp item in amuletsBeforeLoot)
		{
			if (!amuletsAfterLoot.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private void SpawnAmulet(Point arrayIndex, string amulet)
	{
		CTile tile = ScenarioManager.GetTile(arrayIndex);
		string propCustomDescriptionLocKey = null;
		string name = "GoalChest";
		if (amulet == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_2")
		{
			name = "GoalChestAmuletReanimation118";
			propCustomDescriptionLocKey = "FH/FH_118_Amulet_of_Reanimation_D";
		}
		else if (amulet == "FH/FH_Scenario_Campaign_118_SCENARIO_OBJECT_1")
		{
			name = "GoalChestAmuletProtection118";
			propCustomDescriptionLocKey = "FH/FH_118_Amulet_of_Protection_D";
		}
		CObjectChest cObjectChest = new CObjectChest(name, ScenarioManager.ObjectImportType.GoalChest, arrayIndex.ToTileIndex(), null, null, null, tile.m_HexMap.MapGuid);
		cObjectChest.PropCustomLocKey = amulet;
		cObjectChest.PropCustomDescriptionLocKey = propCustomDescriptionLocKey;
		tile.SpawnProp(cObjectChest);
		Log.GameplayInfo($"ScenarioScript_FH118:: spawned {amulet} prop at {arrayIndex}");
	}

	private bool IsLivingBonesOrFrozenCorpseActor(CActor actor)
	{
		if (!IsLivingBones(actor))
		{
			return IsFrozenCorpse(actor);
		}
		return true;
	}

	private bool IsLivingBones(CActor actor)
	{
		if (actor == null)
		{
			return false;
		}
		if (!(actor.Class.ID == "LivingBonesID"))
		{
			return actor.Class.ID == "LivingBonesEliteID";
		}
		return true;
	}

	private bool IsFrozenCorpse(CActor actor)
	{
		if (actor == null)
		{
			return false;
		}
		if (!(actor.Class.ID == "FrozenCorpseID"))
		{
			return actor.Class.ID == "FrozenCorpseEliteID";
		}
		return true;
	}

	private List<CEnemyActor> GetDeactivatedLivingBonesActorsAt(Point point)
	{
		return ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.Deactivated && point == x.ArrayIndex && (x.Class.ID == "LivingBonesID" || x.Class.ID == "LivingBonesEliteID")).ToList();
	}

	private bool TryDisableGoldDrop(CActor actor)
	{
		if (IsLivingBonesOrFrozenCorpseActor(actor))
		{
			actor.NoGoldDrop = true;
			return true;
		}
		return false;
	}

	private bool TryApplyAmuletBonusToActor(CActor actor)
	{
		if (GetAmuletActiveBonus(actor) != null)
		{
			return false;
		}
		if (IsLivingBonesOrFrozenCorpseActor(actor))
		{
			if (_amuletModifiersByLocation.TryGetValue(actor.ArrayIndex, out var amuletModifier))
			{
				CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == amuletModifier.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
				cAbility.IsScenarioModifierAbility = true;
				cAbility.Start(actor, actor);
				cAbility.ApplyToActor(actor);
				return true;
			}
		}
		return false;
	}

	private void ApplyAmuletBonusToActor(CActor actor, CScenarioModifier amuletModifier)
	{
		CAbility cAbility = CAbility.CopyAbility(ScenarioRuleClient.SRLYML.ScenarioAbilities.FirstOrDefault((ScenarioAbilitiesYMLData a) => a.ScenarioAbilityID == amuletModifier.ScenarioAbilityID)?.ScenarioAbilities[0], generateNewID: false);
		cAbility.IsScenarioModifierAbility = true;
		cAbility.Start(actor, actor);
		cAbility.ApplyToActor(actor);
	}

	public override void ApplyReward(bool won)
	{
		base.ApplyReward(won);
		if (!won)
		{
			return;
		}
		int num = _lootedAmuletsOfProtection + _lootedAmuletsOfReanimation;
		int num2 = (int)Math.Ceiling((float)num / (float)ScenarioManager.Scenario.InitialPlayers.Count);
		foreach (CPlayerActor allPlayer in ScenarioManager.Scenario.AllPlayers)
		{
			if (num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					allPlayer.GainLoot(endOfScenario: true, incLootTokenCount: false, i == 0);
				}
				if (!allPlayer.IsDead)
				{
					allPlayer.ShowLastLootReward("", "MoneyToken", null, focusCamera: true);
				}
			}
			num -= num2;
			num2 = Math.Min(num2, num);
		}
	}

	public static bool HasLootableEnemiesInRange(CAbility ability)
	{
		if (ability is CAbilityLoot { TilesInRange: var tilesInRange })
		{
			return tilesInRange.Any((CTile tile) => tile.LivingActors.Any((CActor actor) => GetAmuletActiveBonus(actor) != null));
		}
		return false;
	}

	public static CActiveBonus GetAmuletActiveBonus(CActor actor)
	{
		return actor.ActiveBonuses.FirstOrDefault((CActiveBonus ab) => ab.Ability.ActiveBonusData.GetMiscVar("Amulet").GetBool());
	}
}
