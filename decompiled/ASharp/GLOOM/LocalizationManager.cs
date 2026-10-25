using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using I2.Loc;
using ScenarioRuleLibrary;
using UnityEngine;

namespace GLOOM;

public static class LocalizationManager
{
	private const string ERROR_STRIP_PREFIX = "<color=\"red\">";

	private const string ERROR_STRIP_POSTFIX = "</color>";

	public static string GetTranslation(string term, bool fixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool skipWarnings = false, bool useDefaultIfMissing = false, bool returnNullIfNotFound = false)
	{
		if (string.IsNullOrEmpty(term))
		{
			Log.Error("A null or empty term was sent to GetTranslation.");
			return "UNDEFINED";
		}
		if (term.StartsWith('$'.ToString()) && term.EndsWith('$'.ToString()))
		{
			term = CardLayoutRow.GetKey(term, '$');
		}
		string text = I2.Loc.LocalizationManager.GetTranslation(term, fixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage);
		if (text == null)
		{
			text = I2.Loc.LocalizationManager.GetTranslation(term.ToUpperInvariant(), fixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage);
		}
		if (text == null)
		{
			text = I2.Loc.LocalizationManager.GetTranslation(term.ToLowerInvariant(), fixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage);
		}
		if (text == null)
		{
			if (useDefaultIfMissing && I2.Loc.LocalizationManager.CurrentLanguage != "English")
			{
				text = I2.Loc.LocalizationManager.GetTranslation(term, fixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, "English");
			}
			if (text == null)
			{
				if (!skipWarnings)
				{
					Log.Warning("GetTranslation term not found for " + term);
				}
				if (returnNullIfNotFound)
				{
					return null;
				}
				text = "UNDEFINED <color=\"red\">" + term + "</color>";
			}
		}
		text = ReplaceTextVariables(text);
		if (text.StartsWith("<color=\"red\">") && text.EndsWith("</color>"))
		{
			int length = "</color>".Length;
			text = text.Remove(0, "<color=\"red\">".Length);
			text = text.Remove(text.Length - length, length);
		}
		return text;
	}

	public static bool TryGetTranslation(string term, out string translation, bool fixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null)
	{
		translation = null;
		if (term.StartsWith('$'.ToString()) && term.EndsWith('$'.ToString()))
		{
			term = CardLayoutRow.GetKey(term, '$');
		}
		if (I2.Loc.LocalizationManager.TryGetTranslation(term.ToLowerInvariant(), out translation, fixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage))
		{
			translation = ReplaceTextVariables(translation);
			return true;
		}
		return false;
	}

	public static string ReplaceTextVariables(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (text.IndexOf("{C}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = ((ScenarioManager.CurrentScenarioState?.Players == null || ScenarioManager.CurrentScenarioState.Players.Count <= 0) ? text.Replace("{C}", "C") : text.Replace("{C}", ScenarioManager.CurrentScenarioState.Players.Count.ToString()));
		}
		if (text.IndexOf("{L}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = ((ScenarioManager.CurrentScenarioState == null) ? text.Replace("{L}", "L") : text.Replace("{L}", ScenarioManager.CurrentScenarioState.Level.ToString()));
		}
		if (text.IndexOf("{C-1}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = ((ScenarioManager.CurrentScenarioState?.Players == null || ScenarioManager.CurrentScenarioState.Players.Count <= 0) ? text.Replace("{C-1}", "C-1") : text.Replace("{C-1}", (ScenarioManager.CurrentScenarioState.Players.Count - 1).ToString()));
		}
		if (text.IndexOf("{C/2D}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = ((ScenarioManager.CurrentScenarioState?.Players != null && ScenarioManager.CurrentScenarioState.Players.Count > 0) ? text.Replace("{C/2D}", (ScenarioManager.CurrentScenarioState.Players.Count / 2).ToString()) : ((!TryGetTranslation("rounded_down", out var translation) || string.IsNullOrEmpty(translation)) ? text.Replace("{C/2D}", "C/2") : text.Replace("{C/2D}", "C/2 (" + translation + ")")));
		}
		if (text.IndexOf("{C/2U}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			if (ScenarioManager.CurrentScenarioState?.Players == null || ScenarioManager.CurrentScenarioState.Players.Count <= 0)
			{
				text = ((!TryGetTranslation("rounded_up", out var translation2) || string.IsNullOrEmpty(translation2)) ? text.Replace("{C/2U}", "C/2") : text.Replace("{C/2U}", "C/2 (" + translation2 + ")"));
			}
			else
			{
				int count = ScenarioManager.CurrentScenarioState.Players.Count;
				text = ((count % 2 != 0) ? text.Replace("{C/2U}", (count / 2 + 1).ToString()) : text.Replace("{C/2U}", (count / 2).ToString()));
			}
		}
		if (text.IndexOf("{L+2}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = ((ScenarioManager.CurrentScenarioState == null) ? text.Replace("{L+2}", "L+2") : text.Replace("{L+2}", (ScenarioManager.CurrentScenarioState.Level + 2).ToString()));
		}
		if (text.IndexOf("{L+3}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = ((ScenarioManager.CurrentScenarioState == null) ? text.Replace("{L+3}", "L+3") : text.Replace("{L+3}", (ScenarioManager.CurrentScenarioState.Level + 3).ToString()));
		}
		if (text.IndexOf("{1+L/2U}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = ((ScenarioManager.CurrentScenarioState == null) ? text.Replace("{1+L/2U}", "1+L/2") : text.Replace("{1+L/2U}", Math.Ceiling(1f + (float)ScenarioManager.CurrentScenarioState.Level / 2f).ToString()));
		}
		if (text.IndexOf("{hazardous}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = Regex.Replace(text, "{hazardous}", ScenarioManager.HazardousTerrainDamage.ToString());
		}
		if (text.IndexOf("{trap}", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			text = Regex.Replace(text, "{trap}", ScenarioManager.TrapDamage.ToString());
		}
		return text;
	}

	public static bool AddCSVSource(string csv)
	{
		LanguageSourceData languageSourceData = ImportCSV(csv);
		if (languageSourceData != null)
		{
			I2.Loc.LocalizationManager.Sources.Add(languageSourceData);
			return true;
		}
		return false;
	}

	public static bool InsertCSVSource(string csv, int index = 0)
	{
		LanguageSourceData languageSourceData = ImportCSV(csv);
		if (languageSourceData != null)
		{
			I2.Loc.LocalizationManager.Sources.Insert(index, languageSourceData);
			return true;
		}
		return false;
	}

	public static LanguageSourceData ImportCSV(string csv)
	{
		LanguageSourceData languageSourceData = GenerateLanguageSourceSource();
		if (!(languageSourceData.Import_CSV(string.Empty, File.ReadAllText(csv), eSpreadsheetUpdateMode.Merge) == string.Empty))
		{
			return null;
		}
		return languageSourceData;
	}

	public static LanguageSourceData GenerateLanguageSourceSource()
	{
		return new LanguageSourceData
		{
			CaseInsensitiveTerms = true,
			mDictionary = new Dictionary<string, TermData>(StringComparer.OrdinalIgnoreCase)
		};
	}
}
