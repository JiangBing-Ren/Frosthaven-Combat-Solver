using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FHGUI.Common.Glossary;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class LocalizationGlossaryUtils
{
	private const string Pattern = "<sprite\\s+name=(?:\"([^\"]+)\"|([^\\s>]+))>";

	private static readonly Regex _regexMatch = new Regex("<sprite\\s+name=(?:\"([^\"]+)\"|([^\\s>]+))>", RegexOptions.Compiled);

	private static readonly Dictionary<string, UIGlossaryData> _keys = new Dictionary<string, UIGlossaryData>();

	private static GlossaryDataSO _glossaryData => UIInfoTools.GlossaryData;

	public static bool TryParseGlossaryTexts(GameObject go, out string parsedText, bool checkChildren = true)
	{
		IEnumerable<UIGlossaryData> glossaryEntries = GetGlossaryEntries(go, checkChildren);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (UIGlossaryData item in glossaryEntries)
		{
			stringBuilder.AppendLine(item.GetDescription());
		}
		parsedText = stringBuilder.ToString();
		return parsedText.IsNOTNullOrEmpty();
	}

	public static bool TryParseGlossaryTexts(string text, out string parsedText)
	{
		IEnumerable<UIGlossaryData> glossaryEntries_Internal = GetGlossaryEntries_Internal(text);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (UIGlossaryData item in glossaryEntries_Internal)
		{
			stringBuilder.AppendLine(item.Description);
		}
		parsedText = ((stringBuilder.Length > 0) ? stringBuilder.ToString() : string.Empty);
		return stringBuilder.Length > 0;
	}

	private static IEnumerable<UIGlossaryData> GetGlossaryEntries(bool checkChildren = true, params GameObject[] objects)
	{
		List<UIGlossaryData> list = new List<UIGlossaryData>();
		HashSet<Transform> visited = new HashSet<Transform>();
		foreach (GameObject gameObject in objects)
		{
			if (!(gameObject == null))
			{
				CollectGlossaryEntries(gameObject.transform, checkChildren, visited, list, skipProviderOnRoot: false);
			}
		}
		return list.Distinct();
	}

	private static IEnumerable<UIGlossaryData> GetGlossaryEntries(GameObject obj, bool checkChildren = true)
	{
		return GetGlossaryEntries(checkChildren, obj);
	}

	public static IEnumerable<UIGlossaryData> GetGlossaryContents(GameObject root, bool checkChildren = true)
	{
		List<UIGlossaryData> list = new List<UIGlossaryData>();
		HashSet<Transform> visited = new HashSet<Transform>();
		if (root == null)
		{
			return list;
		}
		CollectGlossaryEntries(root.transform, checkChildren, visited, list, skipProviderOnRoot: true);
		return list.Distinct();
	}

	private static void CollectGlossaryEntries(Transform transform, bool checkChildren, HashSet<Transform> visited, List<UIGlossaryData> result, bool skipProviderOnRoot)
	{
		if (transform == null || !visited.Add(transform))
		{
			return;
		}
		GameObject gameObject = transform.gameObject;
		if (!skipProviderOnRoot)
		{
			IGlossaryProvider[] components = gameObject.GetComponents<IGlossaryProvider>();
			if (components.Length != 0)
			{
				IGlossaryProvider[] array = components;
				foreach (IGlossaryProvider glossaryProvider in array)
				{
					if (glossaryProvider == null)
					{
						continue;
					}
					IEnumerable<UIGlossaryData> enumerable = null;
					try
					{
						enumerable = glossaryProvider.GetGlossary();
					}
					catch (Exception ex)
					{
						Log.GUIWarning("Glossary for provider " + glossaryProvider.GameObject.name + " with:\n " + ex.Message);
					}
					finally
					{
						if (enumerable != null && enumerable.Any())
						{
							result.AddRange(enumerable);
						}
					}
				}
				return;
			}
		}
		TMP_Text[] components2 = gameObject.GetComponents<TMP_Text>();
		foreach (TMP_Text tMP_Text in components2)
		{
			if (!(tMP_Text == null))
			{
				result.AddRange(GetGlossaryEntries_Internal(tMP_Text.text));
			}
		}
		Image[] components3 = gameObject.GetComponents<Image>();
		foreach (Image image in components3)
		{
			if (!(image == null) && image.enabled)
			{
				UIGlossaryData? glossaryEntryFromImage = GetGlossaryEntryFromImage(image);
				if (glossaryEntryFromImage.HasValue)
				{
					result.Add(glossaryEntryFromImage.Value);
				}
			}
		}
		if (!checkChildren)
		{
			return;
		}
		foreach (Transform item in transform)
		{
			if (item.gameObject.activeInHierarchy)
			{
				CollectGlossaryEntries(item, checkChildren: true, visited, result, skipProviderOnRoot: false);
			}
		}
	}

	private static IEnumerable<UIGlossaryData> GetGlossaryEntries_Internal(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			yield break;
		}
		string[] array = (from m in _regexMatch.Matches(text)
			select (!m.Groups[1].Success) ? m.Groups[2].Value : m.Groups[1].Value into k
			where k.IsNOTNullOrEmpty()
			select k).Distinct().ToArray();
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (_keys.TryGetValue(text2, out var value))
			{
				yield return value;
				continue;
			}
			string translation = string.Empty;
			string text3 = string.Empty;
			if (text2 == "SUMMON_HEALTH")
			{
				text3 = LocalizationManager.GetTranslation("SUMMON_HEALTH", fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
				translation = CreateLayout.LocaliseText("$Glossary_Summon_Health$", skipWarnings: true);
			}
			else if (LocalizationManager.TryGetTranslation("Glossary_" + text2, out translation))
			{
				text3 = LocalizationManager.GetTranslation(text2, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
			}
			else if (LocalizationManager.TryGetTranslation(text2, out translation))
			{
				text3 = LocalizationManager.GetTranslation(text2, fixForRTL: true, 0, ignoreRTLnumbers: true, applyParameters: false, null, null, skipWarnings: true);
			}
			if (text3 == translation || translation == null)
			{
				translation = string.Empty;
			}
			if (!translation.StartsWith("UNDEFINED") && !text3.IsNullOrEmpty())
			{
				UIGlossaryData? data = _glossaryData.GetData(text2);
				if (data.HasValue)
				{
					_keys[text2] = data.Value;
					yield return data.Value;
					continue;
				}
				UIGlossaryData uIGlossaryData = new UIGlossaryData
				{
					Key = text2,
					Name = text3,
					Description = translation,
					TMPKey = text2
				};
				_keys[text2] = uIGlossaryData;
				yield return uIGlossaryData;
			}
		}
	}

	public static IEnumerable<UIGlossaryData> GetGlossaryEntriesFromText(string text)
	{
		return GetGlossaryEntriesFromTexts(new string[1] { text });
	}

	public static IEnumerable<UIGlossaryData> GetGlossaryEntriesFromTexts(IEnumerable<string> texts)
	{
		if (texts == null)
		{
			yield break;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string text in texts)
		{
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(text);
			}
		}
		foreach (UIGlossaryData item in GetGlossaryEntries_Internal(stringBuilder.ToString()))
		{
			yield return item;
		}
	}

	public static UIGlossaryData? GetGlossaryEntryFromImage(Image image)
	{
		if (image == null || image.sprite == null)
		{
			return null;
		}
		_glossaryData.GetDataFromImage(image.sprite, out var data);
		return data;
	}
}
