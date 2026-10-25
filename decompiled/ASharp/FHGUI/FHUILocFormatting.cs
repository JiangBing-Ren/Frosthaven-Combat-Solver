using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FHRL;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

namespace FHGUI;

public static class FHUILocFormatting
{
	public const char GUI_NONLOC_PREFIX_SYMBOL = '@';

	public const string GUI_TACTICAL_PHASEBANNER_SUBTEXT = "<size=24><font=\"FH_Sarala-Regular\"><color=#d0d0d0>{0}</color></font></size>";

	public const string GUI_OUTPOST_WARNING_TOOLTIP = "<size=20><font=\"FH_Sarala-Regular\"><color=#BC2E1B>{0}</color></font></size>";

	public const string GUI_OUTPOST_RESOURCE_DEPOT_ACTION_1 = "<sprite name=Gold>{0} <sprite name=ArrowShort> <sprite name={2}>{1}";

	public const string GUI_OUTPOST_RESOURCE_DEPOT_ACTION_2 = "<sprite name={2}>{0} <sprite name=ArrowShort> <sprite name=Gold>{1}";

	public const string GUI_OUTPOST_BARRACKS_ACTION_1 = "<sprite name=Gold>{0} <sprite name={1}> {2} <sprite name=ArrowShort> <sprite name=Soldiers>{3}";

	public const string GUI_OUTPOST_BUILD_INFO_WRECKED = "<style=\"Wrecked\">{0}</style>";

	public const string GUI_SCENARIO_CARRYABLE_QUESTITEM = "{0}_TOOLTIP";

	public const string GUI_SCENARIO_CARRYABLE_QUESTITEM_DESC = "{0}_DESCR_TOOLTIP";

	public const string GUI_SCENARIO_RESOURCE = "{0}_TOOLTIP";

	public const string GUI_SCENARIO_RESOURCE_DESC = "{0}_DESCR_TOOLTIP";

	public const string GUI_PQ_PROGRESS_GENERAL = "{2} - {0}/{1}";

	private static StringBuilder _stringBuilder = new StringBuilder();

	public static string Bold(string text)
	{
		return "<b>" + text + "</b>";
	}

	public static string Color(string text, Color color)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
	}

	public static string Color(string text, string htmlColor)
	{
		return "<color=#" + htmlColor + ">" + text + "</color>";
	}

	public static string Sprite(string spriteName)
	{
		return "<sprite name=" + spriteName + ">";
	}

	public static string Style(string text, string style)
	{
		return "<style=\"" + style + "\">" + text + "</style>";
	}

	public static string AlignCenter(string text)
	{
		return "<align=\"center\">" + text;
	}

	public static string AlignLeft(string text)
	{
		return "<align=\"left\">" + text;
	}

	public static string AlignRight(string text)
	{
		return "<align=\"right\">" + text;
	}

	public static string AlignJustified(string text)
	{
		return "<align=\"justified\">" + text;
	}

	public static string AlignFlush(string text)
	{
		return "<align=\"flush\">" + text;
	}

	public static string Buff(string text)
	{
		return "<color=green>" + text + "</color>";
	}

	public static string Debuff(string text)
	{
		return "<color=red>" + text + "</color>";
	}

	public static string FontSize(string size, string text)
	{
		return "<size=" + size + ">" + text + "</color></size>";
	}

	public static Sprite GetActionIcon(FHInputEventElement.UserInteractionType interaction)
	{
		return null;
	}

	public static string GetActionIconFormat(FHInputEventElement.UserInteractionType interaction)
	{
		return interaction switch
		{
			FHInputEventElement.UserInteractionType.LeftClick => "<sprite name=LeftKey>", 
			FHInputEventElement.UserInteractionType.RightClick => "<sprite name=RightKey>", 
			FHInputEventElement.UserInteractionType.DoubleClick => "2x<sprite name=LeftKey>", 
			FHInputEventElement.UserInteractionType.LongPress => "[LP]", 
			FHInputEventElement.UserInteractionType.MiddleClick => "<sprite name=MiddleKey>", 
			_ => string.Empty, 
		};
	}

	public static string GetActionIconFormat(FHInputEventElement.UserInteractionType interaction, string textKey)
	{
		return GetActionIconFormat(interaction) + " " + LocalizationManager.GetTranslation(textKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
	}

	public static string GetActionIconFormatLiteral(FHInputEventElement.UserInteractionType interaction, string textKey)
	{
		return GetActionIconFormat(interaction) + " " + LocalizationManager.GetTranslation(textKey);
	}

	public static string PopulateParams(string key, params object[] paramsBody)
	{
		TryPopulateParams(out var result, key, paramsBody);
		return result;
	}

	public static bool TryPopulateParams(out string result, string key, params object[] paramsBody)
	{
		result = LocalizationManager.GetTranslation(key, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
		if (result == null)
		{
			return false;
		}
		if (paramsBody == null)
		{
			return true;
		}
		int count = Regex.Matches(result, "\\{\\d+\\}").Count;
		if (count == 0)
		{
			return true;
		}
		if (count > paramsBody.Length)
		{
			result = string.Format(result, paramsBody.Take(count).Concat(GetMissingParams(paramsBody.Length, count - paramsBody.Length)).ToArray());
		}
		else
		{
			result = string.Format(result, paramsBody.Take(count).ToArray());
		}
		return true;
		static IEnumerable<string> GetMissingParams(int staringIndex, int num)
		{
			for (int i = 0; i < num; i++)
			{
				yield return Color($"{{{staringIndex + i}}}", FHUIColors.MISSING_RESOURCE);
			}
		}
	}

	public static string PopulateParams(string key, params LocalizationParam[] localizationParams)
	{
		string text = LocalizationManager.GetTranslation(key);
		foreach (LocalizationParam localizationParam in localizationParams)
		{
			text = localizationParam.Apply(text);
		}
		return text;
	}

	public static string FormatSimpleTooltip(string caption, params string[] paramsBody)
	{
		using TempStringBuilder tempStringBuilder = default;
		tempStringBuilder.Append("<size=+2>");
		tempStringBuilder.Append(LocalizationManager.GetTranslation(caption));
		tempStringBuilder.Append("</size>");
		tempStringBuilder.AppendLine();
		foreach (string term in paramsBody)
		{
			tempStringBuilder.Append(LocalizationManager.GetTranslation(term));
			tempStringBuilder.AppendLine();
		}
		return tempStringBuilder.ToString();
	}

	public static void SetText(FHButton button, string text)
	{
		button.GetComponentsInChildren<TMP_Text>().First((TMP_Text t) => t.name == "Text").text = text;
	}

	public static void SetTextKey(FHButton button, string key)
	{
		button.GetComponentsInChildren<TMP_Text>().First((TMP_Text t) => t.name == "Text").text = LocalizationManager.GetTranslation(key);
	}

	public static string Gold(int gold)
	{
		return $"<size=-2><sprite name=Gold> {gold}</size>";
	}

	public static string Gold(int gold, bool sufficient)
	{
		string text = Gold(gold);
		if (!sufficient)
		{
			text = Color(text, FHUIColors.MISSING_RESOURCE);
		}
		return text;
	}

	public static string PrepareNonLocButtonText(string text)
	{
		return "@" + text;
	}

	public static string GetScenarioQuestItemName(CObjectProp prop)
	{
		if (!string.IsNullOrEmpty(prop.PropCustomLocKey))
		{
			return prop.PropCustomLocKey;
		}
		return $"{prop.PrefabName}_TOOLTIP";
	}

	public static string GetScenarioQuestItemDesc(CObjectProp prop)
	{
		if (!string.IsNullOrEmpty(prop.PropCustomDescriptionLocKey))
		{
			return prop.PropCustomDescriptionLocKey;
		}
		return $"{prop.PrefabName}_DESCR_TOOLTIP";
	}

	public static string GetResourcePropName(CObjectResource resource)
	{
		return resource.ResourceData.LocKey ?? $"{resource.ResourceData.ID}_TOOLTIP";
	}

	public static string GetResourcePropDesc(CObjectResource resource)
	{
		return $"{resource.ResourceData.ID}_DESCR_TOOLTIP";
	}

	public static string GetCanBeLootedByProp(CObjectProp prop)
	{
		return string.Format(LocalizationManager.GetTranslation("GUI_PROP_CAN_BE_LOOTED_BY"), LocalizationManager.GetTranslation(prop.CanLootLocKey));
	}

	public static string GetPropCreateBy(CCharacterClass characterClass)
	{
		return LocalizationManager.GetTranslation("GUI_PROP_CREATED_BY") + " " + LocalizationManager.GetTranslation(characterClass.LocKey);
	}

	public static string PersonalQuestProgress(uint current, uint max)
	{
		return string.Format(LocalizationManager.GetTranslation("PQ_ProgressBasic"), current.ToString(), max.ToString());
	}

	public static string PersonalQuestProgress(string progressLocKey, uint current, uint max)
	{
		return string.Format("{2} - {0}/{1}", current.ToString(), max.ToString(), LocalizationManager.GetTranslation(progressLocKey));
	}

	public static string PersonalQuestProgressTimes(uint current, uint max)
	{
		return string.Format(LocalizationManager.GetTranslation("PQ_ProgressTimes"), current.ToString(), max.ToString());
	}

	public static string PersonalQuestProgressTypes(uint current, uint max)
	{
		return string.Format(LocalizationManager.GetTranslation("PQ_ProgressTypes"), current.ToString(), max.ToString());
	}

	public static string PersonalQuestProgressGold(uint current, uint max)
	{
		return string.Format(LocalizationManager.GetTranslation("PQ_ProgressGold"), current.ToString(), max.ToString());
	}

	public static string AttackModToAtlasKey(string attackMathMod)
	{
		return attackMathMod switch
		{
			"x0" => "NullModifier", 
			"-2" => "-2", 
			"-1" => "-1", 
			"+0" => "0", 
			"+1" => "+1", 
			"+2" => "+2", 
			"+3" => "+3", 
			"+4" => "+4", 
			"*2" => "2x", 
			_ => attackMathMod, 
		};
	}

	public static string GetBasicAttackModifierTooltip(string attackMathMod)
	{
		return string.Format(LocalizationManager.GetTranslation("GUI_ATTACKMODIFIERS_BASE_TT"), attackMathMod);
	}

	public static string GetHerbList(OutpostResources resources, bool showZeroes = false, string separator = ", ")
	{
		_stringBuilder.Clear();
		if (showZeroes || resources.Arrowvine > 0)
		{
			_stringBuilder.Append($"{resources.Arrowvine}<sprite name={FHUILocKeys.GetHerbTMPIcon(OutpostHerbType.Arrowvine)}>");
		}
		if (showZeroes || resources.Axenut > 0)
		{
			if (_stringBuilder.Length > 0)
			{
				_stringBuilder.Append(separator);
			}
			_stringBuilder.Append($"{resources.Axenut}<sprite name={FHUILocKeys.GetHerbTMPIcon(OutpostHerbType.Axenut)}>");
		}
		if (showZeroes || resources.Corpsecap > 0)
		{
			if (_stringBuilder.Length > 0)
			{
				_stringBuilder.Append(separator);
			}
			_stringBuilder.Append($"{resources.Corpsecap}<sprite name={FHUILocKeys.GetHerbTMPIcon(OutpostHerbType.Corpsecap)}>");
		}
		if (showZeroes || resources.Flamefruit > 0)
		{
			if (_stringBuilder.Length > 0)
			{
				_stringBuilder.Append(separator);
			}
			_stringBuilder.Append($"{resources.Flamefruit}<sprite name={FHUILocKeys.GetHerbTMPIcon(OutpostHerbType.Flamefruit)}>");
		}
		if (showZeroes || resources.Rockroot > 0)
		{
			if (_stringBuilder.Length > 0)
			{
				_stringBuilder.Append(separator);
			}
			_stringBuilder.Append($"{resources.Rockroot}<sprite name={FHUILocKeys.GetHerbTMPIcon(OutpostHerbType.Rockroot)}>");
		}
		if (showZeroes || resources.Snowthistle > 0)
		{
			if (_stringBuilder.Length > 0)
			{
				_stringBuilder.Append(separator);
			}
			_stringBuilder.Append($"{resources.Snowthistle}<sprite name={FHUILocKeys.GetHerbTMPIcon(OutpostHerbType.Snowthistle)}>");
		}
		return _stringBuilder.ToString();
	}

	public static string GetLootTypeDescription(LootCard.EType lootType)
	{
		switch (lootType)
		{
		case LootCard.EType.None:
			return string.Empty;
		case LootCard.EType.Money:
			return LocalizationManager.GetTranslation("GUI_Gold_Desc");
		case LootCard.EType.Lumber:
		case LootCard.EType.Metal:
		case LootCard.EType.Hide:
			return string.Format(LocalizationManager.GetTranslation("GUI_Material_Desc"), LocalizationManager.GetTranslation(FHUILocKeys.GetLootTypeKey(lootType)));
		case LootCard.EType.Arrowvine:
		case LootCard.EType.Axenut:
		case LootCard.EType.Corpsecap:
		case LootCard.EType.Flamefruit:
		case LootCard.EType.Rockroot:
		case LootCard.EType.Snowthistle:
			return string.Format(LocalizationManager.GetTranslation("GUI_Herb_Desc"), LocalizationManager.GetTranslation(FHUILocKeys.GetLootTypeKey(lootType)));
		case LootCard.EType.RandomItem:
			return string.Empty;
		default:
			throw new ArgumentOutOfRangeException("lootType", lootType, null);
		}
	}
}
