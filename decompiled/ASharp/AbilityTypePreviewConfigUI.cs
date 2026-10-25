using System;
using ScenarioRuleLibrary;
using UnityEngine;

[Serializable]
public class AbilityTypePreviewConfigUI
{
	private enum Format
	{
		IconAfterValue,
		IconBeforeValue,
		OnlyIcon,
		Custom
	}

	public CAbility.EAbilityType type;

	public string iconName;

	public bool showValueSign;

	[SerializeField]
	private Format format;

	[SerializeField]
	[ConditionalField("format", "Custom", true)]
	private string customFormat;

	private const string ICON_BEFORE_VALUE_FORMAT = "<sprite name={0}>{1}";

	private const string ICON_AFTER_VALUE_FORMAT = "{1}<sprite name={0}>";

	private const string ONLY_ICON_FORMAT = "<sprite name={0}>";

	public string GetFormat()
	{
		if (format == Format.Custom && customFormat.IsNOTNullOrEmpty())
		{
			return CreateLayout.LocaliseText(customFormat);
		}
		if (format != Format.IconBeforeValue)
		{
			if (format != Format.IconAfterValue)
			{
				return "<sprite name={0}>";
			}
			return "{1}<sprite name={0}>";
		}
		return "<sprite name={0}>{1}";
	}
}
