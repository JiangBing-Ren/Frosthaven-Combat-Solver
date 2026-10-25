using System;
using FHGUI;
using FHGUI.Configs;
using GLOOM;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BuildingConfigUI", menuName = "UI Config/Building")]
public class BuildingConfigUI : ScriptableObject
{
	[Serializable]
	public class CodexTableEntry
	{
		[Serializable]
		public class CellItem
		{
			public Sprite Icon;

			public string IconTooltip;

			public string Text;

			public string GetTextLocalized(string textStyle = null)
			{
				if (Text.StartsWith('@'))
				{
					return Text.RemoveChar('@');
				}
				if (Text.IsNullOrEmpty())
				{
					return null;
				}
				string text = LocalizationManager.GetTranslation(Text, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
				if (textStyle.IsNOTNullOrEmpty())
				{
					text = FHUILocFormatting.Style(text, textStyle);
				}
				return text;
			}

			public string GetTooltipLocalized()
			{
				if (IconTooltip.StartsWith('@'))
				{
					return IconTooltip.RemoveChar('@');
				}
				if (!IconTooltip.IsNOTNullOrEmpty())
				{
					return null;
				}
				return LocalizationManager.GetTranslation(IconTooltip, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
			}
		}

		[Serializable]
		public class LevelData
		{
			[FormerlySerializedAs("Effect")]
			public CellItem[] Effects;
		}

		public string ID;

		public Sprite Icon;

		public string Tooltip;

		public string Text;

		public string TextStyle;

		public LevelData[] Levels;

		public LevelData GetData(int level)
		{
			if (Levels == null)
			{
				return null;
			}
			if (level < 0 || level >= Levels.Length)
			{
				return null;
			}
			return Levels[level];
		}

		public string GetHeaderLocalized()
		{
			if (Text.StartsWith('@'))
			{
				return Text.RemoveChar('@');
			}
			if (Text.IsNullOrEmpty())
			{
				return null;
			}
			if (!Text.IsNOTNullOrEmpty())
			{
				return null;
			}
			return LocalizationManager.GetTranslation(Text, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
		}

		public string GetHeaderTooltipLocalized()
		{
			if (Tooltip.StartsWith('@'))
			{
				return Tooltip.RemoveChar('@');
			}
			if (!Tooltip.IsNOTNullOrEmpty())
			{
				return null;
			}
			return LocalizationManager.GetTranslation(Tooltip, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
		}
	}

	public BuildingId BuildingId;

	[ConfigIcon(128, true)]
	public Sprite Icon;

	[ConfigIcon(64)]
	public Sprite StylizedIcon;

	[ConfigIcon(64)]
	public Sprite TimetableIcon;

	[Header("Build Info Screen")]
	[ConfigIcon(128)]
	public Sprite Banner;

	public Color Color = Color.white;

	[Header("Localization")]
	public string[] NameLocKey;

	public string FlavorTextKey;

	[Tooltip("NB: First element is the description of the building before it's constructed")]
	public string[] ActiveEffects;

	public string[] UpgradeEffects;

	public string[] WreckedEffects;

	public string SpecialUpgradeText;

	[Space(30f)]
	[Header("Codex")]
	public CodexTableEntry[] CodexEffects;

	public string CodexNameLocKey;

	public bool HiddenInCodex;

	public string GetName(uint level = 0u)
	{
		if (NameLocKey.Length <= level)
		{
			return NameLocKey[0];
		}
		return NameLocKey[level];
	}

	public string GetCodexName()
	{
		if (CodexNameLocKey.IsNOTNullOrEmpty())
		{
			return CodexNameLocKey;
		}
		return GetName();
	}

	public string GetEffect(uint level)
	{
		if (ActiveEffects.Length <= level)
		{
			return string.Empty;
		}
		return ActiveEffects[level];
	}

	public string GetWreckedEffect(uint level)
	{
		if (WreckedEffects.Length < level)
		{
			return string.Empty;
		}
		return WreckedEffects[level - 1];
	}

	public string GetUpgradeEffect(uint level)
	{
		int num = (int)(level - 1);
		if (num < 0 || UpgradeEffects.Length <= num)
		{
			return string.Empty;
		}
		return UpgradeEffects[num];
	}

	public string GetConstructEffect()
	{
		return GetEffect(0u);
	}
}
