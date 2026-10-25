using System;

namespace ScenarioRuleLibrary;

public class CActionAugmentationOp
{
	public enum EActionAugmentationType
	{
		None,
		AbilityOverride,
		Ability
	}

	public static readonly EActionAugmentationType[] ActionAugmentationTypes = (EActionAugmentationType[])Enum.GetValues(typeof(EActionAugmentationType));

	public EActionAugmentationType Type { get; private set; }

	public string ParentAbilityName { get; private set; }

	public CAbility.EAbilityType ParentAbilityType { get; private set; }

	public CAbility Ability { get; private set; }

	public CAbilityOverride AbilityOverride { get; private set; }

	public CActionAugmentationOp(EActionAugmentationType type, string parentAbilityName, CAbility.EAbilityType parentAbilityType, CAbility ability, CAbilityOverride abilityOverride)
	{
		Type = type;
		ParentAbilityName = parentAbilityName;
		ParentAbilityType = parentAbilityType;
		Ability = ability;
		AbilityOverride = abilityOverride;
	}

	public CActionAugmentationOp Copy()
	{
		return new CActionAugmentationOp(Type, ParentAbilityName, ParentAbilityType, (Ability != null) ? CAbility.CopyAbility(Ability, generateNewID: false) : null, AbilityOverride);
	}
}
