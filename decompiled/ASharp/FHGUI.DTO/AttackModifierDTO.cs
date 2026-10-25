using System.Linq;
using FHGUI.Common;
using FHRL;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.DTO;

public class AttackModifierDTO
{
	public string ID;

	public string DescriptionLocKey;

	public Sprite BaseIcon;

	public string LocalizedValue;

	public int Value;

	public Color ValueColor;

	public Sprite ValueIcon;

	public bool IsBasic;

	public bool IsPremadeIcon;

	public bool Rolling;

	public bool Shuffle;

	public bool IsPersistent;

	public bool IsSuccess;

	public bool IsFailure;

	public bool IsModified;

	public int Count;

	public int CountChange;

	public OutpostAttackState.DrawnCardUsageSummary.CardUsageState Usage;

	public AttackModifierDTO()
	{
	}

	public AttackModifierDTO(CharacterAttackModifierDef def, int count)
	{
		ID = def.ID.ID;
		DescriptionLocKey = def.DescriptionLocKey;
		IsBasic = def.Basic;
		Rolling = def.Rolling;
		Shuffle = def.Shuffle;
		IsPersistent = def.Persistent;
		Count = count;
		BaseIcon = UIInfoTools.Instance.GetPremadeAttackModifierIcon(def);
		if (BaseIcon != null)
		{
			IsPremadeIcon = true;
			return;
		}
		ValueColor = UIInfoTools.Instance.GetAttackModifierColor(def.MathModifier);
		LocalizedValue = UIInfoTools.Instance.GetAttackModifierText(def);
		ValueIcon = UIInfoTools.Instance.GetAttackModifierIcon(def);
		BaseIcon = UIInfoTools.Instance.GetAttackModifierBaseIcon(def);
	}

	public AttackModifierDTO(TownGuardAttackModifierDef def, int count, int change = 0)
	{
		ID = def.ID.ID;
		DescriptionLocKey = def.DescriptionLocKey;
		IsBasic = def.Basic;
		Rolling = def.Rolling;
		Count = count;
		CountChange = change;
		BaseIcon = UIInfoTools.Instance.GetTownGuardAttackModifierValueIcon(def);
		IsPremadeIcon = true;
		IsSuccess = def.Type == TownGuardModType.Success;
		IsFailure = def.Type == TownGuardModType.Failure;
		Value = def.Value;
	}

	public AttackModifierDTO(AttackModifierYMLData modifier, int availableCounters, int usedCounters)
	{
		BaseIcon = UIInfoTools.Instance.GetPremadeAttackModifierIcon(modifier);
		bool flag = BaseIcon != null;
		if (!flag)
		{
			BaseIcon = UIInfoTools.Instance.PerksUIData.BaseModifierIcons.FirstOrDefault((PerksUIDataSO.PerkValueTypeIcon p) => p.Condition == modifier.MathModifier).Icon;
			ValueIcon = UIInfoTools.Instance.GetAttackModifierIconNew(modifier);
		}
		else
		{
			ValueIcon = null;
			Value = 0;
		}
		IsModified = modifier.OriginalModifier != null && modifier.OriginalModifier != modifier.MathModifier;
		Count = availableCounters;
		Rolling = modifier.Rolling && !flag;
		Shuffle = modifier.Shuffle;
		IsPersistent = modifier.IsPersistent;
		IsBasic = UICharacterUtils.IsBasicModifier(modifier);
		ID = modifier.Name;
		DescriptionLocKey = modifier.Name;
		Count = availableCounters;
		LocalizedValue = UIInfoTools.Instance.GetAttackModifierText(modifier);
		if (BaseIcon != null)
		{
			IsPremadeIcon = true;
		}
		else
		{
			ValueColor = UIInfoTools.Instance.GetAttackModifierColor(modifier.MathModifier);
		}
	}

	public AttackModifierDTO Clone()
	{
		return (AttackModifierDTO)MemberwiseClone();
	}

	public override string ToString()
	{
		return $"AttackModifier ID: {ID}, Count {Count}";
	}
}
