using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;

namespace FHGUI.Common;

public static class UIPropUtils
{
	public static int? GetTrapDamage(CObjectTrap trap)
	{
		if (!trap.Damage)
		{
			return null;
		}
		if (trap.Owner != null || trap.HasCustomDamageSet)
		{
			return trap.DamageValue;
		}
		return ScenarioManager.TrapDamage;
	}

	public static IEnumerable<StatusEffectDTO> GetTrapConditions(CObjectTrap trap)
	{
		return GetTrapNegativeConditions(trap).Concat(GetTrapPositiveConditions(trap));
	}

	public static IEnumerable<StatusEffectDTO> GetTrapPositiveConditions(CObjectTrap trap)
	{
		if (trap.PositiveConditions == null)
		{
			yield break;
		}
		foreach (CCondition.EPositiveCondition positiveCondition in trap.PositiveConditions)
		{
			yield return new StatusEffectDTO(positiveCondition);
		}
	}

	public static IEnumerable<StatusEffectDTO> GetTrapNegativeConditions(CObjectTrap trap)
	{
		if (trap.Conditions == null)
		{
			yield break;
		}
		foreach (CCondition.ENegativeCondition condition in trap.Conditions)
		{
			yield return new StatusEffectDTO(condition);
		}
	}

	public static StatusEffectDTO? GetOwnerEffect(CObjectHazardousTerrain hazardousTerrain)
	{
		CActor owner = hazardousTerrain.Owner;
		if (owner?.CharacterClass == null)
		{
			return null;
		}
		ECharacter character = CrossReference.ClassIdToECharacterModel(owner.CharacterClass.CharacterID);
		CharacterConfigUI characterConfigUI = UIInfoTools.Instance.GetCharacterConfigUI(character);
		if (characterConfigUI == null)
		{
			return null;
		}
		return new StatusEffectDTO
		{
			Name = LocalizationManager.GetTranslation("HAZARDOUS_TERRAIN_TOOLTIP"),
			Description = FHUILocFormatting.GetPropCreateBy(owner.CharacterClass),
			Icon = characterConfigUI.iconClass
		};
	}
}
