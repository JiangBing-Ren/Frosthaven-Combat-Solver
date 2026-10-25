using System.Linq;
using FHGUI.DTO;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.Common;

public struct AttackModifierInfo
{
	public Sprite Icon;

	public Sprite ValueIcon;

	public string Value;

	public string MathModifier;

	public int AvailableCounters;

	public int UsedCounters;

	public bool Rolling;

	public bool Shuffle;

	public bool Persistent;

	public bool IsBasicModifier;

	public string DescriptionLocKey;

	public AttackModifierInfo(AttackModifierYMLData modifier, int availableCounters, int usedCounters)
	{
		Icon = UIInfoTools.Instance.GetPremadeAttackModifierIcon(modifier);
		bool flag = Icon != null;
		if (!flag)
		{
			Icon = UIInfoTools.Instance.PerksUIData.BaseModifierIcons.FirstOrDefault((PerksUIDataSO.PerkValueTypeIcon p) => p.Condition == modifier.MathModifier).Icon;
			ValueIcon = UIInfoTools.Instance.GetAttackModifierIconNew(modifier);
			Value = UIInfoTools.Instance.GetAttackModifierText(modifier);
		}
		else
		{
			ValueIcon = null;
			Value = string.Empty;
		}
		MathModifier = modifier.MathModifier;
		AvailableCounters = availableCounters;
		UsedCounters = usedCounters;
		Rolling = modifier.Rolling && !flag;
		Shuffle = modifier.Shuffle;
		Persistent = modifier.IsPersistent;
		IsBasicModifier = UICharacterUtils.IsBasicModifier(modifier);
		DescriptionLocKey = modifier.Name;
	}

	public AttackModifierInfo(AttackModifierDTO modifier)
	{
		AttackModifierYMLData attackModifierYMLData = CrossReference.AttackModifierDefToYML(modifier);
		Icon = modifier.BaseIcon;
		ValueIcon = modifier.ValueIcon;
		Value = modifier.LocalizedValue;
		MathModifier = attackModifierYMLData.MathModifier;
		AvailableCounters = modifier.Count;
		UsedCounters = 0;
		Rolling = modifier.Rolling && !modifier.IsPremadeIcon;
		Shuffle = modifier.Shuffle;
		Persistent = modifier.IsPersistent;
		IsBasicModifier = modifier.IsBasic;
		DescriptionLocKey = modifier.DescriptionLocKey;
	}
}
