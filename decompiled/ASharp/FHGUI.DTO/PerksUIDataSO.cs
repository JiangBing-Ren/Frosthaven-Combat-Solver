using System;
using System.Collections.Generic;
using FHGUI.Configs;
using UnityEngine;

namespace FHGUI.DTO;

[CreateAssetMenu(fileName = "PerksUIDataSO", menuName = "Def/PerksUIDataSO")]
public class PerksUIDataSO : ScriptableObject
{
	[Serializable]
	public struct PerkValueTypeIcon
	{
		public string Condition;

		[ConfigIcon(48, true)]
		public Sprite Icon;
	}

	public const string TOWN_GUARD_PREFIX = "TownGuard";

	public PerkConfigUI[] PerkConfigs;

	public GameObject TooltipPrefab;

	public PerkValueTypeIcon[] BaseModifierIcons;

	public PerkValueTypeIcon[] PremadeModifierIcons;

	public PerkValueTypeIcon[] TownGuardModifierIcons;

	public PerkValueTypeIcon[] ValueTypeIcons;

	private static bool TryGetIconByCondition(IEnumerable<PerkValueTypeIcon> collection, string condition, out Sprite icon)
	{
		icon = null;
		foreach (PerkValueTypeIcon item in collection)
		{
			if (item.Condition.Equals(condition, StringComparison.InvariantCulture))
			{
				icon = item.Icon;
			}
		}
		return icon != null;
	}

	public Sprite GetValueIcon(string condition, Sprite fallbackIcon = null)
	{
		if (!TryGetIconByCondition(ValueTypeIcons, condition, out var icon))
		{
			return fallbackIcon;
		}
		return icon;
	}

	public Sprite GetAttackModifierBaseIcon(string mathModifier)
	{
		if (!TryGetIconByCondition(BaseModifierIcons, mathModifier, out var icon))
		{
			return null;
		}
		return icon;
	}

	public Sprite GetPremadeAttackModifierIcon(string id)
	{
		if (!TryGetIconByCondition(PremadeModifierIcons, id, out var icon))
		{
			return null;
		}
		return icon;
	}

	public Sprite GetTownGuardModifierBaseIcon(string id)
	{
		if (!TryGetIconByCondition(TownGuardModifierIcons, id, out var icon))
		{
			return null;
		}
		return icon;
	}
}
