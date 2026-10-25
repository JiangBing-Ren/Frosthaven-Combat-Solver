using System;
using System.Collections.Generic;
using System.Linq;
using AStar;
using Newtonsoft.Json;
using ScenarioRuleLibrary.YML;

namespace ScenarioRuleLibrary;

public class ScenarioScript_FH097 : ScenarioScript
{
	public class Storage97
	{
		public Dictionary<string, bool> MonsterClassActivatedPressurePlates;

		public Dictionary<string, bool> NextRoundMonsterClassActivatedPressurePlates;
	}

	private enum SEARCH_TILE_MODE
	{
		FURTHEST,
		NEAREST
	}

	private static readonly List<int> BossSpecial2CardsIds = new List<int> { 8822000, 8822001, 8822002 };

	private static readonly Dictionary<string, Point> MonsterClassesPressurePlatesDictionary = new Dictionary<string, Point>
	{
		{
			"RoboticBoltshooterID",
			new Point(2, 5)
		},
		{
			"SteelAutomatonID",
			new Point(1, 15)
		},
		{
			"RuinedMachineID",
			new Point(5, 5)
		},
		{
			"FlamingBladespinnerID",
			new Point(6, 15)
		}
	};

	private const string TurnBackToEnemyBonusName = "ATurnBackToEnemy";

	private Storage97 serializedProperties;

	private CActor _programDirector;

	private IEnumerable<CEnemyActor> ControlledMonsters => ScenarioManager.Scenario.AllyMonsters.Where((CEnemyActor x) => x.FindApplicableActiveBonuses().Any((CActiveBonus cActiveBonus) => cActiveBonus.Ability.Name == "ATurnBackToEnemy"));

	private IEnumerable<CEnemyActor> NonControlledMonsters => ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x != _programDirector);

	private bool BossTreatsControlledMonstersAsAllies
	{
		get
		{
			if (GameState.TurnActor != _programDirector)
			{
				CActor programDirector = _programDirector;
				if (programDirector == null || !programDirector.PlayedThisRound)
				{
					return false;
				}
			}
			return _programDirector?.MonsterClass?.RoundAbilityCard?.Action?.Abilities?.Any((CAbility x) => x.Flags.HasFlag(CAbility.EAbilityFlag.Special2)) == true;
		}
	}

	private Dictionary<string, bool> MonsterClassActivatedPressurePlates
	{
		get
		{
			return serializedProperties.MonsterClassActivatedPressurePlates;
		}
		set
		{
			serializedProperties.MonsterClassActivatedPressurePlates = value;
		}
	}

	private Dictionary<string, bool> NextRoundMonsterClassActivatedPressurePlates
	{
		get
		{
			return serializedProperties.NextRoundMonsterClassActivatedPressurePlates;
		}
		set
		{
			serializedProperties.NextRoundMonsterClassActivatedPressurePlates = value;
		}
	}

	private int TrapDamage => ScenarioManager.TrapDamage;

	protected override string SerializeInternal()
	{
		return JsonConvert.SerializeObject(serializedProperties);
	}

	protected override void DeserializeInternal(string storage)
	{
		base.DeserializeInternal(storage);
		if (!string.IsNullOrEmpty(storage))
		{
			serializedProperties = JsonConvert.DeserializeObject<Storage97>(storage);
		}
		UpdatePressurePlateVFX(updateState: true);
	}

	public override bool ShouldSortModifierTextsInOrderOfModifiers()
	{
		return true;
	}

	public override bool UseCustomAreActorsAllied()
	{
		return true;
	}

	public override bool UseCustomAreActorsEnemies()
	{
		return true;
	}

	private bool HasPressurePlate(CMonsterClass monsterClass)
	{
		return GetPressurePlatesDictionaryKey(monsterClass) != null;
	}

	private string GetPressurePlatesDictionaryKey(CMonsterClass monsterClass)
	{
		return MonsterClassesPressurePlatesDictionary.Keys.FirstOrDefault((string mc) => monsterClass.ID.StartsWith(mc));
	}

	protected override void TickPhaseInternal()
	{
		switch (_scenarioModifier.ScenarioModifierTriggerPhase)
		{
		case EScenarioModifierTriggerPhase.EndTurn:
			TickEndTurn();
			break;
		case EScenarioModifierTriggerPhase.StartRound:
			TickStartRound();
			break;
		default:
			_ = 8;
			break;
		case EScenarioModifierTriggerPhase.EndRound:
		case EScenarioModifierTriggerPhase.StartTurn:
		case EScenarioModifierTriggerPhase.StartScenario:
			break;
		}
	}

	protected override bool Initialize()
	{
		serializedProperties = new Storage97();
		_programDirector = ScenarioManager.CurrentScenarioState.Monsters.Find((EnemyState m) => m.ClassID == "ProgramDirectorID").Actor;
		MonsterClassActivatedPressurePlates = InitializeMonstersPressurePlatesDictionary();
		NextRoundMonsterClassActivatedPressurePlates = InitializeMonstersPressurePlatesDictionary();
		return true;
	}

	private void TickStartRound()
	{
		if (OncePerRound())
		{
			MonsterClassActivatedPressurePlates = NextRoundMonsterClassActivatedPressurePlates;
			NextRoundMonsterClassActivatedPressurePlates = InitializeMonstersPressurePlatesDictionary();
		}
	}

	protected override void TickAttackSummaryCreatedInternal(CAttackSummary.TargetSummary summary)
	{
		base.TickAttackSummaryCreatedInternal(summary);
		GiveProgramDirectorWard(summary);
	}

	private void GiveProgramDirectorWard(CAttackSummary.TargetSummary summary)
	{
		if (summary.ActorToAttack == _programDirector && ControlledMonsters.Contains(summary.DamageSourceActor))
		{
			summary.PostShieldScalarModifiers.Add(("*2", "x2"));
		}
	}

	private void ExecuteSpecial1Teleport()
	{
		CActor cActor = SearchActorWithEmptyAdjacentTile(SEARCH_TILE_MODE.FURTHEST, out var tile);
		if (cActor != null && !ScenarioManager.IsTileAdjacent(_programDirector.Tile, cActor.Tile))
		{
			TeleportActor(_programDirector, tile);
		}
	}

	private void ExecuteSpecial2Teleport()
	{
		CActor cActor = SearchActorWithEmptyAdjacentTile(SEARCH_TILE_MODE.NEAREST, out var tile);
		if (cActor != null && !ScenarioManager.IsTileAdjacent(_programDirector.Tile, cActor.Tile))
		{
			TeleportActor(_programDirector, tile);
		}
	}

	public CTile GetSpecial1TeleportTile()
	{
		CActor cActor = SearchActorWithEmptyAdjacentTile(SEARCH_TILE_MODE.FURTHEST, out var tile);
		if (cActor == null || ScenarioManager.IsTileAdjacent(_programDirector.Tile, cActor.Tile))
		{
			return null;
		}
		return tile;
	}

	public CTile GetSpecial2TeleportTile()
	{
		CActor cActor = SearchActorWithEmptyAdjacentTile(SEARCH_TILE_MODE.NEAREST, out var tile);
		if (cActor == null || ScenarioManager.IsTileAdjacent(_programDirector.Tile, cActor.Tile))
		{
			return null;
		}
		return tile;
	}

	private void ExecuteSpecial1PressurePlatesDamage()
	{
		foreach (Point value in MonsterClassesPressurePlatesDictionary.Values)
		{
			CActor livingActor = ScenarioManager.GetTile(value).LivingActor;
			if (livingActor is CPlayerActor)
			{
				CustomAbility.PlayVFX("EMPWave1HEX", livingActor.ArrayIndex);
				DealScenarioDamage(livingActor, TrapDamage, isTrapDamage: false, isTerrainDamage: false, _programDirector.ActorLocKey());
			}
		}
	}

	private CActor SearchActorWithEmptyAdjacentTile(SEARCH_TILE_MODE searchTileMode, out CTile tile)
	{
		CActor cActor = null;
		int num = ((searchTileMode != SEARCH_TILE_MODE.FURTHEST) ? int.MaxValue : 0);
		List<CTile> list = new List<CTile>();
		tile = null;
		foreach (CActor item in ScenarioManager.Scenario.AllAliveActors.Where((CActor actor) => CustomAreActorsEnemies(_programDirector.Type, actor.Type, _programDirector, actor)))
		{
			Point point = new Point(item.Tile);
			int num2 = Point.HexDist(point, _programDirector.ArrayIndex);
			if ((searchTileMode != SEARCH_TILE_MODE.FURTHEST || num2 >= num) && (searchTileMode != SEARCH_TILE_MODE.NEAREST || num2 <= num))
			{
				GameState.GetAdjacentTiles(point, list);
				if (GetNearestFreeTile(_programDirector.ArrayIndex.X, _programDirector.ArrayIndex.Y, out var nearestTile, list) && (cActor == null || num2 != num || item.Initiative() <= cActor.Initiative()))
				{
					cActor = item;
					num = num2;
					tile = nearestTile;
				}
			}
		}
		return cActor;
	}

	private Dictionary<string, bool> InitializeMonstersPressurePlatesDictionary()
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		foreach (string key in MonsterClassesPressurePlatesDictionary.Keys)
		{
			dictionary[key] = false;
		}
		return dictionary;
	}

	public static void SetNextCard(CAbility ability)
	{
		CPlayerActor obj = ability.TargetingActor as CPlayerActor;
		string monsterClassKey = ability.GetMiscVar("MonsterClassID").GetString();
		CMonsterClass monsterClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.NonEliteVariant == null && x.ID.StartsWith(monsterClassKey, StringComparison.InvariantCultureIgnoreCase));
		CActiveBonus cActiveBonus = obj.FindApplicableActiveBonuses().SingleOrDefault((CActiveBonus x) => x.Ability.Name == "AChooseAndPlayMonsterCardActive");
		if (cActiveBonus != null)
		{
			if ((cActiveBonus.Ability as CAbilityAddActiveBonus)?.AddAbility is CAbilityChooseAbility cAbilityChooseAbility)
			{
				CMonsterAbilityCard cMonsterAbilityCard = monsterClass.AbilityCardsPool.FirstOrDefault((CMonsterAbilityCard x) => x.ID.ToString().Equals(ability.Name, StringComparison.InvariantCultureIgnoreCase));
				foreach (CAbility chooseAbility in cAbilityChooseAbility.ChooseAbilities)
				{
					int id = chooseAbility.GetMiscVar("CardIdToShow").GetInt();
					CMonsterAbilityCard cMonsterAbilityCard2 = monsterClass.AbilityCardsPool.FirstOrDefault((CMonsterAbilityCard x) => x.ID == id);
					if (cMonsterAbilityCard2 == null)
					{
						continue;
					}
					if (cMonsterAbilityCard2 == cMonsterAbilityCard)
					{
						monsterClass.AbilityCards.Remove(cMonsterAbilityCard2);
						monsterClass.AbilityCards.Insert(0, cMonsterAbilityCard2);
						foreach (CEnemyActor item in ScenarioManager.Scenario.AllAliveMonsters.Where((CEnemyActor x) => x.MonsterClass == monsterClass || x.MonsterClass?.NonEliteVariant == monsterClass))
						{
							CustomAbility.CreateShowNextCardBonus(item);
						}
					}
					else
					{
						monsterClass.AbilityCards.Remove(cMonsterAbilityCard2);
						monsterClass.DiscardedAbilityCards.Add(cMonsterAbilityCard2);
					}
				}
			}
			cActiveBonus.Finish();
		}
		PrepForAllegianceChange(monsterClass);
	}

	public static void PrepForAllegianceChange(CMonsterClass monsterClass)
	{
		List<CEnemyActor> list = ScenarioManager.Scenario.Enemies.Where((CEnemyActor x) => x.MonsterClass == monsterClass || x.MonsterClass?.CardsPoolClass == monsterClass).ToList();
		list.Sort(GameState.CompareActorsByGUID);
		CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.ChangeAllegiance, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, new CAbilityChangeAllegiance.ChangeAllegianceAbilityData(CActor.EType.Ally), null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
		cAbility.MiscAbilityData = new AbilityData.MiscAbilityData
		{
			IgnoreStun = true
		};
		CAbility cAbility2 = CAbility.CreateAbility(CAbility.EAbilityType.ChangeAllegiance, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, null, null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, new CAbilityChangeAllegiance.ChangeAllegianceAbilityData(CActor.EType.Enemy), null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
		cAbility2.MiscAbilityData = new AbilityData.MiscAbilityData
		{
			IgnoreStun = true
		};
		cAbility2.SkipAnim = true;
		CAbilityAddActiveBonus cAbilityAddActiveBonus = CAbility.CreateAbility(CAbility.EAbilityType.AddActiveBonus, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData
		{
			IgnoreStun = true
		}, activeBonusData: new AbilityData.ActiveBonusData
		{
			Behaviour = CActiveBonus.EActiveBonusBehaviourType.EndRoundAbility,
			Duration = CActiveBonus.EActiveBonusDurationType.Persistent,
			Hidden = true,
			Tracker = new List<int> { 0 }
		}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: "ATurnBackToEnemy", abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: false, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue) as CAbilityAddActiveBonus;
		cAbilityAddActiveBonus.AddAbility = cAbility2;
		ScenarioScript_FH097 scenarioScript_FH = ScenarioScript.current as ScenarioScript_FH097;
		foreach (CEnemyActor item in list)
		{
			cAbility.Start(item, item);
			cAbility.ApplyToActor(item);
			CustomAbility.SetFocusOverride(item, scenarioScript_FH._programDirector, ignoreHazards: false);
			cAbilityAddActiveBonus = CAbility.CopyAbility(cAbilityAddActiveBonus, generateNewID: true) as CAbilityAddActiveBonus;
			cAbilityAddActiveBonus.Start(item, item);
			cAbilityAddActiveBonus.ApplyToActor(item);
		}
	}

	public override bool CustomAreActorsAllied(CActor.EType actor1Type, CActor.EType actor2Type, CActor actor1 = null, CActor actor2 = null)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (BossTreatsControlledMonstersAsAllies)
			{
				IEnumerable<CEnemyActor> controlledMonsters = ControlledMonsters;
				if (controlledMonsters.Contains(actor1) && actor2 == _programDirector)
				{
					return true;
				}
				if (controlledMonsters.Contains(actor2) && actor1 == _programDirector)
				{
					return true;
				}
			}
			return base.CustomAreActorsAllied(actor1Type, actor2Type, actor1, actor2);
		}
	}

	public override bool CustomAreActorsEnemies(CActor.EType actor1Type, CActor.EType actor2Type, CActor actor1 = null, CActor actor2 = null)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if (BossTreatsControlledMonstersAsAllies)
			{
				IEnumerable<CEnemyActor> controlledMonsters = ControlledMonsters;
				if (controlledMonsters.Contains(actor1) && actor2 == _programDirector)
				{
					return false;
				}
				if (controlledMonsters.Contains(actor2) && actor1 == _programDirector)
				{
					return false;
				}
			}
			return base.CustomAreActorsEnemies(actor1Type, actor2Type, actor1, actor2);
		}
	}

	private void TickEndTurn()
	{
		CActor tickActor = _tickActor;
		CPlayerActor playerActor = tickActor as CPlayerActor;
		if (playerActor == null || !MonsterClassesPressurePlatesDictionary.Values.Contains(playerActor.Tile.m_ArrayIndex))
		{
			return;
		}
		if (playerActor.Tile.m_Props.FirstOrDefault((CObjectProp x) => x is CObjectPressurePlate) == null)
		{
			return;
		}
		string monsterClassKey = MonsterClassesPressurePlatesDictionary.First((KeyValuePair<string, Point> x) => x.Value == playerActor.Tile.m_ArrayIndex).Key;
		NextRoundMonsterClassActivatedPressurePlates[monsterClassKey] = true;
		List<int> list = new List<int>();
		CMonsterClass cMonsterClass = MonsterClassManager.Classes.Find((CMonsterClass x) => x.NonEliteVariant == null && x.ID.StartsWith(monsterClassKey, StringComparison.InvariantCultureIgnoreCase));
		List<CMonsterAbilityCard> list2 = cMonsterClass.AbilityCards.ToList();
		list2.Shuffle(ScenarioManager.CurrentScenarioState.ScenarioRNG);
		list.AddRange(from x in list2.Take(2)
			select x.ID);
		CAbilityAddActiveBonus cAbilityAddActiveBonus = CAbility.CreateAbility(CAbility.EAbilityType.AddActiveBonus, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, null, null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, miscAbilityData: new AbilityData.MiscAbilityData(), activeBonusData: new AbilityData.ActiveBonusData
		{
			Behaviour = CActiveBonus.EActiveBonusBehaviourType.StartRoundAbility,
			Duration = CActiveBonus.EActiveBonusDurationType.Persistent,
			ListLayoutOverride = ("$ScenarioAbility_FH097_ChooseAndPlayMonsterCard$", "MindControl")
		}, activeBonusYML: null, areaEffectYMLString: null, areaEffectLayoutOverrideYMLString: null, areaEffectFlipAxis: null, name: "AChooseAndPlayMonsterCardActive", abilityEnhancements: null, positiveConditions: null, negativeConditions: null, multiPassAttack: false, chainAttack: false, chainAttackRange: 0, chainAttackDamageReduction: 0, damageSelfBeforeAttack: 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, text: null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, summons: null, xpPerTargetData: null, targeting: CAbility.EAbilityTargeting.All, targetingVars: null, vars: null, performVars: null, elementsToInfuse: null, elementalConditions: null, isMonster: false, propName: null, trapData: null, isSubAbility: false, isInlineSubAbility: false, pierce: 0, retaliateRange: 0, isTargetedAbility: false, spawnDelay: 0f, pullType: CAbilityPull.EPullType.None, additionalPushEffect: CAbilityPush.EAdditionalPushEffect.None, additionalPushEffectDamage: 0, additionalPushEffectXP: 0, lootData: null, conditionalOverrides: null, startAbilityConditions: null, abilityXP: 0, parentName: null, skipAnim: false, conditionDuration: 0, conditionDecTrigger: EConditionDecTrigger.None, augment: null, song: null, doom: null, controlActorData: null, changeAllegianceData: null, attackEffects: null, targetActorWithTrapEffects: false, targetActorWithTrapEffectRange: 0, isMergedAbility: false, mergedAbilities: null, statIsBasedOnXEntries: null, moveObstacleRange: 0, moveObstacleAnimOverload: null, slotsToRefresh: null, slotStatesToRefresh: null, addActiveBonusAbility: null, overrideAugmentAttackType: CAbility.EAttackType.None, chooseAbilities: null, recoverCardsAbilityTypeFilter: null, cardAction: CAbility.ECardAction.None, forgoTopActionAbility: null, forgoBottomActionAbility: null, extraTurnType: CAbilityExtraTurn.EExtraTurnType.None, actionMonsterClassID: null, swapAbilityFirstTargetFilter: null, swapAbilitySecondTargetFilter: null, supplyCardsToGive: null, consumes: null, teleportData: null, immunityToAbilityTypes: null, immuneToAttackTypes: null, immuneToSelfDamage: false, addOrRemoveModifiers: null, healData: null, resourcesToAddOnAbilityEnd: null, resourcesToTakeFromTargets: null, resourcesToGiveToTargets: null, destroyObstacleData: null, changeCharacterModel: ECharacter.None, tileFilter: CAbilityFilter.EFilterTile.None, disableCardActionData: null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, parentBaseCard: null, previewEffectId: null, previewEffectText: null, canUndoOverride: null, exploitVulnerability: false, helpBoxTooltipLocKey: null, extraTurnAbilityTypeFilter: null, maxExtraTurnAbilityLevel: int.MaxValue) as CAbilityAddActiveBonus;
		if (list.Count > 0)
		{
			List<CAbility> list3 = new List<CAbility>();
			foreach (int cardID in list)
			{
				CMonsterAbilityCard cMonsterAbilityCard = cMonsterClass.AbilityCards.Find((CMonsterAbilityCard x) => x.ID == cardID);
				CAbility cAbility = CAbility.CreateAbility(CAbility.EAbilityType.Null, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, cMonsterAbilityCard.ID.ToString(), null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, null, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, cMonsterAbilityCard.Name, null, exploitVulnerability: false, null, null, int.MaxValue);
				if (cAbility.MiscAbilityData == null)
				{
					cAbility.MiscAbilityData = new AbilityData.MiscAbilityData();
				}
				cAbility.MiscAbilityData.AutotriggerAbility = true;
				cAbility.MiscAbilityData.IgnoreStun = true;
				cAbility.PerformVars = new List<(string, List<(string, TValue)>)> { ("OnAbilityEnded", new List<(string, TValue)> { ("SetCard", new TValue(CExpression.Parse("ScenarioScript_FH097.SetNextCard(obj)"))) }) };
				cAbility.SetMiscVar("CardIdToShow", cMonsterAbilityCard.ID);
				cAbility.SetMiscVar("MonsterClassID", monsterClassKey);
				list3.Add(cAbility);
			}
			CAbility cAbility2 = CAbility.CreateAbility(CAbility.EAbilityType.Choose, 1, useSpecialBaseStat: false, 1, 1, 0, CAbility.EAbilityFlag.None, new CAbilityFilterContainer(CAbilityFilter.EFilterTargetType.Self), null, null, null, null, attackSourcesOnly: false, jump: false, fly: false, ignoreDifficultTerrain: false, ignoreHazardousTerrain: false, ignoreBlockedTileMoveCost: false, carryOtherActorsOnHex: false, null, CAbilityMove.EMoveRestrictionType.None, null, null, null, null, null, "ScenarioScript_FH097_ChooseMonsterAbility", null, null, null, multiPassAttack: false, chainAttack: false, 0, 0, 0, addAttackBaseStat: false, strengthIsBase: false, rangeIsBase: false, targetIsBase: false, null, textOnly: false, noLayoutSpace: false, showRange: false, showTarget: false, showArea: false, onDeath: false, isConsumeAbility: false, allTargetsOnMovePath: false, allTargetsOnMovePathSameStartAndEnd: false, allTargetsOnAttackPath: false, allTargetsInRangeAlongPath: false, null, null, CAbility.EAbilityTargeting.All, null, null, null, null, null, isMonster: false, null, null, isSubAbility: false, isInlineSubAbility: false, 0, 0, isTargetedAbility: false, 0f, CAbilityPull.EPullType.None, CAbilityPush.EAdditionalPushEffect.None, 0, 0, null, null, null, 0, null, null, skipAnim: false, 0, EConditionDecTrigger.None, null, null, null, null, null, null, targetActorWithTrapEffects: false, 0, isMergedAbility: false, null, null, 0, null, null, null, null, CAbility.EAttackType.None, list3, null, CAbility.ECardAction.None, null, null, CAbilityExtraTurn.EExtraTurnType.None, null, null, null, null, null, null, null, null, immuneToSelfDamage: false, null, null, null, null, null, null, ECharacter.None, CAbilityFilter.EFilterTile.None, null, isDefault: false, isItemAbility: false, showElementPicker: false, isModifierAbility: false, null, null, null, null, exploitVulnerability: false, null, null, int.MaxValue);
			cAbility2.MiscAbilityData = new AbilityData.MiscAbilityData
			{
				HasCondition = false,
				CanSkip = false,
				CanUndo = false,
				IgnoreStun = true
			};
			cAbility2.SetMiscVar("MonsterClassID", monsterClassKey);
			cAbility2.SetMiscVar("ShowMonsterCards", true);
			cAbilityAddActiveBonus.AddAbility = cAbility2;
		}
		else
		{
			CAbility cAbility3 = CAbilityNullTargeting.CreateDefaultNullTargeting(1, 1, isMonster: false);
			cAbility3.MiscAbilityData = new AbilityData.MiscAbilityData
			{
				HasCondition = false,
				CanSkip = false,
				CanUndo = false,
				AutotriggerAbility = true,
				IgnoreStun = true
			};
			cAbility3.SkipAnim = true;
			cAbility3.SetMiscVar("MonsterClassID", monsterClassKey);
			cAbility3.SetMiscVar("ShowMonsterCards", true);
			cAbility3.PerformVars = new List<(string, List<(string, TValue)>)> { ("OnAbilityEnded", new List<(string, TValue)> { ("SetCard", new TValue(CExpression.Parse("ScenarioScript_FH097.SetNextCard(obj)"))) }) };
			cAbilityAddActiveBonus.AddAbility = cAbility3;
		}
		cAbilityAddActiveBonus.Start(playerActor, playerActor);
		cAbilityAddActiveBonus.ApplyToActor(playerActor);
	}

	protected override void TickAbilityStartedInternal()
	{
		base.TickAbilityStartedInternal();
		if (!_tickAbility.Flags.HasFlag(CAbility.EAbilityFlag.Special2))
		{
			return;
		}
		List<CTile> list = new List<CTile>();
		foreach (CEnemyActor controlledMonster in ControlledMonsters)
		{
			list.Add(controlledMonster.Tile);
			CustomAbility.SetFocusOverride(controlledMonster, null, true);
		}
		if (list.Count > 0)
		{
			CPlayCharacterAnimation_MessageData message = new CPlayCharacterAnimation_MessageData("Reprogram", _programDirector)
			{
				m_TargetedTilesForVFX = list,
				m_BlockMessageProcessingDuringAnimation = true
			};
			ScenarioRuleClient.MessageHandler(message);
		}
	}

	private void UpdatePressurePlateVFX(bool updateState)
	{
		foreach (KeyValuePair<string, Point> item in MonsterClassesPressurePlatesDictionary)
		{
			CTile tile = ScenarioManager.GetTile(item.Value);
			CObjectProp cObjectProp = tile.m_Props.FirstOrDefault((CObjectProp x) => x.PrefabName == EVFXPropType.RotatingMonsterProjection.ToString());
			if (tile.LivingActor == null && tile.m_Props.Any((CObjectProp x) => x is CObjectPressurePlate))
			{
				bool flag = cObjectProp == null;
				if (cObjectProp == null)
				{
					cObjectProp = SpawnVFXProp(tile, EVFXPropType.RotatingMonsterProjection.ToString());
				}
				if (flag | updateState)
				{
					CChangePropVisualState_MessageData message = new CChangePropVisualState_MessageData
					{
						m_prop = cObjectProp,
						m_stateName = "State_" + item.Key
					};
					ScenarioRuleClient.MessageHandler(message);
				}
			}
			else
			{
				cObjectProp?.DestroyProp();
			}
		}
	}

	protected override void TickRepositionInternal()
	{
		base.TickRepositionInternal();
		UpdatePressurePlateVFX(updateState: false);
	}

	protected override void TickDeathInternal()
	{
		base.TickDeathInternal();
		if (MonsterClassesPressurePlatesDictionary.Any((KeyValuePair<string, Point> x) => x.Value == _tickActor.ArrayIndex))
		{
			UpdatePressurePlateVFX(updateState: false);
		}
	}
}
