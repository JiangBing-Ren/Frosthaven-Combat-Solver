using System;
using System.Collections.Generic;
using FHGUI.Configs;
using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Scenario Narrative ConfigUI", fileName = "ScenarioNarrativeConfigUI")]
public class ScenarioNarrativeConfigUI : ScriptableObject
{
	[Serializable]
	public class NarrativeConfig
	{
		public string id;

		[ConfigIcon(48, null, false, true)]
		public Sprite Illustration;

		[ConfigIcon(48, null, false, true)]
		public StoryCharacterConfigUI StoryCharacter;
	}

	public string ScenarioId;

	public List<NarrativeConfig> Intro;

	public List<NarrativeConfig> Outro;

	public List<NarrativeConfig> Rooms;

	public List<NarrativeConfig> Choice;

	public Sprite GetIllustration(string id)
	{
		if (TryGetIllustration(id, Intro, out var sprite))
		{
			return sprite;
		}
		if (TryGetIllustration(id, Outro, out var sprite2))
		{
			return sprite2;
		}
		if (TryGetIllustration(id, Rooms, out var sprite3))
		{
			return sprite3;
		}
		if (TryGetIllustration(id, Choice, out var sprite4))
		{
			return sprite4;
		}
		return null;
	}

	public StoryCharacterConfigUI GetStoryCharacter(string id)
	{
		if (TryGetStoryCharacter(id, Intro, out var sprite))
		{
			return sprite;
		}
		if (TryGetStoryCharacter(id, Outro, out var sprite2))
		{
			return sprite2;
		}
		if (TryGetStoryCharacter(id, Rooms, out var sprite3))
		{
			return sprite3;
		}
		return null;
	}

	private bool TryGetIllustration(string id, List<NarrativeConfig> list, out Sprite sprite)
	{
		sprite = null;
		if (list == null || list.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Equals(list[i].id, id, StringComparison.OrdinalIgnoreCase))
			{
				sprite = list[i].Illustration;
				return true;
			}
		}
		return false;
	}

	private bool TryGetStoryCharacter(string id, List<NarrativeConfig> list, out StoryCharacterConfigUI sprite)
	{
		sprite = null;
		if (list == null || list.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Equals(list[i].id, id, StringComparison.OrdinalIgnoreCase))
			{
				sprite = list[i].StoryCharacter;
				return true;
			}
		}
		return false;
	}

	public bool SetSprite(Sprite sprite, List<NarrativeConfig> list, string id)
	{
		if (list == null || list.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Equals(list[i].id, id, StringComparison.OrdinalIgnoreCase))
			{
				list[i].Illustration = sprite;
				return true;
			}
		}
		return false;
	}

	public bool SetSprite(Sprite sprite, string id)
	{
		if (SetSprite(sprite, Intro, id))
		{
			return true;
		}
		if (SetSprite(sprite, Outro, id))
		{
			return true;
		}
		if (SetSprite(sprite, Rooms, id))
		{
			return true;
		}
		return false;
	}
}
