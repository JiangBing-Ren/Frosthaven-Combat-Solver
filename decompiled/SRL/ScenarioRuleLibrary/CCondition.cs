using System;
using System.Linq;
using FH.Tools;

namespace ScenarioRuleLibrary;

public class CCondition
{
	[Serializable]
	[KronosEnumPolice(0)]
	public enum ENegativeCondition
	{
		NA = 0,
		Poison = 1,
		Wound = 2,
		Immobilize = 4,
		Disarm = 8,
		Stun = 0x10,
		Muddle = 0x20,
		Curse = 0x40,
		Disadvantage = 0x80,
		StopFlying = 0x100,
		Sleep = 0x200,
		Bane = 0x400,
		Brittle = 0x800,
		Impair = 0x1000
	}

	[Serializable]
	[KronosEnumPolice(0)]
	public enum EPositiveCondition
	{
		NA = 0,
		Invisible = 1,
		Strengthen = 2,
		Bless = 4,
		Advantage = 8,
		Immovable = 0x10,
		Regenerate = 0x20,
		Ward = 0x40,
		Mutate = 0x80
	}

	public static readonly ENegativeCondition[] NegativeConditions = (ENegativeCondition[])Enum.GetValues(typeof(ENegativeCondition));

	public static readonly EPositiveCondition[] PositiveConditions = (EPositiveCondition[])Enum.GetValues(typeof(EPositiveCondition));

	public static EPositiveCondition PositiveConditionFromAbility(CAbility.EAbilityType type)
	{
		return PositiveConditions.SingleOrDefault((EPositiveCondition x) => x.ToString() == type.ToString());
	}

	public static ENegativeCondition NegativeConditionFromAbility(CAbility.EAbilityType type)
	{
		return NegativeConditions.SingleOrDefault((ENegativeCondition x) => x.ToString() == type.ToString());
	}

	public static CAbility.EAbilityType AbilityTypeFromPositiveCondition(EPositiveCondition type)
	{
		return CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == type.ToString());
	}

	public static CAbility.EAbilityType AbilityTypeFromNegativeCondition(ENegativeCondition type)
	{
		return CAbility.AbilityTypes.SingleOrDefault((CAbility.EAbilityType x) => x.ToString() == type.ToString());
	}
}
