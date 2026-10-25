using System;
using UnityEngine;

namespace FHGUI.DTO.Outpost;

public class NarrativeDataSO : ScriptableObject
{
	public struct ScenarioIds
	{
		private static readonly string[] TYPES = new string[6] { "_Intro", "_Outro", "_Room", "_Special", "_Start", "_Choice" };

		public string ScenarioID;

		public string Type;

		public string Sufix;

		public readonly bool Valid;

		public ScenarioIds(string id)
		{
			if (id == null)
			{
				Type = string.Empty;
				Sufix = string.Empty;
				ScenarioID = string.Empty;
				Valid = false;
				return;
			}
			ReadOnlySpan<char> span = id;
			string[] tYPES = TYPES;
			foreach (string text in tYPES)
			{
				int num = span.IndexOf(text, StringComparison.Ordinal);
				if (num != -1)
				{
					Type = text;
					ReadOnlySpan<char> readOnlySpan = span.Slice(0, num);
					ReadOnlySpan<char> readOnlySpan2 = span.Slice(num + text.Length);
					ScenarioID = readOnlySpan.ToString();
					Sufix = readOnlySpan2.ToString();
					Valid = true;
					return;
				}
			}
			Valid = false;
			Type = string.Empty;
			Sufix = string.Empty;
			ScenarioID = id;
		}

		public override string ToString()
		{
			return "ScenarioID: " + ScenarioID + ", Type: " + Type;
		}
	}

	public Sprite DefaultIllustration;

	public ScenarioNarrativeConfigUI[] EventConfigs;

	public ScenarioNarrativeConfigUI this[string id]
	{
		get
		{
			ScenarioNarrativeConfigUI[] eventConfigs = EventConfigs;
			foreach (ScenarioNarrativeConfigUI scenarioNarrativeConfigUI in eventConfigs)
			{
				if (!(scenarioNarrativeConfigUI == null) && string.Equals(scenarioNarrativeConfigUI.ScenarioId, id, StringComparison.OrdinalIgnoreCase))
				{
					return scenarioNarrativeConfigUI;
				}
			}
			Debug.LogError("Missing data for narration " + id);
			return null;
		}
	}

	public Sprite GetIllustration(string fullId, bool defaultIfEmpty = false)
	{
		Sprite sprite = null;
		ScenarioIds scenarioIds = new ScenarioIds(fullId);
		string match;
		if (HasValue(scenarioIds.ScenarioID))
		{
			sprite = this[scenarioIds.ScenarioID].GetIllustration(fullId);
		}
		else if (HasSimilarValue(scenarioIds.ScenarioID, out match))
		{
			sprite = this[match].GetIllustration(fullId.Replace(scenarioIds.ScenarioID, match));
		}
		if ((sprite == null) & defaultIfEmpty)
		{
			sprite = DefaultIllustration;
		}
		return sprite;
	}

	public StoryCharacterConfigUI GetStoryCharacter(string fullId, bool defaultIfEmpty = false)
	{
		StoryCharacterConfigUI storyCharacterConfigUI = null;
		ScenarioIds scenarioIds = new ScenarioIds(fullId);
		if (HasValue(scenarioIds.ScenarioID))
		{
			storyCharacterConfigUI = this[scenarioIds.ScenarioID].GetStoryCharacter(fullId);
		}
		if ((storyCharacterConfigUI == null) & defaultIfEmpty)
		{
			storyCharacterConfigUI = null;
		}
		return storyCharacterConfigUI;
	}

	public bool HasValue(string id)
	{
		ScenarioNarrativeConfigUI[] eventConfigs = EventConfigs;
		foreach (ScenarioNarrativeConfigUI scenarioNarrativeConfigUI in eventConfigs)
		{
			if (!(scenarioNarrativeConfigUI == null) && string.Equals(scenarioNarrativeConfigUI.ScenarioId, id, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSimilarValue(string id, out string match)
	{
		ScenarioNarrativeConfigUI[] eventConfigs = EventConfigs;
		foreach (ScenarioNarrativeConfigUI scenarioNarrativeConfigUI in eventConfigs)
		{
			if (!(scenarioNarrativeConfigUI == null) && string.Equals(scenarioNarrativeConfigUI.ScenarioId.RemoveChar('\''), id, StringComparison.OrdinalIgnoreCase))
			{
				match = scenarioNarrativeConfigUI.ScenarioId;
				return true;
			}
		}
		match = null;
		return false;
	}

	public void AddScenario(ScenarioNarrativeConfigUI so)
	{
		int num = EventConfigs.Length + 1;
		Array.Resize(ref EventConfigs, num);
		EventConfigs[num - 1] = so;
	}
}
