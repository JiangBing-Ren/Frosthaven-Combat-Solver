using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScenarioRuleLibrary;

[JsonObject(MemberSerialization.OptIn)]
public class CAugment
{
	public enum EAugmentType
	{
		None,
		Ability,
		Override,
		Bonus
	}

	public static readonly EAugmentType[] AugmentTypes = (EAugmentType[])Enum.GetValues(typeof(EAugmentType));

	public Guid ID;

	[JsonProperty]
	public EAugmentType AugmentType;

	[JsonProperty]
	public CAbility.EAttackType AttackType;

	[JsonProperty]
	public List<CAbility> Abilities;

	[JsonProperty]
	public List<CAbilityOverride> AbilityOverrides;

	public string ActiveBonusLayout;

	public CBaseCard RegisteredBaseCard { get; set; }

	public CAugment()
	{
	}

	public CAugment(List<CAbility> abilities, CAbility.EAttackType type, bool augBonus = false)
	{
		ID = Guid.NewGuid();
		AttackType = type;
		if (augBonus)
		{
			AugmentType = EAugmentType.Bonus;
		}
		else
		{
			AugmentType = EAugmentType.Ability;
		}
		Abilities = abilities;
	}

	public CAugment(List<CAbilityOverride> abilityOverrides, CAbility.EAttackType type, bool augBonus = false)
	{
		ID = Guid.NewGuid();
		AugmentType = EAugmentType.Override;
		AbilityOverrides = abilityOverrides;
		AttackType = type;
	}

	public CAugment(CAbility.EAttackType type)
	{
		ID = Guid.NewGuid();
		AugmentType = EAugmentType.Bonus;
		AttackType = type;
	}
}
