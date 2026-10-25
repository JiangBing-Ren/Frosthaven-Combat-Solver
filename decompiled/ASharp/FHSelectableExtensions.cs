using System.Collections.Generic;
using UnityEngine.UI;

public static class FHSelectableExtensions
{
	public static void SetTextKeyFormatted(this Selectable selectable, string key, params string[] parameters)
	{
		UIHelperUtil.SetTextKeyFormatted(selectable, key, parameters);
	}

	public static void SetTextKey(this Selectable selectable, string key, Dictionary<string, string> parameters)
	{
		UIHelperUtil.SetTextKey(selectable, key, parameters);
	}

	public static void SetTextKey(this Selectable selectable, string key, params LocalizationParam[] parameters)
	{
		UIHelperUtil.SetTextKey(selectable, key, parameters);
	}

	public static void SetNonLocalizedText(this Selectable selectable, string nonlocalizedText)
	{
		UIHelperUtil.SetNonLocalizedText(selectable, nonlocalizedText);
	}
}
