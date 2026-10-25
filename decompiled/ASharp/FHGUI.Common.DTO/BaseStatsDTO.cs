using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;

namespace FHGUI.Common.DTO;

public struct BaseStatsDTO
{
	public int Health;

	public int MaxHealth;

	public int Movement;

	public int Attack;

	public int Range;

	public bool HasMovementStatIsBasedOnX;

	public bool HasAttackStatIsBasedOnX;

	public bool ShowRange;

	public float HealthPercent
	{
		get
		{
			if (MaxHealth != 0)
			{
				return (float)Health / (float)MaxHealth;
			}
			return 1f;
		}
	}

	public BaseStatsDTO(int health, int maxHealth, int movement, int attack, int range)
	{
		Health = health;
		MaxHealth = maxHealth;
		Movement = movement;
		Attack = attack;
		Range = range;
		HasAttackStatIsBasedOnX = false;
		HasMovementStatIsBasedOnX = false;
		ShowRange = range > 1;
	}

	public BaseStatsDTO(CEnemyActor actor)
		: this(actor.MonsterClass)
	{
		Health = actor.Health;
		MaxHealth = actor.MaxHealth;
		Attack = actor.AttackStrengthForUI;
	}

	public BaseStatsDTO(CMonsterClass monster)
	{
		Health = monster.Health();
		MaxHealth = monster.GetMonsterStatsData(monster.StatLevel).Health;
		Movement = monster.Move;
		Attack = monster.Attack;
		Range = monster.Range;
		ShowRange = false;
		GetStatIsBasedOnX(monster.StatIsBasedOnXEntries, out HasMovementStatIsBasedOnX, out HasAttackStatIsBasedOnX);
	}

	public BaseStatsDTO(MonsterYMLData monster, int level = 1)
		: this(monster.MonsterBaseStats[level - 1])
	{
	}

	public BaseStatsDTO(BaseStats stats)
		: this(stats.Health, stats.Health, stats.Move, stats.Attack, stats.Range)
	{
	}

	public BaseStatsDTO(CHeroSummonActor summon)
	{
		HeroSummonYMLData summonData = summon.SummonData;
		int num = 0;
		CAbilityAttack.GetAttackBonuses(summon, out var _, out var range, out var _, out var _, out var _);
		Health = summon.Health;
		MaxHealth = summon.MaxHealth;
		Movement = summonData.GetMoveFromActor(summon) + num + summon.GetEnhancementMoveStrength();
		Attack = summon.AttackStrength + summon.GetEnhancementAttackStrength();
		Range = summon.HeroSummonClass.SummonYML.Range + range + summon.GetEnhancementRangeStrength();
		ShowRange = Range > 1;
		GetStatIsBasedOnX(summonData.StatIsBasedOnXEntries, out HasMovementStatIsBasedOnX, out HasAttackStatIsBasedOnX);
	}

	public BaseStatsDTO(HeroSummonYMLData summonYmlData, int level = 1)
	{
		Health = summonYmlData.GetHealth(level);
		MaxHealth = Health;
		Attack = summonYmlData.Attack;
		Movement = summonYmlData.Move;
		Range = summonYmlData.Range;
		ShowRange = Range > 1;
		GetStatIsBasedOnX(summonYmlData.StatIsBasedOnXEntries, out HasMovementStatIsBasedOnX, out HasAttackStatIsBasedOnX);
	}

	private static void GetStatIsBasedOnX(IReadOnlyCollection<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries, out bool hasMovementStatIsBasedOnX, out bool hasAttackStatIsBasedOnX)
	{
		hasMovementStatIsBasedOnX = statIsBasedOnXEntries?.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.BaseStatType == EMonsterBaseStats.Move) ?? false;
		hasAttackStatIsBasedOnX = statIsBasedOnXEntries?.Any((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength && x.BaseStatType == EMonsterBaseStats.Attack) ?? false;
	}
}
