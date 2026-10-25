using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;

namespace FHGUI.Common;

public static class ActiveBonusUIUtils
{
	public const string OVERRIDE_TARGET_DESC_KEY = "OverrideTargetBonusDesc";

	public const string OVERRIDE_TARGET_NAME_KEY = "OverrideTargetBonusName";

	public const string OVERRIDE_TARGET_ICON_KEY = "OverrideTargetBonusIcon";

	public static string GetActiveBonusName(CActiveBonus activeBonus, CActor contextActor = null, bool forceGeneration = false)
	{
		CheckForLayoutToGenerate(activeBonus, out var title);
		if (activeBonus.Layout == null || (activeBonus.Ability.ActiveBonusData.Hidden && !forceGeneration))
		{
			return string.Empty;
		}
		if (contextActor != null && ((activeBonus.IsAura && activeBonus.Actor != contextActor) || (activeBonus.Actor == contextActor && activeBonus.Caster != activeBonus.Actor)))
		{
			TValue miscVar = activeBonus.Ability.ActiveBonusData.GetMiscVar("OverrideTargetBonusName");
			if (miscVar.isString)
			{
				return CreateLayout.LocaliseText(miscVar.GetString());
			}
		}
		if (title.IsNullOrEmpty() && activeBonus.Ability.ActiveBonusData?.GetMiscVar("OverrideLayoutName") != TValue.Unknown && activeBonus.Ability.ActiveBonusData != null)
		{
			title = CreateLayout.LocaliseText(activeBonus.Ability.ActiveBonusData?.GetMiscVar("OverrideLayoutName").GetString(), skipWarnings: false, activeBonus.Ability.Strength.ToString());
		}
		if (title.IsNullOrEmpty() && activeBonus.Layout.CardName.IsNOTNullOrEmpty())
		{
			if (LocalizationManager.TryGetTranslation(activeBonus.Layout.CardName, out var translation))
			{
				title = translation;
			}
			else if (activeBonus.Layout.CardName.Contains('$'))
			{
				int strength = activeBonus.Ability.Strength;
				title = CreateLayout.LocaliseText(activeBonus.Layout.CardName, skipWarnings: false, strength.ToString());
			}
		}
		if (!title.IsNullOrEmpty() || activeBonus.BaseCard == null)
		{
			return title;
		}
		if (LocalizationManager.TryGetTranslation(activeBonus.BaseCard.Name, out var translation2))
		{
			title = translation2;
		}
		if (title.IsNullOrEmpty() && activeBonus.Caster != null)
		{
			return UICharacterUtils.GetName(activeBonus.Caster);
		}
		return title;
	}

	public static Sprite GetActiveBonusIcon(CActiveBonus activeBonus, CActor contextActor = null)
	{
		if (activeBonus.Layout == null)
		{
			return null;
		}
		if (contextActor != null && activeBonus.Actor != contextActor && activeBonus.IsAura)
		{
			TValue miscVar = activeBonus.Ability.ActiveBonusData.GetMiscVar("OverrideTargetBonusIcon");
			if (miscVar.isString)
			{
				return UIInfoTools.Instance.GetActiveAbilityIcon(miscVar.GetString());
			}
		}
		if (activeBonus.Layout.IconNames.Any())
		{
			return UIInfoTools.Instance.GetActiveAbilityIcon(activeBonus.Layout.IconNames[0]);
		}
		string text = activeBonus.Ability.ActiveBonusData.GetMiscVar("OverrideIcon").GetString();
		if (!string.IsNullOrEmpty(text))
		{
			return UIInfoTools.Instance.GetActiveAbilityIcon(text);
		}
		if (activeBonus.BaseCard is CItem cItem)
		{
			return UIInfoTools.Instance.GetItemMiniSprite(cItem.YMLData.Art);
		}
		if (activeBonus.Ability.PerkID.IsNOTNullOrEmpty())
		{
			return UIInfoTools.Instance.GetPerkIcon(activeBonus.Ability.PerkID);
		}
		if (activeBonus.IsAura && activeBonus.Actor is CPlayerActor cPlayerActor)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor.CharacterClass.ID, cPlayerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		}
		if (activeBonus.Caster is CPlayerActor cPlayerActor2)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor2.Class.ID, cPlayerActor2.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		if (activeBonus.Actor is CPlayerActor cPlayerActor3)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cPlayerActor3.Class.ID, cPlayerActor3?.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		if (activeBonus.Actor is CHeroSummonActor cHeroSummonActor)
		{
			return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(cHeroSummonActor.Summoner.Class.ID, cHeroSummonActor.Summoner?.CharacterClass?.CharacterYML.CustomCharacterConfig);
		}
		return UIInfoTools.Instance.GetCharacterActiveAbilityIcon(activeBonus.Actor.Class.ID);
	}

	public static string GetActiveBonusDescription(CActiveBonus activeBonus, CActor contextActor = null)
	{
		string empty = string.Empty;
		if (activeBonus?.Layout == null)
		{
			return empty;
		}
		if (contextActor != null && activeBonus.Actor != contextActor)
		{
			TValue miscVar = activeBonus.Ability.ActiveBonusData.GetMiscVar("OverrideTargetBonusDesc");
			if (!miscVar.isString)
			{
				return CreateLayout.LocaliseText(ActivePropertyLookup(activeBonus, LocalisationAndPropertyLookup(activeBonus, activeBonus.Layout.ListLayouts.FirstOrDefault())));
			}
			return CreateLayout.LocaliseText(miscVar.GetString());
		}
		return ActivePropertyLookup(activeBonus, LocalisationAndPropertyLookup(activeBonus, activeBonus.Layout.ListLayouts.FirstOrDefault()));
	}

	public static string LocalisationAndPropertyLookup(CActiveBonus activeBonus, string desc)
	{
		if (desc == null)
		{
			desc = string.Empty;
		}
		desc = CreateLayout.LocaliseText(desc);
		desc = (desc.Contains('*'.ToString()) ? CardLayoutRow.ReplaceKey(desc, '*', activeBonus.Ability.Strength.ToString()) : desc);
		return desc;
	}

	public static string ActivePropertyLookup(CActiveBonus activeBonus, string text)
	{
		while (text.Contains('^'.ToString()))
		{
			string text2 = CardLayoutRow.GetKey(text, '^').Replace(" ", "");
			int num = 0;
			if (text2 == activeBonus.Ability.Name)
			{
				num += activeBonus.Ability.Strength;
			}
			text = CardLayoutRow.ReplaceKey(text, '^', num.ToString());
		}
		return text;
	}

	public static void CheckForLayoutToGenerate(CActiveBonus activeBonus, out string title, CActor contextActor = null)
	{
		title = string.Empty;
		if ((activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1 != null && activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item2 != null) || activeBonus.Layout == null || (activeBonus.Layout.Layout == null && activeBonus.IsDoom) || activeBonus.Ability.ActiveBonusData.GroupID != null || activeBonus.Ability.AbilityType == CAbility.EAbilityType.ShowNextAttackModifier)
		{
			DiscardType discard = DiscardType.Discard;
			if (activeBonus.BaseCard is CAbilityCard cAbilityCard)
			{
				switch (cAbilityCard.GetAbilityActionType(activeBonus.Ability))
				{
				case CBaseCard.ActionType.TopAction:
					discard = cAbilityCard.GetAbilityCardYML.TopDiscardType;
					break;
				case CBaseCard.ActionType.BottomAction:
					discard = cAbilityCard.GetAbilityCardYML.BottomDiscardType;
					break;
				}
			}
			activeBonus.Layout = new ActiveBonusLayout(string.Empty, new List<string>(), null, null, new List<string>(), null, activeBonus.Ability.ActiveBonusData.Tracker, discard);
			if (activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1 != null && activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item2 != null)
			{
				if (activeBonus.Ability.ActiveBonusData.GroupID != null && activeBonus.Actor != null)
				{
					int num = 0;
					foreach (CActiveBonus item4 in (from x in CActiveBonus.FindApplicableActiveBonuses(activeBonus.Actor)
						where x?.Ability?.ActiveBonusData?.GroupID == activeBonus.Ability.ActiveBonusData.GroupID
						select x).ToList())
					{
						AbilityData.StatIsBasedOnXData statIsBasedOnXData = item4.Ability.StatIsBasedOnXEntries.FirstOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
						num = ((statIsBasedOnXData == null) ? (num + item4.Ability.Strength) : (num + item4.Ability.GetStatIsBasedOnXValue(item4.Actor, statIsBasedOnXData, item4.Ability.AbilityFilter)));
					}
					string item = CreateLayout.LocaliseText(activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1, skipWarnings: false, num.ToString());
					activeBonus.Layout.ListLayouts.Add(item);
				}
				else
				{
					int strength = activeBonus.Ability.Strength;
					List<string> list = activeBonus.Ability.ListLayoutOverrideParams(activeBonus);
					if (list != null)
					{
						for (int num2 = 0; num2 < list.Count; num2++)
						{
							if (LocalizationManager.TryGetTranslation(list[num2], out var translation))
							{
								list[num2] = translation;
							}
						}
					}
					string item2 = CreateLayout.LocaliseText(activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item1, skipWarnings: false, strength.ToString(), list?.FirstOrDefault());
					string text = activeBonus.BaseCard.Name;
					if (activeBonus.BaseCard.CardType != CBaseCard.ECardType.CharacterAbility && activeBonus.BaseCard.CardType != CBaseCard.ECardType.Item && activeBonus.BaseCard.CardType != CBaseCard.ECardType.Pet)
					{
						text = ((activeBonus.Ability.ParentAbilityBaseCard == null || (activeBonus.Ability.ParentAbilityBaseCard.CardType != CBaseCard.ECardType.CharacterAbility && activeBonus.Ability.ParentAbilityBaseCard.CardType != CBaseCard.ECardType.Item && activeBonus.Ability.ParentAbilityBaseCard.CardType != CBaseCard.ECardType.Pet)) ? null : activeBonus.Ability.ParentAbilityBaseCard.Name);
					}
					if (text != null)
					{
						activeBonus.Layout.CardName = text;
					}
					activeBonus.Layout.ListLayouts.Add(item2);
				}
				activeBonus.Layout.IconNames.Add(activeBonus.Ability.ActiveBonusData.ListLayoutOverride.Item2);
			}
			else if (!(activeBonus.Actor is CObjectActor) && !activeBonus.IsDoom)
			{
				if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.ShowNextAttackModifier)
				{
					CShowNextAttackModifierActiveBonus cShowNextAttackModifierActiveBonus = activeBonus as CShowNextAttackModifierActiveBonus;
					activeBonus.Layout.IconNames.Add("Positive_Item_Placeholder_Card");
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText("$GUI_NextAttackModifier$ " + cShowNextAttackModifierActiveBonus?.NextAttackModifierString()));
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Shield)
				{
					int strengthOfActiveBonus = GetStrengthOfActiveBonus(activeBonus, CAbility.EAbilityType.Shield);
					string text2 = "";
					if (activeBonus.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.ShieldAll))
					{
						text2 = "Positive_Shield";
					}
					else if (activeBonus.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.ShieldMelee))
					{
						text2 = "Positive_ShieldMelee";
					}
					else
					{
						text2 = ((!activeBonus.Ability.ActiveBonusData.Flags.HasFlag(AbilityData.EActiveBonusFlag.ShieldRanged)) ? "Positive_Shield" : "Positive_ShieldRanged");
					}
					activeBonus.Layout.IconNames.Add(text2);
					activeBonus.Layout.ListLayouts.Add($"<sprite name=Shield> {strengthOfActiveBonus}");
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Retaliate)
				{
					int strengthOfActiveBonus2 = GetStrengthOfActiveBonus(activeBonus, CAbility.EAbilityType.Retaliate);
					string text3 = $"<sprite name=Retaliate> {strengthOfActiveBonus2}";
					CAbilityRetaliate cAbilityRetaliate = (CAbilityRetaliate)activeBonus.Ability;
					if (cAbilityRetaliate.RetaliateRange > 1)
					{
						int retaliateRange = cAbilityRetaliate.RetaliateRange;
						text3 = ((retaliateRange != 99999) ? (text3 + "\n" + CreateLayout.LocaliseText("$Range$ ") + retaliateRange) : (text3 + "\n" + CreateLayout.LocaliseText("$Any$ $Range$")));
					}
					activeBonus.Layout.IconNames.Add("Positive_Retaliate");
					activeBonus.Layout.ListLayouts.Add(text3);
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.AttackersGainDisadvantage)
				{
					activeBonus.Layout.IconNames.Add("Negative_Disadvantage_Campaign");
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText("$AttackersGainDisadvantage$ "));
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack || activeBonus.Ability.AbilityType == CAbility.EAbilityType.Move)
				{
					string cardName = string.Empty;
					string item3 = ((activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack) ? "Attack" : "Move");
					string empty = string.Empty;
					CBaseCard baseCard = activeBonus.BaseCard;
					if (baseCard != null && baseCard.IsItemOrPet && activeBonus.Caster != activeBonus.Actor && activeBonus.Ability.AbilityText.IsNOTNullOrEmpty())
					{
						cardName = CreateLayout.LocaliseText(activeBonus.BaseCard.Name);
						item3 = activeBonus.Caster.Class.DefaultModel;
						empty = string.Format(CreateLayout.LocaliseText(activeBonus.Ability.AbilityText), CreateLayout.LocaliseText(activeBonus.Caster.Class.LocKey));
					}
					else if (activeBonus.Ability.AbilityText.IsNOTNullOrEmpty())
					{
						empty = CreateLayout.LocaliseText(activeBonus.Ability.AbilityText);
					}
					else
					{
						string text4 = ((activeBonus.Ability.Strength < 0) ? "-" : "+");
						string text5 = CreateLayout.LocaliseText((activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack) ? " $Attack$ " : " $Move$ ");
						string text6 = string.Empty;
						if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Attack)
						{
							text6 = activeBonus.Ability?.ActiveBonusData?.AttackType switch
							{
								CAbility.EAttackType.Melee => CreateLayout.LocaliseText("$To_AllMeleeAttacks$"), 
								CAbility.EAttackType.Ranged => CreateLayout.LocaliseText("$To_AllRangedAttacks$"), 
								_ => CreateLayout.LocaliseText("$To_AllAttacks$"), 
							};
						}
						empty = text4 + activeBonus.Ability.Strength + text5 + text6;
					}
					activeBonus.Layout.CardName = cardName;
					activeBonus.Layout.IconNames.Add(item3);
					activeBonus.Layout.ListLayouts.Add(empty);
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.ExploitableVulnerability)
				{
					int strengthOfActiveBonus3 = GetStrengthOfActiveBonus(activeBonus, CAbility.EAbilityType.ExploitableVulnerability);
					activeBonus.Layout.CardName = $"$FH/Scenario_034_ChaosCharge_Title$ {strengthOfActiveBonus3}";
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.PreventDamage)
				{
					int strengthOfActiveBonus4 = GetStrengthOfActiveBonus(activeBonus, CAbility.EAbilityType.PreventDamage);
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText("PreventDamage ") + strengthOfActiveBonus4);
				}
				else if (activeBonus.Ability.AbilityType == CAbility.EAbilityType.Overheal)
				{
					int strengthOfActiveBonus5 = GetStrengthOfActiveBonus(activeBonus, CAbility.EAbilityType.Overheal);
					activeBonus.Layout.IconNames.Add("Health");
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText("$FH/Overheal_DefaultBonusText$", skipWarnings: false, strengthOfActiveBonus5.ToString()));
				}
				else if (activeBonus.Ability.PerkID.IsNOTNullOrEmpty())
				{
					PerksYMLData perksYMLData = ScenarioRuleClient.SRLYML.Perks.FirstOrDefault((PerksYMLData p) => p.ID == activeBonus.Ability.PerkID);
					if (perksYMLData != null)
					{
						activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText("$" + perksYMLData.Description + "$"));
						activeBonus.Layout.CardName = perksYMLData.Name;
					}
				}
				if (activeBonus.Actor is CHeroSummonActor cHeroSummonActor && cHeroSummonActor.SummonData.OnSummonAbilities != null && cHeroSummonActor.SummonData.OnSummonAbilities.Any((CAbility x) => x.ID == activeBonus.Ability.ID && activeBonus.Ability.AbilityText.IsNOTNullOrEmpty()))
				{
					activeBonus.Layout.CardName = cHeroSummonActor.ActorLocKey();
					if ((activeBonus.Layout.IconNames?.Count ?? 0) > 0 || (activeBonus.Layout.ListLayouts?.Count ?? 0) > 0)
					{
						activeBonus.Layout.IconNames?.Clear();
						activeBonus.Layout.ListLayouts.Clear();
					}
					activeBonus.Layout.IconNames.Add((activeBonus.Ability is CAbilityAddActiveBonus cAbilityAddActiveBonus) ? cAbilityAddActiveBonus.AddAbility.AbilityType.ToString() : activeBonus.Ability.AbilityType.ToString());
					activeBonus.Layout.ListLayouts.Add(CreateLayout.LocaliseText(activeBonus.Ability.AbilityText));
					title = CreateLayout.LocaliseText(cHeroSummonActor.ActorLocKey());
				}
			}
			else if (activeBonus.IsAugment)
			{
				activeBonus.Layout.CardName = activeBonus.BaseCard.Name;
				activeBonus.Layout.IconNames.Add("Mindthief");
				activeBonus.Layout.ListLayouts.Add(activeBonus.Ability?.Augment?.ActiveBonusLayout ?? activeBonus.Actor.Augments[0].ActiveBonusLayout);
				title = LocalizationManager.GetTranslation(activeBonus.BaseCard.Name) + " - " + CreateLayout.LocaliseText("<color=#A7B3D2>$Augment$</color> <sprite name=Augment>");
			}
			else if (activeBonus.IsSong)
			{
				if (activeBonus.Ability?.Song != null)
				{
					activeBonus.Layout.CardName = activeBonus.BaseCard.Name;
					activeBonus.Layout.IconNames.Add("Soothsinger");
					activeBonus.Layout.ListLayouts.Add(activeBonus.Ability.Song.ActiveBonusLayout);
				}
				title = LocalizationManager.GetTranslation(activeBonus.BaseCard.Name) + " - " + CreateLayout.LocaliseText("<color=#EBB0AA>$Song$</color> <sprite name=Song>");
			}
			else if (activeBonus.IsDoom)
			{
				CDoom cDoom = activeBonus?.Caster?.Dooms?.SingleOrDefault((CDoom x) => x.DoomAbilities.Any((CAbility y) => y == activeBonus.Ability));
				if (cDoom != null)
				{
					activeBonus.Layout.CardName = activeBonus.BaseCard.Name;
					activeBonus.Layout.IconNames.Add("Doomstalker");
					activeBonus.Layout.ListLayouts.Add(cDoom.ActiveBonusLayout);
					if (activeBonus?.Ability?.ActiveBonusData?.Tracker != null)
					{
						activeBonus.Layout.TrackerPattern.AddRange(activeBonus.Ability.ActiveBonusData.Tracker);
					}
				}
				title = LocalizationManager.GetTranslation(activeBonus.BaseCard.Name) + " - " + CreateLayout.LocaliseText("<color=#39BEED>$ChooseAbility$</color> <sprite name=ChooseAbility>");
			}
			else
			{
				activeBonus.Layout = null;
			}
		}
		if (activeBonus.Layout == null || activeBonus.BaseCard == null)
		{
			return;
		}
		string text7 = activeBonus.BaseCard.Name;
		bool flag;
		if (activeBonus.BaseCard.CardType != CBaseCard.ECardType.CharacterAbility && activeBonus.BaseCard.CardType != CBaseCard.ECardType.Item && activeBonus.BaseCard.CardType != CBaseCard.ECardType.Pet)
		{
			CBaseCard baseCard = activeBonus.Ability?.ParentAbilityBaseCard;
			if (baseCard != null)
			{
				CBaseCard.ECardType cardType = baseCard.CardType;
				if (cardType == CBaseCard.ECardType.CharacterAbility || cardType == CBaseCard.ECardType.Item || cardType == CBaseCard.ECardType.Pet)
				{
					flag = true;
					goto IL_112b;
				}
			}
			flag = false;
			goto IL_112b;
		}
		goto IL_114b;
		IL_112b:
		text7 = ((!flag) ? null : activeBonus.Ability.ParentAbilityBaseCard.Name);
		goto IL_114b;
		IL_114b:
		if (text7 != null)
		{
			activeBonus.Layout.CardName = text7;
		}
		title = (text7.IsNullOrEmpty() ? string.Empty : LocalizationManager.GetTranslation(text7, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: false, useDefaultIfMissing: false, returnNullIfNotFound: true));
	}

	private static int GetStrengthOfActiveBonus(CActiveBonus bonus, CAbility.EAbilityType type)
	{
		int num = 0;
		AbilityData.StatIsBasedOnXData statIsBasedOnXData = bonus.Ability.StatIsBasedOnXEntries.FirstOrDefault((AbilityData.StatIsBasedOnXData x) => x.AbilityStatType == CAbility.EAbilityStatType.Strength);
		if (statIsBasedOnXData != null)
		{
			if (statIsBasedOnXData.AddTo)
			{
				return num + bonus.Ability.Strength;
			}
			return num + bonus.Ability.GetStatIsBasedOnXValue(bonus.Actor, statIsBasedOnXData, bonus.Ability.AbilityFilter);
		}
		return bonus.Ability.Strength;
	}

	private static List<string> ListLayoutOverrideParams(CActiveBonus bonus)
	{
		List<string> list = null;
		CVarsContext context = null;
		CVars cVars = null;
		if (bonus.Ability.ActiveBonusData.MiscVars == null)
		{
			return null;
		}
		foreach (var miscVar in bonus.Ability.ActiveBonusData.MiscVars)
		{
			if (miscVar.Item1.StartsWith("ListLayoutParamVar"))
			{
				list = new List<string>();
				cVars = new CVars();
				context = new CVarsContext(cVars, bonus.Ability.VarsContext);
				cVars.Set("ActiveBonus", new TValue(bonus));
				break;
			}
			if (miscVar.Item1.Equals("ActiveBonusName"))
			{
				ActiveBonusLayout layout = bonus.Layout;
				TValue item = miscVar.Item2;
				layout.CardName = item.GetString();
			}
		}
		if (list != null)
		{
			foreach (var miscVar2 in bonus.Ability.ActiveBonusData.MiscVars)
			{
				TValue item = miscVar2.Item2;
				TValue value = item.Calc(context);
				cVars.Set(miscVar2.Item1, value);
				if (miscVar2.Item1.StartsWith("ListLayoutParamVar"))
				{
					list.Add(value.ValueString());
				}
			}
		}
		return list;
	}
}
