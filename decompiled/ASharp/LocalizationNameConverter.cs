using System.Text;
using GLOOM;

public class LocalizationNameConverter
{
	private const string PREFIX_SEPARATOR = "_";

	public const string ABILITY_CARD_KEY_PREFIX = "ABILITY_CARD_";

	public const string ITEM_KEY_PREFIX = "ITEM_NAME_";

	public const string COMBAT_LOG_SUFFIX = "COMBAT_LOG";

	public static string MultiLookupLocalization(string locKey, out bool isItem, bool suppressErrors = false)
	{
		isItem = false;
		string text = LocalizationManager.GetTranslation(locKey, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
		if (text == null)
		{
			text = LocalizationManager.GetTranslation(locKey.Replace(" ", string.Empty).Replace(" ", string.Empty), fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
			if (text == null)
			{
				string translation = LocalizationManager.GetTranslation(ToNameLocalizationKey(locKey.Replace(" ", string.Empty).Replace(" ", string.Empty), "ITEM_NAME_"), fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true, useDefaultIfMissing: false, returnNullIfNotFound: true);
				if (translation != null)
				{
					text = translation;
					isItem = true;
				}
				else if (!suppressErrors)
				{
					Log.Error("No localization string found for " + locKey);
				}
			}
		}
		return text;
	}

	public static string ToNameLocalizationKey(string name, string prefix = null, string suffix = null, bool removeDashes = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!prefix.IsNullOrEmpty())
		{
			stringBuilder.Append(prefix);
			if (!prefix.EndsWith("_"))
			{
				stringBuilder.Append("_");
			}
		}
		string value = name;
		value = value.RemoveSpaces().RemoveChar('\'');
		if (removeDashes)
		{
			value = value.RemoveChar('-');
		}
		stringBuilder.Append(value);
		if (!suffix.IsNullOrEmpty())
		{
			if (!suffix.StartsWith("_"))
			{
				stringBuilder.Append("_");
			}
			stringBuilder.Append(suffix);
		}
		return stringBuilder.ToString();
	}

	public static bool TryLocalizeItemCombatLogMessage(string name, out string translation)
	{
		return LocalizationManager.TryGetTranslation(ToNameLocalizationKey(name, null, "COMBAT_LOG"), out translation);
	}

	public static string LocalizeWithPrefix(string name, string prefix, bool returnError = false)
	{
		string term = ToNameLocalizationKey(name, prefix, null, removeDashes: true);
		if (!returnError)
		{
			if (!LocalizationManager.TryGetTranslation(term, out var translation))
			{
				return null;
			}
			return translation;
		}
		return LocalizationManager.GetTranslation(term);
	}
}
