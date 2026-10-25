using System.Collections.Generic;
using System.Linq;

namespace ScenarioRuleLibrary.YML;

public class BaseStats
{
	public int Level { get; private set; }

	public int Health { get; set; }

	public int Move { get; set; }

	public int Attack { get; set; }

	public int Range { get; set; }

	public int Target { get; set; }

	public int Shield { get; set; }

	public int Retaliate { get; set; }

	public int RetaliateRange { get; set; }

	public int Pierce { get; set; }

	public int Push { get; set; }

	public bool Flying { get; private set; }

	public bool Advantage { get; private set; }

	public bool AttackersGainDisadvantage { get; private set; }

	public bool Invulnerable { get; set; }

	public bool PierceInvulnerablility { get; private set; }

	public bool Untargetable { get; private set; }

	public bool DoesNotblock { get; private set; }

	public bool IsWaterMonster { get; private set; }

	public bool IgnoreActorCollision { get; private set; }

	public List<CCondition.ENegativeCondition> Conditions { get; private set; }

	public List<CAbility.EAbilityType> Immunities { get; private set; }

	public Dictionary<CAbility.EAbilityType, int> SpecialBaseStats { get; private set; }

	public List<AbilityData.StatIsBasedOnXData> StatIsBasedOnXEntries { get; private set; }

	public List<CAbility> OnDeathAbilities { get; set; }

	public bool CanActivatePressurePlates { get; private set; }

	public bool ImmuneToForcedMovement { get; private set; }

	public List<AbilityData.StatIsBasedOnXData> AttackStatIsBasedOnXEntries => StatIsBasedOnXEntries?.Where((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Attack || x.BaseStatType == EMonsterBaseStats.Range || x.BaseStatType == EMonsterBaseStats.Target).ToList();

	public List<AbilityData.StatIsBasedOnXData> MoveStatIsBasedOnXEntries => StatIsBasedOnXEntries?.Where((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Move).ToList();

	public List<AbilityData.StatIsBasedOnXData> ShieldStatIsBasedOnXEntries => StatIsBasedOnXEntries?.Where((AbilityData.StatIsBasedOnXData x) => x.BaseStatType == EMonsterBaseStats.Shield).ToList();

	public BaseStats(int level, int health, int move, int attack, int range, int target, int shield, int retaliate, int retaliateRange, int pierce, int push, bool flying, bool advantage, bool attackersGainDisadvantage, bool invulnerable, bool pierceInvulnerablility, bool untargetable, bool doesNotBlock, bool isWaterMonster, bool ignoreActorCollision, List<CCondition.ENegativeCondition> conditions, List<CAbility.EAbilityType> immunities, Dictionary<CAbility.EAbilityType, int> specialBaseStats, List<AbilityData.StatIsBasedOnXData> statIsBasedOnXEntries, List<CAbility> onDeathAbilities, bool canActivatePressurePlates, bool immuneToForcedMovement)
	{
		Level = level;
		Health = health;
		Move = move;
		Attack = attack;
		Range = range;
		Target = target;
		Shield = shield;
		Retaliate = retaliate;
		RetaliateRange = retaliateRange;
		Pierce = pierce;
		Push = push;
		Flying = flying;
		Advantage = advantage;
		AttackersGainDisadvantage = attackersGainDisadvantage;
		Invulnerable = invulnerable;
		PierceInvulnerablility = pierceInvulnerablility;
		Untargetable = untargetable;
		DoesNotblock = doesNotBlock;
		IsWaterMonster = isWaterMonster;
		IgnoreActorCollision = ignoreActorCollision;
		Conditions = conditions;
		Immunities = immunities;
		SpecialBaseStats = specialBaseStats;
		StatIsBasedOnXEntries = statIsBasedOnXEntries;
		OnDeathAbilities = onDeathAbilities;
		CanActivatePressurePlates = canActivatePressurePlates;
		ImmuneToForcedMovement = immuneToForcedMovement;
	}

	public BaseStats Copy()
	{
		return new BaseStats(Level, Health, Move, Attack, Range, Target, Shield, Retaliate, RetaliateRange, Pierce, Push, Flying, Advantage, AttackersGainDisadvantage, Invulnerable, PierceInvulnerablility, Untargetable, DoesNotblock, IsWaterMonster, IgnoreActorCollision, Conditions.ToList(), Immunities.ToList(), new Dictionary<CAbility.EAbilityType, int>(SpecialBaseStats), StatIsBasedOnXEntries?.Select((AbilityData.StatIsBasedOnXData x) => x.Copy()).ToList(), OnDeathAbilities?.Select((CAbility x) => CAbility.CopyAbility(x, generateNewID: false, fullCopy: true)).ToList(), CanActivatePressurePlates, ImmuneToForcedMovement);
	}
}
