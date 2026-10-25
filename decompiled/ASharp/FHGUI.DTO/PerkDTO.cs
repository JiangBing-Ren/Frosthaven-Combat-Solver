using System.Collections.Generic;
using System.Linq;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.DTO;

public class PerkDTO
{
	public enum ActionType
	{
		Learn,
		Improve,
		Forget
	}

	public string ID;

	public Sprite Icon;

	public AttackModifierDTO DisplayModifier;

	public List<string> ModifiersToAdd;

	public List<string> ModifiersToRemove;

	public bool HasAbility;

	public string NameKey;

	public string NameLocalized;

	public string DescriptionKey;

	public string DescriptionLocalized;

	public string CostKey;

	public int UnlockedCount;

	public int MaxCount;

	public bool Affordable;

	public int Cost;

	public bool IsTactical => HasAbility;

	public bool IsComplete => UnlockedCount == MaxCount;

	public PerkDTO()
	{
	}

	public PerkDTO(PerkDef def)
	{
		ID = def.ID.ID;
		NameKey = def.Name;
		DescriptionKey = def.Description;
		NameLocalized = LocalizationManager.GetTranslation(NameKey);
		DescriptionLocalized = LocalizationManager.GetTranslation(DescriptionKey);
		MaxCount = def.Available;
		ModifiersToAdd = def.ModifiersToAdd.Select((CharacterAttackModifierID id) => id.ID).ToList();
		ModifiersToRemove = def.ModifiersToRemove.Select((CharacterAttackModifierID id) => id.ID).ToList();
		Icon = UIInfoTools.Instance.GetPerkIcon(def);
		Cost = def.Cost;
	}

	public PerkDTO(TownGuardPerkDef def)
	{
		uint iD = def.ID.ID;
		string text = iD.ToString();
		string text2 = "TOWNGUARD_PERK_" + text;
		string text3 = text2 + "_TITLE";
		string text4 = text2 + "_DESC";
		ID = text;
		NameKey = text3;
		DescriptionKey = text4;
		NameLocalized = LocalizationManager.GetTranslation(text3);
		DescriptionLocalized = LocalizationManager.GetTranslation(text4);
		ModifiersToAdd = (from id in def.TownGuardModsToAdd()
			select id.ID).ToList();
		ModifiersToRemove = (from id in def.TownGuardModsToRemove()
			select id.ID).ToList();
		Icon = UIInfoTools.Instance.GetPerkIcon(def);
		Cost = 1;
	}

	public PerkDTO(PerksYMLData perk)
	{
		List<AttackModifierYMLData> attackModifiers = ScenarioRuleClient.SRLYML.AttackModifiers;
		ID = perk.ID;
		NameKey = perk.Name;
		DescriptionKey = perk.Description;
		NameLocalized = LocalizationManager.GetTranslation(NameKey);
		DescriptionLocalized = LocalizationManager.GetTranslation(DescriptionKey);
		MaxCount = 1;
		ModifiersToAdd = (from data in perk.CardsToAdd
			where attackModifiers.Any((AttackModifierYMLData mod) => mod.Name == data.Name)
			select data.Name).ToList();
		ModifiersToRemove = (from data in perk.CardsToRemove
			where attackModifiers.Any((AttackModifierYMLData mod) => mod.Name == data.Name)
			select data.Name).ToList();
		Icon = UIInfoTools.Instance.GetPerkIcon(perk.ID);
		Cost = perk.Cost;
	}

	public PerkDTO(CharacterPerk perk)
		: this(ScenarioRuleClient.SRLYML.Perks.FirstOrDefault((PerksYMLData p) => p.ID == perk.PerkID))
	{
	}

	public ActionType GetActionType()
	{
		if (HasAbility)
		{
			return ActionType.Learn;
		}
		bool flag = ModifiersToAdd != null && ModifiersToAdd.Count > 0;
		bool flag2 = ModifiersToRemove != null && ModifiersToRemove.Count > 0;
		if (flag & flag2)
		{
			return ActionType.Improve;
		}
		if (flag)
		{
			return ActionType.Learn;
		}
		return ActionType.Forget;
	}

	public PerkDTO Clone()
	{
		return (PerkDTO)MemberwiseClone();
	}

	public override string ToString()
	{
		return "ID: " + ID + ", Name " + NameKey;
	}
}
